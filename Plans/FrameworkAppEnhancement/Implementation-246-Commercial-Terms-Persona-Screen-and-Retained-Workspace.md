# Implementation 246 — Commercial persona terms screen and retained intent workspace

## Visible screen and data workflow

Each recorded revenue row now offers `Prepare or review commercial terms`. It opens CommercialTermsWorkbench inside the existing revenue source page with that exact FIELD, captured date window and transaction. This complements the recorded contract review rather than adding another company or source selector. Existing commercial/revenue landing navigation reaches the same source page.

The MudBlazor panel shows company/FIELD, transaction/contract, seller/buyer and source revision; native recorded price/currency/pricing method and missing pricing basis; native effective/expiry facts and contract/revenue maker provenance. Missing native values remain unavailable. A recorded preparation shows proposed version, stored-calendar effective dates, explicit open end when applicable, decimal price/currency, unit/basis, pricing method, notes, preparer UTC and revision. A recorded decision shows Approve/Reject, distinct reviewer, UTC and reason. Razor encodes recorded text, and decimal display uses current culture. No FX or unit conversion is performed.

An account with the app-owned preparation grant sees a blank explicit form for version, date window, currency, nullable entered price, price basis, method and notes. Required fields and maximum lengths match canonical limits; no price/date/unit/version is invented from defaults. Submission validates the canonical terms before constructing an intent. A separately authorized independent reviewer sees reason plus approve/reject actions against the exact undecided recorded preparation. Missing maker evidence or same-maker identity suppresses decision controls. Already-decided evidence shows its recorded outcome without offering another decision.

The panel states that invoice/recognition, amendments and assigned-task completion remain separate. Approval here records commercial evidence and does not rewrite original native source prices. The existing native/API schema, source, maker and transactional constraints remain authoritative.

## Standard access and lifecycle

Before loading or acting, the component verifies authentication, selected account identity, a standard ASP.NET role matching the selected app-owned role set, Accounting.View and the existing current FIELD selector matching its source parameters. Preparation uses the selected ProductionAccounting.Adjust grant; decision uses ProductionAccounting.Approve plus the canonical factual maker-separation checks. These UI controls are advisory; the actual endpoints independently resolve and enforce current authority and FIELD READ/WRITE.

Company/FIELD/authentication/source-parameter changes clear visible evidence, errors, grant flags and editable drafts. State generation and company snapshot fences discard late data and acknowledgements; component generation fences prevent old access/error/action continuations from restoring a prior display. Unchanged parent parameters no longer reload or clear an in-progress form on an unrelated parent render. The scoped state is owned/disposed by the Web circuit; the panel removes its own subscriptions on disposal and invalidates its attached source without deleting an uncertain command.

No page-local palette, custom CSS/JavaScript, direct frontend database access, IdentityServer authorization or duplicate domain model is introduced. Production Web registers CommercialTermsWorkspaceState as scoped. All business operations use the existing typed accounting client and canonical Data contracts.

## Retained intent and exact confirmation

CommercialTermsWorkspaceState retains one copied pending preparation or decision for its circuit. A fresh valid source is required before creating a command. Once a command is pending, creating another preparation or decision is blocked across context/source changes. Clear/invalidate/dispose of the view removes displayed data while retaining uncertain intent in the circuit-owned state.

After command acknowledgement, the workspace reloads exact terms through the typed API and validates the native recorded read review. It clears pending state only when the recorded preparation or decision exactly matches the original intent and original preparation/time. A transport error, different recorded intent, unavailable read-back or late context response keeps the pending command and clears the current display. A later reload can confirm a lost response without reissuing a write. When native read-back proves no preparation was recorded, the workspace retries the same original command UUID and frozen terms. Decision retries similarly preserve the original preparation/decision, using the endpoint's native idempotent reconciliation.

Retained command identity and terms/reason are shown only in the original selected company/account/source with a valid current review. Other contexts see a generic pending warning and cannot retry against another source. Retry is disabled until the original matching source has been reloaded. Recorded preparation/decision views are returned as copies so external presentation code cannot alter state evidence.

Retention is in the active Blazor circuit, not durable recovery across browser restart or circuit loss. Changed historical source/native authority can still require reconciliation beyond the current exact-source API. Amendment/history, durable cross-circuit intent storage and historical recovery remain open acceptance work; this screen does not claim those paths.

## Focused verification

One hundred twenty-nine focused tests pass with zero failures/skips. The gate compiles the exact scoped state, new Razor panel and modified revenue page with the configured MudBlazor version against the actual canonical Data assembly. The existing canonical Data build from Implementation 245 remains unchanged and clean. Cached source/dependency composition warnings remain; full host build is not claimed.

Sixteen new cases comprise eight retained-state cases, six direct HtmlRenderer panel cases, one parent revenue-row opening case and one actual state/client/Kestrel/native journey. State cases cover successful prepare/decision, failure before recording followed by same-UUID retry, lost preparation/decision response confirmed by native read without another command, late company response followed by original-context recovery, changed recorded intent refusal and failed post-command read-back recovery. Rendered cases verify explicit blank preparation fields and price basis, independent approve/reject controls with proposed facts/audit, same-maker suppression, recorded outcome display and denied/failed evidence states. The parent-page handler opens the exact transaction panel and calls its current typed review.

The full retained-state journey uses actual typed client, authenticated local Kestrel controllers and native Beep transaction fixtures. Distinct preparer/reviewer accounts prepare, read back, approve and read back through the new workspace, and exact retries add no write. The preceding 113 canonical/native/API/SDK/source-rendered tests remain green. These checks prove the scoped code/state/render behavior; they do not exercise physical browser input, MudForm keyboard/focus behavior, live OIDC/provider/isolation, concurrency/performance or RTL/accessibility acceptance.

Reproduce with Scripts/Test-PurchaseOrderOwnership.ps1, actual canonical Data DLL, IncludeWorkflowInbox and FullyQualifiedName~FieldRevenueSourceTests. Native writes occur only in focused fixtures.

## Next execution and phase status

Route real commercial preparation/decision work through existing configured persona process/task owners. Extend their source ownership and reads/commands coherently beyond the currently supported AP_INVOICE/PURCHASE_ORDER company types, preserve maker separation and exact native outcome/task linkage, then verify persona inbox handoff and task completion against persisted terms. Continue amendment/history and durable recovery, invoice/recognition, royalty/partner queues and wider thirteen-persona/multinational/native/provider/browser work.

No handwritten SQL, business EF lane, live schema/business write, permission assignment, deployment or commit/push is executed. No CA/FG phase or overall implementation goal closes.
