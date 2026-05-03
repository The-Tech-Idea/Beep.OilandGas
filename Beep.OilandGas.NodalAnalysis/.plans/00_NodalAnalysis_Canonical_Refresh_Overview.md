# Nodal Analysis Canonical Refresh Plan

## Scope

Create a phased, execution-ready plan for `Beep.OilandGas.NodalAnalysis` using the same planning pattern used in ProductionForecasting: canonical contract alignment, data/calculation surface hardening, API hardening, tests/verification, PPDM reference alignment, and advanced engine backlog.

## Current Baseline

- **Canonical API contract**: `Beep.OilandGas.Models/Core/Interfaces/INodalAnalysisService.cs`.
- **Implementation**: `Beep.OilandGas.NodalAnalysis/Services/NodalAnalysisService.cs` and partial `NodalAnalysisService.Advanced.cs`.
- **API DI registration**: `Beep.OilandGas.ApiService/Program.cs` (`AddScoped<INodalAnalysisService>`).
- **Persistence target in service**: `NODAL_ANALYSIS_RESULT` through `PPDMGenericRepository`.
- **Known gap**: optimization path still contains placeholder recommendation text.

## Architecture Objective

- Keep `INodalAnalysisService` as canonical HTTP-facing contract.
- Ensure analysis/optimization paths are deterministic and PPDM-safe.
- Harden API/service behavior for argument validation, cancellation propagation, and predictable error contracts.
- Expand regression coverage for nodal operating-point and edge-case behaviors.

## Phase Index

1. `01_Phase_Service_Contracts.md`
2. `02_Phase_Data_And_Calculation_Surface.md`
3. `03_Phase_API_Hardening.md`
4. `04_Phase_Tests_And_Verification.md`
5. `05_Phase_PPDM_And_Reference_Alignment.md`
6. `06_Phase_Advanced_Engine_Backlog.md`
