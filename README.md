# DynamicOrmLib (C#)

Status: POC / alpha

Getting started:
- Open solution and add `DynamicOrmLib` project.
- Build with `dotnet build`.

API overview:
- `ManifestLoader` - carga manifiestos de módulo.
- `DynamicContext` - registra modelos y gestiona registros dinámicos en memoria.
- `ModelDefinition` / `FieldDefinition` - clases de metadata.

- `ModuleManager.Install(manifests, provider)` - orchestrates module installation. It validates manifests, resolves dependencies (`dependsOn`), initializes the provider, registers models, and applies `impacts` in an ordered and safe manner using the underlying `IStoreProvider`.
- `IStoreProvider.ApplyImpact(ModuleInfo module, JsonObject impact)` - adapters implement this hook to apply impacts from manifests (persist metadata updates and optionally mutate DB schema or create indexes).
	- Idempotence: `ApplyImpact` must be idempotent (applying the same impact multiple times should be a no-op or a safe update) and avoid creating duplicate fields, relations, enum entries, or indexes.
- SQL injection safety: adapters must validate model and field identifiers taken from manifests and runtime calls. The core `SqlProtection` helper validates identifiers against `^[A-Za-z0-9_.]+$`.
- `DynamicClient` - API de alto nivel (recomendada para la mayoría de escenarios). Ofrece definiciones Fluent (ModelBuilder), creación y consulta de registros y carga de manifests.

Capabilities summary:
- Dynamic models with fields defined at runtime via JSON manifests.
- No-code model extension using manifest `impacts` (fields, relations, enums, indices).
- Module dependency resolution with version constraints (`dependsOn` with `@` comparator), cycle detection and install ordering via `ModuleManager`.
- Persistence adapters: SQLite adapter (POC) implemented; Postgres adapter planned.
- Simple query support, joins (single join POC), and CRUD APIs.
- Fluent developer API with `DynamicClient` and `ModelBuilder`.
- Adapter contract allows pluggable DB backend implementations (`IStoreProvider`), including `ApplyImpact` for schema updates. Adapters should store metadata in a `schema_manager` table and use per-model typed tables (eg `records_<model>`) instead of a global `models`/`records` legacy schema.

Quick example: install two manifests with dependencies and impacts
```csharp
var adapter = new SqliteStoreAdapter("Data Source=./demo.db");
var mgr = new ModuleManager();
var manifests = new[] { ManifestLoader.LoadFromFile("crm-manifest.json"), ManifestLoader.LoadFromFile("contract-manifest.json") };
mgr.Install(manifests, adapter);
```
Manifests are validated at load time; invalid model or field names or malformed impacts will cause `ManifestLoader.Validate` to throw.
- `DynamicClient` - API de alto nivel (recomendada para la mayoría de escenarios). Ofrece definiciones Fluent (ModelBuilder), creación y consulta de registros y carga de manifests.

Packaging & NuGet:
- Este proyecto está preparado para publicarse como paquete NuGet (`PackageId: DynamicOrmLib`). Para publicar:
	- `dotnet pack` en `DynamicOrmLib` y en adaptadores.
	- `dotnet nuget push` al feed NuGet (o GitHub Packages) con la API key.
	- Recomendar versionado semántico. Documenta breaking changes en el changelog.

	Manifest dependencies & impacts:
	- `dependsOn` allows manifests to declare other modules they depend on; a module manager should validate and ensure required modules are installed.
	- `impacts` allow a manifest to extend or modify other modules (eg add fields, add relations, extend enums, add indices).
	- The actual application of impacts is adapter-specific and should be performed safely (transactional, backups if needed).
