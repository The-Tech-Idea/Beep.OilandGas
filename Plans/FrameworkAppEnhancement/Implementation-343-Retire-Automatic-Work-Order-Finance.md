# Implementation 343: Retire automatic work-order finance paths

2026-10-25. Complete within the locally verified retirement scope under CA-22 / FG-52. Continues [342](Implementation-342-Development-Investment-Review.md). Full goal remains active.

The actual work-order detail now uses the existing company/book/FIELD budget preparation, independent budget decision, recorded decision status and labor/material costing controls. The competing automatic AFE linkage, unqualified budget/actual cards and cost tab are removed. No replacement compatibility route is supplied.

## Application changes

- Removed the automatic WorkOrderAccountingService bridge, its unused WorkOrderProcessService caller and DI registrations. The old path generated financial identities and interpreted process-step completion as AFE approval without the canonical explicit book/currency/decision workflow.
- Removed the lifecycle work-order finance controller, including its unauthenticated caller-selected connection surface. Removed five old field work-order AFE/cost actions and their injected financial dependencies. The existing authenticated inspection, scheduling, contractor and recorded work-order operations remain.
- Removed the refusal-only CostCaptureService/ICostCaptureService shim and tests of that shim. Removed unused AFEResponse, work-order cost request/response and cost-line/upsert DTOs. WorkOrderDetailModel no longer publishes the unused CostLines collection.
- Removed obsolete typed-client methods and generic lifecycle-client work-order finance methods. The production intervention controller retains its explicit refusal of unowned automatic financial handoff and no longer carries the unreachable automatic-creation branch or dependency. Historical intervention references remain readable.
- During the HTTP build, removed the uncalled PPDMDevelopmentService.PlanDevelopmentAsync method that wrote development plans into FIELD_VERSION and referenced an already-retired projection. The canonical FIELD_DEVELOPMENT_PLAN draft/review/baseline owner remains the application path. The focused project no longer includes its two removed projection/input files.

No handwritten SQL, new business EF or duplicate financial owner was introduced. This removes the identified paths; it does not claim the entire repository is free of every obsolete integration or assembly forwarder.

## Focused verification

All final gates reached terminal exit 0, with zero failures/skips:

| Gate | Passed | Test duration |
| --- | ---: | ---: |
| Work-order native/source/state/access/inspection | 365 | 1.1052 seconds |
| Native budget, independent decision and labor costing | 64 | 14.1787 seconds |
| Actual work-order/intervention parent components | 44 | 2.2195 seconds |
| Inspection and queue rendering | 15 | 1.3003 seconds |
| Canonical financial contract ownership | 6 | 0.9415 seconds |
| Financial UI, state and typed clients | 123 | 20.8700 seconds |
| Actual HTTP and development boundary | 97 | 1.6397 minutes |

Counts describe separate selected gates and overlap historical checkpoints; they are not added to prior totals. Data builds with zero errors and one existing missing IdentityServer-analyzer warning, 6.58 seconds. [Exact filters, class counts, logs, source/generated/runtime hashes and limits](evidence/343-retire-automatic-work-order-finance/observed-evidence.json).

The current HTTP inspection test makes eight requests to retired field/lifecycle paths and observes 404 before exercising successful protected inspection creation, result recording, readback and revoked-authority refusals. The focused host registers the actual field controller; deleted lifecycle source absence is audited separately. This is not production-wide endpoint discovery. Actual parent renders assert that retired AFE linkage and budget/actual controls are absent while current finance controls still compose under their existing roles/capabilities.

Three initial verification issues were corrected and retained in evidence: an outdated manifest expectation omitted the already-owned maintenance return policy table; the focused HTTP project still included removed development files and an uncalled method using the old projection; and the financial contract harness omitted the actual DataManagementModels runtime dependency. Product authorization/transaction checks were not weakened.

## Remaining delivery work

[344](Implementation-344-Investment-Routing-Readiness.md) now supplies the source-screen configured workflow/current-recipient readiness review without recording assignment. Durable lifecycle assignment/recovery/inbox/reconciliation, notifications and configured investment tiers, later engineering commitments/actuals and handover, richer design/program/scenario and execution/operations workflows, bounded-queue paging, and live migration/provider/host/browser/multinational acceptance remain open under the existing plans. Current source/component/HTTP/controlled-provider proof does not close those gates.

Run affected focused tests after every coherent change and resolve failures before recording completion. Commit/push remains deferred until the full goal and plan finish.
