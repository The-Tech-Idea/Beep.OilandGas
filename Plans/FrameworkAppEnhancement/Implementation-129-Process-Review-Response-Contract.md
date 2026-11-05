# Implementation 129: Process review response contract

## Company role action review

The existing business-process Web client previously accepted any non-empty action-review response. GetTransitionReviewAsync now validates the requested process identity before sending, captures the original selected company/actor, rejects absent selection and changed scope, then validates the response through Data-owned ProcessTransitionReview.Require. No new endpoint, SDK/store, role mechanism or fallback is introduced.

The shared contract check requires the original company/actor/process, FIELD, a SHA-256-shaped revision, confirmed state/step for actionable choices, complete unique action identities and unambiguous from/to/trigger choices. It rejects mixed journal and maintenance source families. Journal source timestamps/line keys and maintenance header/component/inspection evidence are validated through their current owners. A process without an active step may still display read-only evidence when it has no action choices. Empty or malformed evidence cannot create actions.

The actual API review producer applies the same check before returning a review. ProcessInstanceDetailState delegates common validation to that contract, then checks the displayed original FIELD/state/step and exact source entity identity. An empty response now produces a controlled unavailable-review failure, clears the previous action review and preserves the recorded process, instead of raising a NullReferenceException. WorkOrderSourceReview.Require explicitly requires non-empty original identities, including when malformed JSON leaves both compared values empty.

Standard app-owned roles and recorded resource authority remain independently enforced. This contract validation does not grant roles, permissions or maintenance acceptance. Scope capture/checks in the actual typed client are source changes; current full typed-client assembly and real HTTP execution are not verified by this slice.

## Focused verification

Current actual Data source builds cleanly: zero warnings/errors, 1.74 seconds. No schema change, migration execution, SQL or new business EF lane.

- Scripts/Test-ProcessInstanceDetailState.ps1: **64 passed**, zero failed/skipped, terminal exit 0, 1.4502 seconds. Includes 25 new actual JSON serialization/deserialization contract cases and one empty-response state case, plus the preceding 38 state cases. Cases cover original maintenance facts/missing dates, foreign company/actor/process, malformed FIELD/revision/choices, duplicate and ambiguous actions, maintenance identity/component/time/inspection failures, mixed source families and read-only no-step behavior. Log: `%TEMP%/oilgas-process-response-state-129.log`.
- Scripts/Test-ProcessTransitionReviews.ps1: **41 passed**, zero failed/skipped, terminal exit 0, 1.6619 seconds. The actual API helper now applies the shared response check; maintenance and journal evidence/approval regressions pass. Log: `%TEMP%/oilgas-process-response-api-129.log`.
- Scripts/Test-JournalSourceRendering.ps1: **eight passed**, zero failed/skipped, terminal exit 0, 1.4204 seconds. Actual journal/maintenance components render with the current Data contract. Log: `%TEMP%/oilgas-process-response-render-129.log`.

These are actual current contract/API-helper/page-state/component checks with controlled boundaries. They do not compile/execute BusinessProcessServiceClient and its full ApiClient/diagnostics dependencies, the controller/parent page/full host, live HTTP/OIDC, browser behavior or native Beep transactions. The JSON round-trip gate does not establish transport correctness. Counts retain their individual scopes, not unique summed coverage. Whitespace check passes.

## Remaining plan

Verify the current client/controller/parent page together and original-company HTTP request/response lifecycle. Implement authoritative maintenance duty mapping, independent acceptance, permits and return-to-service/accounting handoffs through existing owners and durable transactional outcomes. Replace unsupported legacy PROJECT work-order transition/delete behavior; verify native schema/provider/date/concurrency gates. Continue every other persona and multinational requirement under the master tracker. No persona/phase/goal closure or live writes/deployment.
