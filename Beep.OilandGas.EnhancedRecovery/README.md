# Beep.OilandGas.EnhancedRecovery

**Enhanced Oil Recovery (EOR)** services for the Beep Oil & Gas stack: **PDEN-backed** scheme and injection operations, **economics**, and **screening-level** analytics for waterflooding, gas injection, chemical EOR, and thermal recovery.

This library sits between **ASP.NET Core (`Beep.OilandGas.ApiService`)**, **LifeCycle orchestration**, and **PPDM39** persistence via **`IDMEEditor`** and **`UnitOfWork`**.

---

## Module setup (`IModuleSetup`)

**`EnhancedRecoveryModule`** (`Beep.OilandGas.EnhancedRecovery.Modules`) inherits **`ModuleSetupBase`** and is **auto-discovered** when **`Beep.OilandGas.ApiService`** loads (same pattern as **ChokeAnalysis**, **CompressorAnalysis**, **NodalAnalysis**).

| Property | Value |
|----------|--------|
| **`ModuleId`** | **`ENHANCED_RECOVERY`** |
| **`Order`** | **79** (after Compressor **78**, before Facility **80**) |
| **`EntityTypes`** | **`R_ENHANCED_RECOVERY_REFERENCE_CODE`** — extension **`ModelEntityBase`** table for domain LOVs (**not** core PPDM tables such as **`PDEN`**, which remain on the main schema path). |

**Seeding** (**`SeedAsync`**): idempotent inserts into **`R_ENHANCED_RECOVERY_REFERENCE_CODE`** from **`EnhancedRecoveryReferenceCodeSeed`** — EOR method categories (**`WATER_FLOOD`**, **`GAS_INJECTION`**, **`CO2_MISCIBLE`**, …) and screening class tags. Constants live under **`Data/Constants/`** (**`EnhancedRecoveryReferenceSets`**, **`EnhancedRecoveryConstants`**).

Physical DDL for **`R_ENHANCED_RECOVERY_REFERENCE_CODE`** comes from **entity-driven** PPDM / migration tooling tied to **`EntityTypes`** — **no** hand-written feature DDL under **`Models/Scripts`** (see root **`CLAUDE.md`**).

---

## Capabilities

### Persistence & operations (PPDM-aligned)

- **Enhanced recovery schemes** and **injection** modeled primarily as **`PDEN`** rows with **`ENHANCED_RECOVERY_TYPE`**, **`PDEN_SUBTYPE`**, **`AREA_ID` / `FIELD_ID`**, well linkage (**`CURRENT_WELL_STR_NUMBER`**), and status dates.
- **Injection / allocation rates** recorded on **`PDEN_FLOW_MEASUREMENT`** (e.g. **`INJECTION_RATE`**, **`FLOW_RATE_OUOM`**, **`PRODUCT_TYPE`**).
- **CRUD-style queries** for operations: list/get/create EOR operations, list injection operations, waterflood and gas-injection lists filtered by **`PDEN_SUBTYPE`**.
- **Injection management**: create/update injection **`PDEN`** and upsert latest **flow measurement** for a well.

### Shared contract (`Beep.OilandGas.Models.Core.Interfaces`)

Implemented by **`EnhancedRecoveryService`** and registered in **`ApiService`** for HTTP and LifeCycle:

| Method | Purpose |
|--------|---------|
| **`AnalyzeEORPotentialAsync(fieldId, eorMethod)`** | Screening entry: finds existing EOR op for field/method or creates a **`PDEN`** record via **`CreateEnhancedRecoveryOperationAsync`**. |
| **`CalculateRecoveryFactorAsync(operationId)`** | Loads the operation and attaches a **screening** incremental RF (% OOIP) on **`Efficiency`** plus an explanatory **`Remarks`** suffix (EOR-class heuristic — not volumetric decline RF). |
| **`AnalyzeEOReconomicsAsync(...)`** | Pilot / project economics: NPV, IRR, payback, sensitivity bands on oil price. |
| **`GetInjectionOperationsAsync(wellUWI)`** | Active injection operations from **`PDEN`** where type resolves to injection. |
| **`ManageInjectionAsync(injectionWellId, injectionRate)`** | Creates injection **`PDEN`** for well and upserts **`PDEN_FLOW_MEASUREMENT`**. |

### Extended operations (feature interface)

**`Beep.OilandGas.EnhancedRecovery.Services.IEnhancedRecoveryOperationsService`** — list/create helpers beyond the shared **`Models.Core.Interfaces`** contract:

