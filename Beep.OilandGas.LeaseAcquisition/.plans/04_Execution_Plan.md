# LeaseAcquisition Canonical PPDM39 Execution Plan

## Objective
Deliver canonical lease acquisition boundaries, PPDM-aligned persistence, idempotent seeding, and verification-ready API/service behavior.

## Phase A - Contract and Boundary Cleanup
### Target Files
- `Core/Interfaces/ILeaseAcquisitionService.cs`
- `Services/ILeaseAcquisitionService.cs`
- `Services/ILeaseManagementService.cs`
- `Services/LeaseAcquisitionService*.cs`

### Tasks
- Keep models-core interface as canonical API contract.
- Isolate advanced staged members into explicit advanced contract if retained.
- Move nested DTOs out of interface files into projection/contract files.
- Add compatibility notes where temporary overlap remains.

### Acceptance
- Canonical interface is strict and stable.
- Advanced methods are not accidentally promoted to API.

## Phase B - Data Access Canonicalization
### Target Files
- `Services/LeaseManagementService.cs`
- `Services/LeaseAcquisitionService*.cs`
- `Data/Lease/Tables/*.cs`

### Tasks
- Ensure canonical writes target PPDM-backed table classes only.
- Remove/avoid synthetic in-memory placeholder behavior in canonical paths.
- Normalize status/value mappings for deterministic reads/writes.
- Keep table classes scalar-only and projection classes write-safe.

### Acceptance
- Lease CRUD/status paths are repository-backed and deterministic.
- No table/projection misuse remains.

## Phase C - Seed Catalog + Module Setup
### Target Files
- `Modules/LeaseAcquisitionModule.cs`
- `Constants/LeaseReferenceCodes.cs` (new)
- `Constants/LeaseReferenceCodeSeed.cs` (new)
- `Data/Lease/Tables/R_LEASE_REFERENCE_CODE.cs` (new)

### Tasks
- Register new reference table type in module entity list.
- Implement idempotent seed upsert by `(REFERENCE_SET, REFERENCE_CODE)`.
- Seed required families from Phase 3 strategy.

### Acceptance
- `SeedAsync` performs deterministic inserts and skip reporting.
- Rerun is idempotent.

## Phase D - API + DI Alignment
### Target Files
- `Beep.OilandGas.ApiService/Program.cs`
- `Beep.OilandGas.ApiService/Controllers/Operations/LeaseAcquisitionController.cs`
- Optional new: `Beep.OilandGas.ApiService/Controllers/Field/LeaseAcquisitionController.cs`

### Tasks
- Ensure DI factory registration remains explicit and stable.
- Harden controller guards (`null` request, missing IDs, bad statuses).
- Map `ArgumentException` style validation failures to `400`.
- Add/align field-current routes when ready.

### Acceptance
- Controller behavior is deterministic with clear 400/404/500 boundaries.
- DI resolves canonical service without ambiguity.

## Phase E - Tests, Verification, and Closeout
### Target Files
- `Beep.OilandGas.ApiService.Tests/LeaseAcquisitionControllerTests.cs` (new)
- `Beep.OilandGas.ApiService.Tests/LeaseAcquisitionReferenceSeedCatalogTests.cs` (new)
- `Beep.OilandGas.LeaseAcquisition/MASTER-TODO-TRACKER.md`

### Tasks
- Add focused controller tests for happy/error paths.
- Add seed catalog uniqueness/coverage tests.
- Validate module build, API build, and full solution build.
- Sync tracker and migration notes with final status.

### Acceptance
- Build/test gates pass.
- Tracker reflects completed phased checklist and any residual risks.
