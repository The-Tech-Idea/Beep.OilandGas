# Implementation 346: Assigned investment receiving source

2026-10-26. Receiving source locally verified under CA-22 / FG-52. Continues [345](Implementation-345-Investment-Handoff-Persistence.md). Full goal remains active.

A current independent original receiving individual can open their native investment assignment from company work. The existing AFE page resolves investmentHandoff to the exact task/workflow/role/sender, recorded request amount/currency/book/calendar date and maker, approved FDP basis and any recorded independent investment decision. This receiving view is read-only and does not change the active FIELD preference, record a decision or complete a task.

The existing investment routing/financial-source owners, AFE controller/typed client, native PPDMProcessService, workflow inbox and persona work panel are extended. New receiving DTO metadata stays in TheTechIdea.Data and references the existing financial source contract. No SQL, new business EF, schema/migration change, duplicate entity/workflow engine, compatibility API or automatic grant is added.

## Admission and source closure

- Current approved company membership/roles and standard role claims, Accounting/DevelopmentPlanning/Workflow view, investment review and Workflow Approve permissions, current profile/mapped workspace and original role/persona recipient identity are required. Maker and sender cannot borrow recipient authority.
- Company ownership is checked before private payload projection. Current PPDM_CORE/planning/lifecycle/production bindings, exact FIELD WRITE grant and actual resource WRITE authority are enforced through the existing financial admission owner. Current start/preparation permission is unnecessary for the receiver.
- The native reader returns a defensive copy of the process snapshot validated with its original complete task set. The receiving owner uses that snapshot rather than another unqualified process read. Frozen source/configuration and exact native process/task revisions close around two observations and current financial source readback.
- Qualified AFE inbox rows require the dedicated receiving proof. Initial refusals omit ineligible work; late revocation refuses the whole response, and changed evidence cannot publish partial rows/counts. Foreign company headers are filtered before private projection; malformed ownership cannot become generic FIELD admission.
- The shared persona inbox/landing adds a qualified AFE filter, matching selected counts and an exact receiving-source link. Company/persona/route/disposal changes and failed reloads clear stale source evidence. The actual AFE parent keeps assigned-source navigation separate from request creation.

## Focused verification

| Gate | Passed | Seconds | Terminal exit |
| --- | ---: | ---: | ---: |
| native-source-api-receiving-ui-workflow | 163 | 361.8900 | 0 |
| persona-inbox-and-landing | 92 | 4.5224 | 0 |
| qualified-http-source-inbox-counts-list | 1 | 259.5540 | 0 |

The credited gates contain **256 successful executions covering 255 distinct cases**, zero failures/skips. The 163-case gate covers native snapshot/graph readback, independent receiving admission/revocation, actual source HTTP/client, inbox closure, source page/state and affected workflow/AFE regressions. The 92-case gate verifies the current persona source filter/labels/route and shared landing regressions. The final single-case gate repeats and strengthens the protected HTTP journey with the real qualified inbox, counts and filtered-list clients. Counts overlap earlier checkpoints and are not cumulative goal totals.

The current Data build passed with zero errors in 7.60 seconds; its one warning is the existing missing IdentityServer analyzer project. Generated harnesses use the actual Data DLL and current source owners with real app RBAC/registry SQLite and controlled Beep/auth/resource/JS/diagnostics seams. Source controllers, typed clients, actual Razor parents/shared panel and bUnit query navigation are exercised; no full-host/OIDC/browser/live-provider claim follows.

[Observed evidence, logs and hashes](evidence/346-investment-receiving-source/observed-evidence.json) records exact gates, runtime/source artifacts and initial failures. The profile-denial fixture now creates an actual unmapped persona instead of violating its foreign key; the query test uses NavigationManager; direct controller fixtures supply the new required reader; existing unrelated source captions retain their asserted behavior. A failed new source-filter case revealed the missing AFE facet, which was implemented and rendered. Tests and production authorization/transaction/readback guards were not weakened. Native LocalDB-dependent role/provider qualification remains outside these focused gates.

## Remaining application delivery

[347](Implementation-347-Investment-Stage-Decisions.md) now supplies source-bound ordered stage decisions, full funding rationale, current next-recipient admission, encrypted and server-restored original recovery, explicit partial continuation, native advancement/task closure and exact canonical financial reconciliation. The initial read-only receiving checkpoint above retains its historical scope; current behavior is documented in 347. Notifications, tiers and wider delivery/qualification remain open.

Notifications, configured investment tiers, later engineering commitments/actuals/handover, richer design/program/scenario, execution/operations and queue paging remain open. Live Beep migration/provider/isolation/concurrency/LOB/schema/constraint behavior, full-host/OIDC/browser/accessibility/RTL and multinational acceptance remain open. Bracketed source observations do not establish a cross-provider atomic snapshot.

Run affected focused tests after every coherent change. Commit/push waits for full goal and plan completion.
