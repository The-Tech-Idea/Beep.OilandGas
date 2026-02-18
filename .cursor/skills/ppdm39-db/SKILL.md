---
name: ppdm39-db
description: PPDM39 database services for Beep.OilandGas - creator, seeder, datamanager, LOV, and data operations. Use when working with PPDM39 data access, database creation, reference data seeding, or CRUD on PPDM entities.
---

# PPDM39 Database Services

Developer guide for the consolidated PPDM39 database layer: creator, seeder, datamanager, LOV management, and related services.

## Quick Start – One Line Setup

Register all PPDM39 data management services with a single call:

```csharp
// In Program.cs (API or Web) - after AddBeepServices, before app.Build()
builder.Services.AddPPDM39DataManagement(
    connectionName: "PPDM39",  // or from config: builder.Configuration["ConnectionStrings:PPDM39"]
    includeSeeder: true);      // true = include reference data seeder and LOV importers
```

This registers: metadata, defaults, validation, quality, audit, versioning, LOV management, seeder, CRUD (DataService), and workflow services.

## Architecture

### PPDM39DataService Structure (Partial Classes + Helpers)

| File | Purpose |
|------|---------|
| `PPDM39DataService.cs` | Core: constructor, CRUD (Get/Insert/Update/Delete) |
| `PPDM39DataService.Helpers.cs` | GetRepository, GetEntityTypeByTableNameInternal (typed entities only, no Dictionary) |
| `PPDM39DataService.Migration.cs` | CreateSchemaFromEntitiesAsync, GetMigrationSummary (Beep.Sample pattern) |

### Core Services (in `Beep.OilandGas.PPDM39.DataManagement`)

| Service | Purpose | Inject As |
|--------|---------|-----------|
| **PPDM39DataService** | CRUD + Migration schema creation (partial classes + helpers) | `IPPDM39DataService` |
| **PPDMReferenceDataSeeder** | Seed LIST_OF_VALUE, R_*, RA_* tables | `PPDMReferenceDataSeeder` |
| **LOVManagementService** | List-of-values CRUD and bulk import | `LOVManagementService` |
| **PPDMMappingService** | DTO ↔ PPDM model conversion | `PPDMMappingService` |
| **PPDM39WorkflowService** | Workflow and state operations | `PPDM39WorkflowService` |
| **PPDMDataValidationService** | Entity validation | `IPPDMDataValidationService` |
| **PPDMDataQualityService** | Quality metrics | `IPPDMDataQualityService` |
| **PPDMDataAccessAuditService** | Access tracking | `IPPDMDataAccessAuditService` |
| **PPDMDataVersioningService** | Version snapshots and rollback | `IPPDMDataVersioningService` |

### Seed Data Support (when `includeSeeder: true`)

- **PPDMReferenceDataSeeder** – reference/lookup tables
- **CSVLOVImporter** – CSV → LOV import
- **PPDMStandardValueImporter** – PPDM standard values
- **IHSStandardValueImporter** – IHS reference data
- **StandardValueMapper** – value mapping for import

## Common Use Cases

### 1. CRUD on Any PPDM Table

```csharp
[ApiController]
public class MyController : ControllerBase
{
    private readonly IPPDM39DataService _data;

    public MyController(IPPDM39DataService data) => _data = data;

    [HttpGet("wells")]
    public async Task<IActionResult> GetWells()
    {
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        var result = await _data.GetEntitiesAsync("WELL", filters);
        return Ok(result.Entities);
    }

    [HttpPost("wells")]
    public async Task<IActionResult> CreateWell([FromBody] Dictionary<string, object> well, [FromQuery] string userId)
    {
        var result = await _data.InsertEntityAsync("WELL", well, userId);
        return result.Success ? Ok(result) : BadRequest(result.ErrorMessage);
    }
}
```

### 2. Seed Reference Data After Schema Creation

```csharp
var seeder = sp.GetRequiredService<PPDMReferenceDataSeeder>();
var seedResult = await seeder.SeedPPDMReferenceTablesAsync(
    connectionName: "PPDM39",
    tableNames: null,      // null = all reference tables
    skipExisting: true,
    userId: "SYSTEM");
```

### 3. LOV Management

```csharp
var lovService = sp.GetRequiredService<LOVManagementService>();
var lovs = new List<LIST_OF_VALUE> { /* ... */ };
var result = await lovService.BulkAddLOVsAsync(lovs, userId, skipDuplicates: true, connectionName);
```

### 4. Validation Before Insert

```csharp
var validationService = sp.GetRequiredService<IPPDMDataValidationService>();
var result = await validationService.ValidateAsync(wellEntity, "WELL");
if (!result.IsValid)
    foreach (var e in result.Errors)
        Console.WriteLine($"{e.FieldName}: {e.ErrorMessage}");
```

## Data Access Pattern (PPDMGenericRepository)

For custom repositories, use the same pattern as PPDM39DataService:

```csharp
var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME");
var entityType = Type.GetType($"Beep.OilandGas.PPDM.Models.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, connectionName, "TABLE_NAME", logger);

var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId }
};
var entities = await repo.GetAsync(filters);
await repo.InsertAsync(entity, userId);
await repo.UpdateAsync(entity, userId);
```

## Naming Conventions

- **Interfaces**: `I{ServiceName}` in `Beep.OilandGas.Models.Core.Interfaces`
- **Implementations**: `{ServiceName}Service` in `Beep.OilandGas.PPDM39.DataManagement.Services`
- **Connection**: Default `PPDM39`; overridable via config or `AddPPDM39DataManagement(connectionName)`

## Create Database via MigrationManager (Beep.Sample Pattern)

**One service: `IPPDM39DataService`** – handles CRUD and Migration-based schema creation.

Uses the same methodology as `Beep.Sample.Winform` CreateEntitiesStepControl:

1. **MigrationManager** discovers Entity types (or explicit types)
2. **classCreator.ConvertToEntityStructure()** converts POCO → EntityStructure
3. **IDataSource.CreateEntityAs()** creates tables (datasource-agnostic)

```csharp
// Inject the single PPDM39 data service
var dataService = sp.GetRequiredService<IPPDM39DataService>();

// CRUD (GetEntitiesAsync, InsertEntityAsync, etc.)
var entities = await dataService.GetEntitiesAsync("WELL", filters);

// Schema creation via Migration (Beep.Sample pattern)
var result = await dataService.CreateSchemaFromEntitiesAsync(
    connectionName: "PPDM39",
    entityTypes: null,           // null = discover from PPDM39.Models namespace
    addMissingColumns: false);

// Or create specific tables only
var result = await dataService.CreateSchemaFromEntitiesAsync(
    connectionName: "PPDM39",
    entityTypes: new[] { typeof(WELL), typeof(LIST_OF_VALUE) });

// Get migration summary (what would be created/updated)
var summary = dataService.GetMigrationSummary("PPDM39");
```

Reference: `Beep.Sample.Winform/Forms/WizardSteps/CreateEntitiesStepControl.cs`.

## Related

- `.cursor/commands/beep-dataaccess-overview.md` – data access overview
- `.cursor/commands/beep-dataaccess-generic-repository.md` – PPDMGenericRepository
- `Beep.OilandGas.PPDM39.DataManagement/DATA_MANAGEMENT_SERVICES.md` – full service list
