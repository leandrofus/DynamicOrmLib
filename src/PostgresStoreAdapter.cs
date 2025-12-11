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

  public void DeleteRecords(string modelName, QueryOptions options)
  {
    throw new NotImplementedException();
  }

  public DynamicRecord UpsertRecord(string modelName, string? id, System.Text.Json.Nodes.JsonObject data)
  {
    throw new NotImplementedException();
  }

  // ApplyImpact implemented above
}
