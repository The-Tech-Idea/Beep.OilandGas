# 325 - Independent completion evidence review

The next maintenance handoff in 317 requires a supervisor to inspect the recorded executor report, current completion inspections and authoritative person independence. The original report recovery path is private to its executor. This implementation adds a separate authorized reviewer read and current comparison to native work-order detail; it does not yet record a verification decision or accept return to service.

## Reviewer workflow

- A current standard app role with Maintenance.VerifyCompletion and existing original FIELD/order WRITE authority supplies the recorded maintenance process and submission identities. No private executor start/submission intent is required or returned.
- The existing native execution owner reads the complete unique original task submission twice, validates schema/company/FIELD/order/process headers before retained command decoding, and projects work performed, explicit defects, executor/native employee, completion/recording UTC and original scope/requirement/operational-inspection revisions. Executor rationale, retained body, prerequisite bodies and recovery UUID remain private. Existing own-executor recovery uses the same reader with its original additional ownership/start checks.
- The API independently reads the current native order and full immutable requirement proof, plus the existing company personnel identity catalog and actual current reviewer employment/person source. Two complete observations must agree; source, identity, ownership, grant, capability, selection and binding drift refuses publication.
- The current comparison exposes changed submitted scope/components, changed immutable requirements/applicability, changed originally submitted operational inspection evidence, missing/unaccepted/early/future completion inspections, unresolved reported defects and unknown/conflicting person independence. Current reviewer identity must be independently recorded and effective; a different account linked to the executor's native person still conflicts. Completion inspectors also resolve through effective account/person links at their actual inspection UTC. Names and recording accounts do not substitute for native person identity.
- The existing typed workflow client, circuit read state and MudBlazor detail panel fence company/account/FIELD/auth/capability changes and late responses. The panel shows the original work report, explicit defects, current reviewer person, actual completion checks/inspector/UTC, original revision and current blockers.

The comparison always retains the explicit pending recorded-verification blocker. A clean read does not complete a configured reviewer assignment, record a physical decision, mutate an inspection/report, post costs, accept return or advance lifecycle state. Verification and return commands must add actual recorded evidence and configured-role handoffs next.

## Architecture

Data owns the public report/query/comparison contracts. Existing WorkOrderExecutionPermitService, InspectionService and ContractorManagementService remain the source owners. ApiService is the only business data gateway; current app roles and independent endpoint authority remain in the app/API, with authentication-only IdentityServer. The native existing work-submission Beep schema is mandatory; this read introduces no table, migration apply, SQL, business EF or parallel report store. No custom CSS or JavaScript is added.

## Focused verification

- Current isolated Data: zero warnings/errors, 2.02 seconds.
- Final native original report/submission gate: 26 passed, 16.8119 seconds, including another authorized actor's public report facts, private recovery refusal, unchanged writes/transactions, schema/authority/binding/foreign/query/changed-report refusal and preserved original submission/recovery behavior.
- Final current comparison, typed client, state and actual reviewer panel: 23 passed, 9.0770 seconds. Tests cover actual UTC/result/defect/scope/identity blockers, same native person despite another account, failed self-mapping evidence, operational-result drift, capability/FIELD/late-context fences and zero submission calls. The initial UI run correctly refused malformed UNRECORDED inspection and self-authored identity fixtures; fixtures now reflect canonical unavailable evidence rather than invented valid records.
- Native work-order parent: 15 passed, 2.0597 seconds, including dedicated completion reviewer entry without finance recording controls.
- Protected actual API report/current inspection/person source journey: 1 passed, 1.1473 minutes, zero failures/skips, terminal exit 0 in oilgas-325-completion-http.log. Session 49417 is closed; no HTTP test remains live. It verifies missing/revoked capability refusal before report reads, independent public report privacy, explicit unknown reviewer identity, actual native mapped reviewer/person read, current scope-change blocker, self-review refusal and zero business writes. The fixture supplies already-recorded canonical report/inspection/identity evidence and controlled auth/provider/FIELD seams; it does not certify the original execution or identity mapping write endpoints. Shared HTTP output was preserved throughout the run.

Live provider/migration/concurrency, OIDC, full solution, browser/RTL/accessibility and transactional cross-source reporting acceptance remain open. Paired source observations are scoped proof rather than a transactional reporting snapshot.

## Required continuation

Add the recorded independent verify/reject/return decision, original uncertainty/history recovery, configured executor-to-reviewer-to-return handoff and exact source-bound native closure. Then implement explicit facility/production return acceptance, lifecycle advancement and responsible queues/dashboard drill-down. Financial posting/commitments/corrections/amendments and all remaining persona/multinational requirements remain in scope. The full goal stays active; commit/push follows the complete plan audit.
