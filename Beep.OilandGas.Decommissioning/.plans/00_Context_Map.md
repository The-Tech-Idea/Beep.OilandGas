# Decommissioning Context Map

## Runtime Path
- `Beep.OilandGas.LifeCycle/Services/FieldOrchestrator.cs` resolves decommissioning service for active field scope.
- `Beep.OilandGas.LifeCycle/Services/Decommissioning/PPDMDecommissioningService.cs` is canonical runtime implementation.
- `Beep.OilandGas.ApiService/Controllers/Field/DecommissioningController.cs` exposes field-scoped endpoints.

## Module Assets
- `Beep.OilandGas.Decommissioning/Modules/DecommissioningModule.cs`
- `Beep.OilandGas.Decommissioning/Data/Tables/*`
- `Beep.OilandGas.Decommissioning/Services/FieldDecommissioningService*.cs`

## Canonical PPDM Focus
- `WELL`, `WELL_ABANDONMENT`, `WELL_STATUS`
- `FACILITY`, `FACILITY_STATUS`
- `WELL_ACTIVITY`
- `R_WELL_STATUS`, `R_WELL_STATUS_TYPE`
