# Beep.OilandGas

Solution guide for services, data classes, and layering.

## Architecture (3 layers)

- Web: `Beep.OilandGas.Web` (Blazor Server UI)
- API: `Beep.OilandGas.ApiService` (controllers + orchestration)
- Data: Beep framework + PPDM repositories (`IDMEEditor`, `PPDMGenericRepository`)

## Data classes (no DTOs)

- All shared models live in `Beep.OilandGas.Models/Data`.
- Use `ModelEntityBase` for any cross-layer model; it implements `IPPDMEntity` and includes PPDM audit columns.
- Avoid creating separate DTO namespaces. Use Data classes directly across services and controllers.

## Namespaces and locations

- Interfaces: `Beep.OilandGas.Models.Core.Interfaces`
- Shared models/entities: `Beep.OilandGas.Models.Data`
- PPDM39 metadata/repositories: `Beep.OilandGas.PPDM39.*`

## Service conventions

- Interfaces in `Beep.OilandGas.Models.Core.Interfaces` with names `I{Domain}Service`.
- Implementations in the domain project (e.g., `Beep.OilandGas.ProductionAccounting`).
- Register services in `Beep.OilandGas.ApiService/Program.cs` using factory pattern (see CLAUDE.md).

## Data access pattern (required)

Use metadata-driven `PPDMGenericRepository` with `AppFilter` for all CRUD.

```csharp
var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME");
var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(
    _editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "TABLE_NAME");

var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
};

var entities = await repo.GetAsync(filters);
```

## FieldOrchestrator rules

- Field-scoped operations run through FieldOrchestrator with `FIELD_ID` filtering.
- API endpoints for field context live under `/api/field/current/*`.

## Build

```bash
dotnet build Beep.OilandGas.sln
```