- **`GetEnhancedRecoveryOperationsAsync`**, **`GetEnhancedRecoveryOperationAsync`**, **`CreateEnhancedRecoveryOperationAsync`**
- **`GetWaterFloodingOperationsAsync`**, **`GetGasInjectionOperationsAsync`**

Prefer resolving **`Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService`** in API/LifeCycle for stable cross-assembly DI.

### Advanced screening analytics (`EnhancedRecoveryService.Advanced.cs`)

These methods return **projection** types in **`Beep.OilandGas.Models.Data.Calculations`**. They implement **engineering screening** correlations and simplified material-balance-style reasoning — **not** full compositional or finite-difference simulation. Most are **not yet exposed** on **`Models.Core.Interfaces.IEnhancedRecoveryService`** or **`EnhancedRecoveryController`**; exposure is tracked in **[`.plans/04`](.plans/04_Phase_API_Web_And_Lifecycle_Orchestration.md)**.

| Method | Output type (summary) |
|--------|------------------------|
| **`AnalyzeWaterfloodPerformanceAsync`** | **`WaterfloodPerformanceAnalysis`** — recovery factor trends, sweep / water-cut proxies. |
| **`AnalyzeGasInjectionAsync`** | **`GasInjectionAnalysis`** — miscible vs immiscible screening vs MMP. |
| **`AnalyzeChemicalEORAsync`** | **`ChemicalEORAnalysis`** — polymer / chemical screening-style metrics. |
| **`AnalyzeThermalRecoveryAsync`** | **`ThermalRecoveryAnalysis`** — thermal scenario screening. |
| **`CompareEORMethodsAsync`** | **`EORMethodComparison`** — ranked method comparison. |
| **`OptimizeInjectionWellPlacementAsync`** | **`InjectionWellOptimization`** — sensitivity / heuristic placement (not a reservoir simulator). |
| **`AnalyzePressurePerformanceAsync`** | **`PressurePerformanceAnalysis`** — pressure maintenance narrative metrics. |
| **`AnalyzeEOReconomicsAsync`** | **`EOREconomicAnalysis`** — same economics path as the shared interface (implemented on the concrete service). |

---

## PPDM mapping

| Concept | PPDM entity | Notes |
|---------|-------------|--------|
| EOR / injection **scheme** | **`PDEN`** | **`ENHANCED_RECOVERY_TYPE`**, **`PDEN_SUBTYPE`** (`WATER_FLOOD`, `GAS_INJECTION`, `INJECTION`, etc.), **`SOURCE`** (e.g. `ENHANCED_RECOVERY`). |
| **Field** scope | **`AREA_ID`**, **`FIELD_ID`**, **`AREA_TYPE`** | Filters use **`AREA_ID`** for field-scoped lists where populated. |
| **Well** link | **`CURRENT_WELL_STR_NUMBER`** | Injection well UWI for pattern/injection queries. |
| **Rates** | **`PDEN_FLOW_MEASUREMENT`** | **`MEASUREMENT_TYPE`** `INJECTION_RATE`; **`FLOW_RATE`**, **`FLOW_RATE_OUOM`** (e.g. `BBL/D`). |

**Well integrity**: Any future change that creates or updates **`WELL`**, **`WELL_STATUS`**, or **`WELL_XREF`** must use **`WellServices`** per solution **`CLAUDE.md`** — this module currently references **`WELL`** read-only for injection creation via **`UnitOfWork`**.

---

## Units & conventions (baseline)

Document **inputs** when calling APIs or building requests:

| Quantity | Typical unit in this codebase | Notes |
|----------|-------------------------------|--------|
| Pressure | **psia** (screening) | Confirm against field data contracts. |
| Liquid rate | **STB/d**, **`BBL/D`** on measurement | **`UpsertFlowMeasurementAsync`** defaults oil-field units when unit omitted. |
| Gas | **Mscf/d** or **MMscf/d** | Use consistent **`FLOW_RATE_OUOM`** when extending gas injection measurements. |
| Economics | **USD**, **$/bbl**, discount as **fraction** (e.g. `0.10` for 10%) | ApiService may accept **percent** for discount and convert — see **`EnhancedRecoveryController`**. |

Always validate ranges in production UIs (pressures, rates, OOIP, project life).

---

## Limitations (important)

