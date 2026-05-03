# LeaseAcquisition Context Map

## Objective
Establish the current baseline for `Beep.OilandGas.LeaseAcquisition` before canonical PPDM39 alignment work.

## Current Topology Snapshot
- `Modules/LeaseAcquisitionModule.cs` registers:
  - `LEASE_ACQUISITION`
  - `FEE_MINERAL_LEASE`
  - `GOVERNMENT_LEASE`
  - `NET_PROFIT_LEASE`
- Core service implementation is split into partial files under `Services/LeaseAcquisitionService*.cs`.
- Additional service `Services/LeaseManagementService.cs` uses UnitOfWork wrappers over PPDM tables (`LAND_RIGHT`, `LAND_AGREEMENT`).
- API surface currently exposed through `Beep.OilandGas.ApiService/Controllers/Operations/LeaseAcquisitionController.cs`.

## Existing Contract Surfaces
- Canonical models-core interface exists at `Core/Interfaces/ILeaseAcquisitionService.cs` (`Beep.OilandGas.Models.Core.Interfaces` namespace).
- Module-local service interface exists at `Services/ILeaseAcquisitionService.cs` with very broad staged methods and many nested DTOs.
- Additional service contract exists at `Services/ILeaseManagementService.cs`.

## Data Shapes and Persistence Baseline
- Table classes in `Data/Lease/Tables/` are scalar and `ModelEntityBase` based (good baseline).
- Projection/contract classes in `Data/Lease/Projections/` and `Data/Lease/Contracts/` coexist with nested DTOs defined directly inside `Services/ILeaseAcquisitionService.cs`.
- `LeaseAcquisitionModule.SeedAsync` currently returns success with skip reason (no reference seed catalog yet).

## API + DI Baseline
- DI registration in `Beep.OilandGas.ApiService/Program.cs` wires `Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService` to `LeaseAcquisitionService`.
- `LeaseAcquisitionController` routes are under `api/LeaseAcquisition/*` (not field-current scoped).

## Canonicalization Gaps
- No local `.plans` execution package and no module `MASTER-TODO-TRACKER.md`.
- No explicit canonical vs advanced boundary map for lease acquisition behavior.
- Mixed contract ownership across:
  - models-core interface
  - module-local interface
  - nested DTO definitions inside service interface file
- No idempotent module-owned reference seed catalog.
- No explicit PPDM-first usage matrix + minimal column contracts.
- No focused verification pack (build/test/data gates) tied to lease acquisition canonicalization.

## Phase 0 Exit Criteria
- Context baseline documented and accepted.
- High-risk drift areas identified and queued for phased execution docs.
