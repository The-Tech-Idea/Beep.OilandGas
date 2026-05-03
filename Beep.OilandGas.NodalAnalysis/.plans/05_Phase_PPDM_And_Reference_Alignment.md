# Phase 5: PPDM And Reference Alignment

## Goal

Ensure Nodal Analysis persistence, table usage, and any nodal reference metadata are aligned with PPDM39 patterns and module-driven registration.

## Target Files

- `Beep.OilandGas.NodalAnalysis/Services/NodalAnalysisService.cs`
- `Beep.OilandGas.NodalAnalysis/Modules/*` (if module registration exists / required)
- `Beep.OilandGas.Models/Data/NodalAnalysis/*`

## TODO Checklist

- [x] Verify canonical persistence tables for nodal run outputs and related entities.
- [x] Validate active-indicator and audit-column behavior in insert/update flows.
- [x] Confirm module/entity registration covers nodal entities for setup/migration flows.
- [x] Add reference-seed plan only where nodal workflows require controlled code lists.

## Alignment Notes

- `SaveAnalysisResultAsync` persists to `NODAL_ANALYSIS_RESULT` via `PPDMGenericRepository`.
- Save flow sets `ACTIVE_IND = "Y"` and calls `_commonColumnHandler.PrepareForInsert(...)` before repository insert, preserving PPDM audit-column conventions.
- History flow filters by `WELL_UWI` and `ACTIVE_IND = "Y"` through `AppFilter` predicates.
- Added PPDM-style nodal reference entity `R_NODAL_ANALYSIS_REFERENCE_CODE` with auto-discovered module seeding.
- Added auto-discovered setup module `NodalAnalysisModule` (no explicit startup wiring needed) to register nodal table classes and seed reference sets/codes.
- Added PPDM-backed `NODAL_ANALYSIS_RUN_METADATA` and extended save flow to persist run metadata plus optional IPR/VLP point snapshots.

## Verification

- `dotnet build Beep.OilandGas.NodalAnalysis/Beep.OilandGas.NodalAnalysis.csproj`
- `dotnet run --project Beep.OilandGas.ApiService` (smoke check for DI + metadata wiring)
