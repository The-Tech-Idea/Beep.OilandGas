# Implementation 175 — Controller terminal workflow and landing refresh

Date: 2026-10-06. Status: Controller Workflow.Execute page-to-API-to-native terminal workflow, retained reconciliation and refreshed landing/inbox pass focused verification. Financial approval/post/payment commands remain separate and open.

## Controller decision on the existing screen

The existing company process page handles its assigned Controller next step through the existing API review and typed completion command. Current role and Workflow.Execute authority are independently enforced by the API. The page still requires the source evidence/reason and retains the original intent if the outcome cannot be confirmed. Execute does not create a second approval for a non-approval step.

Completed company workflows now display recorded completion timestamp and completing account. The page stops offering current-step review for a terminal process and refuses repeat invocation after confirmed completion. Original unconfirmed reconciliation remains available even when a reload shows the recorded process completed: recorded workflow state alone does not override a refused original-source/command confirmation.

The shared source panel explicitly distinguishes workflow completion from financial matching, posting and payment. No financial completion or liability result is inferred from the configured step name, which can be Post.

## Terminal read-back evidence

The existing ProcessInstanceDetailState remains the company process/step/history validation owner. Before publishing a completed company process it now requires:
- Explicit COMPLETED state, recorded nondefault UTC start/completion times in order and completing account.
- A nonempty set of unique step identities/sequences; the current step is the final sequence and agrees with the process completion actor/time.
- Every step is completed with a recorded actor, nonblank reason and completion time within the process interval.
- Exactly one STEP_COMPLETED history entry for each step, matching its actor/time/reason.
- The final history identifies its original step and records the terminal COMPLETED state.

Missing, competing, inconsistent or incomplete terminal facts clear source/history/links rather than becoming a completion display. Existing in-progress ownership/snapshot checks continue to apply. This is recorded workflow evidence; it is not a financial-source audit or atomic cross-module snapshot.

## Complete persona journey

Actual AP and PO workflows first complete their independent reviewer step through the company HTTP endpoint. A separate explicit Controller app-RBAC/workspace fixture supplies current Workflow.Execute. The complete process page invokes its actual bound review/completion handlers, sees the assigned Controller permission, submits the required reason and reads the native terminal process.

The receipt closes the configured next task set and has no next assignments. Both steps and all three start/completion history records are persisted; the first approval remains the only approval. Exact terminal retry adds no writes. The actual Controller persona inbox and complete accounting landing reload to zero pending tasks, and reopening the source process shows its completion/reason without action controls.

Post-commit source drift retains an unconfirmed Controller command. The page reload still permits explicit original-intent reconciliation. Restoring unchanged original source evidence confirms the same command/reason without additional writes. Missing API Execute permission or standard role refuses mutation after an eligible page review.

These role fixtures retain the test account actor in separate explicit assignments, rather than proving two live people authenticated. Production account/module authority and physical provider records/transactions remain test seams; live grants are not modified.

## Focused verification

Final actual-source gate: Scripts/Test-PurchaseOrderOwnership.ps1 with actual Data and -IncludeWorkflowInbox. **772 passed, zero failed/skipped, terminal exit 0, 19.0210 seconds**. Log: %TEMP%/oilgas-controller-terminal-175-final.log.

Seventeen added cases comprise six actual Controller page/API/native/landing journeys (AP/PO completion and reconciliation, permission loss, role loss) and eleven terminal process evidence cases (valid outcome and missing/inconsistent date, actor, status, history, reason, time, interval, terminal state, competing history and absent steps). The first six-case expansion passed 761 checks before the final terminal evidence expansion. Counts overlap previous gates and are not additive.

Actual Data contracts are unchanged; the clean Implementation 174 Data assembly is reused. The affected composition retains 319 existing dependency/source-precedence/nullable warnings, with no new changed-source warning reported. Whitespace checks pass.

The gate compiles actual page/state/source panel, complete clients, company controller/service, PPDM native process/history/approval/task owner and actual Data; existing SQLite app RBAC/configuration checks remain included. Static rendering calls bound handlers and reopens complete parent screens. FIELD preference/source-client/diagnostics/current account/module/native physical provider boundaries remain explicit fixtures. Production host/DI/OIDC, browser interaction/keyboard/RTL/accessibility and real provider concurrency/durability are unverified.

## Next delivery and remaining plan

Implement the company's financial source decision review and permitted preparer/controller actions on the existing invoice screen, using actual PO/receipt/allocation/journal owners. Keep workflow status distinct from matched/approved/posted/paid source outcomes and retain reviewed source/version/independence constraints.

Durable browser/new-circuit/historical recovery/outbox, configured policy/conditional routing/delegation, procurement receipt acceptance/concurrent consumption, financial posting/payment, accounting period/currency/KPI dashboard composition, other personas and multinational requirements remain active. Beep MigrationManager remains the business schema owner. This slice adds no schema, handwritten SQL, business EF lane, live migration/grant or deployment. No feature, phase or full-plan closure.

