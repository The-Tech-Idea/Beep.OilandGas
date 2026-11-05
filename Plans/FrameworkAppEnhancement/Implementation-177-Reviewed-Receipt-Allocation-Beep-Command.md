# Implementation 177 — Reviewed receipt allocation Beep command

Date: 2026-10-06. Status: native command and Data-owned intent/proof implemented and focused verified. API write authorization, typed submission and invoice-line screen intent/reconciliation remain open. This is a prerequisite to the accountant's existing receipt-allocation screen, not completion of the persona feature.

## Original preparer intent

APReceiptAllocationCommand lives in TheTechIdea.Data. It retains the exact company, current account, invoice/line, receipt/quantity/unit proposal, invoice/order/allocation/contributor/proposal revisions, original binding, GUID intent and explicit bounded reason. The command validates its original proposal before awaiting providers. The service freezes both inputs before upstream calls.

Recording requires an active draft invoice and active owned invoice/order lines, exact recorded units, and positive proposal quantity within both the invoice-line and receipt limits. Receipt consumption includes every owned contributing invoice, including inactive allocation history. Missing or ambiguous activity/units and changed contributor evidence refuse arithmetic or writes. Currency/price differences stay visible financial evidence; recording does not resolve them or authorize acceptance, matching, posting or payment.

## Existing native owner and transaction

APInvoiceService is extended as a partial class; it continues using the existing request-bound PRODUCTION resolver, AP/PO source owners, PPDMGenericRepository and PPDMDataSourceTransaction. The caller must supply fresh independent write authority for the original company/account/binding and canonical owned purchase reads. No API endpoint exposes the new command in this slice.

The command requires the same open physical datasource and verifiable owned Serializable transaction. It reconstructs the complete proposal from fresh invoice, purchase, line allocations and all receipt contributors before insertion and before commit. Closing evidence drift or authority/binding loss rolls back. The existing Beep common-column handler runs before the narrow insertion policy creates the durable row proof. No parallel store, handwritten SQL or business EF migration is introduced.

Stable allocation identity derives from company/account/original intent. An exact unchanged retry reads the existing allocation and verifies its original proof and full canonical row content, then reconstructs the original review while excluding only that exact proven row. It never excludes another contributor. Changed source/contributors, reason, activity, actor, quantity, proof or audit fields refuse confirmation and repair. An uncertain commit returns no success; an unchanged original retry can confirm the recorded outcome without another insert.

The exact row is read back before commit and after transaction release. A successful provider commit flag with missing durable row evidence cannot report success. Authority loss after commit leaves an unconfirmed result for original-intent reconciliation rather than authorizing another allocation.

## Beep schema evolution

The existing canonical AP_RECEIPT_ALLOCATION gains nullable REVIEW_COMMAND_JSON with MaxLength(4000). It stores the bounded original command and exact post-common-column row revision; REMARK retains the preparer's human reason. Historical rows can remain without proof and remain readable, but cannot be adopted as this command's outcome. Allocation content revisions include the new proof field.

ProductionAccountingModuleSetup already declares the canonical allocation type in its explicit PRODUCTION entity list. MigrationManager remains the schema owner. The new command checks fresh native metadata for a unique string proof column of sufficient size and refuses an unmigrated schema. Focused ClassCreator checks verify the canonical 4000-character metadata. No migration plan was executed against a live provider; actual target planning, column evolution, isolation, predicate protection and durability still require provider verification.

## Focused verification

Actual Data source build: zero warnings/errors, 1.72 seconds. Log: %TEMP%/oilgas-allocation-177-data.log.

Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox: **844 passed, zero failed/skipped, terminal exit 0, 19.3875 seconds**. Log: %TEMP%/oilgas-allocation-177-final.log. Forty-two new native/contract cases extend the prior 802-case gate; counts overlap prior slices and are not added.

Cases cover original intent, exact retry and immutable returned evidence; original invoice/order/receipt/contributor drift; quantity and unit limits; altered recorded proof/quantity/activity/actor/audit/owner/reason; original-intent replacement; insertion corruption and closing competitor rollback; uncertain commit and durable-row absence; Serializable and borrowed-transaction requirements; fresh schema bounds/uniqueness; contributor evidence from another invoice; preservation of unresolved financial differences; changed binding; and permission loss before insertion, after insertion and after commit.

The gate compiles the actual AP/PO services, repository, transaction owner and shared CommonColumnHandler source. Physical datasource, metadata repository, current authorization and provider transaction behavior remain explicit fixtures. The expanded composition reports 323 warnings: 319 existing warnings plus four source-precedence warnings selecting current transaction/repository/common-column sources over cached assemblies. No new nullable or other changed-source compiler warning is reported. Whitespace checks pass.

## Next delivery

Add an independently enforced allocation-recording grant and original company/account/role/permission/binding checks to the existing AP API. Do not turn Accounting.PrepareReceiptAllocation, which currently authorizes read-only proposal review, into an implicit write grant. No automatic live grants.

Wire the complete accounting typed client and existing invoice-line screen with retained original intent, explicit submission and reconciliation, failed/unknown outcomes, exact durable receipt validation, and source/financial review refresh. Avoid repeated clicks creating new intents while an outcome is unknown. Confirm these through actual API/client/state/parent-page tests before claiming user-facing allocation recording.

Receiver acceptance, independent financial decisions, company matching/tax/rounding policy, book/period/journal linkage, posting/payment, durable browser/new-circuit recovery, configured routing/delegation, broader dashboards/personas, multinational requirements and live host/browser/provider checks remain active. No feature or phase closes.
