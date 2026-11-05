# Implementation 111: Company-bound work-order service access

## Existing owners changed

WorkOrderService, InspectionService, SchedulingService, ContractorManagementService and CostCaptureService no longer accept the default startup connection. Their existing repository factories are asynchronous and require a callback supplied by the API's existing CompanyModuleConnection gateway. That gateway refreshes authoritative company membership/roles and resolves the verified PPDM_CORE physical binding. All five API registrations now supply that owner; no alternate store or fallback connection was introduced.

The shared internal WorkOrderDatabaseBinding retains the original company, actor and existing AssetDatabaseScope (connection plus fingerprint) for each service operation scope. Every repository construction requires a binding; every actual repository read checks it before and after the read; every insert/update/soft-delete checks it and the supplied actor immediately before writing. A changed company, actor, tenant, connection or physical fingerprint refuses rather than adopting a new target. Null/incomplete context and resolver failures do not become default-database access. This is infrastructure around the existing owners, not a replacement authorization authority or database model.

## Verification

Scripts/Test-WorkOrderListState.ps1 compiled all five changed actual service sources, WorkOrderRowValues, the binding helper, current actual service interfaces, workflow/request/DTO/state sources and tests using actual Data plus cached PPDM/Beep dependencies. No fake service or replacement missing project was introduced. **100 cases passed**, zero failures/skips, terminal exit 0, 1.0923 seconds. Log: %TEMP%/oilgas-workorder-binding-111.log. The 17 additional binding cases cover fresh resolution, original company/tenant/actor/connection/fingerprint retention, missing fields, wrong mutation actor, upstream failure, changed target after a read and refusal before provider invocation.

The previous 83 state/graph/inspection cases pass in the same gate. Old cached DTO collisions are suppressed with actual current source precedence. This demonstrates changed-source compilation and helper/state behavior; it does not execute native API Program registration, actual CompanyModuleConnection/provider reads/writes, the Razor landing or OIDC/role integration. Whitespace diff check passed.

## Remaining end state

The startup connection gap documented in Implementation 110 is corrected in these five sources. Canonical PROJECT/PROJECT_PLAN/checklist/finance relationship and column assumptions still need reconciliation against actual PPDM entities. Child operations must independently validate their original order/FIELD/resource and current action roles; scoped connection identity alone does not authorize arbitrary records. WorkOrderAccountingService and other production/accounting owners require their own binding review before the full AFE/cost handoff is certified.

Recorded returns are not a transactional snapshot. A pre-write binding check does not supply provider atomic compare-and-swap or a durable receipt. Supervisor acceptance, reviewed jurisdiction templates, material/permit prerequisites, independent verification, durable next-role handoff and real provider/browser acceptance remain open. No persona, phase or goal closure.

No handwritten SQL, schema change, new business EF lane, live migration or grants; future business schema work remains Beep MigrationManager-owned.
