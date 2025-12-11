using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;

namespace DynamicOrmLib;

public static class ManifestLoader
{
  public static ModuleManifest LoadFromJson(string json)
  {
    var manifest = JsonSerializer.Deserialize<ModuleManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (manifest == null) throw new ArgumentException("Invalid manifest json");
    return manifest;
  }

  public static ModuleManifest LoadFromFile(string path)
  {
    var json = File.ReadAllText(path);
    return LoadFromJson(json);
  }

  public static void Validate(ModuleManifest manifest)
  {
    if (manifest.Module == null) throw new ArgumentException("Manifest must include module info");
    // Validate module name
    SqlProtection.ValidateModelName(manifest.Module.Name);
    if (manifest.Models == null || manifest.Models.Count == 0) return; // no models is allowed
    // Check model names and field names
    foreach (var m in manifest.Models)
    {
      if (string.IsNullOrWhiteSpace(m.Name)) throw new ArgumentException("Model must have a name");
      // model name validation for safety
      SqlProtection.ValidateModelName(m.Name);
      if (m.Fields != null)
      {
        foreach (var f in m.Fields)
        {
          if (string.IsNullOrWhiteSpace(f.Name)) throw new ArgumentException($"Model {m.Name} has a field without a name");
          SqlProtection.ValidateFieldName(f.Name);
        }
      }
    }

    // Validate dependencies format
    if (manifest.DependsOn != null)
    {
      foreach (var d in manifest.DependsOn)
      {
        if (string.IsNullOrWhiteSpace(d)) throw new ArgumentException("dependsOn must not contain empty values");
      }
    }

    // Validate impacts structure (basic validation)
    if (manifest.Impacts != null)
    {
      foreach (var impact in manifest.Impacts)
      {
        if (!impact.TryGetPropertyValue("action", out var actionNode) || actionNode == null)
        {
          throw new ArgumentException("Each impact must have an 'action' field");
        }
        var action = actionNode.GetValue<string?>();
        var allowed = new[] { "addField", "extendEnum", "addRelation", "addIndex", "createModelTable" };
        if (string.IsNullOrEmpty(action) || !allowed.Contains(action)) throw new ArgumentException($"Unsupported impact action: {action}");
        // Additional validations per action
        if (action == "addField" || action == "addRelation")
        {
          if (!impact.TryGetPropertyValue("field", out var fnode) || fnode == null) throw new ArgumentException("addField/addRelation impact must include field object");
          // field should have a name
          try
          {
            var fd = JsonSerializer.Deserialize<FieldDefinition>(fnode.ToJsonString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (fd == null || string.IsNullOrWhiteSpace(fd.Name)) throw new ArgumentException("Impact field must include a name");
            SqlProtection.ValidateFieldName(fd.Name);
          }
          catch (JsonException) { throw new ArgumentException("Invalid field object in impact"); }
        }
        if (action == "extendEnum")
        {
          if (!impact.TryGetPropertyValue("field", out var fnode) || fnode == null) throw new ArgumentException("extendEnum impact must include field name");
          if (!impact.TryGetPropertyValue("values", out var vals) || vals == null || vals is not JsonArray) throw new ArgumentException("extendEnum impact must include values array");
          SqlProtection.ValidateFieldName(fnode.GetValue<string>());
        }
        if (action == "addIndex")
        {
          if (!impact.TryGetPropertyValue("field", out var fn) || fn == null) throw new ArgumentException("addIndex impact must include field name");
          SqlProtection.ValidateFieldName(fn.GetValue<string>());
        }
        if (action == "createModelTable")
        {
          if (!impact.TryGetPropertyValue("targetModel", out var tm) || tm == null) throw new ArgumentException("createModelTable impact must include targetModel");
          SqlProtection.ValidateModelName(tm.GetValue<string>());
        }
      }
    }
  }
}
