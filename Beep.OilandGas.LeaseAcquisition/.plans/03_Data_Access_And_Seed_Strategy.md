# LeaseAcquisition Data Access and Seed Strategy

## Objective
Define PPDM-first persistence and idempotent seeding strategy for lease acquisition canonicalization.

## PPDM-First Usage Matrix
| Capability | Preferred PPDM Tables | Notes |
|---|---|---|
| Lease rights baseline | `LAND_RIGHT`, `LAND_AGREEMENT` | Already used in `LeaseManagementService` |
| Module-specific acquisition tracking | `LEASE_ACQUISITION` | Keep as canonical module table |
| Fee lease tracking | `FEE_MINERAL_LEASE` | Keep scalar table contract |
| Government lease tracking | `GOVERNMENT_LEASE` | Keep scalar table contract |
| Net profit lease tracking | `NET_PROFIT_LEASE` | Keep scalar table contract |
| Stakeholders/owners linkage | `BUSINESS_ASSOCIATE` (by BA IDs) | Prefer BA references over free text |

## Table vs Projection Contracts
- **Table classes**
  - Must remain scalar-only `ModelEntityBase` classes.
  - Live under `Data/Lease/Tables/`.
- **Projection/DTO classes**
  - Can include lists/nested objects.
  - Must not be passed directly to repository writes.
  - Live under `Data/Lease/Projections/` and `Data/Lease/Contracts/`.

## Minimal Required Column Contracts

### `LEASE_ACQUISITION`
- Required: `LEASE_ACQUISITION_ID`, `FIELD_ID`, `LEASE_NAME`, `LOCATION_ID`, `ACQUISITION_DATE`, `ACQUISITION_STATUS`.
- Strongly recommended: `LESSOR_BA_ID`, `LESSEE_BA_ID`, `ACQUISITION_COST`, `ACQUISITION_COST_OUOM`, `LEASE_EXPIRATION`.

### `FEE_MINERAL_LEASE`
- Required: `LEASE_ID`, `PROPERTY_ID`, `LEASE_NUMBER`, `LEASE_NAME`, `MINERAL_OWNER_BA_ID`, `EXPIRATION_DATE`.
- Strongly recommended: `WORKING_INTEREST`, `NET_REVENUE_INTEREST`, `ROYALTY_RATE`, `PRIMARY_TERM_MONTHS`.

### `GOVERNMENT_LEASE`
- Required: `LEASE_ID`, `PROPERTY_ID`, `GOVERNMENT_AGENCY`, `LEASE_NUMBER`, `EXPIRATION_DATE`.
- Strongly recommended: `IS_FEDERAL`, `IS_INDIAN`, `WORKING_INTEREST`, `ROYALTY_RATE`.

### `NET_PROFIT_LEASE`
- Required: `LEASE_ID`, `PROPERTY_ID`, `LEASE_NUMBER`, `LEASE_NAME`, `NET_PROFIT_INTEREST`.
- Strongly recommended: `RECOVERY_PERCENTAGE`, `EXPIRATION_DATE`, `PRIMARY_TERM_MONTHS`, `BUSINESS_ASSOCIATE_ID`.

## Seed Strategy
- Introduce module-owned reference catalog table:
  - `R_LEASE_REFERENCE_CODE` (new table class).
- Add constants + seed catalog:
  - `Constants/LeaseReferenceCodes.cs`
  - `Constants/LeaseReferenceCodeSeed.cs`
- Seed families (idempotent by natural key):
  - `LEASE_STATUS`
  - `LEASE_TYPE`
  - `NEGOTIATION_STATUS`
  - `OBLIGATION_STATUS`
  - `RENEWAL_STATUS`
  - `TERMINATION_REASON`
  - `LEASE_PAYMENT_TYPE`

## Idempotency Rule
- Upsert by `(REFERENCE_SET, REFERENCE_CODE)` and skip existing keys.
- Module setup reruns must not produce duplicate rows.

## Phase 3 Exit Criteria
- PPDM usage matrix is explicit and accepted.
- Minimal column contracts are defined.
- Seed strategy is concrete, deterministic, and testable.
