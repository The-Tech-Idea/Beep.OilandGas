# Beep.OilandGas.ChokeAnalysis — implementation summary

## Role in the solution

Library for **gas choke flow** calculations and related helpers (performance curves, validation, multiphase/advanced extensions). Shared entity types (`CHOKE_PROPERTIES`, `GAS_CHOKE_PROPERTIES`, `CHOKE_FLOW_RESULT`, …) live in **`Beep.OilandGas.Models`** under `Data/ChokeAnalysis/` (`FLOW_REGIME` is stored as **string** codes such as `SONIC` / `SUBSONIC`, aligned with **`ChokeAnalysisReferenceCodes`** in this project). **`ChokePerformanceCurveCalculator`** gas CPC points (`CpcPoint.Regime`) use the same codes; multiphase CPC curve labels remain descriptive strings (for example Gilbert correlation labels). Domain enums/constants (`ChokeFlowRegime`, `ChokeType`, reference seed rows) live under **`Beep.OilandGas.ChokeAnalysis.Constants`**. The HTTP contract for packaged choke runs is **`ChokeAnalysisRequest` / `ChokeAnalysisResult`** (`Models/Data/Calculations/`), orchestrated by **`ICalculationService.PerformChokeAnalysisAsync`** in **LifeCycle** (`PPDMCalculationService`) — when **both** absolute pressures are supplied, it delegates to **`IChokeAnalysisService`** (same physics as `GasChokeCalculator`) **unless** **`ChokeAnalysisOptions.CorrelationMethod`** is one of the empirical multiphase methods (`GILBERT`, `ROS`, `ACHONG`, `PILEHVARI`, `SACHDEVA`, or the **`MULTIPHASE`** alias), as determined by **`ChokeAnalysisReferenceCodes.UseMultiphaseOrchestration`**. That multiphase path uses the Gilbert-style `MultiphaseChokeCalculator` estimate and still populates **`ChokeAnalysisResult.FlowRegime`** with **`SONIC`** / **`SUBSONIC`**. Omitting the correlation or using **`GAS_SINGLE_PHASE`** keeps the single-phase gas path when pressures are present. Unknown correlation strings do not switch to multiphase (they follow single-phase gas when valid pressures are set). When upstream pressure is omitted on the multiphase path, **`MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure`** picks Gilbert vs Ros vs Achong vs Baxendell estimates from **`CalculatePressures`**; **`ChokeAnalysisOptions.CriticalPressureRatioOverride`** (when in (0,1)) replaces the default **0.528** critical ratio for **`FlowRegime`** labeling on that path only.

## Web integration

**`Beep.OilandGas.Web`** [`Pages/PPDM39/Calculations/ChokeAnalysis.razor`](../Beep.OilandGas.Web/Pages/PPDM39/Calculations/ChokeAnalysis.razor) project-references this library and binds a **Correlation method** `MudSelect` to **`ChokeAnalysisRequest.AdditionalParameters.CorrelationMethod`** using **`ChokeAnalysisReferenceCodes`**, so Blazor users can select single-phase gas vs empirical multiphase orchestration without hard-coded strings in the page.

## Canonical service interface

**`IChokeAnalysisService`** (`Beep.OilandGas.Models.Core.Interfaces`) defines the cross-library choke API:

- Downhole / uphole flow (`CHOKE_PROPERTIES` + `GAS_CHOKE_PROPERTIES`)
- Downstream pressure and required choke size
- Validation and performance curves

**`ChokeAnalysisService`** implements this interface. Additional **extended** methods (erosion, bean design, nodal-with-choke, sand risk, etc.) exist on the **concrete class** only — see [.plans/09_Interface_Surface_Canonical_vs_Extended.md](.plans/09_Interface_Surface_Canonical_vs_Extended.md).

## Project layout (high level)

| Area | Purpose |
|------|---------|
| `Calculations/` | `GasChokeCalculator`, multiphase, performance curve, erosion/sizing, discharge coefficient |
| `Services/` | `ChokeAnalysisService` (+ `Advanced`, `Multiphase` partials) |
| `Validation/` | `ChokeValidator` |
| `Constants/` | Physical constants, `ChokeFlowRegime` / `ChokeType`, reference codes + seed rows (`ChokeAnalysisReferenceCodeSeed`) |
| `Modules/` | `ChokeAnalysisModule` — registers **extension** choke tables only (`CHOKE_*`, `R_CHOKE_ANALYSIS_REFERENCE_CODE`); physical schema from entity-driven tooling / ModuleSetup, **not** hand-authored `.sql` in this workflow |
| `Models/ChokeModels.cs` | Migration note only — entities moved to **Models** |

## Dependencies

- **`Beep.OilandGas.GasProperties`** — gas property support (project reference)
- **`Beep.OilandGas.PPDM39`** — PPDM entity types when mapping from `WELL` / tests

## Packaging

NuGet packaging uses **`README.md`** via `PackageReadmeFile` (see `.csproj`). Build should remain **warning-free** with **nullable** enabled.

## Further reading

- [README.md](README.md) — usage and examples
- [.plans/README.md](.plans/README.md) — phased engineering and test plans
- [MASTER-TODO-TRACKER.md](MASTER-TODO-TRACKER.md) — status rollup
