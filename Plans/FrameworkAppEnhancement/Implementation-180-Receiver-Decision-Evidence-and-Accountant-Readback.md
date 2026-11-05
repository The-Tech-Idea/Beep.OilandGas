# Implementation 180 — Receiver decision evidence and accountant read-back

Date: 2026-10-07. Status: the existing receipt's first final receiver decision is implemented in its native Beep owner, with verified procurement history and accountant diagnostics. Receiver write API, typed submission and record/reconcile controls remain next. This is a prerequisite slice; the receiving and purchase-to-payment features remain open.

## Canonical receipt and original intent

TheTechIdea.Data owns PurchaseReceiptDecisionCommand and PurchaseReceiptDecisionProof. PO_RECEIPT gains nullable ACCEPTED_QUANTITY and REJECTED_QUANTITY, plus nullable RECEIVER_DECISION_JSON bounded to 16000 characters. The existing PRODUCTION ProductionAccountingModuleSetup already declares PO_RECEIPT and remains the Beep MigrationManager schema owner. No handwritten SQL, business EF migration, separate decision table or client entity is added. No live migration or grant was applied.

A first final decision must partition the exact positive recorded received quantity into nonnegative accepted and rejected quantities, including full acceptance and full rejection. The active owned header, line and receipt must agree on exact recorded units. No default unit, conversion, tolerance, accepted quantity or decision is inferred from status. Received quantity, receipt status, source creator and the original physical row remain unchanged. Stock movement, warehouse/location, returns, invoice allocations and financial approval remain separate owners.

The original command contains company, receiver, order/receipt, quantities/unit, complete reviewed order revision, binding, GUID and bounded single-line reason. The proof retains the original receipt and exact result digest after the canonical common-column update handler. The result is constrained to accepted/rejected quantities and receiver change audit; other physical/audit fields must exactly match the original row. PurchaseOrderSourceReview includes all three added fields in its content revision, invalidating stale AP proposals and purchase reviews when receiving evidence changes.

## Existing native mutation owner

PurchaseOrderService is extended as a partial class. RecordReceiptDecisionAsync freezes inputs before awaits and requires a caller-supplied independent original company/receiver write-grant and binding guard. The public write endpoint has not yet been wired; this callback is not a replacement for standard API roles and current company permissions. Existing read routes retain their current app-role/Procurement.View checks.

The operation uses the existing PPDMGenericRepository and PPDMDataSourceTransaction, requires an owned verifiable Serializable transaction on the original physical PRODUCTION source, refreshes migrated nullable decimal/proof metadata, and rechecks authority/binding around native reads and writes. It reads the receipt key globally to refuse foreign or duplicate identity hidden by an order predicate. A narrow existing repository policy grants only the exact original receipt/order read needed by its update check, validates unchanged previous content, and builds proof after common-column preparation and before the SDK update.

Before writing and before commit, fresh complete order evidence must reproduce the original revision. On exact retry, only the verified original receipt is substituted into the fresh order snapshot to reconstruct the original review. Another command, changed reason/quantity/actor/intent, damaged proof, physical-row drift or changes to other reviewed order sources refuse without repair or replacement. Corrupt updates, failed SDK writes, closing changes and revoked authority roll back. Unknown commit retains the recorded original outcome; an unchanged retry reads it without another update. Exact durable row content and authority must be confirmed after transaction release before returning success.

Only the first final decision is supported here. Independent correction, return history and new-circuit recovery need explicit subsequent commands and evidence; they cannot overwrite this proof through retry.

## Procurement screen and accountant handoff

The existing PurchaseOrderWorkspaceState projects a verified receiver decision into its existing receipt history. The existing Mud receipt panel shows accepted and rejected quantity, unit, decision actor/time and reason. Missing or altered proof shows an explicit unavailable/unconfirmed decision and never exposes raw acceptance fields as verified. The status/received-quantity explanation remains explicit. There is no additional workspace store, CSS, JavaScript or theme source.

APInvoiceFinancialReview consumes the same canonical receipt proof. Its acceptance check compares each current active invoice line/receipt allocation group with that receipt's accepted quantity in exact recorded units, requiring active owned order/line/receipt. Excess allocation is a recorded difference; missing or altered proof and unit/activity uncertainty are unavailable. This diagnostic covers this invoice line's allocations, not complete consumption by other invoices. The existing separate receipt-consumption owner is still required for global capacity. Company matching policy, book/period/journal linkage and independent financial decision remain unavailable; acceptance does not approve, post or pay an invoice.

## Focused verification

Actual Data source build: zero warnings/errors, 2.24 seconds. Log: %TEMP%/oilgas-receiving-180-data.log.

Scripts/Test-PurchaseOrderOwnership.ps1 with actual Data and -IncludeWorkflowInbox: **943 passed, zero failed/skipped, terminal exit 0, 29.4096 seconds**. Log: %TEMP%/oilgas-receiving-180-final.log. Fifty new cases extend the prior 893-case gate; counts overlap prior slices and are not added.

New cases cover full/partial/rejected receipt decisions, source/audit preservation, copied output, content revision changes, original retries, invalid partition/reason/actor/GUID/overflow, changed sources/owner/unit/activity, replaced intent or altered proof, duplicate receipt identity, schema bounds/nullability/type, borrowed or insufficient transactions, SDK write/corrupt outcome/closing binding/source/authority rollback, uncertain commit and missing durable read-back. The actual native allocation and receiving writers feed fresh AP financial diagnostics, and the actual procurement state and Mud history render verified/unconfirmed evidence. These render tests are static component evidence rather than browser interaction.

The smaller source composition passes **517 checks**, zero failed/skipped, terminal exit 0, 7.5946 seconds. Log: %TEMP%/oilgas-receiving-180-source.log. It compiles the actual new native owner; its workflow-only native receiving tests run in the larger composition. The counts overlap.

The larger composition retains 333 warnings, including three new current-source precedence warnings from the receipt owner's existing transaction/repository types. No new changed-source nullable or other compiler warning is reported. Provider metadata/isolation/durability and authority are explicit fixtures. Real host/DI/OIDC, live MigrationManager application/provider concurrency, browser/keyboard/RTL/accessibility and durable recovery are not claimed. Whitespace checks pass.

## Next delivery and remaining roadmap

Wire original receiver submission and exact-intent reconciliation through the existing PO API, complete accounting client and procurement receipt screen. Independently enforce current standard app/company roles, a separate explicit receiver write grant, original account/company/binding and unchanged authority. Retain unresolved intent across page/context changes, refuse late success and refresh verified whole-order evidence before confirmation. Use the existing company process/inbox handoff to expose receipt evidence to accountants; do not infer process completion from this physical decision.

Independent correction/returns, stock/valuation, matching/tax/rounding policy, ledger linkage, financial decisions/post/payment, durable recovery/outbox, configured routing/delegation, wider dashboards and personas, multinational requirements and live host/browser/provider verification remain active. No phase or feature closes; no commit, push or deployment was performed.
