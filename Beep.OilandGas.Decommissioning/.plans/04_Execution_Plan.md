# Decommissioning Execution Plan

## Target Files
- `Beep.OilandGas.Decommissioning/Modules/DecommissioningModule.cs`
- `Beep.OilandGas.Decommissioning/Constants/DecommissioningReferenceCodes.cs`
- `Beep.OilandGas.Decommissioning/Constants/DecommissioningReferenceCodeSeed.cs`
- `Beep.OilandGas.Decommissioning/Data/Tables/R_DECOMMISSIONING_REFERENCE_CODE.cs`
- `Beep.OilandGas.Decommissioning/Services/FieldDecommissioningService.cs`
- `Beep.OilandGas.LifeCycle/Services/Decommissioning/PPDMDecommissioningService.cs`
- `Beep.OilandGas.ApiService/Controllers/Field/DecommissioningController.cs`
- `Beep.OilandGas.ApiService.Tests/*Decommissioning*Tests.cs`

## Ordered Steps
1. Add module-local planning/tracker artifacts.
2. Add decommissioning reference sets and idempotent module seeding.
3. Remove project-discriminator persistence paths from module service.
4. Add WELL_STATUS transition write + reference validation in runtime service.
5. Harden controller-level behavior checks.
6. Add seed and controller tests, then verify builds/tests.
