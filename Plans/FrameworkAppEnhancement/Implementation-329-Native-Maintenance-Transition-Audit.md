# 329 - Native maintenance transition audit

The recorded completion decision previously checked its immutable approval/history and retained body, but private recovery did not validate the complete surrounding workflow population. This change retains and validates the exact native before/after row changes as a foundation for subsequent configured return and rework transitions.

## Delivered native behavior

- The existing completion decision still performs exactly three owned writes: append WORK_COMPLETION approval, append matching history, update the original process payload/audit. No step/task/physical report/stock/cost/finance mutation is added.
- The original reviewed graph revision is rechecked inside the owned command before the append. The retained delta identifies only the original process-row update and the exact new approval/history identities. Unexpected sections, row deletion, definition edits, history/approval rewrites, duplicate identities, missing ownership or unbounded populations refuse.
- After readback and during recovery/public decision reads, the native owner validates the complete current definition/process/step/history/approval/task graph against the recorded result. It then restores the original row view in memory and checks the exact original reviewed revision and canonical source revision. Restoring this view never issues persistence writes.
- Canonical comparison handles object-key representation while preserving array identity/order and every row value. The exact original raw reviewed revision also remains bound to the restored source, so changing both retained row content and a secondary digest cannot replace the reviewed graph.
- Changes to a reviewer step, cancellation of an expected task, an unrelated approval append or corruption of the retained source/row evidence cannot be published as the original decision. Existing uncertainty recovery, original creator audit, same-source owned proof and separate CORE boundaries remain intact.
- A dedicated mutation policy permits only the frozen exact approval/history inserts and original process-row update. It restores owned common audit fields after the standard handler, refuses any changed business field and preserves the original process creator. New audit protocol version 1 records deterministic distinct PPDM GUIDs and one authoritative UTC across row/history/receipt. Unknown or downgraded protocol evidence refuses; earlier version-0 evidence retains its original strict audit validation without rewriting history.
- The delta stays in the existing private process evidence. Public outcome contracts do not expose the retained rows, command, rationale or recovery UUID. Existing earlier decision evidence without this added delta retains its original approval/history validation; no historical migration or source rewriting is performed.

This is not the return/rework handoff itself. Later transition owners must use their own declared row changes and historical validation before earlier receipts can be recovered across those transitions. A recorded verification still does not establish physical return acceptance or asset lifecycle advancement.

## Focused verification

- Initial native decision/status/handoff: 32 passed, 27.5092 seconds, oilgas-329-graph-native.log.
- Final affected decision/status/handoff/assigned-stage/submission gate: 84 passed, 40.8843 seconds, oilgas-329-audit-native6.log. Includes graph/evidence/protocol corruption, actual common-column handler UTC/GUID/creator preservation, handler business-field mutation refusal, exact three-write behavior and original retry/recovery. Earlier scoped 38 and full 80/82-case runs also passed; counts overlap.
- The protected actual API decision/status journey before this delta change passed: 1 test, 7.8534 minutes, zero failures/skips, terminal exit 0 in oilgas-328-outcome-http5.log; session 60021 is closed. It proves the current app-role/asset/selection boundary, configured role review, public absence without approval capability, unresolved verification refusal, recorded rejection, immutable physical report, public outcome privacy and original no-replay recovery. Auth/provider/FIELD and already-recorded physical source seams remain explicit.
- Protected graph-delta/API journey: 1 passed, 6.0215 minutes, zero failures/skips, terminal exit 0 in oilgas-329-graph-http6.log; session 44081 is closed. It predates the final owned common-audit policy.
- Latest owned-audit/API rerun: 1 passed, 5.2227 minutes, zero failures/skips, terminal exit 0 in oilgas-329-audit-http7.log; session 85860 closed.

No SQL, business EF, new native table/store, live migration, custom CSS/JavaScript or client database access is introduced. Existing PPDM repositories, Beep transaction ownership and private process payload are reused. UI/API contracts and current public outcome screen remain unchanged by the delta; prior focused UI/parent evidence is scoped to that unchanged surface. Live provider/concurrency, browser/OIDC and cross-source atomicity are not established.

## Required continuation

Implement configured review-to-return/rework routing, historical recovery across subsequent transitions, actual facility/production return acceptance, lifecycle advancement and responsible queues/dashboard drill-down. Complete the remaining financial/persona/multinational plan and full completion audit, then commit and push. The full goal remains active.
