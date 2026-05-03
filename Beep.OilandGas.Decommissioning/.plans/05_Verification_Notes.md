# Decommissioning Verification Notes

## Build Gates
- `dotnet build Beep.OilandGas.Decommissioning.csproj`
- `dotnet build Beep.OilandGas.LifeCycle.csproj`
- `dotnet build Beep.OilandGas.ApiService.csproj`
- `dotnet build Beep.OilandGas.sln`
- Result: all succeeded (solution gate now green).

## Test Gates
- focused `ApiService.Tests` for Decommissioning controller
- seed-catalog integrity tests
- Result: passed (`DecommissioningControllerTests`, `DecommissioningReferenceSeedCatalogTests`).

## Data/Behavior Gates
- Seed reruns are idempotent (no duplicate `(REFERENCE_SET, REFERENCE_CODE)`).
- Decommissioning transitions update/read `WELL_STATUS` with deterministic per-`STATUS_TYPE` resolution.
- Optional fallback behavior is explicit and test-covered.
