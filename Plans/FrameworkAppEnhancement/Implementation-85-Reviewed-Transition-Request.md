# Implementation 85: reviewed transition request

Updated 2026-10-06. Extends [independent workflow review](Implementation-83-Independent-Workflow-Reviewer.md) and accountant queue/source/action journey. Partial: actual standard-role transition checks verified; component/controller/rendered command flow remains source-only.

## Concrete action defect

The existing StateTransitionPanel sent the transition's identifier as ToStateId, left FromStateId empty and omitted Trigger. The existing API requires a trigger and expects the destination state. The component could therefore never submit its displayed action correctly, and showed success without recorded readback.

[Existing panel](../../Beep.OilandGas.Web/Components/BusinessProcess/StateTransitionPanel.razor) now sends the reviewed current state, actual transition destination and trigger. Busy controls prevent duplicate clicks. It freezes account/company generation, process and state around the confirmation dialog and request; late context changes suppress response publication. The result must report success and exact process/source/destination through the existing result contract (NewStepId currently carries the destination state). It asks the user to review recorded source history and invokes the existing reload callback; it does not certify posting or display a recorded-success claim. No automatic retry or invented reason field.

[Existing controller](../../Beep.OilandGas.ApiService/Controllers/BusinessProcess/BusinessProcessController.cs) requires reviewed source/destination, refuses changed current state with Conflict and uses the actual definition to match exactly one source/destination/trigger tuple. Standard app-role checks apply to transition RequiredRoles; required approval refuses the recorded process creator, including Administrator. Existing current-step checks and process service CanTransition remain additional gates. The same endpoint/store remains active.

## Focused evidence

[23 role/transition cases](../../Beep.OilandGas.ApiService.Tests/ProcessStepRoleChecksTests.cs) pass, zero failed/skipped, terminal exit 0, 1.2794 seconds through [Test-ProcessStepRoles.ps1](../../Scripts/Test-ProcessStepRoles.ps1). This includes the prior 16 role/independence cases and seven transition cases: valid different transition ID vs destination, stale source, incorrect target/trigger, role denial, maker self-approval and ambiguous definition refusal. Log `%TEMP%/oilgas-process-transition-review.log`.

Actual helper source compiles against actual shared Data/Models dependencies and standard ClaimsPrincipal fixtures. Existing controller test source supplies the reviewed state and actual transition definition. Controller/component/API/client/host/browser and atomic workflow command behavior are not executed proof here.

## Finance source findings and remaining journey

The journal owner currently uses SOURCE/REMARK for book metadata and has no evident typed company/book/currency review on JOURNAL_ENTRY. Do not guess company ownership from a database connection or promote its default paths as a financial review. Existing Traditional GL routes and async JournalEntryService need reconciliation through active owners before finance posting readiness.

Connect the existing source page to a current authorized transition review and this panel, with exact retained source/definition/history revisions and actual current authority at mutation. The corrected panel is not yet rendered on that source page. Independently verify journal preparer, book/period/currency and source/resource authority, approval persistence, uncertain intent/reconciliation, atomic GL posting and recorded reload/KPI drill-down. Generic transition completion is not a financial posting decision.

No SQL, schema, new business EF lane, duplicate journal/workflow store, role grant or phase/goal closure.
