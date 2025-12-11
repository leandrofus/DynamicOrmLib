namespace DynamicOrmLib;

public enum FilterOp { Eq, Neq, Gt, Gte, Lt, Lte, Contains }

public enum LogicOp { And, Or }

public class FilterCondition
{
  public string Field { get; set; } = null!;
  public FilterOp Op { get; set; }
  public object? Value { get; set; }
  public LogicOp LogicOp { get; set; } = LogicOp.And;
}

public class QueryOptions
{
  public List<FilterCondition> Where { get; set; } = new List<FilterCondition>();
  public string? OrderBy { get; set; }
  public bool OrderDesc { get; set; }
  public int? Limit { get; set; }
  public List<JoinDefinition>? Joins { get; set; }
  public List<IncludeDefinition>? Includes { get; set; }
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
  public FilterOp Comparator { get; set; } = FilterOp.Eq; // comparator for join condition
}

public class IncludeDefinition
{
  public string Model { get; set; } = null!; // the model to include (e.g., "task")
  public string? As { get; set; } // alias for the included model (e.g., "t")
  public string? ForeignKey { get; set; } // the field in the source model that references the target (e.g., "task_id")
  public string? TargetKey { get; set; } = "id"; // the field in the target model to match (default "id")
  public bool Required { get; set; } = true; // true for inner join, false for left join
  public List<FilterCondition>? Where { get; set; } // conditions for this include
  public List<IncludeDefinition>? Include { get; set; } // nested includes
}

public class IncludeSpec
{
  public string Model { get; set; } = null!;
  public string? As { get; set; }
  public string? ForeignKey { get; set; }
  public string? TargetKey { get; set; } = "id";
  public bool Required { get; set; } = true;
  public object? Where { get; set; }
  public List<IncludeSpec>? Include { get; set; }
}

public class QuerySpec
{
  public IncludeSpec? Include { get; set; }
  public object? Where { get; set; }
  public string? OrderBy { get; set; }
  public bool OrderDesc { get; set; }
  public int? Limit { get; set; }
  public int? Offset { get; set; }
}
