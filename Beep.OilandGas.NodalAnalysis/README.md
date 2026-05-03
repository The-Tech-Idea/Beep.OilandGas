# Beep.OilandGas.NodalAnalysis

IPR/VLP nodal analysis for oil and gas wells, integrated with **Beep.OilandGas** (single PPDM39 schema), **ASP.NET Core API**, **Blazor Web**, and **desktop** calculation clients.

This package combines:

- **Calculation engine** — IPR/VLP correlations, operating-point solving, sensitivity helpers, rendering (SkiaSharp).
- **Application service** — `NodalAnalysisService` implements **`INodalAnalysisService`** (persisted runs, diagnostics, screening APIs consumed by **`Beep.OilandGas.ApiService`**).

---

## Table of contents

1. [Features (calculation library)](#features-calculation-library)
2. [Solution architecture](#solution-architecture)
3. [Contracts and models](#contracts-and-models)
4. [Canonical HTTP API](#canonical-http-api)
5. [Legacy HTTP API (`CalculationsController`)](#legacy-http-api-calculationscontroller)
6. [Web layer (`Beep.OilandGas.Web`)](#web-layer-beepoilandgasweb)
7. [Desktop client (`Beep.OilandGas.Client`)](#desktop-client-beepoilandgasclient)
8. [Service implementation layout](#service-implementation-layout)
9. [Module and reference data](#module-and-reference-data)
10. [Error handling and cancellation](#error-handling-and-cancellation)
11. [Testing](#testing)
12. [Documentation and trackers](#documentation-and-trackers)
13. [Quick start (library)](#quick-start-library)
14. [Installation](#installation)
15. [License](#license)

---

## Features (calculation library)

### Core

- **IPR curves** — Vogel, Fetkovich, Wiggins, composite, gas-well IPR.
- **VLP curves** — Hagedorn-Brown, Beggs-Brill, Duns-Ros, Orkiszewski, Aziz-Govier-Fogarasi (via calculators under `Calculations/`).
- **Nodal analysis** — Operating point from IPR/VLP intersection (`NodalAnalyzer`, `OperatingPointCalculator`).
- **Gas lift / ESP / choke / tubing** — Supporting calculators where wired in this repo (see `Calculations/` and `USAGE_EXAMPLES.md`).
- **Visualization** — SkiaSharp-based rendering (`Rendering/`).

### Integration-specific

- **`INodalAnalysisService`** — Analyze, optimize, save, history, performance matching, sensitivity, artificial lift ranking, diagnostics, production forecast screening, pressure-maintenance screening.
- **PPDM persistence** — Analysis runs and related entities via **`PPDMGenericRepository`** patterns inside **`NodalAnalysisService`** (partial classes).

---

## Solution architecture

```text
┌─────────────────────────────────────────────────────────────────┐
│  Blazor Web: NodalAnalysis.razor                                │
│    → ICalculationServiceClient / CalculationServiceClient       │
│    → ApiClient (HTTP)                                           │
└────────────────────────────┬────────────────────────────────────┘
                             │  /api/nodalanalysis/*
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  ApiService: NodalAnalysisController                           │
│    → INodalAnalysisService → NodalAnalysisService                │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│  Beep.OilandGas.NodalAnalysis (this project)                   │
│    Calculations / Services / Modules / Constants               │
└─────────────────────────────────────────────────────────────────┘

Legacy path (calculation store): /api/Calculations/nodal* → ICalculationService (different contract).
Prefer canonical routes below for new work.
```

---

## Contracts and models

| Artifact | Location |
|----------|----------|
| **`INodalAnalysisService`** | `Beep.OilandGas.Models/Core/Interfaces/INodalAnalysisService.cs` |
| **Canonical route constants** | `Beep.OilandGas.Models/Data/Calculations/NodalAnalysisHttpRoutes.cs` |
| **Run results / parameters / requests** | `Beep.OilandGas.Models/Data/NodalAnalysis/` (and related projections under `Data/`) |

**`NodalAnalysisHttpRoutes.Prefix`** is **`/api/nodalanalysis`** (lowercase for consistent client URLs; ASP.NET Core routing is case-insensitive).

**`NodalAnalysisHttpRoutes.History(wellUwi)`** uses **`Uri.EscapeDataString`** for the path segment.

---

## Canonical HTTP API

Controller: **`Beep.OilandGas.ApiService/Controllers/Calculations/NodalAnalysisController.cs`**  
Route template: **`[Route("api/[controller]")]`** → **`/api/NodalAnalysis/...`** (same as **`/api/nodalanalysis/...`**).

| HTTP | Relative path | Purpose |
|------|-----------------|--------|
| POST | `/api/nodalanalysis/analyze` | Run nodal analysis (`PerformNodalAnalysisRequest`) |
| POST | `/api/nodalanalysis/optimize` | Optimize system (`OptimizeSystemRequest`) |
| POST | `/api/nodalanalysis/result` | Save run (`NodalAnalysisRunResult`; optional `?userId=`) |
| GET | `/api/nodalanalysis/history/{wellUWI}` | History for well |
| POST | `/api/nodalanalysis/performance-matching` | IPR/VLP performance matching |
| POST | `/api/nodalanalysis/sensitivity` | Economic sensitivity |
| POST | `/api/nodalanalysis/artificial-lift` | Artificial lift recommendation |
| POST | `/api/nodalanalysis/diagnostics` | Well performance diagnostics |
| POST | `/api/nodalanalysis/production-forecast` | Screening forecast (`NodalProductionForecastRequest`) |
| POST | `/api/nodalanalysis/pressure-maintenance` | Pressure maintenance screening |

Validation highlights: non-null bodies where required; **`Well UWI`** required (whitespace rejected); forecast **`ForecastMonths`** positive; decline rate **0–1**; **`OperationCanceledException`** rethrown (not converted to 500).

User identity for save: controller resolves **`sub`**, then **`ClaimTypes.NameIdentifier`**, then **`SYSTEM`**.

---

## Legacy HTTP API (`CalculationsController`)

**`Beep.OilandGas.ApiService/Controllers/CalculationsController.cs`**

| HTTP | Route | Notes |
|------|-------|--------|
| POST | `/api/Calculations/nodal` | **`NodalAnalysisRequest`** → **`NodalAnalysisResult`** (calculation-integration / legacy store). Null body → **400**. **`ArgumentException`** → **400** with message. **`OperationCanceledException`** rethrown. |
| GET | `/api/Calculations/nodal/{calculationId}` | Legacy store lookup by ID. **`OperationCanceledException`** rethrown. |
| GET | `/api/Calculations/nodal` | Query legacy list. **`OperationCanceledException`** rethrown. |

Remarks on controller actions point consumers to **`NodalAnalysisHttpRoutes`** for well-scoped **`NodalAnalysisRunResult`** workflows.

---

## Web layer (`Beep.OilandGas.Web`)

| Piece | Role |
|-------|------|
| **`CalculationServiceClient`** | Implements **`ICalculationServiceClient`**; all nodal calls use **`NodalAnalysisHttpRoutes`**; validates well UWI where applicable; **rethrows `OperationCanceledException`** from nodal wrappers (no fake empty history / false save on cancel). |
| **`ApiClient`** | HTTP helpers; **warnings** log HTTP status on failure paths (GET, typed POST/PUT/PATCH/DELETE, bool POST/PUT/PATCH/DELETE, multipart POST, stream POST) to aid diagnostics (e.g. history GET, save POST). |
| **`Pages/PPDM39/Calculations/NodalAnalysis.razor`** | Route **`/ppdm39/calculations/nodalanalysis`**; well **`MudSelect`** (non-empty UWI only); **`Perform Analysis`** disabled until init completes and wells exist; **`ArgumentException`** / **`InvalidOperationException`** surfaced to user; **`OperationCanceledException`** swallowed during lifecycle header load, well load, history reload, and analyze/save (no misleading snackbars on navigation cancel); save **`false`** shows warning (not success). |

---

## Desktop client (`Beep.OilandGas.Client`)

**`App/Services/Calculations/CalculationsService.NodalAnalysis.cs`** — partial **`CalculationsService`**:

- Remote mode: HTTP via **`NodalAnalysisHttpRoutes`** (aligned with Web).
- **`CancellationToken`** supported on nodal methods where exposed.
- Local mode: **`InvalidOperationException`** (“Local mode not yet implemented”) until a host wires local **`INodalAnalysisService`**.

**`ICalculationsService`** XML docs reference **`NodalAnalysisHttpRoutes`** for nodal endpoints.

---

## Service implementation layout

Partial **`NodalAnalysisService`** (examples):

- **`NodalAnalysisService.cs`** — Core orchestration.
- **`NodalAnalysisService.Advanced.cs`**, **`NodalAnalysisService.SensitivityScenarios.cs`**, **`NodalAnalysisService.ArtificialLiftScoring.cs`** — Focused behaviors.

Calculators live under **`Calculations/`** (e.g. **`IPRCalculator`**, **`VLPCalculator`**, **`NodalCalculator`**, **`OperatingPointCalculator`**).

---

## Module and reference data

**`Modules/NodalAnalysisModule.cs`** — Auto-discovered module: entity registration and **`NodalAnalysisReferenceCodeSeed`** LOV-style seed rows for statuses, optimization types, lift methods, etc.

---

## Error handling and cancellation

- **API (`NodalAnalysisController` + legacy nodal actions on `CalculationsController`)** — **`OperationCanceledException`** propagates to ASP.NET Core host (no **500** wrapper for cancellation).
- **Web `CalculationServiceClient` (nodal)** — Cancels **do not** clear history as empty or report save failure solely due to cancel (explicit **rethrow** paths).
- **Blazor page** — Cancels during init / history / analyze do not show misleading **failed wells** or generic error snackbars.

---

## Testing

Project: **`Beep.OilandGas.ApiService.Tests`**

Filter nodal-related tests:

```bash
dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~Nodal"
```

Includes:

- **`NodalAnalysisControllerTests`** — Validation, success paths, **`ArgumentException`**, **`SaveResult`** user id / cancellation, **`GetHistory`**, **`Optimize`**, **`PerformAnalysis`** cancellation, subsidiary POST actions.
- **`NodalAnalysisLegacyCalculationsControllerTests`** — Legacy **`POST /api/Calculations/nodal`** null body; **`OperationCanceledException`** on legacy POST and GET nodal list/by-id.

*(Last documented roll-up: **70** passing tests under that filter — see **`MASTER-TODO-TRACKER.md`** for current verification block.)*

---

## Documentation and trackers

| Doc | Purpose |
|-----|---------|
| **`MASTER-TODO-TRACKER.md`** | Phase rollup, verification commands, integration checklist (Web, Client, ApiClient, legacy controller). |
| **`.plans/*.md`** | Phased plans (contracts, API hardening, PPDM alignment, tests, backlog). |
| **`USAGE_EXAMPLES.md`** | Usage patterns for the calculation surface. |
| **`ENHANCEMENTS_SUMMARY.md`**, **`ENHANCEMENT_PLAN.md`** | Historical enhancement notes. |

---

## Quick start (library)

```csharp
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;

var reservoir = new ReservoirProperties
{
    ReservoirPressure = 3000,
    BubblePointPressure = 2500,
    ProductivityIndex = 2.5m,
    WaterCut = 0.1m
};

var iprCurve = NodalAnalyzer.GenerateVogelIPR(reservoir, maxFlowRate: 5000);

var wellbore = new WellboreProperties
{
    TubingDiameter = 2.875,
    TubingLength = 8000,
    WellheadPressure = 500,
    WaterCut = 0.1,
    GasOilRatio = 500
};

var flowRates = iprCurve.Select(p => p.FlowRate).ToArray();
var vlpCurve = NodalAnalyzer.GenerateVLP(wellbore, flowRates);

var operatingPoint = NodalAnalyzer.FindOperatingPoint(iprCurve, vlpCurve);
Console.WriteLine($"Flow rate: {operatingPoint.FlowRate}, BHP: {operatingPoint.BottomholePressure}");
```

Adjust property types to match **`ReservoirProperties`** / **`WellboreProperties`** definitions in **`Beep.OilandGas.Models`** (decimals vs doubles per property).

---

## Installation

As a **project reference** inside the Beep.OilandGas solution:

```xml
<ProjectReference Include="..\Beep.OilandGas.NodalAnalysis\Beep.OilandGas.NodalAnalysis.csproj" />
```

The project produces a NuGet package on build (**`GeneratePackageOnBuild`**) — consume from your internal feed or **`dotnet add reference`** as above for development.

---

## License

MIT License (see repository/package metadata).
