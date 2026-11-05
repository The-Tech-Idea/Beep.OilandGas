# 330 - Configured return approval role review

The independent completion reviewer needs to see who is responsible for the next return approval before any task handoff. A recorded VERIFIED outcome is insufficient when its actual work or inspection evidence has changed.

## Delivered behavior

- The existing Data completion comparison now computes a work-subject revision separately from the current reader's authorization and comparison clock. It binds the original physical report, current order/components, immutable requirement population, pre-start/submission/completion results and the account/person links effective at each accepted completion inspection. Current viewer changes and later RETURN_TO_SERVICE result recordings do not redefine the earlier verified subject. Changed inspection results or original requirement definitions do.
- Public recorded decision facts add this subject revision without publishing the private command, rationale or recovery intent. The revision is derived from the existing immutable recorded decision evidence, including earlier compatible decisions; no historical records are rewritten.
- The native PPDM process owner resolves the current configured independent reviewer, reads the actual recorded decision and compares that decision with the current complete completion evidence. Missing/rejected decisions, unresolved defects, unaccepted completion inspections, changed evidence, unauthorized reviewers and executor-person conflicts refuse.
- The next step must be the explicit immediately following approval step with exactly one configured role, distinct from both the executor and completion reviewer roles. The native future assignment must still be blocked, unstarted, uncompleted and unassigned. No default facility role is invented.
- Complete paired graph and decision observations retain the original company/account/FIELD/order and lifecycle bindings. The current CORE report/inspection/person comparison closes around this observation at the API boundary.
- A no-store protected API read, existing typed workflow client, circuit state and the existing completion screen expose "Review configured return approval role". The result shows the responsible role, step/assignment and recorded verification UTC, with an explicit still-blocked status. Company/account/FIELD/auth/capability changes clear the result and fence late responses.
- Standard app roles, Maintenance.VerifyCompletion, Workflow.Approve, current FIELD/order WRITE, stored selection and original native bindings remain authoritative. IdentityServer remains authentication-only.

This slice performs no writes: it does not close reviewer tasks, open return tasks, accept physical return, post costs or advance the asset lifecycle. The full handoff command and its historical recovery protocol remain open. No speculative command/recovery contract is exposed before that protocol is implemented. REJECTED must take a separately configured rework path.

## Focused verification

- Current Data build: zero warnings/errors, 11.66 seconds, oilgas-330-review-data.log.
- Initial native return-review/decision/status gate: 44 passed, 24.6803 seconds, oilgas-330-return-native2.log. Includes actual configured fourth native step and no-write assertions, missing/rejected decisions, absent/conflicting return roles, changed work evidence/step/company/authority, current viewer/clock independence and existing decision recovery/audit tests.
- Final native assigned-stage/report/submission/decision/status/return gate: 96 passed, 35.8804 seconds, oilgas-330-return-native4.log, session 96572 closed. Includes the explicit future-return-result subject test. The first extended run correctly refused an invalid CLOSE_OUT fixture for RETURN_TO_SERVICE; the fixture now uses the existing valid REGULATORY applicability rule. Counts overlap.
- Actual typed client, circuit and Razor completion/reviewer screens: 77 passed, 18.9120 seconds, oilgas-330-return-ui2.log. Includes missing approval capability, configured role mismatch, failure, late company/source responses, native FIELD fence and explicit blocked-state display. Existing independent decision/status and original recovery tests also pass; counts overlap.
- Native detail parent gate: 17 passed, 4.0841 seconds, oilgas-330-return-parent2.log, session 6840 closed. Asserts the configured-role review entry is present for a current approval-capable reviewer and absent for a read-only completion viewer.
- Protected actual configured reviewer API journey: 1 passed, 7.2112 minutes, zero failures/skips, terminal exit 0 in oilgas-330-return-http.log; session 29447 closed. Proves return-review approval refusal before source reads, no-store conflict for rejected/unconfigured work and revoked capability refusal while preserving original decision retry/recovery. The positive configured return read is currently covered by the native owner and actual client/form gates, not a positive protected HTTP/provider journey.
- Prior final owned-audit API journey is now terminal: 1 passed, 5.2227 minutes, oilgas-329-audit-http7.log, session 85860 closed. This predates the added return-review source.

No handwritten SQL, business EF, new store/table, migration apply or custom CSS/JavaScript is introduced. Existing native Beep repositories and process evidence are reused. Real provider concurrency, browser/OIDC, accessibility/RTL and cross-source atomicity remain outside this focused evidence.

## Remaining plan

Implement the review-to-return command, complete task routing, historical recovery across later transitions, separately configured rejection/rework, actual facility/production return acceptance, asset lifecycle advancement and responsible landing queues/dashboard drill-down. The remaining finance, engineering, HSE, all-persona and multinational requirements retain their existing incomplete status. Complete the full goal/plan audit before committing and pushing.
