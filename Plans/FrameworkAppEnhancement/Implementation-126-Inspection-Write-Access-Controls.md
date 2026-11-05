# Implementation 126: Inspection write access controls

## Maintenance screen behavior

The existing inspection panel previously displayed requirement/result editors to every reader and relied on command refusal at the API. It now reviews actual WRITE access to the original FIELD and WORK_ORDER through the existing typed access SDK after loading recorded evidence. Denied or failed write review preserves that evidence and presents an explicit explanation without mutation controls. Unrecorded requirements remain unrecorded; a read-only view does not fabricate an inspector or recorded timestamp.

Before a submission begins, the panel repeats both current resource checks. A changed company, source or inspection review version prevents the earlier click from starting a command in a new context. WorkOrderInspectionState requires confirmed current write review before creating retained intent. It invalidates access with company/source changes and uses a separate generation to stop an older allowed response overriding a newer refusal. Pending unknown outcomes retain their existing block and are not automatically retried.

Read/access/submission failures are logged through the actual ILogger boundary and shown with fixed explanatory messages; exception text is no longer copied into this panel's product UI. Access review errors propagate from state to the reporting component. No new authorization store, role mechanism, endpoint or permission bypass is added. API command/resource gates remain independently authoritative.

This is resource-aware screen behavior. It does not supply the still-open approved planner/technician/permit-issuer duty-to-role mapping or independent supervisor acceptance. Existing page authorization and app-owned company roles remain; IdentityServer does not own these permissions.

## Focused verification

Scripts/Test-WorkOrderListState.ps1: **322 passed**, zero failed/skipped, terminal exit 0, 1.2055 seconds. Log: `%TEMP%/oilgas-inspection-access-126.log`. Six new cases cover both original resource identities, field/order denial, company-switch late responses, overlapping access reviews, and preserved read evidence after access failure. Existing retained-intent tests now explicitly review authorized resource access before testing command behavior; access is not assumed from a persona or read result. The preceding 316 cases remain green.

Scripts/Test-WorkOrderInspectionRendering.ps1: **seven passed**, zero failed/skipped, terminal exit 0, 1.3162 seconds. Log: `%TEMP%/oilgas-inspection-access-render-126.log`. The actual inspection component renders current resource checks through a strict typed SDK fixture; two new denied/failed cases retain actual requirement provenance/status and remove edit controls and fictitious recorded timestamps. Five existing actual component cases remain green. Logging is registered through the actual framework. These checks use static HtmlRenderer and controlled typed-client boundaries; they do not execute live grant checks, browser clicks, parent-page DI, HTTP/OIDC, provider transactions or role assignments.

No Data schema/entity changes. The current actual Data assembly is the clean Implementation 125 checkpoint. Whitespace check passed. Existing isolated gate warnings remain; no full clean host build is claimed.

## Remaining delivery

Implement approved maintenance duty-to-role mapping with standard role guards, then verify live API access review -> original-context command -> actual readback through the parent page and browser. Native inspection Beep migration/provider transactions, durable receipts/corrections, independent acceptance, return-to-service/accounting, regional/date/concurrency acceptance and all other persona journeys remain open. No handwritten SQL, new business EF lane, live grants/writes, migration execution, deployment or persona/phase/goal closure.
