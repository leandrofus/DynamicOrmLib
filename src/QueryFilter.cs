namespace DynamicOrmLib;

public enum FilterOp { Eq, Neq, Gt, Gte, Lt, Lte, Contains }

public class FilterCondition
{
  public string Field { get; set; } = null!;
  public FilterOp Op { get; set; }
  public object? Value { get; set; }
}

public class QueryOptions
{
  public List<FilterCondition> Where { get; set; } = new List<FilterCondition>();
  public string? OrderBy { get; set; }
  public bool OrderDesc { get; set; }
  public int? Limit { get; set; }
  public List<JoinDefinition>? Joins { get; set; }
  public List<string>? GroupBy { get; set; }
  public List<FilterCondition>? Having { get; set; }
  public int? Offset { get; set; }
}

public enum JoinType { Inner, Left }
public class JoinDefinition
{
  public JoinType Type { get; set; } = JoinType.Inner;
  public string SourceModel { get; set; } = null!; // the model to join from (e.g., product)
  public string TargetModel { get; set; } = null!; // the model to join to (e.g., contact)
  public string SourceField { get; set; } = null!; // field in source used for join (e.g., contact_id)
  public string TargetField { get; set; } = "id"; // field in target to match (default id)
  public string? Alias { get; set; }
}
