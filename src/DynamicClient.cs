using System.Text.Json.Nodes;

namespace DynamicOrmLib;

/// <summary>
/// Builder class for defining dynamic models with fields and relationships.
/// </summary>
public class ModelBuilder
{
  private readonly DynamicClient _client;
  private readonly ModelDefinition _model = new ModelDefinition();

  internal ModelBuilder(DynamicClient client, string name)
  {
    _client = client;
    _model.Name = name;
  }

  /// <summary>
  /// Sets the module name for the model.
  /// </summary>
  /// <param name="module">The module name.</param>
  /// <returns>The ModelBuilder instance for method chaining.</returns>
  public ModelBuilder Module(string module)
  {
    _model.Module = module;
    return this;
  }

  /// <summary>
  /// Adds a field to the model definition.
  /// </summary>
  /// <param name="name">The field name.</param>
  /// <param name="type">The field type.</param>
  /// <param name="required">Whether the field is required.</param>
  /// <param name="label">Optional display label for the field.</param>
  /// <param name="length">Optional maximum length for string fields.</param>
  /// <param name="defaultValue">Optional default value for the field.</param>
  /// <param name="primaryKey">Whether this field is the primary key.</param>
  /// <param name="relationModel">Optional related model name for relations.</param>
  /// <param name="onDelete">Optional cascade action on delete.</param>
  /// <param name="onUpdate">Optional cascade action on update.</param>
  /// <returns>The ModelBuilder instance for method chaining.</returns>
  public ModelBuilder Field(string name, FieldType type, bool required = false, string? label = null, int? length = null, object? defaultValue = null, bool primaryKey = false, string? relationModel = null, string? onDelete = null, string? onUpdate = null)
  {
    var field = new FieldDefinition { Name = name, Type = type, Required = required, Label = label, Length = length, DefaultValue = defaultValue, PrimaryKey = primaryKey };
    if (!string.IsNullOrWhiteSpace(relationModel)) field.Relation = new RelationDefinition { Model = relationModel, OnDelete = onDelete, OnUpdate = onUpdate };
    _model.Fields.Add(field);
    return this;
  }

  /// <summary>
  /// Builds and registers the model with the client.
  /// </summary>
  /// <returns>The DynamicClient instance.</returns>
  public DynamicClient Build()
  {
    _client.RegisterModel(_model);
    return _client;
  }
}

/// <summary>
/// Fluent API for building filter conditions.
/// </summary>
public class FilterBuilder
{
  internal List<FilterCondition> Conditions { get; } = new();

  /// <summary>
  /// Starts a WHERE condition for a field.
  /// </summary>
  public FieldCondition Where(string fieldName)
  {
    return new FieldCondition(this, fieldName);
  }

  /// <summary>
  /// Adds an AND condition group.
  /// </summary>
  public FilterBuilder And(Action<FilterBuilder> conditionGroup)
  {
    var subBuilder = new FilterBuilder();
    conditionGroup(subBuilder);
    foreach (var condition in subBuilder.Conditions)
    {
      condition.LogicOp = LogicOp.And;
      Conditions.Add(condition);
    }
    return this;
  }

  /// <summary>
  /// Adds an OR condition group.
  /// </summary>
  public FilterBuilder Or(Action<FilterBuilder> conditionGroup)
  {
    var subBuilder = new FilterBuilder();
    conditionGroup(subBuilder);
    foreach (var condition in subBuilder.Conditions)
    {
      condition.LogicOp = LogicOp.Or;
      Conditions.Add(condition);
    }
    return this;
  }
}

/// <summary>
/// Represents a field condition in the fluent API.
/// </summary>
public class FieldCondition
{
  private readonly FilterBuilder _builder;
  private readonly string _fieldName;

  internal FieldCondition(FilterBuilder builder, string fieldName)
  {
    _builder = builder;
    _fieldName = fieldName;
  }

