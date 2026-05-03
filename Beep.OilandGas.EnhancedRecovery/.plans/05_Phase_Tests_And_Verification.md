# Phase 5 — Tests and verification

## Goal

Prevent regressions in **PDEN mapping**, **flow measurement upsert**, **economics**, and **advanced screening** outputs through **automated tests** (unit + API contract).

## Target projects

- **`Beep.OilandGas.EnhancedRecovery.Tests`** (new **xUnit**, **net10.0**) — reference **`EnhancedRecovery`**, **`Models`**, **`PPDM39`** as needed.
- **`Beep.OilandGas.ApiService.Tests`** — controller tests mirroring **`EnhancedRecoveryController`** patterns used elsewhere.

## Test matrix (minimum)

| Area | Test case |
|------|-----------|
| **Economics** | NPV/IRR/payback — known spreadsheet vector (oil price, capex, opex, life, discount). |
| **PDEN classification** | **`IsInjectionOperation`** / **`IsEnhancedRecoveryOperation`** with synthetic **`PDEN`** rows. |
| **Flow upsert** | **`UpsertFlowMeasurementAsync`** creates vs updates **`PDEN_FLOW_MEASUREMENT`** (mock **`IUnitOfWorkWrapper`** or integration DB — pick one strategy per CI budget). |
| **Waterflood analytics** | **`AnalyzeWaterfloodPerformanceAsync`** stable output for fixed inputs (snapshot tolerance). |
| **Gas injection** | Miscible vs immiscible branch when **`InjectionPressure`** crosses **`MinimumMiscibilityPressure`**. |
| **API** | **`recovery-factor`** returns **404** when operation missing; **`economics`** validation errors (**`BadRequest`**). |

## TODO checklist

- [x] Test project **`Beep.OilandGas.EnhancedRecovery.Tests`** in **`.sln`** — reference seed catalog, **`EnhancedRecoveryModule`** contract, **`GetScreeningRecoveryFactorPercent`**.
- [ ] Prefer **pure helper tests** for **`AnalyzeEOReconomicsAsync`** NPV/IRR when extracted to static helpers.
- [ ] Optional API/controller tests under **`ApiService.Tests`** (filtered **`EnhancedRecovery`**).

## Verification criteria

```bash
dotnet test Beep.OilandGas.EnhancedRecovery.Tests
dotnet test Beep.OilandGas.ApiService.Tests --filter FullyQualifiedName~EnhancedRecovery
```

## Risks

| Risk | Mitigation |
|------|------------|
| Heavy DB integration tests | Use **in-memory** or **SQLite** PPDM test harness if solution provides one; else focused mocks |
