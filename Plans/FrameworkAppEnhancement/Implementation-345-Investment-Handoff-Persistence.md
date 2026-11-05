# Implementation 345: Persisted investment review handoff

2026-10-26. Sender persistence locally verified under CA-22 / FG-52. Continues [344](Implementation-344-Investment-Routing-Readiness.md). Full goal remains active.

The actual AFE screen supports current company workflow selection, frozen source/stage/recipient confirmation and an attributable native assignment receipt. Recorded-status reload shows sender, UTC time and task destinations. The existing protected tab journal retains exact company/actor input before transport; lost-response recovery never sends another creation command. Status errors do not become absence.

The existing PPDMProcessService and CrossPersonaTaskRouter own PROCESS_INSTANCE, PROCESS_STEP_INSTANCE, PROCESS_HISTORY and CROSS_PERSONA_TASK in one Serializable Beep transaction. Contracts stay in TheTechIdea.Data; the AFE controller/typed client, routing owner and journal are extended. The immutable AFE remains DRAFT; assignment is separate from financial approval, commitment or expenditure. No handwritten SQL, new business EF, schema/migration change, compatibility API or alternative workflow store is introduced.

## Implemented boundaries

- Current company/account/FIELD, approved FDP, immutable AFE, bindings, sender start/preparation and exact FIELD WRITE authority close around the write. Every configured role has current independent recipients/personas; maker and sender are excluded.
- Frozen definition identity/name/version, stage names/order/roles/SLA and recipient projection match the configured native workflow. Caller-supplied labels cannot borrow its definition hash. PROCESS_DATA uses existing serialized payload normalization; exact readback remains strict.
- Process, steps, initial audit and first-stage task destinations commit together. Every initial insert failure rolls back. Exact retry adds no graph; another intent cannot create a competing company AFE assignment.
- Original read-only recovery confirms a complete graph or original absence and never repairs partial evidence. Current start/write permission, current recipients or the old workflow definition are unnecessary for recovery; original source/bindings and current view authority remain required.
- Source task checks reject orphaned/retagged evidence before recorded-process absence or another intent. Process population is bounded at 256 and source tasks at 4096. Unknown ownership or commit outcomes require reconciliation/recovery.
- Generic process starts/mutations/approvals and FIELD detail/history/list/inbox routes cannot bypass the source-bound owner. Company/persona/route/disposal changes fence late UI results. Retention/transport/cleanup errors preserve recovery input.

## Focused verification

| Gate | Passed | Seconds | Terminal exit |
| --- | ---: | ---: | ---: |
| sender-ui-routing-technical | 144 | 937.9500 | 0 |
| source-closure-api-shared-workflow | 96 | 568.3500 | 0 |
| final-native-metadata-graph | 23 | 5.5884 | 0 |
| shared-native-start-routing-budget-labor | 132 | 16.7155 | 0 |
| budget-decision | 15 | 8.7082 | 0 |

The credited gates contain **410 successful executions covering 384 distinct focused cases**, zero failures/skips. They are incremental: the 144-case sender/UI/routing/technical gate preceded final orphan-task hardening; the 96-case gate rechecks native graph/source closure, actual API/typed HTTP recovery and shared workflow boundaries after it. The final 23-case native gate additionally verifies exact configured name/version admission. Repeated API/native cases count once in the distinct total. Unchanged UI, technical-review and maintenance paths retain their affected checks.

The current Data build passed with zero errors in 3.33 seconds; its one warning is the existing missing IdentityServer analyzer project. Focused projects use the actual Data DLL, app RBAC/registry SQLite, native/controller/client/Razor/state/journal sources and controlled Beep/auth/resource/JS/diagnostics seams. Parent Razor compilation and bUnit rendering do not establish full-host/browser/OIDC acceptance.

[Observed evidence, logs and hashes](evidence/345-investment-handoff-persistence/observed-evidence.json) records completed gates, generated projects, runtime artifacts, build and current source hashes. Failed construction/fixture runs are retained and excluded from credited totals. Corrections normalized PROCESS_DATA, selected actual source contracts/SDK namespaces in the harness, supplied the strict mock SDK error-status read and materialized the bounded source enumeration. No production authorization, transaction or readback guard was weakened.

## Remaining application delivery

[346](Implementation-346-Investment-Receiving-Source.md) provides receiving admission, qualified inbox/count/list and exact source navigation. [347](Implementation-347-Investment-Stage-Decisions.md) adds ordered stage decisions, complete rationale, native advancement/task closure and canonical financial reconciliation. General graph validation preserves original sender retry/recovery after advancement. Notifications, tiers and wider qualification remain open.

Notifications, configured investment tiers, later engineering commitments/actuals/handover, richer design/program/scenario, execution/operations and queue paging remain open. Live Beep provider/migration, isolation/concurrency, LOB/schema/constraint behavior, full-host/OIDC/browser/accessibility/RTL and multinational acceptance remain open. Source observations do not establish a cross-provider atomic snapshot.

Run affected focused tests after every coherent change. Commit/push remains deferred until the full goal and plan finish.
