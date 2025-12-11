using System.Text.Json;
using System.Text.Json.Nodes;

namespace DynamicOrmLib;

public class DynamicRecord
{
  public string Id { get; set; } = null!;
  public JsonObject Data { get; set; } = new JsonObject();
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public class DynamicContext
{
  private readonly Dictionary<string, ModelDefinition> _models = new();
  private readonly Dictionary<string, List<DynamicRecord>> _records = new();

  private readonly IStoreProvider? _provider;

  public DynamicContext(IStoreProvider? provider = null)
  {
    _provider = provider;
    _provider?.Init();
  }

  public void RegisterModel(ModelDefinition model)
  {
    if (!_models.ContainsKey(model.Name))
    {
      _models[model.Name] = model;
      _records[model.Name] = new List<DynamicRecord>();
      if (_provider != null)
      {
        _provider.RegisterModel(model);
      }
    }
    else
    {
      // Upsert existing model definition in-memory and in provider
      _models[model.Name] = model;
      if (_provider != null)
      {
        _provider.RegisterModel(model);
      }
    }
  }

  public void RegisterManifest(ModuleManifest manifest)
  {
    foreach (var m in manifest.Models)
    {
      RegisterModel(m);
    }
  }

  public IEnumerable<ModelDefinition> GetModels() => _models.Values;

  public List<FieldDefinition>? GetFields(string modelName)
  {
    if (!_models.ContainsKey(modelName)) return null;
    return _models[modelName].Fields;
  }

  public DynamicRecord CreateRecord(string modelName, JsonObject data)
  {
    if (!_models.ContainsKey(modelName)) throw new KeyNotFoundException("Model not found");
    var model = _models[modelName];
    var required = model.Fields.Where(f => f.Required).Select(f => f.Name).ToList();
    // If a required field has a default value, supply it instead of treating as missing
    var missing = new System.Collections.Generic.List<string>();
    foreach (var r in required)
    {
      if (data.ContainsKey(r)) continue;
      var fd = model.Fields.FirstOrDefault(f => f.Name == r);
      if (fd != null && fd.DefaultValue != null)
      {
        // Add default value into data
        try
        {
          if (fd.Type == FieldType.Number && fd.DefaultValue is JsonElement jeNum)
          {
            data[r] = JsonNode.Parse(jeNum.GetRawText());
          }
          else if (fd.DefaultValue is JsonElement je)
          {
            data[r] = JsonNode.Parse(je.GetRawText()) as JsonNode ?? JsonValue.Create(fd.DefaultValue.ToString() ?? string.Empty);
          }
          else
          {
            // Fallback convert to primitive as string
            if (fd.Type == FieldType.Number) data[r] = JsonValue.Create(Convert.ToDouble(fd.DefaultValue));
            else if (fd.Type == FieldType.Boolean) data[r] = JsonValue.Create(Convert.ToBoolean(fd.DefaultValue));
            else data[r] = JsonValue.Create(fd.DefaultValue?.ToString() ?? string.Empty);
          }
        }
        catch { data[r] = JsonValue.Create(fd.DefaultValue?.ToString() ?? string.Empty); }
        continue;
      }
      missing.Add(r);
    }
    if (missing.Any()) throw new ArgumentException($"Missing required fields: {string.Join(',', missing)}");

    if (_provider != null)
    {
      return _provider.CreateRecord(modelName, data);
    }
    var id = Guid.NewGuid().ToString("N");
    var rec = new DynamicRecord { Id = id, Data = data, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    _records[modelName].Add(rec);
    return rec;
  }

  public IEnumerable<DynamicRecord> GetRecords(string modelName, QueryOptions? options = null)
  {
    if (!_records.ContainsKey(modelName)) throw new KeyNotFoundException("Model not found");
    if (_provider != null)
    {
      return _provider.GetRecords(modelName, options);
    }
    var list = _records[modelName].AsEnumerable();
    if (options != null && options.Where != null && options.Where.Any())
    {
      foreach (var c in options.Where)
      {
        switch (c.Op)
        {
          case FilterOp.Eq:
            list = list.Where(r => r.Data.ContainsKey(c.Field) && r.Data[c.Field]?.ToString() == (c.Value?.ToString() ?? string.Empty));
            break;
          // For POC, support only Eq
          default:
            break;
        }
      }
    }
    if (!string.IsNullOrWhiteSpace(options?.OrderBy))
    {
      if (options.OrderDesc) list = list.OrderByDescending(r => r.Data[options.OrderBy]?.ToString());
      else list = list.OrderBy(r => r.Data[options.OrderBy]?.ToString());
    }
    if (options?.Limit != null)
      list = list.Take(options.Limit.Value);
    return list;
  }

  public IEnumerable<DynamicRecord> GetManyWithJoins(string modelName, QueryOptions options)
  {
    // If provider supports joins, let provider handle it
    if (_provider != null) return _provider.GetRecords(modelName, options);
    // In-memory join support (basic inner join for a single join)
    if (options.Joins == null || options.Joins.Count == 0) return GetRecords(modelName, options);
    var join = options.Joins.First();
    var left = GetRecords(join.SourceModel).ToList();
    var right = GetRecords(join.TargetModel).ToList();
    var result = new List<DynamicRecord>();
    foreach (var l in left)
    {
      var lval = l.Data.ContainsKey(join.SourceField) ? l.Data[join.SourceField]?.ToString() : null;
      foreach (var r in right)
      {
        var rval = r.Data.ContainsKey(join.TargetField) ? r.Data[join.TargetField]?.ToString() : null;
        if (lval != null && rval != null && lval == rval)
        {
          // Merge data: left keys first, then prefixed right keys
          var merged = new JsonObject();
          foreach (var kv in l.Data) merged[kv.Key] = kv.Value != null ? JsonNode.Parse(kv.Value!.ToJsonString()) : null;
          foreach (var kv in r.Data) merged[$"{join.TargetModel}.{kv.Key}"] = kv.Value != null ? JsonNode.Parse(kv.Value!.ToJsonString()) : null;
          result.Add(new DynamicRecord { Id = l.Id, Data = merged, CreatedAt = l.CreatedAt, UpdatedAt = l.UpdatedAt });
        }
      }
    }
    return result;
  }

  public DynamicRecord? GetById(string id)
  {
    if (_provider != null) return _provider.GetRecordById(id);
    foreach (var rlist in _records.Values)
    {
      var found = rlist.FirstOrDefault(r => r.Id == id);
      if (found != null) return found;
    }
    return null;
  }

  public DynamicRecord UpdateRecord(string id, JsonObject data)
  {
    if (_provider != null) return _provider.UpdateRecord(id, data);
    foreach (var rlist in _records.Values)
    {
      var idx = rlist.FindIndex(r => r.Id == id);
      if (idx >= 0)
      {
        rlist[idx].Data = data;
        rlist[idx].UpdatedAt = DateTime.UtcNow;
        return rlist[idx];
      }
    }
    throw new KeyNotFoundException("Record not found");
  }

  public void DeleteRecord(string id)
  {
    if (_provider != null) { _provider.DeleteRecord(id); return; }
    foreach (var rlist in _records.Values)
    {
      var idx = rlist.FindIndex(r => r.Id == id);
      if (idx >= 0) { rlist.RemoveAt(idx); return; }
    }
    throw new KeyNotFoundException("Record not found");
  }

  public DynamicRecord UpsertRecord(string modelName, string? id, JsonObject data)
  {
    if (_provider != null) return _provider.UpsertRecord(modelName, id, data);
    if (id != null)
    {
      try { return UpdateRecord(id, data); } catch { }
    }
    return CreateRecord(modelName, data);
  }
}
