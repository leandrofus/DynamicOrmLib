using System.Text.Json.Nodes;

namespace DynamicOrmLib;

public class ModuleManifest
{
  public List<ModelDefinition> Models { get; set; } = new();
}
