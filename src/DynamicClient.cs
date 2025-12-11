using System.Text.Json.Nodes;

namespace DynamicOrmLib;

public class ModelBuilder
{
  private readonly DynamicClient _client;
  private readonly ModelDefinition _model = new ModelDefinition();

  internal ModelBuilder(DynamicClient client, string name)
  {
    _client = client;
    _model.Name = name;
  }

  public ModelBuilder Module(string module)
  {
    _model.Module = module;
    return this;
  }

  public ModelBuilder Field(string name, FieldType type, bool required = false, string? label = null, int? length = null, object? defaultValue = null, bool primaryKey = false, string? relationModel = null, string? onDelete = null, string? onUpdate = null)
  {
    var field = new FieldDefinition { Name = name, Type = type, Required = required, Label = label, Length = length, DefaultValue = defaultValue, PrimaryKey = primaryKey };
    if (!string.IsNullOrWhiteSpace(relationModel)) field.Relation = new RelationDefinition { Model = relationModel, OnDelete = onDelete, OnUpdate = onUpdate };
    _model.Fields.Add(field);
    return this;
  }

  public DynamicClient Build()
  {
    _client.RegisterModel(_model);
    return _client;
  }
}

public class DynamicClient
{
  private readonly DynamicContext _context;

  public DynamicClient(IStoreProvider? provider = null)
  {
    _context = new DynamicContext(provider);
  }

  public void RegisterManifest(ModuleManifest manifest) => _context.RegisterManifest(manifest);

  public ModuleManifest RegisterManifestFromFile(string path)
  {
    var manifest = ManifestLoader.LoadFromFile(path);
    _context.RegisterManifest(manifest);
    return manifest;
  }

  public ModelBuilder DefineModel(string name) => new ModelBuilder(this, name);

  internal void RegisterModel(ModelDefinition model) => _context.RegisterModel(model);

  public DynamicRecord Create(string modelName, JsonObject data) => _context.CreateRecord(modelName, data);

  public IEnumerable<DynamicRecord> Query(string modelName) => _context.GetRecords(modelName);

  public DynamicRecord? GetOne(string modelName, string id) => _context.GetById(id);

  public IEnumerable<DynamicRecord> GetMany(string modelName, QueryOptions? options = null) => _context.GetRecords(modelName, options);

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

  public DynamicRecord Update(string id, JsonObject data) => _context.UpdateRecord(id, data);

  public void Delete(string id) => _context.DeleteRecord(id);

  public DynamicRecord Upsert(string modelName, string? id, JsonObject data) => _context.UpsertRecord(modelName, id, data);
}
