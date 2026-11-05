# Implementation 249 — Commercial rejection routing and original-preparer review

## Recorded workflow and role configuration

The existing SALES_CONTRACT company process now supports a recorded FINANCIAL approval step followed by an optional ACTION step configured as SALES_CONTRACT_REJECTION_REVIEW. Each step still requires one recorded app role; active company role/persona configuration determines task destinations. The rejection-review step must be configured before the handoff starts. No role, persona destination, grant or workflow definition is seeded by this change.

Approval follows the existing native decision gate, completes the process and marks the optional rejection-review step SKIPPED with a recorded explanation. No rejection task is created for approved terms. A rejected preparation requires its configured follow-up: the independent reviewer sees ROUTE_COMMERCIAL_REJECTION only after their exact native rejection is recorded and current company standard role, Workflow.Approve and ProductionAccounting.Approve checks pass. A missing follow-up produces a distinct configuration-required state. Existing one-step handoffs are not silently changed: retroactively changing their definition invalidates recorded start evidence and requires administrative reconciliation.

## Native rejection and follow-up

The route command closes the reviewer task set, records the original step outcome REJECTED and a native workflow decision audit with APPROVAL_STATUS REJECTED and REQUIRED_ACTION REJECT, opens the configured action step and routes its tasks through the existing CrossPersonaTaskRouter. The source preparation/decision digest, workflow definition, binding, actor, maker and step evidence are checked before and after the owned Serializable LIFECYCLE operation; only current RBAC/configuration is queried inside that transaction.

The original terms preparer must complete the follow-up. Other accounts receive COMMERCIAL_PREPARER_REQUIRED, even when they hold the configured role or are administrators. Current ProductionAccounting.Adjust, Workflow.Execute, Accounting.View and original FIELD/source authority are independently required. The follow-up also requires the exact persisted rejection-routing command and decision digest. It records COMMERCIAL_REJECTION_REVIEWED history and closes the workflow with CurrentState REJECTION_REVIEWED. Native terms remain rejected; prices, preparation, reviewer decision and reason are preserved. Reviewing rejection does not approve terms or submit an amendment.

Both command paths reuse the existing retained step intent, receipt, source and history owners. Exact routing retries do not create another task set. Authority loss during routing rolls back all graph/task/audit changes; uncertain commit recovery uses the same recorded intent. The native decision record is not rewritten during either lifecycle action.

## Screens and read-back

The process page offers Route recorded rejection to original preparer with a required routing reason and explains the preserved rejected outcome. Its existing command handler selects the explicit route action; the same retained command state confirms authoritative process/history and next tasks. Inbox evidence filters and messages distinguish recorded rejection ready to route, missing follow-up configuration, original preparer required and missing preparation grant. Original-preparer eligibility explains that terms stay rejected after review.

Terminal commercial read-back now verifies approved completion separately from rejection review: exact action history, actor/timestamp/notes, maker separation, final preparer, rejected outcomes and deliberately skipped optional work are checked. It cannot present a rejection-reviewed process as approved completion. Existing invoice completion and correction-return validation remains separate and unchanged in meaning.

## Focused verification

Canonical Data builds with zero warnings/errors. Five new real local HTTP/typed-client/native journeys exercise rejection routing and original-preparer review, approval with optional follow-up skipped, uncertain route commit/exact retry, in-transaction authority-loss rollback and a different follow-up actor. Existing missing-rejection-configuration checks are updated to the explicit state. The rejection route case renders the actual company process page and invokes its real review/routing handlers. Follow-up uses the actual retained completion state and committed source/history read-back; tests also require preparation permission and refuse an altered terminal state that would misrepresent rejected terms as approval.

The focused commercial source/workflow gate covers sixteen cases. The expanded source, company start/step, persona inbox, invoice correction-return and company process-state regression is recorded with its final result below. A stalled first expanded observation was cancelled; an isolated commercial run passed and the next expanded run produced ongoing real HTTP/test progress. No successful result is inferred from a live process or a timeout.

Verification result: the expanded regression passed all 353 tests with zero failures/skips in 2.7 minutes. A subsequent exact-source commercial rerun passed all 16 cases with zero failures/skips, verifying the final strengthened terminal read-back and rejection-as-approval refusal.

Reproduce using Scripts/Test-PurchaseOrderOwnership.ps1 with the actual canonical Data DLL, IncludeWorkflowInbox and filter FullyQualifiedName~FieldRevenueSourceTests|FullyQualifiedName~CompanyProcessStart|FullyQualifiedName~CompanyStep|FullyQualifiedName~PersonaInbox|FullyQualifiedName~CorrectionReturn|FullyQualifiedName~CompanyAccountingProcessState. Cached dependency/source composition warnings remain. HtmlRenderer and native/provider fixtures do not prove live browser/OIDC/provider acceptance.

## Remaining execution

Implement native versioned amendment/correction preparation after rejection while preserving every prior preparation and decision. Connect that version to a new independently reviewed configured handoff; do not overwrite rejected evidence or reuse an old approval. Complete durable circuit-loss and historical progressed-intent recovery, then approved delivery/effective terms to invoice/recognition and ledger read-back. Actual original-preparer persona inbox/screen journey after rejection remains an additional gate; native tasks/API/retained completion are covered here, and the rendered reviewer route is covered separately.

The wider thirteen-persona/multinational plan, royalty/partner queues, real Beep schema provisioning/isolation/concurrency, OIDC, physical browser/keyboard/RTL/accessibility and performance gates remain open. Separate PRODUCTION/LIFECYCLE checks are not an atomic cross-module transaction. The broader purchase-order handoff rendering fixture failures from Implementation 247 remain unresolved. No CA/FG phase or overall implementation goal closes.

No handwritten SQL, new business EF lane, live schema/business write, role/permission assignment, deployment or commit/push was executed. Business schema work remains on existing Beep Migration.
