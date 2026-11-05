# Implementation 122: Company-admin direct order grants

## Visible company-admin to maintenance workflow

The current company-admin grant screen and typed access SDK still rejected WORK_ORDER after Implementation 120 admitted the resource in server owners. This prevented the administrator from configuring and reading the direct order access required by maintenance inspection and production handoffs. This slice extends the existing `/admin/access-control/asset-access` screen, state and SDK; no new permission editor or store is added.

The screen retains its standard `[Authorize(Roles = "Administrator")]` guard and the state independently requires the selected company's Administrator role. Its existing asset selector now offers Work order and explains that FIELD inheritance does not grant order access, and that the API checks every recorded component FIELD against current company entitlement. The user enters the exact recorded order ID; no single FIELD parent or well/equipment identity is guessed. This is not a multiple-parent hierarchy visualization.

AssetGrantAdministrationState accepts direct WORK_ORDER rows and commands, defaults inheritance off when that type is selected, refuses inherited order drafts/responses, and retains original subject/company validation and exact recorded grant/revoke readback. Company switching clears the prior list and rejects late responses. The existing SDK now accepts WORK_ORDER for access checks and lists, refuses inherited order responses, and verifies that a positive access decision's level actually meets the requested READ/WRITE/DELETE operation. An API response claiming WRITE access with only READ evidence cannot become a positive client decision.

The existing focused repository test target now links CompanyWorkOrderFieldReview, so the new API helper is included when that native target is next built. That link is not native execution proof.

## Focused evidence

Scripts/Test-WorkOrderListState.ps1: **316 passed**, zero failed/skipped, terminal exit 0, 1.4397 seconds. Log: `%TEMP%/oilgas-order-admin-122.log`. Six new actual admin-state cases cover original subject/company grant and readback, inherited draft/response refusal, confirmed revocation, late company-switch response and missing Administrator authority. Four actual SDK/ClientApiTransport cases cover company-bound order lists, inherited response refusal, canonical request bodies and positive decision resource-level validation. HTTP uses a strict handler fixture, not a running API. Current actual SDK transport, exceptions, options and state partials are compiled in this gate. Earlier 306 cases remain green.

Scripts/Test-WorkOrderGrantPage.ps1 compiles the actual current AssetAccess.razor against the current focused-state and actual Data assemblies and the Web project's MudBlazor version: zero warnings/errors, terminal exit 0, 2.75 seconds. Log: `%TEMP%/oilgas-order-grant-page-122.log`. Evidence directory: `%TEMP%/oilgas-order-grant-page-a57e07f227fd4c4381596ba09b154a40`. The first check omitted the actual PPDM.Models reference needed by the existing FIELD-interest panel; linking that actual assembly corrected the gate. No missing production project is substituted. Page compilation is not rendered events, browser, routing, authentication or live grant proof.

No Data entity/schema changes in this slice. The clean isolated Data build from Implementation 121 remains the current source checkpoint. No SQL, new business EF lane, live grant, deployment or migration execution.

## Remaining persona acceptance

Verify the server grant/row-policy/metadata/provider path, including every component FIELD and revocation while review is awaited. Add truthful multiple-parent hierarchy presentation and approved maintenance duty-to-role guards. Then verify administrator grant -> maintenance landing/task -> order detail -> inspection requirement/result/readback -> independent acceptance -> return-to-service/accounting through the existing screens and APIs. Native inspection Beep migration/transactions, durable outcomes/correction history and all other company persona journeys remain open. This is focused UI/state/SDK progress, not persona/phase/goal closure.
