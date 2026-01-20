# Beep.OilandGas - Agent Assistance Guidelines

This guide helps agents make safe, well-scoped edits that align with Beep.OilandGas architecture and patterns.

## Essential Architecture Knowledge (Read First)

1. **Three-Layer Architecture**:
   - **Web Layer**: Blazor Server (`Beep.OilandGas.Web`)
   - **API Layer**: ASP.NET Core (`Beep.OilandGas.ApiService`)
   - **Data Layer**: Beep framework (`IDMEEditor`, `IDataSource`) + `PPDMGenericRepository`

2. **Single PPDM39 Database Model**:
   - All lifecycle data lives in one PPDM39 schema.
   - Use `PPDMGenericRepository` for CRUD; it resolves tables via metadata.

3. **FieldOrchestrator Pattern**:
   - Manages current active field context.
   - Field-scoped API endpoints live under `/api/field/current/*`.
   - Services filter by `FIELD_ID` when called through FieldOrchestrator.

## Data Classes (No DTOs)

- All shared models live in `Beep.OilandGas.Models/Data`.
- Use `ModelEntityBase` for any shared model; it implements `IPPDMEntity` and includes PPDM audit columns.
- Do not create or reintroduce DTO namespaces.

## Mandatory Pre-Edit Checklist

Before making any code change:

- [ ] Read `Beep.OilandGas.ApiService/Program.cs` lines 1-120 to confirm DI order (AddBeepServices must come before services that consume IDMEEditor).
- [ ] Confirm all new interfaces live in `Beep.OilandGas.Models.Core.Interfaces`.
- [ ] Verify service registration uses factory pattern: `AddScoped<IFoo>(sp => new Foo(sp.GetRequiredService<...>(), ...))`.
- [ ] If touching data access, use the PPDMGenericRepository pattern shown in `.cursor/commands/beep-dataaccess-generic-repository.md`.

## Data Access Pattern (Copy-Paste Template)

```csharp
// 1. Resolve metadata
var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME");
var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");

// 2. Create repository
var repo = new PPDMGenericRepository(
    _editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "TABLE_NAME");

// 3. Query with AppFilter (PREFERRED)
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
};

// 4. Execute
var entities = await repo.GetAsync(filters);  // Get multiple
var entity = await repo.GetByIdAsync(id);     // Get single
await repo.InsertAsync(entity, userId);       // Create
await repo.UpdateAsync(entity, userId);       // Update
await repo.SoftDeleteAsync(id, userId);       // Delete
```

## Service Registration Pattern (Copy-Paste Template)

```csharp
// Register AFTER metadata/defaults, BEFORE services that depend on it
builder.Services.AddScoped<IMyService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<MyService>();

    return new MyService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});
```

## Naming Conventions

- **Projects**: `Beep.OilandGas.{DomainName}` (e.g., `Beep.OilandGas.ProductionAccounting`)
- **Namespaces**: `Beep.OilandGas.Models.Core.Interfaces` for interfaces; `Beep.OilandGas.Models.Data` for shared models; `.DataManagement.Services` for implementations
- **Files**: `I{ServiceName}Service.cs` for interfaces; `{ServiceName}Service.cs` for implementations
- **DB IDs**: All string-based; use `_defaults.FormatIdForTable("TABLE_NAME", id)` to format

## Common Mistakes to Avoid

1. Registering service before `IDMEEditor` is available -> startup null refs.
2. Reintroducing DTO namespaces -> all shared models are `Beep.OilandGas.Models.Data`.
3. Using `ExecuteSql` for SELECT queries -> only use for DDL; use `GetEntityAsync` or `RunQuery` instead.
4. Hardcoding SQL parameter delimiters -> use `dataSource.ParameterDelimiter` or `AppFilter`.
5. Creating separate databases per feature -> always use the single PPDM39 schema.

## Quick Verification Steps

```bash
dotnet build Beep.OilandGas.sln
dotnet run --project Beep.OilandGas.ApiService
```

## Reference Documentation (Treat as Source-of-Truth)

- `.cursor/commands/architecture-patterns.md` - Three-layer patterns, service/API organization
- `.cursor/commands/beep-dataaccess-generic-repository.md` - PPDMGenericRepository CRUD examples
- `.cursor/commands/beep-dataaccess-overview.md` - Data access framework overview
- `.cursor/commands/naming-conventions.md` - Project, namespace, and file naming rules
- `.cursor/commands/data-access-patterns.md` - Data access best practices
- `.cursor/commands/best-practices.md` - FASB compliance, revenue recognition, accounting patterns

## Web Layer Patterns (Critical)

**ApiClient**:
```csharp
@inject ApiClient ApiClient
var result = await ApiClient.GetAsync<List<Field>>("/api/field/fields");
```

**DataManagementService**:
- Centralized data + connection management
- CRUD operations for any PPDM entity
- Import/export with progress
- Validation, quality checks, versioning

## API Controllers Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MyController : ControllerBase
{
    private readonly IMyService _service;
    private readonly ILogger<MyController> _logger;

    [HttpPost("operation")]
    public async Task<IActionResult> DoOperationAsync([FromBody] OperationRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _service.ExecuteAsync(request, userId);
        return Ok(result);
    }
}
```
