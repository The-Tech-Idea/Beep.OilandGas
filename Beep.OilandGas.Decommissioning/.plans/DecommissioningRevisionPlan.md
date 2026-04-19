# Beep.OilandGas.Decommissioning — Revision Plan
## Align to Architecture Standards & PPDM 3.9 Data Model

---

## Current State — Problems Identified

| Problem | Location | Severity |
|---|---|---|
| `UnitOfWorkFactory` used directly for data access | `WellPluggingService.cs` | Critical — anti-pattern |
| No `ICommonColumnHandler`, `IPPDMMetadataRepository`, `IPPDM39DefaultsRepository` injection | Constructor | Critical |
| Missing `Beep.OilandGas.PPDM39.DataManagement` project reference | `.csproj` | Critical |
| Interface `IWellPluggingService` defined in same file as DTOs | `IWellPluggingService.cs` | Major |
| Local interface doesn't match canonical `IFieldDecommissioningService` | Services/ | Major |
| `CreateWellPluggingRequest` DTO defined in service project | `IWellPluggingService.cs` | Major |
| FIELD_ID filter not applied using `_defaults.FormatIdForTable` | `WellPluggingService.cs` | Major |
| No `ILogger` injection pattern | Constructor | Minor |
| `partial class WellPluggingService` Advanced file not wired to canonical service | Advanced file | Minor |

---

## PPDM 3.9 Table Mappings — Decommissioning Domain

| Use Case | PPDM Table | Key Columns | Notes |
|---|---|---|---|
| Well abandonment records | `WELL_ABANDONMENT` | `WELL_ID`, `FIELD_ID`, `ABANDONMENT_TYPE`, `ABANDONMENT_DATE` | Primary table for abandonment records |
| Well plugback/plugging | `WELL_PLUGBACK` | `UWI`, `PLUGBACK_OBS_NO`, `PLUG_DATE`, `PLUG_TYPE` | Child of WELL; records each cement/bridge plug |
| Facility decommissioning | `FACILITY_DECOM` | `FACILITY_ID`, `FIELD_ID`, `DECOM_TYPE`, `DECOM_DATE` | Tracks facility-level decommissioning |
| PPDM facility (parent) | `FACILITY` | `FACILITY_ID`, `PRIMARY_FIELD_ID`, `ABANDONED_DATE` | Parent facility with `ABANDONED_DATE` |
| Well (parent) | `WELL` | `UWI`, `FIELD_ID`, `ABANDONMENT_DATE`, `ACTIVE_IND` | `FIELD_ID` joins well to field |
| Environmental restoration | `PROJECT` + `PROJECT_STEP` | `PROJECT_ID`, `PROJECT_TYPE='ENV_RESTORATION'` | Restoration tracked as project workflow |
| Decommissioning cost tracking | `PROJECT` | `PROJECT_ID`, `PROJECT_TYPE='DECOM_COST'` | Project cost items |

### Entity Resolution Pattern (Required)
```csharp
// NEVER hard-code entity types; always resolve from metadata
var metadata = await _metadata.GetTableMetadataAsync("WELL_ABANDONMENT");
var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "WELL_ABANDONMENT");
```

### FIELD_ID Formatting (Required)
```csharp
// Always format IDs before use in AppFilter or entity assignment
var formattedFieldId = _defaults.FormatIdForTable("WELL_ABANDONMENT", fieldId);
```

### Reflection for Dynamic Properties (Required where FIELD_ID not on generated entity directly)
```csharp
var fieldIdProp = entityType.GetProperty("FIELD_ID",
    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
if (fieldIdProp?.CanWrite == true)
    fieldIdProp.SetValue(entity, formattedFieldId);
```

---

## Target Architecture

### Service: `FieldDecommissioningService`
- **File**: `Services/FieldDecommissioningService.cs` (partial — CRUD)
- **File**: `Services/FieldDecommissioningService.Analysis.cs` (partial — engineering analysis)
- **Interface**: `IFieldDecommissioningService` (from `Beep.OilandGas.Models.Core.Interfaces`)
- **Namespace**: `Beep.OilandGas.Decommissioning.Services`

### Constructor Signature (Canonical)
```csharp
public FieldDecommissioningService(
    IDMEEditor editor,
    ICommonColumnHandler commonColumnHandler,
    IPPDM39DefaultsRepository defaults,
    IPPDMMetadataRepository metadata,
    PPDMMappingService mappingService,
    string connectionName = "PPDM39",
    ILogger<FieldDecommissioningService>? logger = null)
```

