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
  void DeleteRecords(string modelName, QueryOptions options);
  DynamicRecord UpsertRecord(string modelName, string? id, JsonObject data);
  bool ModelExists(string modelName);
  ModelDefinition? GetModelDefinition(string modelName);
  // Transaction support for adapters
  void BeginTransaction();
  void Commit();
  void Rollback();

}
