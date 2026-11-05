# Implementation 86: source action review and screen

Updated 2026-10-06. Extends [transition request correction](Implementation-85-Reviewed-Transition-Request.md), [source detail](Implementation-81-Recorded-Process-Source-Detail.md) and accountant queue/source/action journey. Partial: review factory and view state verified; actual API/host/rendered mutation flow and finance posting remain open.

## User workflow

The existing source detail now offers Review available actions after loading confirmed source/history. It displays only the API-reviewed eligible choices through the existing StateTransitionPanel. Company/FIELD/source reload clears the action review. A failed or late review cannot attach choices to a different source. The confirmation submits the original revision; a changed source/workflow requires an explicit reload. After a returned request the existing source reload callback refreshes recorded details/history. No request is automatically repeated and no journal posting success is inferred.

## Active owner changes

- TheTechIdea.Data owns new ProcessTransitionReview/ProcessTransitionChoice projection contracts. The existing ProcessTransitionRequest moved from Models to Data under its existing namespace; its duplicate definition is removed. ExpectedRevision is a required JSON member. Existing domain ProcessInstance/Definition/History types remain current owners; this does not claim broader legacy process ownership migration.
- Existing BusinessProcessController adds GET on the same instance/transitions resource. It uses the selected request company/account, existing bound operation service and source/FIELD/entity read authority. Two captures must agree on source/definition/history review revision. Shared failure reporting handles refused/changed review outcomes.
- ProcessTransitionReviews projects choices through existing standard app-role/current-step/independence checks and hashes company, actor, roles, full source/definition and recorded history. POST verifies the original revision and exact returned source/destination/trigger before the existing CanTransition/TransitionState operations. This preflight is not a transaction-level compare-and-swap or durable intent guarantee.
- BusinessProcessServiceClient consumes the new review through the same ApiClient; no parallel transport, process or approval store. Existing source state validates company/actor/FIELD/process/state/step/revision/choice identities before publication. Existing page and panel consume the exact review and suppress late publication after context changes.

## Focused evidence

Actual Data sources built in isolated output with zero warnings/errors.

- ProcessTransitionReviewTests: **8 passed**, zero failed/skipped, terminal exit 0, 1.0809 seconds through Test-ProcessTransitionReviews.ps1. Tests exact eligible choice, changed company/actor/roles/state/definition/history revisions and creator self-approval exclusion. Actual factory/role-check sources with actual Models/Data dependencies and standard role fixtures. Log `%TEMP%/oilgas-process-transition-reviews.log`.
- ProcessInstanceDetailStateTests: **16 passed**, zero failed/skipped, terminal exit 0, 1.1673 seconds through Test-ProcessInstanceDetailState.ps1. Includes previous 11 source lifecycle cases plus wrong company/state/actor action review, late invalidation and exact revision/choice publication. Log `%TEMP%/oilgas-source-action-state.log`.

Controller test source now explicitly refuses an unreviewed transition before mutation rather than treating that call as authorized. That suite, JSON MVC rejection, typed-client HTTP, current role/resource stores, DI, Razor compilation/rendering and actual process mutation/readback are not executed evidence here. Missing production projects are not cloned or replaced.

## Remaining full journey gates

Execute native GET-review -> original POST -> persisted source/history reload, with two companies/colliding IDs, revoked authority, stale/ambiguous definitions/history, invalid JSON, maker/checker denial and changed FIELD/binding. Verify command-owned atomic version checks, durable uncertain intent/reconciliation and provider consistency. The review SHA256 is current evidence, not proof those gates are complete.

Verify journal preparer, company/book/period/currency, actual accounting permission and persisted approval/posting through existing accounting/workflow owners; source page action availability is not accounting approval or GL posting certification. Render the queue/source/confirmation/readback journey and test keyboard/RTL/error/retry/sign-out before promoting its persona status.

No SQL, schema, business EF lane, role grant, fabricated financial totals or phase/goal closure.
