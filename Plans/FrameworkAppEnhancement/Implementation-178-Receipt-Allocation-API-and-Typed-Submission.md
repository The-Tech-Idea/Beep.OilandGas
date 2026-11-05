# Implementation 178 — Receipt allocation API and typed submission

Date: 2026-10-07. Status: the existing native command is connected to the actual AP API and complete accounting typed client, with focused HTTP verification. Invoice-line submission controls and retained circuit intent/reconciliation remain next; the full persona feature is not complete.

## Independent recording authority

The existing AP controller exposes POST api/accounting/traditional/ap/{invoice}/lines/{line}/receipt-allocations. Its existing route ID parser preserves exact encoded identities; route/body mismatches refuse before mutation.

Accounting.RecordReceiptAllocation is a separate explicit app permission. Accounting.PrepareReceiptAllocation remains read-only. Recording requires current Accounting.View, Procurement.View and RecordReceiptAllocation with standard authenticated ASP.NET company roles admitted by the existing AP/PO checks. No grant is automatic. A valid original intent may be reconciled with recording authority even when preparation authority is absent.

The endpoint freezes the request and checks exact original company, authenticated account and PRODUCTION binding. Current role/permission sets are captured and rechecked throughout the native transaction and after commit. Drift refuses confirmation. Repository Forbidden refusals return 403, including revocation inside the transaction. Provider/schema/evidence conflicts and uncertain transaction outcomes return fixed original-intent reconciliation guidance without exposing exception text.

CompanyModuleConnection remains the production context owner, refreshing repository access and fingerprinting tenant-owned module bindings and the configured physical target. No alternative authorization or connection resolver is introduced.

## Canonical reviewed input and outcome

APReceiptAllocationProposalReview includes BindingRevision in its content revision. The actual proposal endpoint returns the reviewed PRODUCTION fingerprint. Unbound diagnostic reviews remain readable, but a recording command requires the exact displayed nonempty binding. The native closing review reproduces it.

Data-owned APReceiptAllocationSubmission carries the original command and displayed proposal. APReceiptAllocationReceipt freezes the stored allocation and validates original intent, owned invoice/line/order/receipt membership, quantity/unit/activity, audit provenance, reason, stable GUID and durable row proof. The native owner delegates outcome validation to this shared contract rather than retaining another validator. Acceptance and financial approval/post/payment remain separate and unresolved.

The endpoint calls the existing AP native command with fresh authority and canonical PurchaseOrderService reads. Exact retry reads the same allocation without another insert. An unconfirmed commit does not report success; the unchanged original intent can reconcile its recorded outcome.

## Complete accounting client

AccountingServiceClient.RecordAPReceiptAllocationAsync freezes and validates input before transport, requires the original selected company/account, encodes exact IDs, invokes the API and validates the complete receipt. Existing CompanyClientContext owns request headers, selection cancellation and late response rejection. A company/account change after commit cannot adopt the result as current success.

Wrong returned intent, order, quantity, proof, confirmation time or missing allocation is refused. Tests include internally consistent but incorrect row proofs, proving source/outcome validation beyond a hash check. The upcoming screen must retain unknown outcomes for original-intent recovery.

## Focused verification

Actual Data build: zero warnings/errors, 1.82 seconds. Log: %TEMP%/oilgas-allocation-178-data.log.

Scripts/Test-PurchaseOrderOwnership.ps1 with actual Data and -IncludeWorkflowInbox: **873 passed, zero failed/skipped, terminal exit 0, 22.1211 seconds**. Log: %TEMP%/oilgas-allocation-178-final.log. Twenty-nine new HTTP/complete-client cases extend the prior 844-case gate; counts overlap and are not added.

New cases cover bound proposal evidence, encoded invoice/line source journey, recording/exact retry; authentication/standard-role/read/write/preparation-only denial; original company/account/binding, source/contributor/proposal/route refusal; transaction rollback on role/permission/binding and repository grant changes; uncertain commit and typed retry; late company/account cancellation; wrong initial selection without transport; independent recording authority; and incorrect/incomplete returned receipt evidence.

The actual controller, full typed client, Data contracts, AP/PO services, repository, transaction owner and common-column handler compile and execute. HTTP tests use actual native allocation insertion/read-back/rollback with explicit physical-provider fixtures. A post-registration test seam selects these accounting owners; earlier source-only tests keep their existing owners. Current account/module/access resolution and physical provider metadata/transactions remain seams. Production context source was inspected; live host/DI/OIDC and real provider execution remain unverified.

The composition reports 328 warnings: prior 323 plus five current-source precedence warnings from permission/refusal references. No new nullable or other changed-source compiler warning is reported. Whitespace checks pass. No SQL, business EF migration, live migration/grant, commit, push or deployment was performed.

## Next delivery

Wire the existing invoice-line screen with retained original command, explicit reason/submission, duplicate-click protection, unknown outcome and reconciliation. Validate the API receipt and reload owned allocation/contributor/whole-invoice evidence before showing refreshed diagnostics. Context/source changes must prevent adopting late success while preserving unresolved intent for recovery. Verify shared state and complete parent-page handlers.

Receiver acceptance, independent financial decisions, matching/tax/rounding policy, book/period/journal linkage, posting/payment, durable browser/new-circuit recovery, routing/delegation, wider dashboards/personas, multinational requirements and live host/browser/provider evidence remain active. Beep MigrationManager remains the business schema owner. No feature or phase closes.
