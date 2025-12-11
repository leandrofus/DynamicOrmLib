using System.Text.Json.Nodes;

namespace DynamicOrmLib;

public class ModuleManager
{
  private readonly Dictionary<string, List<ModuleManifest>> _manifests = new();

  public void AddManifest(ModuleManifest manifest)
  {
    var name = manifest.Module?.Name ?? throw new ArgumentException("Manifest missing module name");
    if (!_manifests.ContainsKey(name)) _manifests[name] = new List<ModuleManifest>();
    _manifests[name].Add(manifest);
  }

  public List<ModuleManifest> ResolveOrder(IEnumerable<ModuleManifest> manifests)
  {
    // Build name -> selected manifest version mapping from provided manifests
    var map = manifests.ToDictionary(m => m.Module.Name, m => m);

    // Build graph nodes
    var graph = new Dictionary<string, HashSet<string>>(); // node -> dependents
    var indegree = new Dictionary<string, int>();

    foreach (var kv in map)
    {
      graph[kv.Key] = new HashSet<string>();
      indegree[kv.Key] = 0;
    }

    // Add edges based on dependsOn
    foreach (var kv in map)
    {
      var name = kv.Key;
      var manifest = kv.Value;
      if (manifest.DependsOn == null) continue;
      foreach (var depRaw in manifest.DependsOn)
      {
        var dep = ParseDependency(depRaw);
        if (!map.ContainsKey(dep.Name)) throw new InvalidOperationException($"Missing dependency: {dep.Name} required by {name}");
        // Validate version if constraint present
        var other = map[dep.Name].Module.Version;
        if (!VersionSatisfies(other, dep.Comparator, dep.Version)) throw new InvalidOperationException($"Dependency version mismatch: {dep.Name}{(string.IsNullOrEmpty(dep.Comparator) ? "" : "@" + dep.Comparator + dep.Version)} required by {name}");
        // edge: dep.Name -> name (dep before dependent)
        graph[dep.Name].Add(name);
        indegree[name] = indegree.GetValueOrDefault(name) + 1;
      }
    }

    // Kahn's algorithm for topological sort
    var queue = new Queue<string>(indegree.Where(x => x.Value == 0).Select(x => x.Key));
    var resultNames = new List<string>();
    while (queue.Any())
    {
      var n = queue.Dequeue();
      resultNames.Add(n);
      foreach (var m in graph[n])
      {
        indegree[m]--;
        if (indegree[m] == 0) queue.Enqueue(m);
      }
    }

    if (resultNames.Count != map.Count) throw new InvalidOperationException("Dependency cycle detected or missing dependency");

    var ordered = resultNames.Select(n => map[n]).ToList();
    return ordered;
  }

  private static (string Name, string Comparator, string Version) ParseDependency(string raw)
  {
    if (string.IsNullOrWhiteSpace(raw)) throw new ArgumentException("Invalid dependency string");
    var parts = raw.Split('@', 2);
    var name = parts[0].Trim();
    if (parts.Length == 1) return (name, string.Empty, string.Empty);
    var ver = parts[1].Trim();
    // comparator is first non-digit character (like >=, <=, >, <, =). We'll parse operators.
    string comparator = "=";
    var compPrefixes = new[] { ">=", "<=", ">", "<", "!=" };
    foreach (var cp in compPrefixes)
    {
      if (ver.StartsWith(cp)) { comparator = cp; ver = ver.Substring(cp.Length); break; }
    }
    // If no explicit comparator and ver starts with a digit, comparator '='
    return (name, comparator, ver);
  }

  private static bool VersionSatisfies(string actual, string comparator, string required)
  {
    if (string.IsNullOrEmpty(required)) return true;
    comparator = comparator ?? "=";
    // Try to parse as System.Version
    if (!Version.TryParse(required, out var r))
    {
      // fallback simple equality
      comparator = "=";
      r = null;
    }
    if (!Version.TryParse(actual, out var a))
    {
      // fallback to string equality
      return comparator == "=" ? actual == required : false;
    }
    if (r == null) return comparator == "=" ? actual == required : false;

    var cmp = a.CompareTo(r);
    return comparator switch
    {
      "=" => cmp == 0,
      "!=" => cmp != 0,
      ">" => cmp > 0,
      "<" => cmp < 0,
      ">=" => cmp >= 0,
      "<=" => cmp <= 0,
      _ => false
    };
  }

  public void Clear() => _manifests.Clear();

  public void Install(IEnumerable<ModuleManifest> manifests, IStoreProvider provider)
  {
    if (manifests == null) throw new ArgumentNullException(nameof(manifests));
    if (provider == null) throw new ArgumentNullException(nameof(provider));

    // Validate each manifest before attempting to install
    foreach (var m in manifests)
    {
      ManifestLoader.Validate(m);
    }

    // Resolve order based on dependsOn
    var ordered = ResolveOrder(manifests);

    // Initialize provider
    provider.Init();

    // Apply per-manifest in order
    foreach (var manifest in ordered)
    {
      var moduleInfo = manifest.Module ?? throw new InvalidOperationException("Manifest module information missing");
      try
      {
        // Begin transaction (adapter may implement support)
        try { provider.BeginTransaction(); } catch { /* ignore if not implemented by provider */ }

        // Register models for this manifest and persist to managed schema
        if (manifest.Models != null)
        {
          foreach (var model in manifest.Models)
          {
            model.Module = moduleInfo.Name;
            provider.RegisterModel(model);
            try { provider.UpsertManagedSchema(model, moduleInfo); } catch { /* ignore if provider doesn't support managed table */ }
          }
        }

        // Apply impacts declared by this manifest
        if (manifest.Impacts != null)
        {
          foreach (var impact in manifest.Impacts)
          {
            try
            {
              provider.ApplyImpact(moduleInfo, impact);
              try { provider.LogSchemaChange(moduleInfo.Name, impact, moduleInfo, "applyImpact"); } catch { }
            }
            catch (Exception ex)
            {
              try { provider.Rollback(); } catch { }
              throw new InvalidOperationException($"Failed to apply impact for module {manifest.Module?.Name}: {ex.Message}", ex);
            }
          }
        }

        // Commit transaction
        try { provider.Commit(); } catch { }
      }
      catch
      {
        try { provider.Rollback(); } catch { }
        throw;
      }
    }
  }
}
