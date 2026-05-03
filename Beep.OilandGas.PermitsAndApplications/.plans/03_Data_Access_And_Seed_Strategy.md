# PermitsAndApplications Data Access and Seed Strategy

## Canonical Data Access Policy
- Permit lifecycle create/update/read operations use `PERMIT_APPLICATION` through module services.
- PPDM foundation entities are consumed for relationship/reference enrichment.
- Repository operations use metadata + `AppFilter`; avoid reflection-based write/read dispatch.

## Seed Strategy
- Module-owned reference table: `R_PERMITS_REFERENCE_CODE`.
- Deterministic uniqueness key: `(REFERENCE_SET, REFERENCE_CODE)`.
- Seeding behavior: insert-if-missing, no-op if already present.
- Required families:
  - `PERMIT_STATUS`
  - `PERMIT_AUTHORITY_CATEGORY`
  - `COMPLIANCE_OUTCOME`
  - `COMPLIANCE_STATUS`
  - `FORM_REQUIREMENT_TYPE`
