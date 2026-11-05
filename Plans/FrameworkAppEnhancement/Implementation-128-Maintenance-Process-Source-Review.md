# Implementation 128: Maintenance process source review

## Next-role screen and data

The existing linked process detail screen now displays a recorded maintenance source panel when its reviewed instance refers to WORK_ORDER. It shows the recorded order number/type, original FIELD, request/due dates without defaults, instructions, separate component FIELD/well/equipment references, and each recorded inspection's category/status/maker/jurisdiction/reference. Recorded results include the actual inspector, UTC timestamp and rationale; unrecorded requirements do not fabricate them. Empty requirements explicitly leave coverage unconfirmed. An encoded link returns to the original maintenance order.

The shared Data-owned ProcessTransitionReview now carries an optional immutable WorkOrderSourceReview projection using existing component and inspection contracts. This extends the existing process action review and typed-client response, not a parallel workflow owner or store. The existing inspection DTO validation moved into WorkOrderInspectionRules.RequireProjected; both inspection state and process source review reuse it.

ProcessTransitionReviews requires exact original FIELD/order, canonical WORK_ORDER header, valid component and inspection identities and a WORK_ORDER process/entity definition. Equipment definitions cannot establish an order workflow. Missing maintenance evidence offers no action choices. The reviewed revision includes the actual header, components and inspection evidence, so changed instructions, due dates, component references, requirements or results invalidate earlier action review. Existing app-owned standard role, assignment and independent process actor checks remain authoritative; source evidence does not confer access.

The actual BusinessProcessController obtains the header and inspections from existing IWorkOrderService/IInspectionService during each of its two review captures. Existing CompanyProcessReadService reviews original FIELD/source READ authority; existing review comparison and transition submission compare the full source revision. DI uses the existing registered owners. Constructor fixtures were updated to require both existing interfaces. Full controller/host execution remains unverified in this slice.

The Web process state refuses absent, wrong-source, malformed or stale maintenance evidence before publishing actions and clears it with company/source changes. The actual new source panel renders within the existing process page. A process transition is still a process transition: permits, independent maintenance acceptance and return-to-service authorization are explicitly unconfirmed, not inferred from inspection passes, COMPLETE_DATE or a process state name. Authoritative domain acceptance and durable transactional workflow effects remain required.

## Focused verification

Current actual Data source builds cleanly: zero warnings/errors, 1.61 seconds. No schema change, SQL, migration execution or new business EF lane.

- Scripts/Test-ProcessTransitionReviews.ps1: **41 passed**, zero failed/skipped, terminal exit 0, 0.9781 seconds. Fourteen new cases cover actual projection, missing source refusal, wrong header/FIELD/source/components/inspection evidence, unsupported definition, and five source revision changes. Existing journal approval/source tests remain green. Log: `%TEMP%/oilgas-maintenance-process-128.log`.
- Scripts/Test-ProcessInstanceDetailState.ps1: **38 passed**, zero failed/skipped, terminal exit 0, 0.9111 seconds. Seven new cases cover missing/wrong/malformed source, confirmation and company invalidation. Log: `%TEMP%/oilgas-maintenance-process-state-128.log`.
- Scripts/Test-JournalSourceRendering.ps1: **eight passed**, zero failed/skipped, terminal exit 0, 1.0539 seconds. Three new actual maintenance component cases verify actual relationships, missing dates, encoded source text/route, empty evidence boundary, failure provenance/rationale and explicit unavailable state. Five journal component cases remain green. Log: `%TEMP%/oilgas-maintenance-source-render-128.log`.
- Scripts/Test-WorkOrderListState.ps1: **325 passed**, zero failed/skipped, terminal exit 0, 1.0797 seconds. Current source regression after shared inspection validation refactor. Log: `%TEMP%/oilgas-maintenance-regression-128.log`.
- Scripts/Test-WorkOrderInspectionRendering.ps1: **eight passed**, zero failed/skipped, terminal exit 0, 1.2611 seconds. Current actual inspection panel regression. Log: `%TEMP%/oilgas-maintenance-inspection-render-128.log`.

Gates use actual current Data/API helper/Web state/Razor component sources with controlled boundaries and actual MudBlazor. They do not compile/execute the complete controller and parent process page, full host/DI, HTTP/OIDC, live provider, browser interactions or durable process transactions. Whitespace check passes. Counts are per gate, not a combined unique coverage claim.

## Remaining execution

Verify current controller plus parent process screen and typed transport; approve/implement maintenance duty mapping through standard app roles; connect authoritative workflow acceptance and permits/return-to-service handoff to the existing process owner with durable receipts and transactional readback. Canonical order transition/delete still contains an unsupported legacy PROJECT path and must be replaced through authoritative owners. Review exact source population/date windows and native inspection migration/provider transactions. Complete the remaining company personas and multinational gates under their current owners. No persona, phase or full-plan closure; no live writes or deployment.
