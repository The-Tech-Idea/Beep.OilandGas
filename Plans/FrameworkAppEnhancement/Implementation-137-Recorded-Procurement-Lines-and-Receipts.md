# Implementation 137: Recorded procurement lines and receipts

## Buyer and receiver source workflow

The existing `/procurement/orders/{Id}` workbench now displays the owned purchase-order header, its recorded lines and receipt history together. Supplier selection, encoded detail links, company-change clearing and late-response guards remain the same shared workflow. Line identity, number, description, quantity, unit price, line amount and active indicator are shown. Receipt identity, exact recorded line relationship, date, quantity, receiver, status, description, active indicator and recorded/changed provenance are shown. Inactive history remains visible.

These are recorded source facts. Missing values and empty child sets are explicit. Units, currency and warehouse/location evidence remain unconfirmed; the screen makes no quantity comparison, monetary aggregation, stock availability, receipt acceptance, invoice matching, payment or independent approval claim. It offers no unreviewed procurement mutation. The screen uses the existing MudBlazor panel and typed accounting client without custom CSS or JavaScript.

## Shared data and ownership proof

TheTechIdea.Data owns the existing PURCHASE_ORDER, PO_LINE_ITEM and PO_RECEIPT types and the new PurchaseOrderSourceReview contract. PurchaseOrderOwnership now validates line identity, exact parent identity and duplicate/null rows. The compound review additionally requires every receipt's recorded line to exist in that order's actual line set. A foreign or orphan line cannot inherit ownership merely because its receipt names the requested order.

The review binds original tenant/company, actor, header identity, recorded children, a content revision and actual UTC retrieval time. It freezes source records, sorts child identities and hashes explicit business and audit fields. Tracking internals and retrieval timestamps do not determine the content revision. Consumers validate the digest and relationships before displaying results; Web JSON round trips retain the same proof. Changed or invalid payloads clear displayed evidence.

Each of the three canonical table identities now carries a Key annotation. Fresh header metadata must report exactly one nonnullable PURCHASE_ORDER_ID key before child reads, because child ownership depends on that header identity being unique in the physical PRODUCTION store. Missing, nullable or compound keys refuse. Both explicit ownership columns remain required. Controlled metadata tests prove refusal behavior; actual provider constraints, collation, capabilities and metadata accuracy require native acceptance.

The existing explicit PRODUCTION Beep MigrationManager manifest already lists these three canonical types. No migration plan was built, approved or applied, and no legacy owner was assigned. Nullable ownership reconciliation and native key/schema review are required before promotion. There is no handwritten SQL or new business EF migration lane.

## Independent procurement access and existing API path

Procurement reads now require the independently configured `Procurement.View` permission and a freshly resolved selected-company role recognized through standard User.IsInRole. `Accounting.View` alone does not grant procurement access. No role mapping, automatic permission grant or administrator bypass was added. The accounting landing link uses the same selected-company role and procurement permission boundary; buyers need not receive ledger access to inspect purchase orders.

The canonical asynchronous PurchaseOrderService reads owned headers before lines or receipts and validates all returned relationships. Receipt history includes active and inactive records. The existing GET detail route captures the compound source twice, checks content stability, current authority and PRODUCTION binding, and returns the closing review. The existing typed client validates original company, actor, identity and proof; its detail return type now uses the shared compound contract. Repository consumers were updated, but external transport compatibility remains unverified. These successive reads are not an atomic database snapshot.

The legacy HTTP create route remains fenced with an explicit conflict. Internal approval, receipt and closure methods have not been certified for maker/checker duties, concurrency, transactions or durable recovery. No new write action is enabled.

## Focused verification

Current actual Data sources build with zero warnings/errors (1.55 seconds). `Scripts/Test-PurchaseOrderOwnership.ps1` passed **63 tests**, zero failures/skips, terminal exit 0 (1.3142 seconds). Log: `%TEMP%/oilgas-purchase-source-137.log`.

- Twenty ownership/service/authority/key/metadata checks cover the canonical Data types, explicit ownership, fresh role/permission requirements, rejection of accounting-only access, Beep-generated single keys and refusal of missing/nullable/compound header keys.
- Twenty-two shared source-review and JSON checks cover frozen records, provider-order stability, inactive/null facts, invalid line/receipt membership, changed content and mismatched company/actor/revision/provenance.
- Fifteen actual UI-state checks cover source projections, original selection, returned retrieval time, clearing on invalid or absent reviews, failed reads and late responses.
- Six actual MudBlazor render checks cover encoded links/text, line/receipt provenance, inactive history, missing units/currency and explicit empty/unconfirmed states.

The gate compiles the actual current Data assembly, canonical asynchronous service, API read helper, state and source panel. Test-only native-interface alias and popover/JS fixtures retain the previously documented focused boundary. The complete API controller, typed client, parent workbench, accounting landing and host integration remain source-only. HTTP/OIDC, browser interaction, successful native provider queries, migration/reconciliation, atomic consistency and durable writes remain unverified. No persona, phase or full roadmap closes.

## Next role handoffs

Establish source-backed material units, currency and warehouse/location references before quantities or amounts are compared. Configure requester, buyer, receiver and independent reviewer duties through existing app-owned RBAC. Complete approved demand/work-order/AFE linkage, reviewed creation/approval/issue, partial receipt/correction/return, stock movement/valuation and PO/receipt/invoice-to-AP matching, with native migration, HTTP/browser/provider and durable recovery evidence. Preserve all other persona and multinational requirements in the master plan.
