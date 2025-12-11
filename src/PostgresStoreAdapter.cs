namespace DynamicOrmLib;

public class PostgresStoreAdapter : IStoreProvider
{
  // Simple in-memory metadata store to support ApplyImpact for POC (real adapter should use DB)
  private readonly Dictionary<string, ModelDefinition> _models = new();
  public void Init()
  {
    // TODO: implement connection and migrations
    // For POC keep in-memory default; real Postgres adapter should initialize connections
  }

  public void BeginTransaction() { /* POC: no-op or add real transaction support */ }
  public void Commit() { /* no-op */ }
  public void Rollback() { /* no-op */ }

  public void RegisterModel(ModelDefinition model)
  {
    if (model == null) throw new ArgumentNullException(nameof(model));
    model.Module = model.Module ?? model.Name; // keep module name if not set
    SqlProtection.ValidateModelName(model.Name);
    _models[model.Name] = model;
  }

  public DynamicRecord CreateRecord(string modelName, System.Text.Json.Nodes.JsonObject data)
  {
    throw new NotImplementedException();
  }

  public DynamicRecord? GetRecordById(string id)
  {
    throw new NotImplementedException();
  }

  public IEnumerable<DynamicRecord> GetRecords(string modelName, QueryOptions? options = null)
  {
    throw new NotImplementedException();
  }

  public bool ModelExists(string modelName)
  {
    return _models.ContainsKey(modelName);
  }

  public ModelDefinition? GetModelDefinition(string modelName)
  {
    return _models.GetValueOrDefault(modelName);
  }

  public DynamicRecord UpdateRecord(string id, System.Text.Json.Nodes.JsonObject data)
  {
    throw new NotImplementedException();
  }

  public void DeleteRecord(string id)
  {
    throw new NotImplementedException();
  }

  public DynamicRecord UpsertRecord(string modelName, string? id, System.Text.Json.Nodes.JsonObject data)
  {
    throw new NotImplementedException();
  }

