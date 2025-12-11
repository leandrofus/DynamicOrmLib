using System.Text.Json.Nodes;

namespace DynamicOrmLib;

public class ModuleInfo
{
  public string Name { get; set; } = null!;
  public string Version { get; set; } = "0.0.1";
  public string? Author { get; set; }
}

public class ModuleManifest
{
  public ModuleInfo Module { get; set; } = new();
  public List<ModelDefinition> Models { get; set; } = new();
  public List<string>? DependsOn { get; set; }
  public List<JsonObject>? Impacts { get; set; }
  public List<JsonObject>? Views { get; set; }
  public List<JsonObject>? Workflows { get; set; }
}
