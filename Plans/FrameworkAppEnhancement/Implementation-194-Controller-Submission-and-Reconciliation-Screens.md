# Implementation 194 - Controller submission and reconciliation screens

Date: 2026-10-07. Status: existing payables screen and scoped AP owner now consume the native controller command with original-intent retention, fresh source confirmation and reconciliation. Configured accountant-to-controller persona handoff and task completion remain incomplete; no persona phase is closed.

## Controller screen workflow

After a fresh independent variance review, an authorized controller selects only an eligible disposition and supplies an explicit reason. The pickers show Request correction and, when verified policy/evidence permits it, Accept variance within verified policy. The page calls the existing API/client/native command from Implementation 193. Viewing the review is distinct from recording a disposition; successful recording remains separate from task handoff/completion, financial matching, posting and payment.

The controller handler refreshes AuthenticationState and current selected standard app roles, Accounting.View, Accounting.ReviewInvoiceVariance and Accounting.RecordInvoiceVarianceDecision. The existing API independently enforces its own current authority and exact proposal/review/binding. Missing selected recording grant or invalid disposition/reason cannot create a pending intent. Lost server recording grant disables the action and clears stale review, while retaining an already created original submission because refusal can occur after commit.

## One scoped intent and exact current-source confirmation

APInvoiceWorkspaceState.VarianceDecision extends the existing scoped owner, with copied original submission/result, busy/unknown/confirmed state and original-context recovery. One GUID intent is created only after full current review and input validation; it freezes controller account/company/invoice, proposal intent, disposition/reason, evidence/review/binding and complete supplied review. Repeated submission during an active command is refused. Unknown outcomes cannot be replaced, even when input arguments later differ.

Confirmation requires the exact full returned result and a fresh owned invoice-source read. The fresh header must equal the API result, its verified decision command must match the original intent, and reconstructing its predecision header with current lines must reproduce the reviewed source revision. Failed read, altered header/lines or late company/source/generation/disposal invalidation cannot publish confirmation. The original submission remains available for exact reconciliation; financial status remains DRAFT.

Existing financial/source clearing invalidates decision review and transient result. A confirmed intent is retired when its display is cleared; an unknown intent survives source/company clears and page disposal/recreation within the same scoped owner. Result/review/intent/history getters are copied so caller mutation cannot alter retained evidence. Current source is applied through the same existing detail/proof projection path; reload displays verified persisted controller history independently of transient success.

## Recovery, privacy and conflicting actions

The page shows an unconfirmed controller notice outside the review controls. Original company/account/invoice/intent/disposition/reason/proposal/evidence details appear only in the original selected context with Accounting.View and a successful page read. Recovery links target the exact original invoice; reconciliation requires that invoice, original company/account and current review/record authority. Different company/account cannot view retained details or use recovery, and cannot replace the original command. Busy state disables pickers/actions; changing inputs cannot alter a retained submission.

New controller review, accountant proposal preparation/recording, policy command review/recording and receipt-allocation recording are blocked while controller outcome is unsettled. The same existing source-command event refreshes the page. Other source commands block controller recording while active or unresolved. The generic source handoff is suppressed during busy/unknown controller submission so it cannot offer an unconfirmed source handoff. Explicit current-evidence reads remain distinct from mutation; they do not confirm the retained outcome.

Existing MudBlazor/theme conventions and API-only data access are retained. No duplicate data contract, repository, role authority, custom CSS/JS, SQL, business EF lane, schema change, live migration/grant, commit, push or deployment is introduced. Business schema remains owned by Beep MigrationManager.

## Focused verification

Final affected gate: **1362 passed, zero failed/skipped, terminal exit 0**, 1.5815 minutes; log %TEMP%/oilgas-controller-194-final.log. Smaller source gate: **519 passed, zero failed/skipped, terminal exit 0**, 9.3037 seconds; log %TEMP%/oilgas-controller-194-source.log. Adds 22 cases over Implementation 193: 15 workspace and seven complete-page scenarios. Composition retains 366 warnings; Data is warning-free. Whitespace checks, including new files, pass. Actual Data build passes with zero warnings/errors, 1.26 seconds; log %TEMP%/oilgas-controller-194-data.log.

Initial affected checkpoint: **1358 passed, zero failed/skipped, terminal exit 0**, 1.7705 minutes; log %TEMP%/oilgas-controller-194-focused.log. Final source adds four input/concurrent-submission cases and friendly picker labels.

New scoped workspace cases cover full result/current-source confirmation, protected copies and persisted reload, uncertain commit/unchanged retry, unknown-intent survival and conflicting policy-review refusal, altered header/lines and failed reads, late company/source/disposal fencing, missing recording grant before intent creation, privacy/recovery refusal in another company/account, invalid choice/blank/oversized reason, and repeated click/review refusal while recording without invalidating the active generation.

Seven complete actual payables-page scenarios use the native API and typed client: within-policy acceptance, out-of-policy correction, unknown commit, exact retry, page disposal/recreation with the same scoped owner, lost server recording grant and changed contributor refusal during reconciliation. They invoke real review/picker/reason/record handlers and verify confirmed or retained outcomes, visible persisted history, original intent preservation and DRAFT status. Successful proposals plus first controller decision issue exactly two invoice updates; retries add none. Lost server decision grant leaves only the original proposal update.

Counts overlap. Page recreation verifies the same existing scoped owner, not lifetime/new-circuit recovery. Physical provider schema/isolation/durability, full host/DI/OIDC, concurrency, real data, browser/JS/keyboard and broader application acceptance remain fixture seams or unverified. The composition uses current sources with cached dependency assemblies and retains type-conflict warnings; no broad missing-project build is claimed.

## Next persona delivery

Connect configured accountant-to-controller handoff and controller inbox/landing status through existing invoice/process/task owners. Require verified recorded proposal and independent controller disposition for the appropriate configured actions; preserve original process/task/source identities, current roles/grants/binding, durable task history and uncertain-outcome reconciliation. Task completion must follow separately verified disposition evidence and cannot imply matching/posting/payment.

Live Beep proposal/decision columns, correction/cancellation/retirement and further durable history, lifetime recovery, policy-specific tasks, tax/non-PO and book/period/journal evidence, stock/returns, broader persona dashboards, multinational requirements and live host/provider/browser/release gates remain active.
