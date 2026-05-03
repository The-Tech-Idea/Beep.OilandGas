# LeaseAcquisition Interface Status Map

## Objective
Map and govern interface responsibilities to prevent contract drift.

## Interface Inventory
- `Core/Interfaces/ILeaseAcquisitionService.cs`
  - Namespace: `Beep.OilandGas.Models.Core.Interfaces`
  - Purpose: canonical API-facing contract.
- `Services/ILeaseAcquisitionService.cs`
  - Purpose: broad module-local interface with extensive staged methods and inline DTOs.
- `Services/ILeaseManagementService.cs`
  - Purpose: lease management CRUD/lifecycle over PPDM tables.

## Status Classification
- **Active canonical**
  - models-core `ILeaseAcquisitionService` methods.
- **Active canonical candidate**
  - `ILeaseManagementService` methods with direct PPDM backing.
- **Advanced/staged**
  - most methods on `Services/ILeaseAcquisitionService.cs` (negotiation, reporting, extensive due-diligence variants, etc.).

## Required Contract Decisions
- Keep models-core `ILeaseAcquisitionService` as the only canonical API contract.
- Split advanced members from module-local `Services/ILeaseAcquisitionService.cs` into explicit advanced contract (`ILeaseAcquisitionAdvancedService`) if still needed.
- Move nested DTOs out of interface file into projection files under `Data/Lease/Projections/` to enforce clean ownership.

## Target Files (Phase 2+)
- `Services/ILeaseAcquisitionService.cs`
- `Services/LeaseAcquisitionService*.cs`
- `Core/Interfaces/ILeaseAcquisitionService.cs`
- `Beep.OilandGas.ApiService/Program.cs`
- `Beep.OilandGas.ApiService/Controllers/Operations/LeaseAcquisitionController.cs`

## Verification Checklist
- [ ] Canonical interface exposes only stable methods.
- [ ] Advanced methods are isolated from canonical API surface.
- [ ] No nested DTO sprawl remains inside interface declarations.
- [ ] DI registration reflects canonical + optional advanced split clearly.
