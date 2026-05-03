# Decommissioning Interface Status Map

## Primary Interface
- `Beep.OilandGas.Models.Core.Interfaces/IFieldDecommissioningService.cs`

## Service Implementations
- Runtime canonical: `Beep.OilandGas.LifeCycle/Services/Decommissioning/PPDMDecommissioningService.cs`
- Module compatibility/aligned: `Beep.OilandGas.Decommissioning/Services/FieldDecommissioningService.cs`

## API Surface
- `Beep.OilandGas.ApiService/Controllers/Field/DecommissioningController.cs`

## Status
- [x] Interface methods mapped to canonical PPDM behavior.
- [x] WELL_STATUS transition rules included for decommissioning operations.
- [x] Field-scoped validation and deterministic fallback behavior defined.
