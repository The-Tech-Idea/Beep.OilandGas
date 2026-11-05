# Implementation 143: Recorded AP invoice lines

## Accountant detail and matching dependency

The existing `/accounting/payables/invoices/{Id}` detail now displays recorded vendor invoice lines, including inactive history, through the same AP service/controller/client/source-review/state/panel. It shows actual line identity/number/description, quantity/unit, nullable unit price/line amount, recorded currency, activity and recorded/changed provenance. Recorded zero remains zero and missing values remain unavailable. No invoice total is derived by summing lines.

Optional recorded order/PO-line references are displayed as unconfirmed text. They do not create navigation, procurement access or a verified matching relationship. Receipt allocation, actual target ownership, catalog validity, conversion and matching evidence remain open. The source screen still offers no creation, approval, matching, posting or payment command.

The existing INVOICE_LINE_ITEM is owned by customer INVOICE_ID and cannot be relabeled as AP data. Source inventory found no AP invoice-line entity. One new AP_INVOICE_LINE therefore lives in TheTechIdea.Data under the existing production-accounting namespace, with no alternate DTO/table owner. The existing accountant screen and workflow/task owners are extended rather than adding a second AP journey.

## Beep schema and parent membership

AP_INVOICE_LINE declares a single AP_INVOICE_LINE_ID key, bounded identities/description, nullable quantity/price/amount, nullable unit/currency codes and nullable recorded PO/line reference pair. It inherits existing common audit fields and uses SetProperty. The existing explicit PRODUCTION Beep manifest now lists this canonical type next to AP_INVOICE. No handwritten SQL or business EF lane is introduced.

Ownership is inherited from the exact owned AP header, whose fresh native metadata must establish its unique nonnullable identity. The canonical service validates that header before reading children with an exact AP_INVOICE_ID predicate, includes active/inactive rows and rejects null/incompatible/foreign/duplicate line identities. Missing line table/key/columns or duplicate required metadata refuse rather than producing a header-only success. The current native metadata gate validates presence/key shape; physical FK/type/collation/capability acceptance still requires provider verification.

No migration plan was built/applied, no invoice line was created and no old source was backfilled. The new table must be created through reviewed Beep MigrationManager planning/application. Existing header lists remain usable on their existing schema; complete detail now requires confirmed line schema and a successful child read. Missing schema is unavailable, never an assumed empty line set.

## Shared review and frozen presentation

APInvoiceSourceReview now requires the actual line set. Create freezes and sorts line identities; the digest includes recorded business/reference/audit fields along with the existing header/company/actor. Require validates membership, duplicate identities, bounded codes/identities/descriptions and a structurally complete optional PO reference pair before accepting the content revision. Descriptions retain ordinary multiline text; identity/unit/currency control characters refuse.

The typed consumer continues validating this shared review; the state freezes a readonly UI projection of every line and clears lines together with header/provenance on failure or company change. Empty recorded lines are explicit and do not establish matching completion or a valid invoice total. Web JSON preserves nullable/zero values, international units and inactive history. This changes the existing source-review payload; external client compatibility/transport acceptance remains unverified and no fallback synthesizes missing lines.

## Focused verification

Actual current Data sources build with zero warnings/errors (1.63 seconds). The actual-source purchase-to-pay gate passes **155 tests**, zero failed/skipped, terminal exit 0, 1.5825 seconds. Log: `%TEMP%/oilgas-ap-lines-143.log`. The previous 134 cases are included; counts are not summed across implementations.

Twenty-one new cases cover actual Data/Beep key/nullable-field metadata; five invalid membership/reference cases; five changed-content refusals; one international-unit/nullable/zero JSON and frozen-state case; one rendered inactive/provenance/unconfirmed-reference case; one successful native exact-parent line query; three foreign/duplicate/null native line refusals; one missing-header denial before child query; and three missing/ambiguous schema refusals. Existing header-only fixtures now explicitly supply a confirmed empty line set; native fixtures return actual empty child collections and updated metadata. The first rendered checkpoint had an outdated case-sensitive warning-text expectation; the final source/fixture passes.

Current complete AP service and current generic repository/interface/transaction helpers compile in the gate with the previously recorded dependency/source-precedence warnings. The ProductionAccounting module, API controller, typed client, parent/facade/host, HTTP/OIDC/browser/keyboard/RTL, native schema/migration/provider and transaction/durable command behavior remain unverified. Successive header/line captures are not an atomic snapshot. No persona/phase is certified.

## Next cross-role handoff

Verify new Beep schema planning/capabilities/application/reconciliation and the actual API/client/parent path. Resolve recorded PO/line references through fresh same-company procurement authority before exposing target data or links; establish exact receipt allocations and reference/conversion evidence. Support the required matching cardinality and discrepancy/reviewer workflow through a canonical relationship owner rather than treating reference text as a match. Complete independently configured AP/buyer/receiver/reviewer creation/approval/post/payment commands, actual recorded outcomes and durable recovery, plus every remaining persona and multinational requirement.
