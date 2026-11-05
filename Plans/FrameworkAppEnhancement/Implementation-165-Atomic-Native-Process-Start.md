# Implementation 165 — Atomic native process start

Date: 2026-10-06. Status: actual native initial graph persistence passes focused verification; secure company accounting submission and persona command screens remain open.

## Delivery dependency

The accountant's source-to-reviewer handoff needs a recorded process, its full step set and its initial audit entry before the existing process detail/inbox can show a reliable outcome. Implementation 164 made task fanout atomic, but inspection found the existing process start could save a process and some steps before a later step/history failure. This slice fixes that initial graph boundary in PPDMProcessService rather than adding a parallel workflow writer.

ProcessServiceBase now exposes one protected initial-persistence boundary and includes the recorded start audit in its returned instance. Its existing derived-owner behavior is preserved through the default implementation. PPDMProcessService overrides that boundary and its existing StartProcessAsync with native transaction ownership and complete read-back. The existing converters, repositories and tables remain the owners; no new business entity, SQL, schema or EF lane is introduced.

## Recorded start behavior

The existing generic start requires bounded process/source/FIELD/actor identities. An empty FIELD cannot create an implicitly company-owned process; company accounting needs its separate explicit reviewed binding and command authority. This method is not that company command and does not infer company ownership.

An owned Beep Serializable transaction verifies one active workflow definition matching the source type. Duplicate/empty step identities or sequences, unsupported multiple required-role assignments, malformed roles and negative recorded SLA refuse before writes. The complete native definition snapshot is checked again before commit.

The prepared process, steps and history identities must all be new: existing process graphs, step keys or audit keys are never adopted or updated. The actual existing save/history owners insert the complete initial graph. Any insertion or closing-review failure rolls back every new row.

Read-back requires exactly one process, the complete unique initial step set and exactly one start history row. Native business fields must match the prepared graph, with active records and recorded creation actor/date. Full native snapshots, including audit evidence, must agree across closing checks. The original datasource and complete graph are confirmed again after commit. Uncertain commit or absent post-commit records return no successful start. The returned instance is frozen and includes the initial audit.

This generic entry point still generates fresh IDs. It is not an idempotent company submission ledger; an uncertain outcome must be reconciled through recorded source/process reads before another start. It does not join task routing, sub-process parenting, later process transitions or financial posting into the same transaction.

## Focused evidence

Final gate: Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox. **570 passed, zero failed/skipped, terminal exit 0, 7.6470 seconds**. Log: %TEMP%/oilgas-process-start-165-final.log. Whitespace verification passes.

There are 24 new native start cases: complete graph plus recorded audit/read-back; failure at each of the four writes; corrupt process/step/history read-back; definition change during insertion; inactive/source-mismatched/duplicate definition and ambiguous step/sequence/role or negative SLA; empty FIELD refusal; borrowed/weak transaction refusal; uncertain commit; three identity collisions without updates; and successful commit flags without a recorded graph. The earlier 565-case checkpoint is superseded by the final source and is not added to the total.

The gate now compiles the complete actual ProcessServiceBase, PPDMProcessService, native start partial, validator, dynamic routing dependency and native definition/history/approval entities alongside the prior task/router/inbox owners. No production source-body replacement is used. The test subclass fixes only generated IDs to exercise real collision handling. Datasource records and physical transaction capability are explicit mocks, and existing shared identity/diagnostics/reference/wording seams remain the documented WORKFLOW_INBOX_FOCUSED fixtures. Existing RBAC/configuration EF queries use SQLite; native business persistence is not replaced by EF.

The final expanded composition reports 292 dependency/source-precedence and nullable warnings and no compiler errors. Actual sources take precedence over cached SDK types. Initial complete-owner compilation exposed obsolete namespace imports and missing actual dynamic-routing/validator source references; those were corrected before the passing final gate. Real provider locking, isolation, precision, durability/concurrency, production bindings/DI/host, browser/OIDC and unrelated full-solution checks remain unverified.

## Next visible persona work

Join explicit company/source ownership, source revision, app-owned write authority, actual invoice preparer and independent reviewer into the canonical company start command. Compose initial process/history and persona assignments in one owned transaction with durable unchanged-intent retry and confirmed read-back, then consume it from the existing accountant/procurement source screens. Their landing queues, process detail and dashboard drill-downs must show the recorded handoff and result.

The atomic generic initial graph implemented here is a prerequisite, not a completed company accounting journey. Allocation submission/acceptance, concurrent receipt consumption, approval/post/payment, stored-definition/schema reconciliation, outbox and other personas/multinational requirements remain open. No live database write, migration, grant or financial operation was performed. Beep MigrationManager remains the business schema owner; no phase closes.
