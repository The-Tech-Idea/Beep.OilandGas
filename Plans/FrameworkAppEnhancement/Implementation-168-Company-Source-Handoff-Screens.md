# Implementation 168 — Company source handoff screens

Date: 2026-10-06. Status: focused-tested invoice/procurement source-screen handoff controls, recorded workflow selection and circuit-retained intent. Other persona/dashboard delivery and durable recovery remain active.

## Visible persona journey

The existing payables invoice detail and procurement order detail pages now compose one shared MudBlazor handoff panel alongside their actual source evidence. A user explicitly loads recorded workflow choices, selects a workflow, reviews its name/version/initial assigned role and recorded source maker, then submits the reviewed handoff. No workflow is automatically selected. Confirmed receipts show assigned task count and links to the existing recorded process and assigned-work inbox. Empty choices and unconfirmed outcomes have distinct states without success links/counts.

The source currently displayed supplies its exact type, ID and revision. UI authority checks use current app-owned standard roles, Workflow.Start and Accounting.View or Procurement.View. API authority remains independent. IdentityServer remains authentication-only. No local palette, CSS, JavaScript, source mutation, permission grant or financial approval/payment/posting action is introduced.

## Recorded choices and API ownership

TheTechIdea.Data owns the catalog query/envelope; the envelope validates exact company, actor, displayed source revision, binding and unique consistent choice identities. The existing company process controller/service and complete typed BusinessProcessServiceClient expose POST start-catalog. The API captures the original company/module binding, fresh standard roles/permissions, actual owned source evidence and consistent creation audit, then obtains recorded FINANCIAL workflow definitions through the existing process service.

Each matching definition uses the existing full start review. Duplicate/ambiguous definitions, changed source/definition/catalog/binding or unavailable provider evidence refuse the entire response. Closing captures check the complete catalog and source before success. Even an empty catalog requires verified owned source and current authority. The source read/audit helper is shared with existing start review; no parallel source owner or inferred maker is added. Recorded initial creation audit does not prove every historical financial preparer or future edit.

## Pending intent and scope changes

CompanyAccountingHandoffState is scoped to the existing Blazor circuit. Explicit selection creates no intent; submission creates one frozen intent before transport. Busy state prevents duplicate submissions. Exact confirmed receipt identity is mandatory. Failed or late submission never displays success and retains the original uncertain intent. Source/company changes cancel outstanding operations, clear visible evidence and discard late results. Returning to the original company/account/source requires fresh matching choices before confirmation of the same intent; changed definition/binding/maker/revision cannot silently regenerate it. A confirmed submission cannot be submitted again from this state; the recorded process is its next destination.

The retained intent survives component navigation within the same circuit. Browser restart, a new circuit, progressed-process reconciliation and durable ledger/outbox recovery are not implemented. An inconclusive failure can conservatively retain an intent even if mutation never occurred; authoritative outcome/reconciliation UX remains necessary before abandoning or replacing it. There is no cross-database atomic source snapshot: the existing API/native handoff retains its explicit preflight, transaction authority checks and post-commit source verification.

## Focused verification

Actual Data build passed with zero warnings/errors in 1.83 seconds; log: %TEMP%/oilgas-handoff-ui-data-168.log.

Final affected actual-source gate: Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox. **670 passed, zero failed/skipped, terminal exit 0, 9.8813 seconds**. Log: %TEMP%/oilgas-handoff-ui-168-final.log. There are 15 new cases over Implementation 167: five catalog HTTP cases, four retained-state cases and six complete invoice/order workbench render cases. Counts overlap preceding gates and are not added. Whitespace verification passes. Expanded composition reports 315 existing dependency/source-precedence and nullable warnings; no warning is reported for the new panel/state/catalog or compiled workbench sources.

The confirmed page cases use the actual typed catalog/submission client, HTTP API, native workflow/task owner and complete parent/source/panel rendering. Empty cases use real recorded inactive definitions. Unconfirmed render cases explicitly inject a transport failure into submission while retaining actual HTTP catalog/source evidence. Static HTML rendering and programmatic state invocation do not certify browser clicks, interactive popovers or reconnection. The first compile used an incorrect diagnostics method and the next exposed missing accounting references in the focused composition; those were corrected to the existing Explain API and actual model source files. The intermediate 666-case pass is superseded by the final 670-case source.

Existing current-company/binding/account-resolution/diagnostics boundaries remain explicit test fixtures. Physical native records/transactions are mocks; existing app RBAC/configuration SQLite checks run in the affected composition. Production host/DI/OIDC, live provider transactions/concurrency/durability, browser/RTL/accessibility and whole-solution verification remain open, as requested.

## Next delivery

Continue accountant/procurement landing and dashboard drill-downs from confirmed recorded handoffs, followed by reviewed role-specific actions and outcome recovery. Financial preparer/reviewer command evidence, receipt/allocation acceptance and concurrent consumption, approval/post/payment, durable recovery, stored-definition reconciliation, other personas and multinational/company requirements remain active. Beep MigrationManager remains the business schema owner. No handwritten SQL, new business EF lane, live migration or phase closure.
