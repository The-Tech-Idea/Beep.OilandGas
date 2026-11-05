# Implementation 139: Recorded procurement currency and units

## Procurement source to AP matching dependency

The buyer/receiver PO detail now shows recorded header currency and separate ordered-line and receipt quantity-unit codes. Missing codes remain unavailable. Different line/receipt units remain different; the screen neither converts nor compares quantities. It does not aggregate amounts, format a guessed currency symbol, infer outstanding quantities or certify accepted stock. Units/currency catalog validity, conversion basis and warehouse/location evidence remain unconfirmed.

This extends the existing source review, state and MudBlazor panel rather than adding another workbench or model owner. Supplier selection and configured-persona assigned work remain on the existing procurement landing. Currency/unit changes invalidate the existing recorded content proof; the typed caller and UI cannot reuse the earlier revision.

Inspection of the canonical APInvoiceService and AP_INVOICE found no reviewed PO/receipt relationship and no explicit company ownership in that entity. The separate INVOICE is customer-facing and is not a substitute vendor invoice. Supplier equality cannot prove matching, and no PO-to-AP link or amount comparison is fabricated. Reviewed AP ownership, native relationships and exact invoice/line links are required for the next handoff.

## Canonical Data fields and Beep schema boundary

The existing Data-owned PURCHASE_ORDER adds nullable CURRENCY_CODE with a 32-character bound. PO_LINE_ITEM and PO_RECEIPT each add nullable QUANTITY_UOM with a 64-character bound. SetProperty remains the existing change-tracking mechanism. Nullable fields permit additive evolution and explicit legacy reconciliation; no currency or unit is assigned from locale, supplier, selected company, another row or a default.

These are recorded codes, not a new ISO-currency or unit-conversion catalog. Case and international unit characters are preserved. The compound source review includes all three fields in its explicit content digest, refuses overlength/control-character values, freezes values and retains them through Web JSON. Raw line and receipt quantities are still independent recorded facts.

The canonical asynchronous PurchaseOrderService obtains fresh provider metadata for header, line and receipt repositories. Each requires its existing single nonnullable identity key; the header requires owner columns and CURRENCY_CODE, and each child requires parent identity and QUANTITY_UOM. Missing or duplicate required columns refuse before repository access rather than appearing as unavailable values from a schema that has not been migrated.

The existing explicit PRODUCTION Beep manifest still owns all three canonical types. Actual ClassCreator emits nullable bounded string fields. Beep MigrationManager planning, provider capabilities, native type/key/constraint verification, migration approval/application and reviewed legacy reconciliation remain required. No migration was executed, no row was changed, and no SQL or new business EF migration lane was added.

## Focused verification

Actual current Data sources build with zero warnings/errors (1.51 seconds). `Scripts/Test-PurchaseOrderOwnership.ps1` passed **81 tests**, zero failed/skipped, terminal exit 0 (1.2507 seconds). Log: `%TEMP%/oilgas-procurement-measurements-139.log`.

The prior 63-case procurement gate is included; counts are not additive across implementations. Eighteen new cases cover three actual Beep field metadata checks, six missing/duplicate schema-column refusals, three currency/unit revision mutations, one international-unit Web JSON preservation case, three invalid recorded-code refusals, one frozen UI projection case and one actual rendered currency/distinct-unit case. Controlled child repository-factory refusal tests invoke that actual private factory explicitly; they do not represent successful native queries or full public endpoint execution.

The first gate had a test compile error from a mistyped EntityField property name; the fixture was corrected to its actual Fieldtype contract and the final gate passes. Actual service, Data contract, state and source panel compile/render in the focused gate. API controller, complete typed client, parent landing/detail, DI/HTTP/OIDC, browser interactions and real providers remain unverified. Schema presence checks are not proof of native type/constraint enforcement or transactional consistency.

## Remaining role workflow

Resolve authoritative currency/unit catalog and conversion evidence, material/warehouse/location identities, and approved demand/work-order/AFE linkage. Establish explicit company-owned AP invoice and line-to-PO/receipt relationships through the canonical Data/service/API owners before enabling matching. Complete configured buyer/receiver/AP/reviewer duties, independent approvals, receipt correction/return, stock movement/valuation and durable commands/readback. All other personas and multinational requirements remain active; no phase closes.
