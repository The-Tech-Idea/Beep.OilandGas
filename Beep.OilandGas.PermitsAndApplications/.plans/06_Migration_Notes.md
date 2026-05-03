# PermitsAndApplications Migration Notes

## Canonical Source of Truth
- Lifecycle persistence source: `PERMIT_APPLICATION` + module lifecycle service interfaces.
- Reference catalog source: `R_PERMITS_REFERENCE_CODE` seeded idempotently.

## Compatibility Boundaries
- Existing LifeCycle permit service remains available while routing future orchestration to canonical module interfaces.
- PPDM foundation table ownership remains in PPDM39 setup modules.

## Deprecation/Cutover Checklist
- Remove or document any remaining dual-write paths to `APPLICATION`.
- Keep adapter logic explicit where old contracts remain.
- Ensure API endpoints call canonical lifecycle/workflow service operations only.