  /// <summary>
  /// Adds an equality condition.
  /// </summary>
  public FilterBuilder Equal(object value)
  {
    _builder.Conditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Eq,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "not equals" condition.
  /// </summary>
  public FilterBuilder NotEquals(object value)
  {
    _builder.Conditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Neq,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "less than" condition.
  /// </summary>
  public FilterBuilder LessThan(object value)
  {
    _builder.Conditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Lt,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "less than or equal" condition.
  /// </summary>
  public FilterBuilder LessThanOrEqual(object value)
  {
    _builder.Conditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Lte,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "greater than" condition.
  /// </summary>
  public FilterBuilder GreaterThan(object value)
  {
    _builder.Conditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Gt,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "greater than or equal" condition.
  /// </summary>
  public FilterBuilder GreaterThanOrEqual(object value)
  {
    _builder.Conditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Gte,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "contains" condition.
  /// </summary>
  public FilterBuilder Contains(object value)
  {
    _builder.Conditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Contains,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }
}

/// <summary>
/// Represents a field condition in the fluent API for queries.
/// </summary>
public class FieldConditionQuery
{
  private readonly QueryBuilder _builder;
  private readonly string _fieldName;

  internal FieldConditionQuery(QueryBuilder builder, string fieldName)
  {
    _builder = builder;
    _fieldName = fieldName;
  }

  /// <summary>
  /// Adds an equality condition.
  /// </summary>
  public QueryBuilder Equal(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Eq,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "not equals" condition.
  /// </summary>
  public QueryBuilder NotEquals(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Neq,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "less than" condition.
  /// </summary>
  public QueryBuilder LessThan(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Lt,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "less than or equal" condition.
  /// </summary>
  public QueryBuilder LessThanOrEqual(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Lte,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "greater than" condition.
  /// </summary>
  public QueryBuilder GreaterThan(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Gt,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "greater than or equal" condition.
  /// </summary>
  public QueryBuilder GreaterThanOrEqual(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Gte,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "contains" condition.
  /// </summary>
  public QueryBuilder Contains(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Contains,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }
}

/// <summary>
/// Fluent API for building complex queries.
/// </summary>
public class QueryBuilder
{
  internal List<FilterCondition> WhereConditions { get; } = new();
  internal List<FilterCondition> Conditions => WhereConditions;
  internal string? OrderByField { get; set; }
  internal bool OrderDescending { get; set; }
  internal int? LimitValue { get; set; }
  internal int? OffsetValue { get; set; }
  internal List<IncludeBuilder> Includes { get; } = new();

  /// <summary>
  /// Starts a WHERE condition for a field.
  /// </summary>
  public FieldConditionQuery Where(string fieldName)
  {
    return new FieldConditionQuery(this, fieldName);
  }

  /// <summary>
  /// Adds an AND condition group.
  /// </summary>
  public QueryBuilder And(Action<QueryBuilder> conditionGroup)
  {
    var subBuilder = new QueryBuilder();
    conditionGroup(subBuilder);
    foreach (var condition in subBuilder.WhereConditions)
    {
      condition.LogicOp = LogicOp.And;
      WhereConditions.Add(condition);
    }
    return this;
  }

  /// <summary>
  /// Adds an OR condition group.
  /// </summary>
  public QueryBuilder Or(Action<QueryBuilder> conditionGroup)
  {
    var subBuilder = new QueryBuilder();
    conditionGroup(subBuilder);
    foreach (var condition in subBuilder.WhereConditions)
    {
      condition.LogicOp = LogicOp.Or;
      WhereConditions.Add(condition);
    }
    return this;
  }

  /// <summary>
  /// Sets the ORDER BY clause.
  /// </summary>
  public QueryBuilder OrderBy(string field, bool descending = false)
  {
    OrderByField = field;
    OrderDescending = descending;
    return this;
  }

  /// <summary>
  /// Sets the LIMIT clause.
  /// </summary>
  public QueryBuilder Limit(int limit)
  {
    LimitValue = limit;
    return this;
  }

  /// <summary>
  /// Sets the OFFSET clause.
  /// </summary>
  public QueryBuilder Offset(int offset)
  {
    OffsetValue = offset;
    return this;
  }

  /// <summary>
  /// Adds an include for related data.
  /// </summary>
  public QueryBuilder Include(string model, string? alias = null, Action<IncludeBuilder>? includeConfig = null)
  {
    var includeBuilder = new IncludeBuilder(model, alias);
    if (includeConfig != null)
    {
      includeConfig(includeBuilder);
    }
    Includes.Add(includeBuilder);
    return this;
  }
}

/// <summary>
/// Represents a field condition in the fluent API for includes.
/// </summary>
public class FieldConditionInclude
{
  private readonly IncludeBuilder _builder;
  private readonly string _fieldName;

  internal FieldConditionInclude(IncludeBuilder builder, string fieldName)
  {
    _builder = builder;
    _fieldName = fieldName;
  }

  /// <summary>
  /// Adds an equality condition.
  /// </summary>
  public IncludeBuilder Equal(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Eq,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "not equals" condition.
  /// </summary>
  public IncludeBuilder NotEquals(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Neq,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "less than" condition.
  /// </summary>
  public IncludeBuilder LessThan(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Lt,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "less than or equal" condition.
  /// </summary>
  public IncludeBuilder LessThanOrEqual(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Lte,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "greater than" condition.
  /// </summary>
  public IncludeBuilder GreaterThan(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Gt,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "greater than or equal" condition.
  /// </summary>
  public IncludeBuilder GreaterThanOrEqual(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Gte,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }

  /// <summary>
  /// Adds a "contains" condition.
  /// </summary>
  public IncludeBuilder Contains(object value)
  {
    _builder.WhereConditions.Add(new FilterCondition
    {
      Field = _fieldName,
      Op = FilterOp.Contains,
      Value = value,
      LogicOp = LogicOp.And
    });
    return _builder;
  }
}

/// <summary>
/// Fluent API for building include specifications.
/// </summary>
public class IncludeBuilder
{
  internal string Model { get; }
  internal string? Alias { get; }
  internal List<FilterCondition> WhereConditions { get; } = new();
  internal List<IncludeBuilder> NestedIncludes { get; } = new();

  internal IncludeBuilder(string model, string? alias = null)
  {
    Model = model;
    Alias = alias;
  }

  /// <summary>
  /// Starts a WHERE condition for the included model.
  /// </summary>
  public FieldConditionInclude Where(string fieldName)
  {
    return new FieldConditionInclude(this, fieldName);
  }

  /// <summary>
  /// Adds a nested include.
  /// </summary>
  public IncludeBuilder Include(string model, string? alias = null, Action<IncludeBuilder>? includeConfig = null)
  {
    var includeBuilder = new IncludeBuilder(model, alias);
    if (includeConfig != null)
    {
      includeConfig(includeBuilder);
    }
    NestedIncludes.Add(includeBuilder);
    return this;
  }
}

/// <summary>
/// Fluent API for building JSON objects for INSERT/UPDATE operations.
/// </summary>
public class ObjectBuilder
{
  internal Dictionary<string, object?> Fields { get; } = new();

  /// <summary>
  /// Sets a field value.
  /// </summary>
  public ObjectBuilder Set(string fieldName, object? value)
  {
    Fields[fieldName] = value;
    return this;
  }

  /// <summary>
  /// Builds the JSON object.
  /// </summary>
  internal JsonObject Build()
  {
    var jsonObject = new JsonObject();
    foreach (var field in Fields)
    {
      jsonObject[field.Key] = JsonValue.Create(field.Value);
    }
    return jsonObject;
  }
}

/// <summary>
/// Provides a high-level client interface for interacting with dynamic ORM operations.
/// Supports model definition, CRUD operations, and complex queries with includes.
/// </summary>
public class DynamicClient
{
  private readonly DynamicContext _context;

  /// <summary>
  /// Initializes a new instance of the DynamicClient class.
  /// </summary>
  /// <param name="provider">The store provider to use for data operations. If null, uses an in-memory provider.</param>
  public DynamicClient(IStoreProvider? provider = null)
  {
    _context = new DynamicContext(provider);
  }

  /// <summary>
  /// Registers a module manifest with the client.
  /// </summary>
  /// <param name="manifest">The module manifest to register.</param>
  public void RegisterManifest(ModuleManifest manifest) => _context.RegisterManifest(manifest);

  /// <summary>
  /// Registers a module manifest from a JSON file.
  /// </summary>
  /// <param name="path">The path to the manifest JSON file.</param>
  /// <returns>The loaded and registered module manifest.</returns>
  public ModuleManifest RegisterManifestFromFile(string path)
  {
    var manifest = ManifestLoader.LoadFromFile(path);
    _context.RegisterManifest(manifest);
    return manifest;
  }

  /// <summary>
  /// Creates a model builder for defining a new model.
  /// </summary>
  /// <param name="name">The name of the model to define.</param>
  /// <returns>A ModelBuilder instance for fluent model definition.</returns>
  public ModelBuilder DefineModel(string name) => new ModelBuilder(this, name);

  internal void RegisterModel(ModelDefinition model) => _context.RegisterModel(model);

  /// <summary>
  /// Creates a new record in the specified model.
  /// </summary>
  /// <param name="modelName">The name of the model to create the record in.</param>
  /// <param name="data">The data for the new record as a JSON object.</param>
  /// <returns>The created DynamicRecord.</returns>
  public DynamicRecord Create(string modelName, JsonObject data) => _context.CreateRecord(modelName, data);

  /// <summary>
  /// Creates a new record in the specified model using a fluent object builder.
  /// Provides a strongly-typed, IntelliSense-friendly API for building object data.
  /// </summary>
  /// <param name="modelName">The name of the model to create the record in.</param>
  /// <param name="objectBuilder">A function that builds the object data using the fluent API.</param>
  /// <returns>The created DynamicRecord.</returns>
  public DynamicRecord Create(string modelName, Func<ObjectBuilder, ObjectBuilder> objectBuilder)
  {
    var builder = new ObjectBuilder();
    var result = objectBuilder(builder);
    return _context.CreateRecord(modelName, result.Build());
  }

  /// <summary>
  /// Retrieves all records from the specified model.
  /// </summary>
  /// <param name="modelName">The name of the model to query.</param>
  /// <returns>An enumerable collection of DynamicRecord objects.</returns>
  public IEnumerable<DynamicRecord> Query(string modelName) => _context.GetRecords(modelName);

  /// <summary>
  /// Retrieves a single record by its ID.
  /// </summary>
  /// <param name="modelName">The name of the model (not used in current implementation).</param>
  /// <param name="id">The ID of the record to retrieve.</param>
  /// <returns>The DynamicRecord if found, null otherwise.</returns>
  public DynamicRecord? GetOne(string modelName, string id) => _context.GetById(id);

  /// <summary>
  /// Retrieves multiple records from the specified model with optional query options.
  /// </summary>
  /// <param name="modelName">The name of the model to query.</param>
  /// <param name="options">Optional query options including filters, includes, ordering, and limits.</param>
  /// <returns>An enumerable collection of DynamicRecord objects.</returns>
  public IEnumerable<DynamicRecord> GetMany(string modelName, QueryOptions? options = null) => _context.GetRecords(modelName, options);

  /// <summary>
  /// Retrieves multiple records from the specified model using a QuerySpec for human-readable query definition.
  /// </summary>
  /// <param name="modelName">The name of the model to query.</param>
  /// <param name="spec">The QuerySpec containing include, where, order, and limit specifications.</param>
  /// <returns>An enumerable collection of DynamicRecord objects.</returns>
  public IEnumerable<DynamicRecord> GetMany(string modelName, QuerySpec spec)
  {
    var options = ConvertQuerySpecToOptions(spec);
    return _context.GetRecords(modelName, options);
  }

  /// <summary>
  /// Retrieves records from the specified model using a fluent query builder.
  /// Provides a strongly-typed, IntelliSense-friendly API for building complex queries with includes, filters, ordering, and pagination.
  /// </summary>
  /// <param name="modelName">The name of the model to query.</param>
  /// <param name="queryBuilder">A function that builds the query using the fluent API.</param>
  /// <returns>An enumerable collection of DynamicRecord objects.</returns>
  public IEnumerable<DynamicRecord> GetMany(string modelName, Func<QueryBuilder, QueryBuilder> queryBuilder)
  {
    var builder = new QueryBuilder();
    var result = queryBuilder(builder);
    var options = new QueryOptions
    {
      Where = result.WhereConditions,
      OrderBy = result.OrderByField,
      OrderDesc = result.OrderDescending,
      Limit = result.LimitValue,
      Offset = result.OffsetValue,
      Includes = result.Includes.Select(ConvertIncludeBuilderToDefinition).ToList()
    };
    return _context.GetRecords(modelName, options);
  }

  /// <summary>
  /// Retrieves the top N records from the specified model.
  /// </summary>
  /// <param name="modelName">The name of the model to query.</param>
  /// <param name="n">The number of records to retrieve.</param>
  /// <param name="options">Optional query options including filters and includes.</param>
  /// <returns>An enumerable collection of up to N DynamicRecord objects.</returns>
  public IEnumerable<DynamicRecord> GetTopN(string modelName, int n, QueryOptions? options = null)
  {
    options ??= new QueryOptions();
    options.Limit = n;
    if (options != null && options.Joins != null && options.Joins.Count > 0)
    {
      return _context.GetManyWithJoins(modelName, options).Take(n);
    }
    return _context.GetRecords(modelName, options).Take(n);
  }

  /// <summary>
  /// Updates an existing record by its ID.
  /// </summary>
  /// <param name="id">The ID of the record to update.</param>
  /// <param name="data">The updated data as a JSON object.</param>
  /// <returns>The updated DynamicRecord.</returns>
  public DynamicRecord Update(string id, JsonObject data) => _context.UpdateRecord(id, data);

  /// <summary>
  /// Updates a record by its ID using a fluent object builder.
  /// Provides a strongly-typed, IntelliSense-friendly API for building update data.
  /// </summary>
  /// <param name="id">The ID of the record to update.</param>
  /// <param name="objectBuilder">A function that builds the update data using the fluent API.</param>
  /// <returns>The updated DynamicRecord.</returns>
  public DynamicRecord Update(string id, Func<ObjectBuilder, ObjectBuilder> objectBuilder)
  {
    var builder = new ObjectBuilder();
    var result = objectBuilder(builder);
    return _context.UpdateRecord(id, result.Build());
  }

  /// <summary>
  /// Deletes a record by its ID.
  /// </summary>
  /// <param name="id">The ID of the record to delete.</param>
  public void Delete(string id) => _context.DeleteRecord(id);

  /// <summary>
  /// Deletes records from the specified model based on query options.
  /// </summary>
  /// <param name="modelName">The name of the model to delete from.</param>
  /// <param name="options">Query options specifying which records to delete.</param>
  public void Delete(string modelName, QueryOptions options) => _context.DeleteRecords(modelName, options);

  /// <summary>
  /// Deletes records from the specified model based on an object-based where specification.
  /// Supports multiple formats for easy condition specification:
  /// - String: "field op value" (e.g., "hours &lt; 5")
  /// - Simple object: new { field = value } for equality
  /// - Complex object: new { field = new { op = "operator", value = val } }
  /// - Multiple conditions: new { field1 = value1, field2 = value2 } (AND logic)
  /// </summary>
  /// <param name="modelName">The name of the model to delete from.</param>
  /// <param name="whereSpec">An object specifying the conditions for deletion.</param>
  public void Delete(string modelName, object whereSpec)
  {
    var options = new QueryOptions
    {
      Where = ConvertWhereSpecToConditions(whereSpec)
    };
    _context.DeleteRecords(modelName, options);
  }

  /// <summary>
  /// Deletes records from the specified model using a fluent filter builder.
  /// Provides a strongly-typed, IntelliSense-friendly API for building complex conditions.
  /// </summary>
  /// <param name="modelName">The name of the model to delete from.</param>
  /// <param name="filterBuilder">A function that builds the filter conditions using the fluent API.</param>
  public void Delete(string modelName, Func<FilterBuilder, FilterBuilder> filterBuilder)
  {
    var builder = new FilterBuilder();
    var result = filterBuilder(builder);
    var options = new QueryOptions
    {
      Where = result.Conditions
    };
    _context.DeleteRecords(modelName, options);
  }

  /// <summary>
  /// Inserts a new record or updates an existing one if it exists.
  /// </summary>
  /// <param name="modelName">The name of the model to upsert into.</param>
  /// <param name="id">The ID of the record. If null, a new record is created.</param>
  /// <param name="data">The data for the record as a JSON object.</param>
  /// <returns>The created or updated DynamicRecord.</returns>
  public DynamicRecord Upsert(string modelName, string? id, JsonObject data) => _context.UpsertRecord(modelName, id, data);

  private QueryOptions ConvertQuerySpecToOptions(QuerySpec spec)
  {
    var options = new QueryOptions
    {
      OrderBy = spec.OrderBy,
      OrderDesc = spec.OrderDesc,
      Limit = spec.Limit,
      Offset = spec.Offset
    };

    if (spec.Where != null)
    {
      options.Where = ConvertWhereSpecToConditions(spec.Where);
    }

    if (spec.Include != null)
    {
      options.Includes = new List<IncludeDefinition> { ConvertIncludeSpecToDefinition(spec.Include) };
    }

    return options;
  }

  private List<FilterCondition> ConvertWhereSpecToConditions(object whereSpec)
  {
    var conditions = new List<FilterCondition>();

    if (whereSpec is string str)
    {
      // Parse string conditions that may include logical operators
      conditions.AddRange(ParseStringConditions(str));
    }
    else if (whereSpec is System.Collections.IDictionary dict)
    {
      if (dict.Contains("or") && dict["or"] is System.Collections.IEnumerable orArray)
      {
        // Handle OR conditions: { or = [ { hours = 5 }, { description = "test" } ] }
        foreach (var item in orArray)
        {
          if (item is System.Collections.IDictionary subDict)
          {
            foreach (System.Collections.DictionaryEntry subEntry in subDict)
            {
              var subField = subEntry.Key.ToString()!;
              var subValue = subEntry.Value;

              if (subValue is System.Collections.IDictionary subOpDict)
              {
                var op = subOpDict["op"]?.ToString() ?? subOpDict["operator"]?.ToString() ?? "=";
                var val = subOpDict["value"] ?? subOpDict["val"] ?? subOpDict["v"];
                conditions.Add(new FilterCondition
                {
                  Field = subField,
                  Op = ParseOperator(op),
                  Value = val,
                  LogicOp = LogicOp.Or
                });
              }
              else
              {
                conditions.Add(new FilterCondition
                {
                  Field = subField,
                  Op = FilterOp.Eq,
                  Value = subValue,
                  LogicOp = LogicOp.Or
                });
              }
            }
          }
        }
      }
      else
      {
        // Handle AND conditions (default)
        foreach (System.Collections.DictionaryEntry entry in dict)
        {
          var field = entry.Key.ToString()!;
          var value = entry.Value;

          if (value is System.Collections.IDictionary opDict)
          {
            // Handle { op = "<", value = 5 } or similar
            var op = opDict["op"]?.ToString() ?? opDict["operator"]?.ToString() ?? "=";
            var val = opDict["value"] ?? opDict["val"] ?? opDict["v"];
            conditions.Add(new FilterCondition
            {
              Field = field,
              Op = ParseOperator(op),
              Value = val,
              LogicOp = LogicOp.And
            });
          }
          else
          {
            // Simple equality
            conditions.Add(new FilterCondition
            {
              Field = field,
              Op = FilterOp.Eq,
              Value = value,
              LogicOp = LogicOp.And
            });
          }
        }
      }
    }

    return conditions;
  }

  private List<FilterCondition> ParseStringConditions(string condition)
  {
    var conditions = new List<FilterCondition>();

    // Handle parentheses for grouping
    var parenGroups = ExtractParenthesizedGroups(condition);
    if (parenGroups.Any())
    {
      // Replace parenthesized groups with placeholders and parse recursively
      var placeholderMap = new Dictionary<string, List<FilterCondition>>();
      var modifiedCondition = condition;

      for (int i = 0; i < parenGroups.Count; i++)
      {
        var placeholder = $"__GROUP{i}__";
        var groupContent = parenGroups[i].Trim('(', ')');
        placeholderMap[placeholder] = ParseStringConditions(groupContent);
        modifiedCondition = modifiedCondition.Replace(parenGroups[i], placeholder);
      }

      // Now parse the modified condition
      conditions = ParseLogicalExpression(modifiedCondition, placeholderMap);
    }
    else
    {
      conditions = ParseLogicalExpression(condition, new Dictionary<string, List<FilterCondition>>());
    }

    return conditions;
  }

  private List<string> ExtractParenthesizedGroups(string input)
  {
    var groups = new List<string>();
    var level = 0;
    var start = -1;

    for (int i = 0; i < input.Length; i++)
    {
      if (input[i] == '(')
      {
        if (level == 0) start = i;
        level++;
      }
      else if (input[i] == ')')
      {
        level--;
        if (level == 0 && start != -1)
        {
          groups.Add(input.Substring(start, i - start + 1));
          start = -1;
        }
      }
    }

    return groups;
  }

  private List<FilterCondition> ParseLogicalExpression(string expression, Dictionary<string, List<FilterCondition>> placeholderMap)
  {
    var conditions = new List<FilterCondition>();

    // First, handle OR conditions
    var orParts = SplitLogicalOperators(expression, "or");
    if (orParts.Length > 1)
    {
      foreach (var part in orParts)
      {
        var subConditions = ParseLogicalExpression(part.Trim(), placeholderMap);
        foreach (var subCond in subConditions)
        {
          subCond.LogicOp = LogicOp.Or;
          conditions.Add(subCond);
        }
      }
      return conditions;
    }

    // Then handle AND conditions
    var andParts = SplitLogicalOperators(expression, "and");
    if (andParts.Length > 1)
    {
      foreach (var part in andParts)
      {
        var subConditions = ParseLogicalExpression(part.Trim(), placeholderMap);
        foreach (var subCond in subConditions)
        {
          subCond.LogicOp = LogicOp.And;
          conditions.Add(subCond);
        }
      }
      return conditions;
    }

    // Handle placeholders
    if (placeholderMap.TryGetValue(expression.Trim(), out var groupConditions))
    {
      return groupConditions;
    }

    // Parse single condition
    var singleCondition = ParseSingleStringCondition(expression.Trim());
    if (singleCondition != null)
    {
      conditions.Add(singleCondition);
    }

    return conditions;
  }

  private string[] SplitLogicalOperators(string input, string operatorKeyword)
  {
    // Simple split that doesn't break on operators within quotes
    // For now, assume no quotes in conditions
    return input.Split(new[] { $" {operatorKeyword} ", $" {operatorKeyword.ToUpper()} " },
                       StringSplitOptions.RemoveEmptyEntries);
  }

  private FilterCondition? ParseSingleStringCondition(string condition)
  {
    // Simple parsing for "field op value" format
    var parts = condition.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length >= 3)
    {
      var field = parts[0];
      var op = parts[1];
      var valueStr = string.Join(" ", parts.Skip(2));

      // Try to parse value as number or keep as string
      object value = valueStr;
      if (int.TryParse(valueStr, out var intVal))
        value = intVal;
      else if (double.TryParse(valueStr, out var doubleVal))
        value = doubleVal;
      else if (bool.TryParse(valueStr, out var boolVal))
        value = boolVal;

      return new FilterCondition
      {
        Field = field,
        Op = ParseOperator(op),
        Value = value
      };
    }
    return null;
  }

  private FilterOp ParseOperator(string op)
  {
    return op switch
    {
      "=" or "==" or "eq" => FilterOp.Eq,
      "!=" or "<>" or "ne" or "neq" => FilterOp.Neq,
      "<" => FilterOp.Lt,
      "<=" or "lte" => FilterOp.Lte,
      ">" => FilterOp.Gt,
      ">=" or "gte" => FilterOp.Gte,
      "contains" or "like" => FilterOp.Contains,
      _ => FilterOp.Eq
    };
  }

  private IncludeDefinition ConvertIncludeSpecToDefinition(IncludeSpec spec)
  {
    var def = new IncludeDefinition
    {
      Model = spec.Model,
      As = spec.As,
      ForeignKey = spec.ForeignKey,
      TargetKey = spec.TargetKey,
      Required = spec.Required
    };

    if (spec.Where != null)
    {
      def.Where = ConvertWhereSpecToConditions(spec.Where);
    }

    if (spec.Include != null && spec.Include.Count > 0)
    {
      def.Include = spec.Include.Select(s => ConvertIncludeSpecToDefinition(s)).ToList();
    }

    return def;
  }

  private IncludeDefinition ConvertIncludeBuilderToDefinition(IncludeBuilder builder)
  {
    var def = new IncludeDefinition
    {
      Model = builder.Model,
      As = builder.Alias,
      Where = builder.WhereConditions
    };

    if (builder.NestedIncludes.Count > 0)
    {
      def.Include = builder.NestedIncludes.Select(ConvertIncludeBuilderToDefinition).ToList();
    }

    return def;
  }
}
