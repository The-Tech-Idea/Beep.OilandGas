# Implementation 189 - Accountant invoice variance preparation

Date: 2026-10-07. Status: implemented and focused-verified preparation through Data/API/client/workspace/complete invoice screen. Durable proposal recording and independent controller decision remain incomplete. No persona phase is closed.

## Accountant workflow outcome

The existing invoice detail page now offers explicit variance preparation after complete financial evidence is loaded. The accountant chooses request independent variance review or request source correction, explains every confirmed recorded difference exactly once, and supplies an overall reason. The API refreshes the entire invoice/purchase/allocation/global-receipt/policy capture under current app-owned company authority before returning a frozen preparation. The screen displays exact disposition, explanations, account, evidence/preparation/binding revisions and UTC retrieval time, with unavailable diagnostic count.

The UI explicitly labels this as unrecorded preparation. It does not create a controller task or report a durable proposal, independent financial decision, posting or payment. These are the next implementation requirements, not inferred success from positive diagnostics.

## Canonical contracts and authority

APInvoiceVarianceProposal, APInvoiceVarianceExplanation and APInvoiceVarianceProposalReview live only in TheTechIdea.Data. The proposal references exact invoice and complete financial revisions, disposition, explanation codes/reasons and overall reason. Validation requires an active draft, confirmed active source lines, complete creation audit and an account independent of the invoice source maker. Matching policy history and receipt consumption must have been explicitly read. Every confirmed RecordedDifference/ExceedsRecordedLimit diagnostic must have exactly one explanation; unknown evidence cannot be asserted as a known variance. Reasons are explicit, bounded and control-free; unsupported disposition, empty/double/missing/unknown explanations and changed revisions refuse.

Request variance review additionally requires a verified effective policy. Request correction may retain unavailable policy evidence while describing only confirmed differences; no approved policy or zero tax is fabricated. Preparation freezes full evidence and input; revision includes original company/account/invoice, financial evidence, binding and proposal. Require validates the complete original actor, company, invoice, proposal and content.

The existing APController exposes POST `{id}/variance-proposal-review`, a read-only preparation route. It requires the new explicit Accounting.PrepareInvoiceVariance grant plus existing Accounting.View and, for purchase sources, Procurement.View, with current assigned standard ASP.NET app roles. Each native read boundary retains original account/company/binding/roles/permissions; the existing financial reader double-captures complete evidence. A changed invoice, policy or global receipt contributor set prevents adoption. No grant assignment or IdentityServer authorization change is made.

## Existing client and screen owners

AccountingServiceClient gains a partial extending the existing typed owner; no alternate client exists. It freezes outgoing proposals, validates the selected original company/account and incoming full review, and refuses company changes during the call.

The existing scoped APInvoiceWorkspaceState owns preparation and recorded-difference projection. Financial invalidation clears preparation; input changes, source/company changes, disposal and failed reloads also clear it or fence late replies. Unconfirmed allocation/policy intents and active source commands prevent preparation. Returned evidence is copied so caller mutation cannot change stored workspace state.

PayablesWorkbench uses MudBlazor inputs with explicit empty initial selections/reasons, per-difference explanations and a fresh standard-role/grant check before calling the owner/API. It displays clear unrecorded preparation and remaining evidence status. Authorization refusal clears stale preparation/financial evidence and leaves a visible error even when controls disappear. Reload/company change resets inputs and permissions. No custom CSS/JS or client database access is added.

## Focused verification

Actual Data build: zero warnings/errors, 1.82 seconds; log `%TEMP%/oilgas-variance-189-data.log`.

Final affected gate: **1190 passed, zero failed/skipped, terminal exit 0**, 36.6225 seconds; log `%TEMP%/oilgas-variance-189-final.log`. Smaller source gate: **519 passed, zero failed/skipped, terminal exit 0**, 8.5712 seconds; log `%TEMP%/oilgas-variance-189-source.log`. Counts overlap. Composition has 351 warnings, including one additional PermissionConstants cached-assembly/source conflict from the new API grant reference. Actual Data has no warnings. Whitespace checks pass.

Adds 32 cases over the previous 1158: exact known-difference coverage and bounded reasons/disposition; source-maker/audit/activity/status/policy/consumption refusal; frozen evidence and actor/input/content tampering; no variance manufactured from agreeing evidence; correction retaining unknown policy; protected workspace state and failed reload; actual API/client success, lost grant/role, changed global history, binding drift and maker conflict; late company/financial/input/disposal fences; and complete invoice-page handlers with explicit input, changed-reason invalidation/re-review and current server-grant refusal. Native fixture writes remain exactly two policy lifecycle commands; preparation issues no invoice/allocation/financial writes and invoice status stays DRAFT.

The first successful gate passed 1187; three added preparation/workspace cases brought the checkpoint to 1190. Final rerun keeps 1190 and verifies error visibility after authority refusal. Test physical datasource/provider metadata/durability, host/OIDC/DI and JS/browser remain seams; production provider concurrency, browser and durable command acceptance are unverified. No broad missing-project solution build is claimed.

## Next full-roadmap work

Implement durable original-intent accountant proposal submission and independent controller disposition through existing AP invoice/source and lifecycle task owners. The command must revalidate exact full evidence under current grants, enforce independent preparer/reviewer, transaction/read-back/idempotency and retained-intent reconciliation, and create/review the correct persona handoff. Resolve the authoritative storage/history design before recording financial decisions; preview alone is not durable approval.

Separate tax/non-PO evidence, book/period/journal ownership and financial posting/payment, policy task/retirement/correction, lifetime/new-circuit recovery, stock/returns, broader persona dashboards, multinational requirements and live release gates remain open.

Beep MigrationManager remains the business schema owner. This slice adds no business schema, SQL, business EF lane, live migration/grant, commit, push or deployment.
