# Engineering units, timezones and operational calendars

Plan date: 2026-10-05. Task ownership: CA-24. Execution status lives only in the root master tracker. This document details [multinational requirements](Company-Multinational-Requirements.md) and extends the existing company phases; it does not introduce a parallel implementation.

## Dependencies and owners

Engineering C and operations D; depends on validated tenant/asset context.

Responsible implementation areas: Existing calculation contracts, engineering libraries, measurement/import services and reporting. Inventory existing entity/service/API/client/UI/test owners before adding contracts. Shared business contracts belong in TheTechIdea.Data; clients use APIs; IdentityServer authenticates and app RBAC supplies standard roles. Follow applicable repository skills before code edits.

## Implementation sequence

1. Inventory quantity fields, unit metadata, standard/actual gas bases, reference conditions, date-only fields and timestamp conversions.
2. Define canonical dimensional representation and explicit input/output unit contracts by extending active owners. Retain original measurement value/unit/source alongside normalized value where needed for audit.
3. Require gas volume pressure/temperature basis and applicability information; do not silently assume standard conditions or convert dimensionally incompatible quantities.
4. Separate UTC event instant, asset operating timezone, operational business date and user display timezone. Define shift boundaries and production-day cutoffs.
5. Validate imports under declared locale/unit schema; distinguish ambiguous daylight-saving local times explicitly. Persist operating calendar versions where they affect accounting/production attribution.
6. Propagate canonical basis and operational period into studies, allocations and downstream finance; changing display preferences must not change persisted results.

## Verification and exit gate

- Independent dimensional conversions and engineering reference samples meet recorded tolerances.
- Daylight-saving gaps/overlaps, leap day, midnight, overnight shifts and users in different zones produce the correct operational period.
- Localized import/export round trips preserve values and unit/reference basis; ambiguous formats are rejected.
- Comparison and aggregation reject incompatible periods/bases; original source readings remain traceable.

## Delivery and recovery

Split implementation into reviewable changes following the sequence above. Each change records chosen existing owners, migration impacts, success/failure behavior and relevant evidence. Backfill and schema changes require preflight ownership validation, resumable execution and post-migration reconciliation. No compatibility copies, guessed business defaults or duplicate stores. Keep unavailable/error/conflict states explicit through the typed client and UI.

After each slice update the root tracker with commit/environment, checks executed and remaining provider/browser/domain gates. Source-only implementation stays unverified until its acceptance evidence passes. Launch-market and topology discovery inputs are recorded rather than silently assumed.

Implementation 139: [Recorded procurement currency and units](Implementation-139-Recorded-Procurement-Currency-and-Units.md) extends the same PO source review/screen with nullable recorded header currency and separate line/receipt quantity-unit codes, preserving differing international units without conversion or aggregation. Existing PRODUCTION Beep types declare the fields; missing/duplicate fresh schema columns refuse reads. 81 focused Data/service/metadata/content/JSON/state/render checks pass; Data builds cleanly. Migration/reconciliation, authoritative reference/conversion evidence, AP ownership and exact invoice/PO/receipt links, full HTTP/browser/provider and controlled handoffs remain open; no phase closes.