### DI Registration (in `Program.cs` after metadata/defaults)
```csharp
builder.Services.AddScoped<IFieldDecommissioningService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var mapping = sp.GetRequiredService<PPDMMappingService>();
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<FieldDecommissioningService>();
    return new FieldDecommissioningService(editor, commonColumnHandler, defaults, metadata, mapping, connectionName, logger);
});
```

---

## Files Changed

| File | Action | Reason |
|---|---|---|
| `Beep.OilandGas.Decommissioning.csproj` | Add `ProjectReference` to `Beep.OilandGas.PPDM39.DataManagement` | Required for `PPDMGenericRepository`, `PPDMMappingService` |
| `Services/IWellPluggingService.cs` | Clear (replaced) | Interface superseded by `IFieldDecommissioningService`; DTO moved to Models |
| `Services/WellPluggingService.cs` | Clear (replaced) | Replaced by `FieldDecommissioningService.cs` |
| `Services/WellPluggingService.Advanced.cs` | Clear (replaced) | Replaced by `FieldDecommissioningService.Analysis.cs` |
| `Services/FieldDecommissioningService.cs` | **New** | Canonical CRUD implementation |
| `Services/FieldDecommissioningService.Analysis.cs` | **New** | Engineering analysis partial |

---

## `IFieldDecommissioningService` Method Coverage

| Method | PPDM Table | Pattern |
|---|---|---|
| `GetAbandonedWellsForFieldAsync` | `WELL_ABANDONMENT` via `WELL.FIELD_ID` | Metadata → Reflection extract WELL_IDs → AppFilter on WELL_ABANDONMENT |
| `AbandonWellForFieldAsync` | `WELL_ABANDONMENT` | Validate field ownership → Insert via repo → Map to DTO |
| `GetWellAbandonmentForFieldAsync` | `WELL_ABANDONMENT` | AppFilter on WELL_ID + FIELD_ID |
| `GetDecommissionedFacilitiesForFieldAsync` | `FACILITY` via `PRIMARY_FIELD_ID` | AppFilter ABANDONED_DATE not null |
| `DecommissionFacilityForFieldAsync` | `FACILITY` | Update `ABANDONED_DATE` → Insert `FACILITY_DECOM` record |
| `GetFacilityDecommissioningForFieldAsync` | `FACILITY` | AppFilter by ID + FIELD_ID |
| `GetEnvironmentalRestorationsForFieldAsync` | `PROJECT` | AppFilter PROJECT_TYPE='ENV_RESTORATION' + FIELD_ID |
| `CreateEnvironmentalRestorationForFieldAsync` | `PROJECT` | Insert PROJECT with ENV_RESTORATION type |
| `GetDecommissioningCostsForFieldAsync` | `PROJECT` | AppFilter PROJECT_TYPE='DECOM_COST' + FIELD_ID |
| `EstimateCostsForFieldAsync` | `WELL` + `FACILITY` | Count wells/facilities → apply rate model |

---

## Engineering Analysis Methods (Analysis partial)

Preserve from `WellPluggingService.Advanced.cs` — these are pure calculation methods:
- `AnalyzePluggingRequirementsAsync` — cement volume, plug specifications, critical zones
- `AnalyzeDecommissioningCostsAsync` — cost breakdown by category (plugging, wellhead, site, remediation)
- `AnalyzeEnvironmentalRemediationAsync` — risk assessment, contaminant identification, monitoring period
- `AnalyzeRegulatoryComplianceAsync` — jurisdiction-specific requirements (USA/Canada/International)
- `AnalyzePortfolioDecommissioningAsync` — field-level portfolio analysis

---

## Anti-Patterns Removed

| Was | Now |
|---|---|
| `UnitOfWorkFactory.CreateUnitOfWork(typeof(WELL_PLUGBACK), ...)` | `new PPDMGenericRepository(..., entityType, connectionName, tableName)` |
| `await plugbackUow.Get(filters)` | `await repo.GetAsync(filters)` |
| `await plugbackUow.InsertDoc(entity)` + `await uow.Commit()` | `await repo.InsertAsync(entity, userId)` |
| Hard-coded `typeof(WELL_PLUGBACK)` | `Type.GetType($"...{metadata.EntityTypeName}")` |
| No `ICommonColumnHandler` injection | Full canonical constructor |
| No `FIELD_ID` format via `_defaults.FormatIdForTable` | `_defaults.FormatIdForTable("TABLE_NAME", id)` |

---

## Build Verification

```powershell
dotnet build Beep.OilandGas.Decommissioning/Beep.OilandGas.Decommissioning.csproj
```

Expected: 0 errors, 0 warnings related to removed anti-patterns.
