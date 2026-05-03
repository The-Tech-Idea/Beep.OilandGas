# Canonical vs extended surface (ChokeAnalysis)

## Canonical: `IChokeAnalysisService`

**Location:** `Beep.OilandGas.Models/Core/Interfaces/IChokeAnalysisService.cs`

These methods are the **stable contract** for dependency injection and cross-project calls:

| Method | Purpose |
|--------|---------|
| `CalculateDownholeChokeFlowAsync(CHOKE_PROPERTIES, GAS_CHOKE_PROPERTIES)` | Core gas choke rate |
| `CalculateUpholeChokeFlowAsync(...)` | Uphole configuration |
| `CalculateDownstreamPressureAsync(...)` | Pressure from rate |
| `CalculateRequiredChokeSizeAsync(...)` | Size from rate |
| `ValidateChokeConfigurationAsync(...)` | Input QC |
| `CalculatePerformanceCurveAsync(...)` | Curve sweep |

**PPDM overloads** (e.g. `CalculateDownholeChokeFlowAsync(WELL, ...)`) are **not** on the interface; they are convenience entry points on **`ChokeAnalysisService`**. Add them to **`IChokeAnalysisService`** only if other assemblies must call them without referencing the concrete type.

## Extended: public methods on `ChokeAnalysisService` only

Implemented in partials; intended for **advanced** scenarios or future interface splits:

| Partial / area | Examples (non-exhaustive) |
|----------------|---------------------------|
| `ChokeAnalysisService.cs` | PPDM entity overloads; `GetOptimizationRecommendationsAsync` |
| `ChokeAnalysisService.Multiphase.cs` | `CalculateMultiphaseFlowAsync`, `CompareCorrelationsAsync` |
| `ChokeAnalysisService.Advanced.cs` | `DesignBeanChokeAsync`, `AnalyzeVenturiChokeAsync`, `PredictChokeErosionAsync`, `OptimizeBackPressureAsync`, `PerformNodalAnalysisWithChokeAsync`, etc. |

**Guideline:** Prefer injecting **`IChokeAnalysisService`** in API/services. Use **`ChokeAnalysisService`** directly only where extended APIs are required, or introduce **`IChokeAnalysisExtendedService`** in `Models.Core.Interfaces` if multiple consumers need the extended set.

## Related HTTP contract

**`ICalculationService`** (`PerformChokeAnalysisAsync(ChokeAnalysisRequest)`) is the **orchestration** surface for persisted / packaged analyses, not a duplicate of `IChokeAnalysisService`. Mapping from request DTO to `CHOKE_*` types happens inside **LifeCycle** calculation services.
