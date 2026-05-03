# Phase 2: Data And Calculation Surface

## Goal

Document and harden how Nodal Analysis calculators and PPDM persistence interact, while eliminating ambiguous placeholders in production paths.

## Target Files

- `Beep.OilandGas.NodalAnalysis/Services/NodalAnalysisService.cs`
- `Beep.OilandGas.NodalAnalysis/Services/NodalAnalysisService.Advanced.cs`
- `Beep.OilandGas.NodalAnalysis/Calculations/*`

## TODO Checklist

- [x] Confirm `NODAL_ANALYSIS_RESULT` is the canonical persisted output for nodal runs.
- [x] Validate field mappings (`WELL_UWI`, operating rate/pressure, status, audit columns).
- [x] Replace placeholder recommendation text in optimization flow with deterministic logic output.
- [x] Ensure no raw SQL and no non-metadata repository access patterns are introduced.

## Verification

- `dotnet build Beep.OilandGas.NodalAnalysis/Beep.OilandGas.NodalAnalysis.csproj`
