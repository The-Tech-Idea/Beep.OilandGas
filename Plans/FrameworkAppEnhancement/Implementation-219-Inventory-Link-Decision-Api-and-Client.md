# Implementation 219 — Independent inventory-link decision API and typed client

## Reviewer integration delivered

The existing inventory controller, CompanyInventoryReads and typed accounting client now expose independent inventory-link review and decision recording. The shared submission/result validation lives with the existing decision contracts in TheTechIdea.Data. No separate domain owner, client database access, SQL or EF business path is introduced.

GET `api/accounting/traditional/inventory/company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-decision-review` derives the proposed target from the canonical retained preparation on the owned line. It captures complete owned PO and target inventory/movement sources twice, verifies original preparer/reviewer separation and unchanged proposal/source/target/binding, and closes the original authority fence before returning. It refuses resolved proposals, malformed preparation and attempts by the preparer to review their own proposal for decision.

POST `api/accounting/traditional/inventory/company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-decision` validates the original decoded route, company, reviewer, review/binding, explicit approval/rejection, canonical intent and reason before invoking the existing native owner. Approval applies the reviewed reference with its retained independent decision atomically; rejection preserves the reference. Exact original-intent retries add no writes. Different intent/reason refuses, and a resolved line cannot receive a replacement decision review. Original proposal JSON and receiver evidence remain unchanged.

## Current app authority and confirmation

Both endpoints require authentication, a current authoritative app-assigned standard role, Procurement.View, Inventory.View and the separate Procurement.RecordInventoryLinkDecision grant. Preparation authority alone does not permit an independent decision. CompanyModuleConnection resolves current app RBAC, selected actor/company and PRODUCTION binding; IdentityServer remains authentication-only. Native transaction/read/write/confirmation callbacks revalidate the complete sorted role/grant set, company, actor and binding, followed by a final authority check before result return.

The shared submission verifies the exact canonical recorded decision, command and full original review. The typed client freezes/copies the original submission and company/account selection, encodes exact recorded IDs, and validates review/result contracts. Changed intent/decision/reference/full-review evidence, missing/null proof and late company/account changes cannot produce confirmed success. Provider refusal, invalid changed evidence and aggregate commit/rollback uncertainty return explicit 409 outcomes requiring original-intent reconciliation; unauthorized current access returns 403. No permission assignment or migration provisioning is performed.

## Focused verification

Actual Data builds with **zero warnings/errors in 1.24 seconds**. The final affected gate passes **364 checks in 15.4909 seconds**, zero failures/skips, with 416 composition warnings. Thirty-seven new cases cover actual typed-client/Kestrel/controller/native paths for approval/rejection, escaped order/line/target IDs, unknown commit and post-commit drift/recovery, current preparation-versus-decision/read grants, role/actor/binding, source/target changes, revoked authority and changed complete role/grant sets, route/company mismatch, preparer self-decision, changed intent/reason, failed/corrupt writes and resolved-review refusal. Nine typed-client response checks reject late account/company, different intent/decision/reference/full-review, missing proof and null outcome. Approved references additionally pass the existing actual receipt-inventory API/client review without stock/receipt changes.

The overlapping source gate passes **566 checks in 6.8874 seconds**, with 368 composition warnings. An initial fully qualified test entity reference conflicted with the focused composition's cached legacy model; it was corrected to the canonical actual-Data alias before the final gates. All launched handles reached terminal status and whitespace checks pass.

The HTTP composition compiles the current inventory partial endpoints and actual native owners; it does not verify the complete legacy host. Beep provider reads, metadata and transactions remain fixture boundaries. No live isolation/locking, schema/grant provisioning, browser/OIDC, keyboard/screen-reader/RTL/responsive behavior or durable circuit recovery is claimed. The existing decision column remains a Beep MigrationManager prerequisite. No phase closes.

## Next actionable persona delivery

Add independent reviewer source review, explicit approve/reject, reason/save and exact original-intent reconciliation to the existing procurement workspace/screen. Confirm complete fresh owned-order read-back before displaying an outcome. Resolve pending preparer intents from canonically recorded independent resolutions without resending a resolved preparation as a new command. Then deliver preserved-audit reassignment/supersession cycles and confirmed existing order/warehouse/inbox outcomes. Stock/returns, financial consistency, remaining personas and multinational acceptance remain active.
