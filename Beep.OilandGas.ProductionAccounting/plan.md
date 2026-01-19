# Beep.OilandGas.ProductionAccounting - Services Plan

## Goal
Stabilize and enhance the ProductionAccounting service layer to align with the accounting patterns in `Beep.OilandGas.Accounting` while keeping ProductionAccounting-specific domain logic (run tickets, allocations, royalties, pricing, measurement, tank inventory, imbalances).

## Non-goals (for this iteration)
- Reworking the full solution build (other projects currently fail to compile).
- Introducing new database tables beyond the existing PPDM-integrated entities.
- Breaking public interfaces in `Beep.OilandGas.Models.Core.Interfaces`.

## Current Findings (from docs + code)
- Several services use filters on columns not currently represented on the entity classes (e.g., `MEASUREMENT_RECORD.WELL_ID`, `IMBALANCE_ADJUSTMENT.PROPERTY_OR_LEASE_ID`).
- Some query methods accept date ranges but do not apply them (e.g., measurement and pricing history).
- `JournalEntryService.CreateEntryAsync(glAccount, amount, ...)` creates an unbalanced header and does not create line-level rows (`GL_ENTRY`), making downstream reporting and validation inconsistent.
- Constants are duplicated and scattered (e.g., cost categories/types declared inside a service file).

## Principles / Service Contract Standards
- All persistence goes through `PPDMGenericRepository`.
- All services validate input and throw domain exceptions (`ProductionAccountingException` / specific exceptions) for business-rule failures.
- All services are deterministic and side-effect free except for DB writes; log key workflow steps and IDs.
- Date range parameters must be applied at query time (server-side filtering where possible).
- Use consistent default connection name (`PPDM39`) and active indicator (`Y`).
- No DTOs, dictionaries, or object-based models in services. Use only strong-typed entity classes that inherit from `Entity` and implement `IPPDMEntity`.
- All data classes must live in the `Beep.OilandGas.Models` project. If a required class is missing, create it there; if it exists but needs changes, update it there.

## Implementation Phases
### Phase 1 - Data contract correctness (models)
- Remove partial extension files and update the base model classes directly with the missing columns.
- Ensure services set those fields when creating new records.

### Phase 2 - Accounting alignment (GL posting)
- Fix `JournalEntryService` to:
  - create balanced entries by default,
  - write supporting rows (`GL_ENTRY`) for account-based queries,
  - compute balances from `GL_ENTRY`.

### Phase 3 - Service quality improvements
- Apply date range filtering consistently (measurement, pricing, etc.).
- Normalize constants into `Beep.OilandGas.ProductionAccounting.Constants`.
- Remove obvious mistakes (duplicate usings, inconsistent filters, missing required fields).

### Phase 4 - Validation
- Build `Beep.OilandGas.ProductionAccounting` and `Beep.OilandGas.Accounting`.
- Run targeted compilation checks after each service change.
