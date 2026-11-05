# Implementation 344: Company investment routing readiness

Implemented 2026-10-25; verification reconciled 2026-10-26. Locally verified routing-readiness scope under CA-22 / FG-52. Continues [343](Implementation-343-Retire-Automatic-Work-Order-Finance.md). Full goal remains active.

The AFE source screen now offers an explicit routing review to the current company budget preparer with Workflow Start authority. It reads the exact approved FDP/AFE/book source, configured ACCOUNTING/AFE approval stages and eligible independent company reviewers. Missing configuration and stages without eligible recipients are explicit. The source maker may review destinations but cannot become an independent recipient. The source screen states that this review records no assignment or notification.

TheTechIdea.Data owns the routing projection. CompanyDevelopmentInvestmentRoutingService composes the canonical financial source reader, bound native lifecycle definition reader, repository company role/persona owners and the existing protected FIELD-grant reader. It does not create a parallel approval engine, role source or business data store. The existing AFE controller, typed client and source panel expose the flow. Company/persona/source changes clear it and late responses cannot restore previous recipients. No SQL or new business EF is introduced.

## Admission and observation boundaries

- Current sender: authenticated app actor, authoritative selected-company standard role, Accounting View/PrepareDevelopmentBudget, DevelopmentPlanning View and Workflow View/Start, original company/core/planning/lifecycle/production bindings and original FIELD resource read.
- Recipients: active approved company role/member/profile, mapped active workspace, Accounting ReviewDevelopmentBudget/View, DevelopmentPlanning View, Workflow View/Approve and exact original company/FIELD WRITE or DELETE grant. Exclude the AFE maker and current sender.
- Read configured named mandatory sequential approval stages only. Reject ambiguous roles, alternate next-step/conditional paths, optional stages and unsupported validation/configuration. Configured amount/tier semantics require their own implementation and are not guessed.
- Observe the complete financial source before/after the catalog. Compare two independently observed configuration/recipient snapshots, with closing definition observations inside each catalog read. Reuse each role lookup within one observation. This is observational closure, not a cross-provider snapshot or concurrency certification.
- Bounds: 128 accounting definitions, 64 stages per workflow, 256 total AFE stages, 256 candidate recipients per role and 2 MiB serialized routing projection. Exceeding a bound refuses instead of truncating or publishing partial readiness.

## Verification

**277 focused tests pass across two final selected gates**, zero failures/skips: **149 routing/API/HTTP/client/state checks, 12.7335 minutes**, and **128 native/technical-review/financial/UI regressions, 1.6049 minutes**. The separate construction UI gate passed 66 in 4.1226 seconds and overlaps these final gates; it is not added. Data builds with zero errors and one existing missing IdentityServer-analyzer warning, 2.64 seconds.

The 128-test gate returned terminal exit 0. The 149-test run's tool handle expired across the app/environment continuation; its completed log directly reports all 149 passed and `Test Run Successful`. The wrapper exit was not collected and is not invented. Both final harnesses use the matching actual Data assembly. [Exact filters, class counts, logs, source/generated/runtime hashes and limitations](evidence/344-investment-routing-readiness/observed-evidence.json).

The earlier HTTP fixture omitted the actual application refusal handler and returned 500 for revoked authority. It now registers the existing handler; the current-source HTTP gate retains and passes changed-authority 403 and anonymous 401 assertions. The UI foreign-company fixture was corrected to return a valid original source after selecting another company. An obsolete mixed run was explicitly aborted after its test host was identified by its exact generated project path; its 181 partial passes are not credited as a completed gate. The older routing-only 18-pass/1-fail and UI 58-pass/1-fail results remain in evidence. No product authorization, source closure or native transaction guard was weakened.

Self-review covers existing ownership reuse, independent current role/persona/FIELD eligibility, no eager lookup from the landing, explicit no-configuration/no-recipient/decided states, revoked sender access, source/definition/recipient change refusal, exact source/client identity and company/persona/route/disposal response fences. No independent external review or full-host/browser/provider acceptance is claimed.

## Next required delivery work

[345](Implementation-345-Investment-Handoff-Persistence.md) supplies sender persistence/recovery; [346](Implementation-346-Investment-Receiving-Source.md) supplies receiving/inbox; [347](Implementation-347-Investment-Stage-Decisions.md) supplies coordinated ordered decisions, original recovery and canonical financial reconciliation. Notifications, configured tiers and provider/browser/multinational qualification remain open.

Run affected focused tests after every coherent change. Commit/push remains deferred until the full goal and plan finish.
