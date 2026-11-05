# Implementation 171 — Atomic company step handoff

Date: 2026-10-06. Status: existing native command owner now has focused-tested atomic sequential company step/approval/history/persona-task completion and unchanged latest-intent reconciliation. API submission/current authority and visible retained-command controls remain open.

## Native workflow action

TheTechIdea.Data owns the retained CompanyAccountingStepCommand and exact CompanyAccountingStepReceipt. The existing ICompanyAccountingProcessCommands and PPDMProcessService are extended; no alternate process/task store or business model owner is introduced. A completion reason and one immutable intent bind the eligible original review, source/maker/starter, assigned step, definition/module and displayed process revisions.

Under the existing helper's owned Serializable LIFECYCLE transaction, the command validates the original company process binding/start evidence and complete reviewed graph. It closes the entire current role's recorded persona task set through CrossPersonaTaskRouter, records the current step's outcome/reason/actor/time, appends stable action history, advances the next existing blocked step or completes the process, and routes the complete next recipient set through the same router. Existing process and step creation audit is preserved; changed audit is attributed to the acting recipient.

Steps requiring Workflow.Approve also create one stable existing PROCESS_APPROVAL record with STEP_APPROVAL/APPROVED, acting approver, timestamp, original starter and reason in the same transaction. Existing approval/delegation records or reused audit identities refuse adoption. Non-approval steps use Workflow.Execute without inventing an approval. This is workflow approval/completion; invoice/order status, receipt acceptance, financial matching, ledger posting and payment are not mutated.

The API-supplied current-authority callback is mandatory before, during and after the native transaction. It must establish authoritative actor/role/permission/source eligibility; a submitted review alone is not that authority. PRODUCTION source access must stay outside the owned LIFECYCLE transaction. API preflight/transaction callbacks/post-commit source checks and transport endpoints for this new command are the next layer, not certified by native tests.

## Exact evidence and retry

Prepared process/step business and audit fields require exact full native read-back. The complete model graph, definition, process/steps/history/approval rows and closed/next task snapshots are verified before and after commit and after final authority confirmation. Missing post-commit task evidence returns an unconfirmed outcome; no successful receipt or task repair is attempted.

The latest command and its result proof are retained in existing PROCESS_DATA.Data. Identical latest-intent retry confirms the original complete outcome without additional writes, including a terminal process. Changed notes, graph, tasks, approval or definition refuse reconciliation. A newer command first confirms the previous recorded outcome before advancing. An old intent cannot adopt a progressed process. No parallel table, outbox, browser store or schema lane is added.

The command supports explicitly consistent sequential definitions. It refuses custom validation/configuration/conditional branching rather than bypassing recorded policy. Delegation/escalation, approval sequencing, conditional routing and durable older-command lookup remain required future work. The retained latest proof is not a complete historical command ledger.

## Focused evidence

Actual Data build: zero warnings/errors, 1.86 seconds. Log: %TEMP%/oilgas-step-command-171-data-final.log.

Final actual-source affected gate: Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox. **712 passed, zero failed/skipped, terminal exit 0, 11.0991 seconds**. Log: %TEMP%/oilgas-step-command-171-journey.log. Twenty-two new native cases cover complete graph/approval/history/task handoff and exact retry; failure at all ten write positions; authority loss after task closure; terminal completion; configured-policy refusal; provider audit corruption; lost committed task evidence without repair; changed intent/graph/tasks/definition/approval; and typed HTTP review of the next recipient step followed by terminal completion and refusal of the older intent.

Whitespace verification passes. The expanded composition reports 319 dependency/source-precedence and nullable warnings; the four additional warnings are actual transaction-helper source precedence in the new command/router composition. There is no new nullable warning from the command/tests. Counts overlap prior affected gates and are not added.

The initial 705-case checkpoint passed before the stronger prepared-row checks. The 708-case checkpoint passed 701 and failed seven: the new comparison serialized the prepared row through its base ModelEntityBase type while the actual row used its full runtime type. It now compares full native runtime row content. Subsequent 708/709/711 checkpoints passed and are superseded by the final 712-case source, including workflow approval and next-step progression. These checkpoints do not replace the final evidence.

Native records/physical transactions are explicit mocks with complete rollback snapshots and controlled failures. Source review and the next-step review use the actual HTTP API and typed client under current company/binding/account/diagnostics fixtures. The mutation authority callback is an explicit fixture, not a production API action gate. Complete production host/DI/OIDC, real native provider locking/concurrency/durability, browser/RTL/accessibility, command endpoint/client/page handling and whole-solution verification remain open.

## Next persona delivery

Wire this native command through the existing company process controller/service with fresh source/definition/maker/role/permission authority and retained-outcome reconciliation. Extend the existing typed client/process page with one pending intent, explicit completion reason, confirmed receipt/history/next-recipient links and authoritative reload. Keep financial action evidence, receipt policy/concurrency, approval/post/payment, durable recovery/outbox, other personas and multinational/company requirements active. Beep MigrationManager remains the business schema owner; no handwritten SQL, new business EF lane, live migration/grant or phase closure.
