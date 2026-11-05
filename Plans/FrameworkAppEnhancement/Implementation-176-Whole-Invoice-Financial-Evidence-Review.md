# Implementation 176 — Whole invoice financial evidence review

Date: 2026-10-06. Status: whole invoice/source diagnostics are implemented on the actual API/typed client/state/source page and pass focused verification. This is preparer/controller evidence review; durable financial decisions and writes remain open.

## Why the source gate is required

The legacy AP ReceiveBillAsync method posts using fallback accounts and a date substitution, with separate journal and invoice writes. It lacks the reviewed whole-invoice, independent financial decision, book/period and atomic receipt-consumption gate required for the company journey. This slice does not expose that method as a financial action. The existing legacy creation route remains fenced.

The Data-owned APInvoiceFinancialReview aggregates the existing APInvoiceSourceReview and exact APReceiptAllocationReview for every recorded PO-linked invoice line. It validates the complete linked line set, owned supplier/order/receipt membership, actor/company and displayed invoice revision. A missing or duplicate linked line cannot become a successful partial review. Reviews are bounded to 128 invoice lines; larger sources refuse instead of being truncated.

## Recorded diagnostics

The same Data owner derives recorded comparisons for:
- Header and line activity, draft status and the original unmodified header/line maker versus the current account.
- Active line amount sum versus header amount only when line activity, amounts and exact currency agree.
- Per-line exact quantity-times-price amount; arithmetic overflow, missing quantities/prices/currency and unknown activity stay unavailable.
- Owned PO versus invoice unit/currency/price; no conversion or tolerance is applied.
- Active recorded allocation quantity versus the invoice line quantity; this is not accepted quantity or global receipt availability.
- Missing non-PO treatment, authoritative receiver acceptance, company matching policy, invoice-to-book/period/journal evidence and durable independent financial decision.

A recorded agreement grants no matching, review, posting or payment authority. Header totals are comparisons of recorded values, not a tax/rounding/ledger calculation. Maker comparison is source provenance, not a new permission or an assertion that another person's app role was granted. Acceptance/policy/ledger/decision remain explicitly unavailable rather than fabricated from workflow completion or a Post step name.

## Actual API and screen

The existing AP controller exposes the financial-review read for one encoded invoice and its exact displayed revision. APInvoiceReadChecks assembles owned PO/receipt/allocation evidence through the existing read owners, captures the complete whole-invoice review twice and requires the same aggregate revision. Current app role/permission sets, original company/actor and PRODUCTION binding remain stable across the read. PO-linked lines require independent Procurement.View through the existing owned source guard.

The complete AccountingServiceClient validates the original selected company/account, exact review and late response. The focused composition now compiles this entire source class rather than substituting its methods. APInvoiceWorkspaceState freezes the returned review before final validation, protects consumer snapshots and invalidates evidence on company/source changes or failed reload.

PayablesWorkbench invokes the actual typed method from its bound whole-invoice review handler. Its existing source panel displays recorded checks, explicit missing financial evidence, reviewed revisions/time and owned PO drill-downs. Authorization failure clears the previously displayed source and preparation capabilities. Source/schema drift clears the financial review and requires reload. No financial write, approval control, automatic role/grant or alternate source store is introduced.

## Focused verification

Actual Data source build: zero warnings/errors, 1.65 seconds. Log: %TEMP%/oilgas-invoice-financial-176-data-final.log.

Final actual-source affected gate: Scripts/Test-PurchaseOrderOwnership.ps1 with actual Data and -IncludeWorkflowInbox. **802 passed, zero failed/skipped, terminal exit 0, 20.0081 seconds**. Log: %TEMP%/oilgas-invoice-financial-176-final.log.

Thirty added cases cover twelve exact diagnostic inputs; four partial/duplicate/revision/actor refusals; two late company/source invalidations; immutable state snapshots and failed reload; eight actual HTTP/complete typed-client cases including encoded identities, stale source/revision, provider failure, standard role/procurement/binding/role-set changes; and three complete invoice-parent handler renders through the actual typed client/API. HTTP and page tests explicitly confirm no production datasource insert/update/delete/transaction/commit is invoked.

The first complete AccountingServiceClient expansion passed 772 checks; the contract/state expansion passed 791 and the HTTP/page expansion passed 802 before the final source run. Counts overlap previous gates and are not added. The expanded composition retains 319 existing dependency/source-precedence/nullable warnings; no new changed-source warning is reported. Whitespace checks pass.

Actual Data, complete client/state/source page/panel, AP controller/read checks and AP/PO/allocation owners compile and execute. Physical provider rows/schema, current account/module resolution and diagnostics remain explicit seams; existing app SQLite RBAC/configuration checks remain included. The repeated sources are stable captures, not one atomic PRODUCTION snapshot. Browser/keyboard/RTL/accessibility, production host/DI/OIDC and real provider concurrency/durability remain unverified.

## Next delivery and remaining plan

Add durable reviewed receipt-allocation preparation from the existing proposal on the current invoice-line screen, using the actual PRODUCTION Beep transaction and all receipt contributors. Keep allocation recording distinct from receiver acceptance and independent financial matching approval.

Authoritative receiver acceptance, configured company matching/tax/rounding policy, owned book/period/journal linkage, independent invoice decision/post/payment, durable browser/new-circuit/historical recovery/outbox, configured routing/delegation, broader dashboards/personas and multinational requirements remain active. Beep MigrationManager remains the business schema owner. No handwritten SQL, new business EF lane, schema change, live migration/grant, deployment or feature/phase closure.

