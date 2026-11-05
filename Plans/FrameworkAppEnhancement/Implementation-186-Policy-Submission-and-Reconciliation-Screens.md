# Implementation 186 — Policy submission and reconciliation screens

Date: 2026-10-07. Persona priority: accountant policy preparation and independent controller approval from the existing payables landing/detail workspace. Feature 04 and Persona 01. The policy command API/native prerequisites from Implementation 185 now have actual screen handlers and retained scoped intent. Effective policy selection, invoice variance decisions and policy-specific task routing remain open. No phase closes.

## Existing workspace owner

APInvoiceWorkspaceState.MatchingPolicy extends the existing circuit-scoped AP workspace. Bound command review and full history are frozen before display. Controller selection exposes only verified active drafts prepared by another account; legacy/malformed/self-prepared records remain unavailable for selection. Public review/intent/result/proof projections are copied.

Submission freezes the original business identity, action, rules/dates, history/binding revisions, intent and reason before calling the existing typed accounting client. Approval derives exact rules/dates from the selected draft rather than accepting replacement inputs. Busy state prevents command replacement. Unknown outcome retains the original submission; fresh action review cannot replace it. Reconciliation resubmits that exact original intent and ignores later form input.

Success requires the returned command proof and a fresh complete company history containing the exact recorded row. Reconstructing the original history from the proven own row/draft must reproduce the reviewed command revision. Missing/changed/failed history remains unconfirmed. Company/account/source clear, page recreation and late responses cannot publish success in a different context. Confirmed intent is retired on display clear; uncertain intent remains in the existing circuit owner. Durable new-circuit/lifetime recovery remains open.

## Actual accountant/controller screens

PayablesWorkbench adds explicit draft preparation/independent approval review buttons governed by current assigned company roles and action grants. Both landing and invoice detail reuse the same workspace. Draft inputs require business identity/version/currency, nullable quantity/price percentages, full UTC effective instant, optional explicit expiry, selected unit/currency/tax/rounding/non-PO rules and reason. No tolerance, currency, rule, date or reason is prefilled. Rules expose only the currently supported explicit choices.

UTC input accepts complete ISO 8601 date/time ending in Z, with optional fractional seconds. Partial time, date-only and offset inputs refuse; a current date or local timezone cannot be inferred. The controller selects a verified independently prepared draft and reviews its recorded rules/dates in company history; approval cannot edit them. The original preparation/review reason is frozen with the intent.

Handlers freshly check authentication, standard app-assigned roles, Accounting.View and the action-specific prepare/approve grant. API/native authorization remains independent. Loading/busy controls prevent concurrent actions, and labelled progress is shown. Pre-submission validation errors leave no uncertain intent; failures after intent creation preserve it and show original identity/action/reason plus a reconciliation action in its original company/account. Page disposal clears visible evidence and unsubscribes handlers, retaining unresolved intent in the scoped owner. No authorization moves to IdentityServer.

APInvoiceSourcePanel adds verified preparer or independent reviewer/original preparer/review reason to the existing history. Missing or invalid proof is explicitly unavailable. Confirmation distinguishes policy recording/approval from invoice matching, posting or payment; no policy is selected or tolerance applied yet. Existing MudBlazor/theme conventions are reused; no CSS/JS or direct frontend database access is added.

## Focused verification

Actual Data build: zero warnings/errors, 1.39 seconds; log %TEMP%/oilgas-policy-186-data.log.

Final affected gate: 1112 passed, zero failed/skipped, terminal exit 0, 38.4945 seconds; log %TEMP%/oilgas-policy-186-final.log. This adds 24 cases to the previous 1088 (11 workspace and 13 complete-page/native API cases). Smaller source gate: 519 passed, zero failed/skipped, terminal exit 0, 12.2290 seconds; log %TEMP%/oilgas-policy-186-source.log. Counts overlap. The affected composition retains 350 warnings, with no new edited policy-screen/state compiler warning. Whitespace checks pass.

Checks cover actual native API/client/page preparation, landing/detail, independent approval, retained draft/approval intent after unknown commit, page disposal/recreation and exact retry without another write; failed/missing/changed complete history; late company/clear/disposal responses; busy command replacement; copied projections; unverified/self-prepared selection; empty/reason/UTC/partial-time/offset input refusal; and post-review API permission loss. Invoice status remains DRAFT and no allocation is recorded by policy actions.

Initial compilation exposed the cached Models ACCOUNTING_METHOD alongside canonical Data when a Razor helper named the entity directly. Proof projection now belongs to the existing C# workspace owner, using the canonical type rather than a Razor entity signature. A missing Razor Data import was fixed. Corrected workspace checkpoint passed 1099; initial complete-page checkpoint passed 1108; final checks include strict UTC and page-recreation/uncertain-approval cases. Physical datasource/isolation/durability/metadata, authentication, JS/browser and adjacent unselected persona context remain explicit test seams. Full host/DI/OIDC/browser/keyboard/RTL/accessibility and live provider/concurrency remain unverified.

## Next delivery and wider roadmap

Select a verified independently approved effective policy explicitly from complete company history for the recorded invoice currency and UTC date, with no latest-row or ambiguous-window fallback. Connect that evidence to accountant quantity/price/unit/currency/tax/rounding/non-PO variance review and independent controller financial decisions through existing source/task owners. Policy approval alone must not mark an invoice matched or authorize journal posting/payment.

Policy-specific task routing, retirement/correction, durable lifetime/new-circuit recovery, stock/returns, book/period/journal linkage, other persona dashboards, multinational requirements and live release gates remain active. Beep MigrationManager remains the schema owner; this slice adds no business schema, SQL, business EF lane, live migration/grant, commit, push or deployment.