  public void ApplyImpact(ModuleInfo module, System.Text.Json.Nodes.JsonObject impact)
  {
    if (impact == null) throw new ArgumentNullException(nameof(impact));
    if (!impact.TryGetPropertyValue("action", out var actNode) || actNode == null) throw new ArgumentException("Impact missing action");
    var action = actNode.GetValue<string?>();
    if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Impact action empty");

    switch (action)
    {
      case "addField":
      case "addRelation":
        if (!impact.TryGetPropertyValue("targetModel", out var tm) || tm == null) throw new ArgumentException("impact missing targetModel");
        var targetModel = SqlProtection.ValidateModelName(tm.GetValue<string>());
        var fieldNode = impact.TryGetPropertyValue("field", out var fn) ? fn : null;
        if (fieldNode == null) throw new ArgumentException("impact missing field definition");
        var field = System.Text.Json.JsonSerializer.Deserialize<FieldDefinition>(fieldNode.ToJsonString(), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (field == null) throw new ArgumentException("Invalid field definition");
        SqlProtection.ValidateFieldName(field.Name);
        if (field == null) throw new ArgumentException("Invalid field definition");
        if (!_models.TryGetValue(targetModel, out var md)) throw new InvalidOperationException($"Target model not found: {targetModel}");
        if (md.Fields == null) md.Fields = new List<FieldDefinition>();
        var existing = md.Fields.FirstOrDefault(f => f.Name == field.Name);
        if (existing != null)
        {
          var old = System.Text.Json.JsonSerializer.Serialize(existing, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
          var neu = System.Text.Json.JsonSerializer.Serialize(field, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
          if (old != neu)
          {
            md.Fields.Remove(existing);
            md.Fields.Add(field);
          }
        }
        else
        {
          md.Fields.Add(field);
        }
        RegisterModel(md);
        break;
      case "extendEnum":
        if (!impact.TryGetPropertyValue("targetModel", out var tme) || tme == null) throw new ArgumentException("impact missing targetModel");
        var targetModelEnum = SqlProtection.ValidateModelName(tme.GetValue<string>());
        if (!impact.TryGetPropertyValue("field", out var fne) || fne == null) throw new ArgumentException("impact missing field");
        var fieldNameEnum = SqlProtection.ValidateFieldName(fne.GetValue<string>());
        if (!impact.TryGetPropertyValue("values", out var vals) || vals == null) throw new ArgumentException("impact missing values");
        var arr = vals as System.Text.Json.Nodes.JsonArray;
        if (!_models.TryGetValue(targetModelEnum, out var mdE)) throw new InvalidOperationException($"Target model not found: {targetModelEnum}");
        if (mdE.Metadata == null) mdE.Metadata = new System.Text.Json.Nodes.JsonObject();
        if (!mdE.Metadata.TryGetPropertyValue("enums", out var enumsObj) || enumsObj == null) mdE.Metadata["enums"] = new System.Text.Json.Nodes.JsonObject();
        var enumsJson = mdE.Metadata["enums"] as System.Text.Json.Nodes.JsonObject ?? new System.Text.Json.Nodes.JsonObject();
        var existingArr = enumsJson.ContainsKey(fieldNameEnum) && enumsJson[fieldNameEnum] is System.Text.Json.Nodes.JsonArray exArr ? exArr : new System.Text.Json.Nodes.JsonArray();
        var set = new HashSet<string>(existingArr.Select(x => x!.GetValue<string>()));
        foreach (var v in arr!) set.Add(v!.GetValue<string>());
        var merged = new System.Text.Json.Nodes.JsonArray();
        foreach (var s in set) merged.Add(s);
        enumsJson[fieldNameEnum] = merged;
        mdE.Metadata["enums"] = enumsJson;
        RegisterModel(mdE);
        break;
      case "addIndex":
        // We can't implement DB-level DDL in this POC Postgres adapter; recommend using Postgres adapter implementing CREATE INDEX IF NOT EXISTS.
        // For now, we only ensure metadata exists and do not attempt DDL.
        if (!impact.TryGetPropertyValue("targetModel", out var tmi) || tmi == null) throw new ArgumentException("impact missing targetModel");
        var targetModelIndex = SqlProtection.ValidateModelName(tmi.GetValue<string>());
        if (!impact.TryGetPropertyValue("field", out var fii) || fii == null) throw new ArgumentException("impact missing field");
        var fieldNameIndex = SqlProtection.ValidateFieldName(fii.GetValue<string>());
        if (!_models.TryGetValue(targetModelIndex, out var mdI)) throw new InvalidOperationException($"Target model not found: {targetModelIndex}");
        if (mdI.Metadata == null) mdI.Metadata = new System.Text.Json.Nodes.JsonObject();
        // note: Postgres adapter should create index with CREATE INDEX IF NOT EXISTS on expression (data->>'field')::type
        // Here we record indexes in metadata for future adapters to handle.
        if (!mdI.Metadata.TryGetPropertyValue("indexes", out var idxObj) || idxObj == null) mdI.Metadata["indexes"] = new System.Text.Json.Nodes.JsonArray();
        var idxArr = mdI.Metadata["indexes"] as System.Text.Json.Nodes.JsonArray ?? new System.Text.Json.Nodes.JsonArray();
        if (!idxArr.Any(x => x!.GetValue<string>() == fieldNameIndex)) idxArr.Add(fieldNameIndex);
        mdI.Metadata["indexes"] = idxArr;
        RegisterModel(mdI);
        break;
      default:
        throw new NotImplementedException($"Impact action not supported: {action}");
    }
  }

  public void UpsertManagedSchema(ModelDefinition model, ModuleInfo module)
  {
    RegisterModel(model);
  }

  public ModelDefinition? GetManagedSchema(string modelName) => GetModelDefinition(modelName);

  public void LogSchemaChange(string modelName, System.Text.Json.Nodes.JsonObject change, ModuleInfo module, string operation)
  {
    // POC: no-op; production adapter should persist schema change logs
  }

  // ApplyImpact implemented above
}
