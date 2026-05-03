# HSE Canonical Refresh Plan

## Scope
Refresh `Beep.OilandGas.HSE` planning artifacts to match the canonicalization pattern used across other Beep.OilandGas domains.

## Current Baseline
- HSE persistence is PPDM-backed in `Beep.OilandGas.PPDM39.DataManagement/Services/HSE`.
- Field-scoped composition is in `Beep.OilandGas.LifeCycle/Services/HSE/PPDMHSEService.cs`.
- API routes are under `Beep.OilandGas.ApiService/Controllers/HSE/HSEController.cs`.
- Process routes are under `Beep.OilandGas.ApiService/Controllers/BusinessProcess/HSEProcessController.cs`.

## Architecture Objective
- Keep canonical storage PPDM-first.
- Keep field context enforced in lifecycle/orchestrator.
- Harden API behavior for deterministic error mapping.

## Phase Index
1. `01_Phase_Service_Contracts.md`
2. `02_Phase_Lifecycle_Orchestration.md`
3. `03_Phase_API_Hardening.md`
4. `04_Phase_Tests_And_Verification.md`
