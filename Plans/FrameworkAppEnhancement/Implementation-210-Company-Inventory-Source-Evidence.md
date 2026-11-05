# Implementation 210 — Company-owned inventory source and movement evidence

## Warehouse workflow prerequisite delivered

The existing Accounting InventoryService now reads an explicitly owned company item queue and a compound item/movement-history review. Native predicates restrict items by owning tenant and company; detail additionally restricts the original item ID. Returned item ownership is independently checked before reading movements. Missing, duplicate, foreign or wrong-type item rows refuse. A missing item remains distinct from an unavailable provider result.

Movement history uses the verified unique item identity and includes active and inactive rows. Missing/duplicate/foreign movement identities and failed/null reads refuse. Recorded quantities, unit cost, value, transaction type/date/reference and audit remain recorded evidence; null quantities remain unknown, and no availability, reservation balance, receipt completion or financial approval is calculated.

CompanyInventorySourceReview in TheTechIdea.Data owns the shared contract and full serialized item/history content revision. It binds the company, actor, item and retrieval time, freezes native input and validates complete history membership. Changing warehouse/location, values, audit or movement content invalidates the original review. Caller-facing API double-capture/context fencing and screens are the next integration step; a native compound capture alone is not proof of a transactional snapshot.

## Canonical schema and existing repository owner

INVENTORY_ITEM and INVENTORY_TRANSACTION move from Models into TheTechIdea.Data with their existing namespaces and identities, removing their old declarations. Models already references Data. No parallel inventory entity is introduced. Both native keys are explicitly nonnullable keys. Item ownership adds nullable bounded OWNER_TENANT_ID (64), OWNER_COMPANY_BA_ID (128), WAREHOUSE_ID (128), LOCATION_ID (128) and CURRENCY_CODE (32). Null legacy evidence is not assigned to the current company or substituted from supplier/field/default values.

The existing InventoryService repository factory now requires its request-bound PRODUCTION resolver and validates fresh inventory key/ownership/measurement metadata before repository access. Its stale repository namespace import is corrected. All its inventory operations use that existing factory; no alternate factory/default connection is added. Existing stock posting and write behavior is not yet a company-authorized receipt/return command and must not be exposed by the new company screen.

The existing ProductionAccountingModuleSetup manifest already declares typeof(INVENTORY_ITEM) and typeof(INVENTORY_TRANSACTION); those types now resolve to the canonical Data assembly. The established module setup registers scope assemblies and uses Beep MigrationManager. No migration is executed, SQL added or business EF lane created. Live schema upgrade and reviewed legacy ownership reconciliation remain open. Changed item location/currency evidence and downstream physical movement commands require independent review under the planned stock/return workflow.

## Focused verification

Actual Data builds with zero warnings/errors in 1.86 seconds. The final expanded source/HTTP/accounting gate passes **554 checks**, zero failed/skipped, with a reported test duration of 5 seconds and 343 composition warnings. It includes **32 new inventory cases** for canonical type/key ownership, nullable bounded migration metadata, native owner predicates, source/history membership, unavailable quantities/history, frozen evidence, no default connection and refusal before data reads for missing/ambiguous schema. The earlier 541-case checkpoint passed before adding detail/schema tests and consolidating the repository factory; it is superseded by the 554-case result. Initial compile errors from the stale namespace and missing test imports were corrected. All launched handles are terminal. Whitespace checks pass.

The actual native service and repository are exercised with mocked Beep provider boundaries; no ledger posting, provider database, live schema, OIDC or browser acceptance is claimed. Existing read methods add no stock or journal writes. The broader company-persona goal remains active.

## Next integrated slice

Extend the existing inventory API with current company standard-role/grant enforcement, exact recorded ID decoding, repeat source captures and closing company/account/PRODUCTION binding checks. Add the typed client and existing procurement/warehouse screen drill-downs using this canonical review. Keep missing location/currency/unit or receipt linkage visible. Then implement reviewed receipt-linked stock movements and supplier return provenance through the existing inventory/receipt owners, with original-intent reconciliation and independent review. Rejected receipt quantities alone must not generate a return or change stock. Landing metrics require the exact complete authorized source population and record drill-downs.
