# Legal-entity finance, currencies and consolidation

Plan date: 2026-10-05. Task ownership: CA-15. Execution status lives only in the root master tracker. This document details [multinational requirements](Company-Multinational-Requirements.md) and extends the existing company phases; it does not introduce a parallel implementation.

## Dependencies and owners

Finance B; depends on CA-05/06 and relevant FG-41 posting gates.

Responsible implementation areas: Accounting, ProductionAccounting, existing currency translation/consolidation services, API/Client and shared contracts. Inventory existing entity/service/API/client/UI/test owners before adding contracts. Shared business contracts belong in TheTechIdea.Data; clients use APIs; IdentityServer authenticates and app RBAC supplies standard roles. Follow applicable repository skills before code edits.

## Implementation sequence

1. Inventory existing currency translation, consolidation, journal, fiscal period, intercompany and accounting-policy services before modifying ownership.
2. Define separate entity books, functional currency, presentation currency, fiscal calendar and approved account mappings. Keep transaction currency and recorded functional amounts.
3. Version approved exchange-rate sources, rate type, effective time and conversion/rounding rules. Missing or ambiguous rates block posting; reproducibility retains the used rate/version.
4. Extend canonical postings for realized/unrealized exchange differences and entity-scoped close/reopen; reconcile subledgers to ledger before reporting.
5. Connect intercompany transactions with reciprocal references, counterparty approval and explicit mismatch handling. Reversal/correction must preserve both sides and recovery state.
6. Build consolidation from approved entity reports using existing services: reporting period alignment, approved ownership/control basis, translation and eliminations. Preserve entity drill-down and elimination journals.

## Verification and exit gate

- Domain-reviewed samples reconcile transaction, functional and presentation amounts without summing unlike currencies.
- Duplicate/retry/concurrent posting and close tests prove entity scope, balanced entries and durable recovery on supported providers.
- Intercompany mismatches remain visible; consolidated totals trace to entity reports and recorded eliminations.
- Different fiscal calendars, rounding boundaries and changed-rate corrections are tested; permissions restrict both entity books and group reports.

## Delivery and recovery

Split implementation into reviewable changes following the sequence above. Each change records chosen existing owners, migration impacts, success/failure behavior and relevant evidence. Backfill and schema changes require preflight ownership validation, resumable execution and post-migration reconciliation. No compatibility copies, guessed business defaults or duplicate stores. Keep unavailable/error/conflict states explicit through the typed client and UI.

After each slice update the root tracker with commit/environment, checks executed and remaining provider/browser/domain gates. Source-only implementation stays unverified until its acceptance evidence passes. Launch-market and topology discovery inputs are recorded rather than silently assumed.

Implementation 139: [Recorded procurement currency and units](Implementation-139-Recorded-Procurement-Currency-and-Units.md) extends the same PO source review/screen with nullable recorded header currency and separate line/receipt quantity-unit codes, preserving differing international units without conversion or aggregation. Existing PRODUCTION Beep types declare the fields; missing/duplicate fresh schema columns refuse reads. 81 focused Data/service/metadata/content/JSON/state/render checks pass; Data builds cleanly. Migration/reconciliation, authoritative reference/conversion evidence, AP ownership and exact invoice/PO/receipt links, full HTTP/browser/provider and controlled handoffs remain open; no phase closes.

Implementation 140: [Company-owned payables source workbench](Implementation-140-Company-Owned-Payables-Source-Workbench.md) adds supplier-to-recorded-vendor-invoice source review through the existing accounting client/AP controller/async AP owner and configured-persona task queue. AP_INVOICE moves to Data with nullable company ownership/currency and native key metadata. Unreviewed HTTP creation/posting and the company-less AP facade now refuse. 111 focused checks pass, including 30 AP Data/authority/state/render cases; Data builds cleanly. Complete AP service/controller/client/parent/facade/HTTP/browser/provider remain source-only; migration/reconciliation, invoice lines/exact PO/receipt links, controlled duties/post/payment and all other handoffs remain open.

Implementation 143: [Recorded AP invoice lines](Implementation-143-Recorded-AP-Invoice-Lines.md) extends the existing accountant detail with Data-owned AP_INVOICE_LINE, declared by the existing PRODUCTION Beep manifest. Native reads require an owned header and confirmed line schema; reviews validate/freeze actual active/inactive lines and include them in the content revision. The panel shows quantities/units/nullable amounts/provenance and recorded PO references as unconfirmed text. Data builds cleanly; 155 focused checks pass. Migration/application, exact PO/receipt ownership/matching, full HTTP/browser/provider and controlled financial commands remain open.
