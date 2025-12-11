using System.Text.Json.Serialization;

namespace DynamicOrmLib;

public enum FieldType
{
  String,
  Number,
  Boolean,
  Date,
  Text,
  Relation,
  Selection,
  Json
}

public class RelationDefinition
{
  public string Model { get; set; } = null!;
  public string? OnDelete { get; set; }
  public string? OnUpdate { get; set; }
}

public class FieldDefinition
{
  public string Name { get; set; } = null!;
  public string? Label { get; set; }
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public FieldType Type { get; set; }
  public bool Required { get; set; }
  public RelationDefinition? Relation { get; set; }
  public bool AutoIncrement { get; set; }
  // Optional additional attributes
  public int? Length { get; set; }
  public object? DefaultValue { get; set; }
  public bool PrimaryKey { get; set; }
}
