using System.Text.Json.Nodes;

namespace DynamicOrmLib;

public class ModelDefinition
{
  public string Name { get; set; } = null!;
  public string? Module { get; set; }
  public List<FieldDefinition> Fields { get; set; } = new();
  public JsonObject? Metadata { get; set; }
}
