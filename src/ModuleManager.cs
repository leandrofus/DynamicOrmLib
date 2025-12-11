using System.Text.Json.Nodes;

namespace DynamicOrmLib;

public class ModuleManager
{
  private readonly Dictionary<string, List<ModuleManifest>> _manifests = new();

  public List<ModuleManifest> ResolveOrder(IEnumerable<ModuleManifest> manifests)
  {
    // Simplified: no dependency resolution needed
    return manifests.ToList();
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

    // Initialize provider
    provider.Init();

    // Install each manifest (simplified - no dependency resolution or impacts)
    foreach (var manifest in manifests)
    {
      // Register models for this manifest
      if (manifest.Models != null)
      {
        foreach (var model in manifest.Models)
        {
          provider.RegisterModel(model);
        }
      }
    }
  }
}
