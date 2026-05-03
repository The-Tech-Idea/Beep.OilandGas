# LeaseAcquisition Behavior Map

## Objective
Define behavior ownership and canonical promotion boundaries for lease acquisition workflows.

## Behavior Buckets
- **Canonical active (must remain stable)**
  - Lease evaluation (`EvaluateLeaseAsync`)
  - Lease search/list (`GetAvailableLeasesAsync`)
  - Create lease acquisition (`CreateLeaseAcquisitionAsync`)
  - Status transition/update (`UpdateLeaseStatusAsync`)
- **Operational canonical (module service)**
  - Lease CRUD/renewal paths in `ILeaseManagementService` over PPDM tables.
- **Advanced/staged (non-promoted)**
  - Negotiation, due diligence, deep financial/portfolio analysis, export/reporting, and broad stakeholder workflows in `Services/ILeaseAcquisitionService.cs`.

## Route Ownership Snapshot
- Current controller: `api/LeaseAcquisition/*` in `Controllers/Operations/LeaseAcquisitionController.cs`.
- Future canonical target for field context: `api/field/current/lease-acquisition/*` (to align with field orchestration pattern).

## Ownership Rules
- Canonical API routes call only canonical methods from the models-core interface.
- Advanced methods remain internal/non-promoted until:
  - PPDM persistence contract exists,
  - deterministic error handling is implemented,
  - focused tests are added.
- Compatibility endpoints (if retained) must be explicitly labeled and documented as transitional.

## Behavior Matrix
| Capability | Current Owner | Canonical Status | Action |
|---|---|---|---|
| Evaluate lease | Models-core service | Canonical | Keep and harden |
| List/search leases | Models-core service | Canonical | Keep and harden |
| Create acquisition | Models-core service | Canonical | Keep and harden |
| Update status | Models-core service | Canonical | Keep and harden |
| Renewal/expiration management | LeaseManagement service | Canonical candidate | Promote with tests |
| Negotiation/due diligence/reporting mega-surface | Module-local service interface | Advanced | Keep non-promoted |

## Phase 1 Exit Criteria
- Canonical vs advanced behavior boundary is explicit.
- Route ownership and promotion policy are documented and enforceable.
