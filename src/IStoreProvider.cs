using System.Text.Json.Nodes;

namespace DynamicOrmLib;

public interface IStoreProvider
{
  void Init();
  void RegisterModel(ModelDefinition model);
  DynamicRecord CreateRecord(string modelName, JsonObject data);
  DynamicRecord? GetRecordById(string id);
  IEnumerable<DynamicRecord> GetRecords(string modelName, QueryOptions? options = null);
  DynamicRecord UpdateRecord(string id, JsonObject data);
  void DeleteRecord(string id);
  DynamicRecord UpsertRecord(string modelName, string? id, JsonObject data);
  bool ModelExists(string modelName);
  ModelDefinition? GetModelDefinition(string modelName);
  void ApplyImpact(ModuleInfo module, JsonObject impact);
  // Transaction support for adapters
  void BeginTransaction();
  void Commit();
  void Rollback();

  // Managed schemas and change log
  void UpsertManagedSchema(ModelDefinition model, ModuleInfo module);
  ModelDefinition? GetManagedSchema(string modelName);
  void LogSchemaChange(string modelName, JsonObject change, ModuleInfo module, string operation);

}
