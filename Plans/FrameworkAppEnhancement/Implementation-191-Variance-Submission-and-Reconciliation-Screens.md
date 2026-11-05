# Implementation 191 - Accountant variance submission and reconciliation screen

Date: 2026-10-07. Status: scoped workspace and complete invoice-page submission/reconciliation implemented and focused-verified through the existing native API/typed client. Independent controller disposition and persona task handoff remain incomplete. No persona phase is closed.

## Accountant screen outcome

After explicit fresh variance preparation, an authorized accountant can record the reviewed proposal from the existing payables invoice page. The same AP API/client/native Beep path from Implementation 190 is used. One frozen original command, GUID intent, disposition, explanations and reason are retained after an uncertain outcome. The page shows unconfirmed original intent, its exact evidence/reason and the original invoice recovery route, and reconciles that same submission instead of preparing another.

A confirmed result requires the exact API outcome and a fresh owned invoice-source read. The source header must match the returned row, and reconstructing its verified original header with current lines must reproduce the original invoice revision. Source read failure, changed header/lines, company/source invalidation or late results cannot publish confirmation. Invoice financial status remains DRAFT; no controller disposition, task handoff, matching approval, posting or payment is inferred.

## Existing scoped owner and persisted evidence

APInvoiceWorkspaceState.Variance extends the existing scoped owner with frozen intent/result, busy/unconfirmed state, current-context recovery and command events. Complete evidence/input changes invalidate preparation. Confirmed intent is retired when its display is cleared; unknown intent survives source/company clears and page disposal/recreation within the same scoped owner. Different company/account cannot view original intent details or use recovery, and cannot replace the retained original command. New preparation and conflicting policy/allocation commands are refused while variance outcome is unsettled.

Successful record updates the existing detail projection through one shared ApplyDetail path. LoadDetail also reads the existing invoice's canonical APInvoiceVarianceProof, so reload shows recorded evidence independently of transient success state. Malformed existing JSON produces explicit unavailable verification rather than a confirmed proposal. Returned workspace evidence is copied; caller mutation cannot change retained input/result/source proof.

The canonical Data preparation now refuses any existing proposal JSON, including unverified prior evidence. This prevents preparing a replacement before the recorded proposal's lifecycle is resolved. Exact original native reconciliation still reconstructs its proven original header with null proposal JSON and validates all current evidence normally; no fallback or repair path is added.

## UI and current authority

PayablesWorkbench adds record/reconcile actions under current app-owned standard roles, Accounting.View, Accounting.PrepareInvoiceVariance and Accounting.RecordInvoiceVariance. Every handler refreshes AuthenticationState and verifies the original selected company/account before invoking the scoped owner/API; the API independently rechecks its own current roles/grants/binding on each native boundary. Inputs and conflicting controls are disabled while recording or retaining unknown intent. Missing client recording grant cannot invent a pending command; API refusal retains the original submission because refusal can also occur after commit.

The complete source panel shows verified recorded proposal preparer, requested disposition, original intent/reason/explanations, invoice/financial/preparation revisions and recorded row revision. Its notice explicitly separates recorded proposal evidence from controller disposition/task/matching/post/payment. Invalid proof is visibly unavailable. Unconfirmed retained intent suppresses the generic source handoff until reconciled, avoiding an offered stale source handoff. MudBlazor components and existing theme conventions are reused; no custom CSS/JS, database access, duplicate entity/client/repository or role authority is introduced.

## Focused verification

Actual Data build: zero warnings/errors, 1.79 seconds; log `%TEMP%/oilgas-variance-191-data.log`.

Final affected gate: **1255 passed, zero failed/skipped, terminal exit 0**, 1.1243 minutes; log `%TEMP%/oilgas-variance-191-final.log`. Smaller source gate: **519 passed, zero failed/skipped, terminal exit 0**, 8.7819 seconds; log `%TEMP%/oilgas-variance-191-source.log`. Counts overlap. Composition retains 358 warnings; Data is warning-free. Whitespace checks pass.

Adds 19 cases over the previous 1236: 12 scoped workspace cases, six complete-page/API/client/native scenarios, and one existing Data-preparation theory case refusing recorded JSON. Workspace cases verify full result/current-source confirmation, protected result/input, recorded proof reload, unknown commit/exact retry with no extra write, changed header/lines/failed read refusal, late company/source/disposal fences, missing grant before intent creation, malformed proof unavailable, and original intent privacy/recovery refusal in another company/account. Complete-page cases execute actual financial/preparation/input/record handlers, confirmed evidence, uncertain commit, exact retry, disposal/recreation with the same scoped owner, current server recording grant loss, and changed contributor refusal during reconciliation. Successful proposal/retries issue exactly one invoice update; grant refusal issues none. All retain invoice DRAFT status.

Workspace checkpoint passed 1246. Initial screen composition required casting the captured IComponent to IDisposable for the real page-disposal test; this was repaired before execution. Screen checkpoint passed 1252. Final 1255 adds two context-switch cases and recorded-proof preparation refusal. Page recreation is within one existing scoped owner, not verified lifetime/new-circuit recovery. Physical provider schema/isolation/durability, full host/DI/OIDC, concurrency, JS/browser and live acceptance remain seams or unverified; no broad missing-project build is claimed.

## Next persona and full-roadmap work

Implement independent controller variance disposition, fresh complete evidence review and configured accountant-to-controller task handoff through existing invoice/source/lifecycle owners. Enforce distinct verified preparer/reviewer, exact recorded proposal/current evidence, original action/grant/binding, durable disposition history, transaction/read-back/idempotency and recovery. Keep disposition separate from financial matching/post/payment until tax/non-PO and book/period/journal ownership are resolved.

Live Beep proposal-column schema evolution, correction/retirement and durable cancellation/history, lifetime/new-circuit recovery, policy-specific tasks, stock/returns, broader persona dashboards, multinational requirements and live provider/browser/release gates remain open.

Beep MigrationManager remains the business schema owner. This slice adds no schema beyond Implementation 190's un-applied nullable column, SQL, business EF lane, live migration/grant, commit, push or deployment.
