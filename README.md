# DynamicOrmLib (C#)

Status: POC / Beta

---

Getting Started:
- Open solution and add the `DynamicOrmLib` project.
- Build with `dotnet build`.

---

API Overview:
- `ManifestLoader` - Loads module manifests.
- `DynamicContext` - Registers models and manages dynamic records in memory.
- `ModelDefinition` / `FieldDefinition` - Metadata classes.

- `ModuleManager.Install(manifests, provider)` - Orchestrates module installation. It validates manifests, resolves dependencies (`dependsOn`), initializes the provider, registers models, and applies `impacts` in an ordered and safe manner using the underlying `IStoreProvider`.

- `IStoreProvider.ApplyImpact(ModuleInfo module, JsonObject impact)` - Adapters implement this hook to apply impacts from manifests (persist metadata updates and optionally mutate DB schema or create indexes).
    - **Idempotence**: `ApplyImpact` must be idempotent (applying the same impact multiple times should be a no-op or a safe update) and avoid creating duplicate fields, relations, enum entries, or indexes.
    
- **SQL Injection Safety**: Adapters must validate model and field identifiers taken from manifests and runtime calls. The core `SqlProtection` helper validates identifiers against `^[A-Za-z0-9_.]+$`.

- `DynamicClient` - High-level API (recommended for most scenarios). Offers Fluent definitions (`ModelBuilder`), record creation and querying, and manifest loading.

---

Capabilities Summary:
- Dynamic models with fields defined at runtime via JSON manifests.
- No-code model extension using manifest **`impacts`** (fields, relations, enums, indices).
- Module dependency resolution with version constraints (`dependsOn` with `@` comparator), cycle detection and install ordering via `ModuleManager`.
- Persistence adapters: SQLite adapter (POC) implemented; Postgres adapter planned.
- Simple query support, joins (single join POC), and CRUD APIs.
- Fluent developer API with `DynamicClient` and `ModelBuilder`.
- Adapter contract allows pluggable DB backend implementations (`IStoreProvider`), including `ApplyImpact` for schema updates. Adapters should store metadata in a `schema_manager` table and use per-model typed tables (e.g., `records_<model>`) instead of a global `models`/`records` legacy schema.

---

Quick Example: Install two manifests with dependencies and impacts
```csharp
var adapter = new SqliteStoreAdapter("Data Source=./demo.db");
var mgr = new ModuleManager();
var manifests = new[] { ManifestLoader.LoadFromFile("crm-manifest.json"), ManifestLoader.LoadFromFile("contract-manifest.json") };
mgr.Install(manifests, adapter);
```

Manifests are validated at load time; invalid model or field names or malformed impacts will cause ManifestLoader.Validate to throw.

## Packaging & NuGet:

- This project is prepared for publication as a NuGet package (PackageId: DynamicOrmLib). To publish:
- dotnet pack on DynamicOrmLib and adapters.
- dotnet nuget push to the NuGet feed (or GitHub Packages) with the API key.
- Recommend semantic versioning. Document breaking changes in the changelog.

## Manifest Dependencies & Impacts:

- dependsOn allows manifests to declare other modules they depend on; a module manager should validate and ensure required modules are installed.
- impacts allow a manifest to extend or modify other modules (e.g., add fields, add relations, extend enums, add indices).
- The actual application of impacts is adapter-specific and should be performed safely (transactional, backups if needed). See ADAPTERS.md for recommendations.

## Planned:

- Postgres persistence (JSONB) adapter
- Advanced validation and JSON schemas
- Workflow engine and automations
- UI generator via metadata
