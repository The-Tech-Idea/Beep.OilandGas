# Phase 4 — API, LifeCycle orchestration, and client

## Goal

Unify **HTTP** and **orchestrated** calculation entry points so **LifeCycle** and **`CompressorController`** do not diverge silently. Optional: inject **`ICompressorAnalysisService`** everywhere the library is consumed.

## Target files

- `Beep.OilandGas.ApiService/Controllers/CompressorController.cs`
- `Beep.OilandGas.ApiService/Controllers/CalculationsController.cs` (compressor action)
- `Beep.OilandGas.LifeCycle/Services/Calculations/PPDMCalculationService.Facilities.cs`
- `Beep.OilandGas.ApiService/Program.cs` — DI for **`ICompressorAnalysisService`** (after phase 1)
- `Beep.OilandGas.Client/App/Services/Analysis/AnalysisService.Compressor.cs`

## Current behavior (baseline)

| Entry | Behavior |
|-------|----------|
| **`CompressorController`** | Normalizes payloads, delegates to **`ICompressorAnalysisService`** (**`CalculateCentrifugalPowerAsync`** / **`CalculateReciprocatingPowerAsync`**) — no direct static calculator calls from the controller |
| **`PerformCompressorAnalysisAsync`** | Builds **`COMPRESSOR_*`** inputs; **`AnalysisType`** **`PRESSURE`** → **`CalculateRequiredPressureAsync`**; **`POWER`**/**`EFFICIENCY`**/**default** → centrifugal or reciprocating power via **`ICompressorAnalysisService`** (aligned with controller physics) |

## TODO checklist

- [x] After **`ICompressorAnalysisService`** exists: **`CompressorController`** delegates to the service (thin controller).
- [x] **`PerformCompressorAnalysisAsync`** — **`AnalysisType`** branch: **`PRESSURE`** → **`CalculateRequiredPressureAsync`**; otherwise (**`POWER`**, **`EFFICIENCY`**, default) → centrifugal vs reciprocating power via **`ICompressorAnalysisService`**.
- [x] **`COMPRESSOR_POWER_RESULT`** / pressure results populated from service helpers (`ApplyCompressorPowerResult`, `ApplyCompressorPressureResult`) for facility orchestration.
- [ ] Client: verify routes **`/api/compressor/*`** match controller route attributes after any refactor (spot-check when **`AnalysisService.Compressor`** changes).
- [ ] Authorization / **`userId`** — align with **`CalculationsController`** patterns if persisting runs.

## Verification criteria

- `dotnet build Beep.OilandGas.ApiService`
- Swagger/manual: **`POST /api/compressor/power`** (or equivalent) returns **`COMPRESSOR_POWER_RESULT`** with stamped **`OPERATING_CONDITIONS`**
- **`POST`** packaged compressor calculation — unchanged contract unless version bump documented

## Risks

| Risk | Mitigation |
|------|------------|
| Behavior change for existing API consumers | Version API or gate behind **`AnalysisType`** / query flag |
