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
  }
}
