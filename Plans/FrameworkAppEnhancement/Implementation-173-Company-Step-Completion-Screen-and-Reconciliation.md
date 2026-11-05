# Implementation 173 — Company step completion screen and reconciliation

Date: 2026-10-06. Status: company AP/PO process-page completion and circuit-retained reconciliation pass focused verification. Browser/new-circuit recovery and broader persona dashboards remain open.

## Persona screen and decision

The existing company accounting process page now consumes the existing typed company step command. An accountant, controller or procurement reviewer uses the recorded source/process panel, reviews current assigned-role authority, sees the source maker, workflow starter, required permission and independence requirement, then supplies a required completion reason. An eligible reviewed step exposes completion; refused eligibility exposes the recorded refusal. This records the workflow decision and required workflow approval. Financial matching, posting and payment remain separate.

The handler refreshes authenticated standard ASP.NET roles and selected company/account permissions before submission. ApiService independently checks authoritative app roles, source/maker/definition/module evidence and its native command authority. No IdentityServer authorization, automatic grant, alternate business model or direct frontend database access is introduced.

## Circuit command retention and outcome

CompanyAccountingStepState is scoped to the existing Blazor circuit. It freezes one original command and reason before submission and blocks overlapping submissions. An invalid reason creates no retained intent and performs no command write. An uncertain HTTP or read-back result keeps the original command; explicit reconciliation reuses its exact identity and reason even if the input changes.

Company/account/process changes invalidate visible review and receipt, cancel outstanding work and discard late results. The original uncertain intent remains retained and can only reconcile after the original company/account/process has been reattached. It blocks another command until reconciled. This retention ends when the circuit is lost; users are told to keep the session open. Durable browser/new-circuit and historical command recovery remain unimplemented.

The state requires an exact command receipt followed by the existing full owned company process reader. Process/source/step identities, completed actor/reason, matching STEP_COMPLETED history and pending next step or terminal process must match. Missing or changed read-back remains unconfirmed. Receipt collections are copied before exposure.

The page reloads the recorded process after confirmation, clears the reason for any subsequent decision, displays recorded history and closed/next assignment counts, and links back to assigned work. The current role/source panel continues to use the existing process rendering owner. A terminal completion uses a return-to-assigned-work label. The receipt remains visible only for the confirmed process revision; changes invalidate it.

## Focused verification

The final actual-source gate is Scripts/Test-PurchaseOrderOwnership.ps1 with the previously clean-built actual Data assembly and -IncludeWorkflowInbox. **743 passed, zero failed/skipped, terminal exit 0, 14.7040 seconds**. Log: %TEMP%/oilgas-step-page-173-final.log. Data contracts are unchanged in this slice; the clean Implementation 172 Data build is reused.

Thirteen added cases cover actual complete page handlers for AP and PO, terminal completion, invalid reason without mutation, API permission refusal, post-commit uncertainty and original-intent reconciliation for both sources; company/account/process changes while a real completion response is held, overlapping submission refusal, discarded late receipts, restored original-scope reconciliation and receipt-copy protection; and missing/history/source process read-back rejection followed by unchanged reconciliation without additional writes. Existing whole-page eligibility tests and complete inbox journey composition now use the scoped completion state.

The expanded composition retains 319 existing dependency/source-precedence and nullable warnings; no new state/page warning is reported. An initial 743-case run passed 740 and failed three receipt-copy assertions because the JSON materialized collection was a List rather than an array. The assertion now mutates through IList; the subsequent gate passes all cases. Counts overlap previous gates and are not additive.

These checks compile actual page/state/typed client, company API/native process/history/approval/task owners and actual Data. Static rendering invokes the bound private handler; it is not browser-click, accessibility or RTL evidence. Current account/module/provider/diagnostic seams and physical native records/transactions remain explicit fixtures. Production host/DI/OIDC, provider concurrency/durability and broad solution checks remain open.

## Remaining delivery

Next: reflect recorded reviewer-to-controller handoffs in persona landing/inbox/dashboard composition and verify the next recipient's queue/drill-down through the existing owners. Keep durable recovery/outbox, custom configured validation/routing/delegation, receipt acceptance/concurrent consumption, financial preparer/reviewer/post/payment actions, other personas and multinational company requirements active.

Beep MigrationManager remains the business schema owner. This slice adds no schema, handwritten SQL, business EF lane, live migration/grant or deployment. It does not close a feature, phase or the full plan.

