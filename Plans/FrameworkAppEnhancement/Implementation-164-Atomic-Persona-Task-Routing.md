# Implementation 164 — Atomic persona task routing

Date: 2026-10-06. Status: focused native routing and writer-to-inbox verification passed; company accounting submission and the broader persona roadmap remain open.

## Persona outcome and existing owners

The accountant/controller handoff needs a complete, recorded assignment set before its assigned-work screen can present work reliably. This slice extends the existing CrossPersonaTaskRouter and canonical CROSS_PERSONA_TASK repository. It introduces no parallel task store, business entity, migration, SQL or business EF lane. Beep MigrationManager remains the business schema owner.

Routing now requires the recorded active process, current pending/in-progress step, exact parent/source/required role and current unique persona destinations. Company accounting routes require the explicit CompanyAccountingBinding in the existing process payload; FIELD routes retain recorded FIELD scope. Both use the existing Data-owned WorkflowTaskRoute contract and encoded process links.

## Atomic routing and retry behavior

The complete destination set is written inside an owned native Serializable transaction through PPDMDataSourceTransaction. Connection resolution, datasource identity and transaction ownership are checked throughout. Weak isolation and borrowed transactions refuse before writes.

Stable task keys derive from process, step and persona identity. An exact retry returns the complete matching recorded pending set without inserting or reprioritizing tasks. Partial destination sets, changed role/source/binding, conflicting identities, completed tasks and legacy keys require reconciliation; they are not silently adopted, repaired or reopened.

Each write is read back. Before commit, the router rechecks every complete task snapshot, the whole destination set, process/step snapshots and current persona mapping. Insert or closing-review failure rolls back the set. After commit, original datasource identity and task read-back must still agree. An uncertain commit or missing post-commit row produces no successful routing result; an unchanged retry can confirm a previously persisted complete set.

This method validates recorded process/binding evidence. It does not independently authorize a new company command, verify native AP/PO write authority or establish the invoice preparer's identity. Those belong to the still-open secure producer and API submission work.

## Focused verification

Final command: Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox. Result: **546 passed, zero failed/skipped, terminal exit 0, 7.6097 seconds**. Log: %TEMP%/oilgas-persona-task-routing-164-final.log. Whitespace verification passes.

The 27 new cases comprise 25 native routing cases and two actual-router-to-inbox HTTP cases. Native cases cover complete company/FIELD routing, unchanged retry, second-insert rollback, invalid/ambiguous parent and role/source evidence, closing changes, completed-task refusal, weak/borrowed transaction refusal, corrupted read-back, partial-set refusal, uncertain commit recovery and missing post-commit records. HTTP cases feed recorded router output through the complete inbox controller and existing company accounting read boundary: the owned company sees its task; another approved company receives fixed 409 without the task identity or a native invoice lookup.

The gate compiles and executes the actual router, native repository and transaction helper. Datasource persistence, provider transaction capability and process projection remain explicit fixtures. SQLite covers existing app RBAC/configuration queries only. The two HTTP cases do not certify PPDMProcessService persistence or an atomic process/history/task graph.

There are 272 dependency/source-precedence and nullable warnings: the prior 267, four SDK source-precedence warnings in the expanded router composition, and the existing optional logger assignment warning. Initial compilation exposed a stale router namespace import and focused-test imports; these were corrected before the final passing run. No compiler errors remain. Unchanged historical state/panel checks are not summed into this total.

## Open delivery work

Inspection found existing process start/save writes the process, steps and history separately, and no production caller of RouteTaskAsync was found. Therefore this slice is a prerequisite; it does not deliver a durable company accounting submission.

Next connect a securely reviewed company/source submission to the existing process/task owners, with explicit source revision, actual preparer and independent reviewer, atomic process/history/task persistence and confirmed read-back. Consume that command from the existing accountant/procurement screens and show the recorded handoff and outcome on their landing queues and dashboard drill-downs.

Accepted receipt/company policy, concurrent consumption, allocation submission, approval/post/payment, definition/schema reconciliation, durable intent/outbox and the other persona/multinational workflows remain open. Full landing/dashboard, browser/RTL/accessibility, production identity/diagnostics/context/DI/host and actual provider locking/concurrency/durability are unverified. Existing WORKFLOW_INBOX_FOCUSED identity/diagnostics/reference/wording seams remain explicit fixtures. No live database writes, migrations, grants or financial operations were performed. No phase closes.
