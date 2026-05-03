# LeaseAcquisition Migration Notes

## Objective
Capture cutover boundaries and rollout safeguards for canonical lease acquisition.

## Architecture decision: canonical persistence (Option A)
- **Option A (active):** The models-core create/update path persists **only** to `LAND_RIGHT`, `LAND_AGREEMENT`, and `LAND_STATUS` (see `LeaseAcquisitionService.ModelsCoreImpl.cs`). `LEASE_ACQUISITION` and subtype fee/gov/NPL tables are **not** dual-written in this path; they remain available for optional enrichment or future **Option B** (dual-write) if product needs first-class `LEASE_ACQUISITION` reporting without land joins.
- **Option B (deferred):** Not implemented. Only add if analytics require `LEASE_ACQUISITION` rows in sync with `LAND_RIGHT_ID`.

## Canonical Source of Truth
- Canonical API contract: `Beep.OilandGas.Models.Core.Interfaces.ILeaseAcquisitionService` (four methods only).
- Canonical persistence: `LAND_RIGHT` + `LAND_AGREEMENT` + `LAND_STATUS` for those four methods; status values align with `R_LEASE_REFERENCE_CODE` rows for `LeaseReferenceSets.LandRightOperationalStatus`.
- Advanced / staged API: `Beep.OilandGas.LeaseAcquisition.Services.ILeaseAcquisitionAdvancedService` (same `LeaseAcquisitionService` implementation); project DTOs live in `Data/Lease/Projections/`.
- Canonical module setup: `LeaseAcquisitionModule` with idempotent `R_LEASE_REFERENCE_CODE` seed.

## Compatibility Boundaries
- Existing operations controller routes remain temporarily for continuity.
- Advanced/staged methods are non-promoted until PPDM-backed and fully tested.
- Any retained compatibility behavior must be explicitly documented as transitional.

## Risk Register
| Risk | Impact | Mitigation |
|---|---|---|
| Contract drift between interfaces | API instability | strict canonical/advanced split |
| Missing reference seed data | inconsistent statuses | module-owned idempotent seed catalog |
| Placeholder/in-memory behavior in canonical path | non-deterministic behavior | enforce repository-backed canonical methods |
| Table/projection confusion | bad writes/schema drift | enforce table-vs-projection rules |

## Rollout Checklist
- [ ] Implement phases in `04_Execution_Plan.md` order.
- [ ] Execute all verification gates from `05_Verification_Notes.md`.
- [ ] Confirm no unreviewed API surface expansion.
- [ ] Document final compatibility notes.

## Post-Migration Validation
- [ ] Core lease workflows smoke-tested end-to-end.
- [ ] Seed reruns verified idempotent.
- [ ] Cross-module consumers validated for no regressions.

## Exit Criteria
- Migration risks addressed.
- Compatibility boundaries accepted.
- Rollout checklist complete.