1. **Screening vs simulation**: Advanced methods use **simplified** correlations and placeholders suitable for **portfolio screening** and **training workflows**. They **do not** replace **reservoir simulation**, **PVT lab**, or **regulatory** CO₂ storage accounting unless explicitly extended.
2. **`CalculateRecoveryFactorAsync`**: Populates **`Efficiency`** with a **screening** incremental RF (% OOIP) by EOR class (**`GetScreeningRecoveryFactorPercent`**). Integrating **material-balance / decline / OOIP from `POOL`** remains future work — see **[`.plans/01`](.plans/01_Phase_Contracts_PDEN_Reference_And_Data_Fidelity.md)**.
3. **`Efficiency` column semantics**: For **`recovery-factor`** responses, **`Efficiency`** holds **screening RF %**; other endpoints may leave it unset. UI labels should not assume “compressor efficiency” or similar.

---

## Dependencies

- **`Beep.OilandGas.Models`** — shared projections (**`EnhancedRecoveryOperation`**, **`InjectionOperation`**, **`EOREconomicAnalysis`**, analytics result types under **`Models.Data.Calculations`**).
- **`Beep.OilandGas.PPDM39`** — **`PDEN`**, **`WELL`**, **`PDEN_FLOW_MEASUREMENT`**, etc.
- **`Beep.OilandGas.PPDM39.DataManagement`** — **`UnitOfWork`** / **`IDMEEditor`** integration patterns.
- **`Microsoft.Extensions.Logging.Abstractions`** — logging (constructor currently supports **`NullLogger`** default; production registration should supply **`ILogger<EnhancedRecoveryService>`**).

**`PPDM39.DataManagement`** does not reference this project (domain stays out of infrastructure).

---

## Dependency injection (ApiService)

Registered in **`Beep.OilandGas.ApiService/Program.cs`** (scoped factory with **`IDMEEditor`** and connection name), for example:

```csharp
builder.Services.AddScoped<Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var connectionName = /* from configuration */;
    return new Beep.OilandGas.EnhancedRecovery.Services.EnhancedRecoveryService(editor, connectionName);
});
```

Resolve **`Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService`** in controllers and LifeCycle services.

---

## HTTP API (reference)

Implemented in **`Beep.OilandGas.ApiService/Controllers/Operations/EnhancedRecoveryController.cs`** (base path **`/api/EnhancedRecovery`**, **`[Authorize]`**):

| Verb | Route | Description |
|------|-------|-------------|
| POST | **`/analyze-eor`** | **`AnalyzeEORPotentialAsync`** |
| POST | **`/recovery-factor`** | **`CalculateRecoveryFactorAsync`** |
| GET | **`/injection`** | **`GetInjectionOperationsAsync`** |
| POST | **`/economics`** | **`AnalyzeEOReconomicsAsync`** |
| POST | **`/injection`** | **`ManageInjectionAsync`** |

The **Blazor Web** layer uses two clients:

- **`EnhancedRecoveryServiceClient`** — calls the operations controller with paths such as **`/api/enhancedrecovery/analyze-eor`**, **`/api/enhancedrecovery/recovery-factor`**, **`/api/enhancedrecovery/economics`** (ASP.NET Core route matching is case-insensitive; controller class is **`EnhancedRecoveryController`**).
- **`EnhancedRecoveryClient`** — field-scoped listing APIs: **`/api/field/current/enhanced-recovery/operations`**, **`.../injection`**, etc.

Keep these paths aligned when changing controllers or adding a single shared route constant (see **[`.plans/04`](.plans/04_Phase_API_Web_And_Lifecycle_Orchestration.md)**).

---

## Documentation & planning

| Resource | Purpose |
|----------|---------|
| **[`.plans/README.md`](.plans/README.md)** | Phased enhancement plan index and solution standards. |
| **[`MASTER-TODO-TRACKER.md`](MASTER-TODO-TRACKER.md)** | Single rollup of phases and next actions. |
| **[`.plans/07_EOR_Best_Practices_And_Scenario_Matrix.md`](.plans/07_EOR_Best_Practices_And_Scenario_Matrix.md)** | Industry screening / surveillance reference matrix. |

Solution-wide guidance: **[`CLAUDE.md`](../CLAUDE.md)** (repository root).

---

## Build

```bash
dotnet build Beep.OilandGas.EnhancedRecovery/Beep.OilandGas.EnhancedRecovery.csproj
```

**`Beep.OilandGas.EnhancedRecovery.Tests`** — reference LOV catalog, module contract, screening RF helpers.

```bash
dotnet test Beep.OilandGas.EnhancedRecovery.Tests/Beep.OilandGas.EnhancedRecovery.Tests.csproj
```

---

## License / package

Package metadata is defined in **`Beep.OilandGas.EnhancedRecovery.csproj`**. This README is included in the NuGet package when **`PackageReadmeFile`** is set.
