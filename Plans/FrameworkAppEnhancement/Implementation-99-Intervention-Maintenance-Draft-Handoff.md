# Implementation 99: Production intervention to maintenance draft

## Existing journey

The engineer's `/production/intervention` screen lists existing candidates and submits decisions to the current FIELD production API. Previously the request/result contracts were duplicated in the client and controller. Approval implicitly created an order with USA jurisdiction, immediately transitioned it to PLANNED and linked an AFE. A supplied order ID that was not found could silently cause another creation. The screen kept old company data, used supplemental lifecycle lookup silently and repeated initialization after decisions.

## Delivered changes

The existing decision request/result now live once in TheTechIdea.Data. They carry original FIELD and explicit jurisdiction; automatic work-order/AFE flags no longer default on. API and client use this owner, removing their old decision DTO copies. Shared checks require an explicit APPROVED/DEFERRED/REJECTED decision, rationale, matching FIELD and a finite nonnegative declared USD estimate if provided. Only approval can request downstream commands. New order creation requires an explicit jurisdiction.

The current API requests FIELD WRITE through its existing filter and checks WELL WRITE through the existing access owner. It refuses missing/mismatched original FIELD and asserts supplied order identity/FIELD/well/corrective subtype. A missing asserted order returns NotFound instead of creating a replacement. New orders remain drafts for planner review; intervention approval does not supply planning evidence. Returned handoff identity is checked and FIELD/well authority checked around subsequent orchestration. Failures after an earlier write explicitly direct read-back before repeating; these checks do not make the operations atomic.

The existing screen subscribes to company/FIELD changes, clears old candidates/selection/diagnostics and requires an explicit reload. It rejects late candidate/diagnostic responses. Candidate null responses throw rather than become successful empty lists. The decision panel adds explicit jurisdiction, captures the well/FIELD/company and freezes the request. One shared submission method handles approval/deferral/rejection. It stops passing the unsupported displayed cost estimate into the command and requests no automatic financial mutation. A matching response opens its exact recorded work-order detail for maintenance/AFE review. The review button now works from the keyboard through its own action.

Page-lifetime uncertain-decision state blocks repeated decisions for the same actor/company/FIELD/well, including after company away/back. Missing, failed or mismatched FIELD/well/decision/order responses cannot confirm the attempt. Context changes cannot navigate or trigger further actions. The UI explicitly describes the session limitation. This is not a durable command receipt.

## Focused evidence

Actual current Data source isolated build: **succeeded**, zero warnings/errors, 1.83 seconds. Log `%TEMP%/oilgas-intervention-data-build.log`.

Test-ProductionInterventionDecisionState.ps1 compiles the actual decision state and CompanyClientContext against that actual Data assembly: **24 passed**, zero failures/skips, terminal exit 0, 0.9972 seconds. Log `%TEMP%/oilgas-intervention-decision.log`. Coverage includes frozen intent, company away/back, FIELD change, mismatched/null/failed result, exact confirmation, existing order mismatch, unknown decisions, required original FIELD/rationale/jurisdiction, invalid cost, non-approval command flags and missing company. These tests prove shared validation and state only. Whitespace diff check passed.

## Open end-to-end gates

Actual typed transport, controller/filter/service registration, WELL-to-FIELD membership, standard company role admission, native parent Razor/browser rendering, provider binding/concurrency and durable idempotent command receipt/read-back remain unexecuted. Multiple domain writes are not one transaction, and a decision record failure may follow draft creation. Required original source/version and independent approval context are still missing. Physical tenant/company target stability needs provider proof.

Candidate contracts still omit source FIELD/company/version and use unsupported zero financial/production data. The detail still fabricates onset/duration/payout/recommendation values; these must be replaced with recorded evidence or explicit unavailable states before the engineer dashboard can be certified. Its separate decommissioning command and new-order URL handoff remain original-context gaps. Existing financial commands still need their own original review/authority/receipt; this slice avoids calling them automatically from approval but does not certify those commands. Maintenance material/permit/execution/supervisor return-to-service and cost handoffs remain open, as do accountant gates and all other persona plans.

No SQL, migration/schema, new business EF lane, grant or phase/goal closure.


Implementation 100: [Recorded intervention evidence](Implementation-100-Recorded-Intervention-Evidence.md) replaces unsupported metric/recommendation defaults with original company/FIELD/activity references and explicit unavailable values; 19 focused validation/static Razor cases pass. Native/provider/browser/durable action gates remain open.
