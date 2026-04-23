# Phase 10 Pass A — Validation and Coverage

## Objective

Prove that the new route, client, API, domain, and workflow structure works across the full solution before anything is retired.

---

## Whole-Solution Workstreams

| Workstream | Project Groups | Output |
|------------|----------------|--------|
| Build and compile validation | all solution projects | build matrix and blocker list |
| Route and page validation | Web, Branchs, UserManagement | route ownership and access validation |
| Client/API boundary validation | Web, Client, ApiService, Models | typed-client and endpoint coverage proof |
| Domain-service validation | operational and support-domain projects | owner map validated against actual code |
| Persistence and governance validation | PPDM39, PPDM39.DataManagement, DataManager | state, audit, validation, and persistence checks |

---

## Required Outputs

1. Validation checklist by project group.
2. Build/run blocker list.
3. Route and authorization coverage report.
4. Typed-client and endpoint coverage report.
5. Domain-ownership mismatch list, if any.

---

## 2026-04-23 Validation Snapshot

### Produced artifact

- `Documentation/WebApp-Architecture-And-OilGas-Fit-Assessment-2026-04-23.md`

### Evidence-backed findings captured

1. Hardcoded OIDC client secret in startup (`Program.cs`) must be removed before production hardening.
2. First-run gating exists in `Components/Layout/DefaultLayout.razor` while router defaults to `Shared/MainLayout.razor`, so setup-governance behavior can diverge.
3. Inline style usage remains in several `.razor` files, violating the no-inline-style UI guardrail.
4. Shell/layout model overlap (`Shared/MainLayout.razor`, `Components/Layout/DefaultLayout.razor`, `Components/Layout/RoleBasedLayout.razor`) requires consolidation as a hardening task.
5. Domain/workflow breadth is strong for oil and gas, but enterprise readiness depends on completing the above hardening items.

### Tracker mapping

- W10-01 moved to `In Progress` (validation is underway with report published).
- W10-07 moved to `In Progress` (deferred-vs-regression separation now explicitly documented).
- New hardening tasks added in tracker: W10-08, W10-09, W10-10.

### 2026-04-23 implementation update

- `Program.cs` now sources OIDC client secret from configuration/environment instead of hardcoded literal.
- `appsettings.Development.json` no longer ships `web_secret` literal values.
- Build validation remains successful after the secret externalization change.
- `Shared/MainLayout.razor` now applies first-run setup gating through `IFirstRunService`, aligning the active runtime layout path with setup-governance requirements.
- W10-10 first cleanup batch completed for inline-style drift in `Pages/PPDM39/Compliance/GHGReport.razor`, `Components/Reservoir/ReservesChart.razor`, and `Components/Shared/PPDMMapView.razor` using MudBlazor-first composition and no additional CSS files.
- W10-10 second cleanup batch completed for filter/layout/table alignment styles in `Pages/PPDM39/Compliance/ComplianceDashboard.razor`, `Pages/PPDM39/BusinessProcess/HSEIncidents.razor`, `Pages/PPDM39/BusinessProcess/ProcessInstances.razor`, `Pages/PPDM39/BusinessProcess/ProcessDefinitions.razor`, `Pages/PPDM39/BusinessProcess/GateReviews.razor`, `Pages/PPDM39/BusinessProcess/ProcessAnalyticsDashboard.razor`, and `Pages/PPDM39/BusinessProcess/ReservesMaturationReport.razor`.
- W10-10 third cleanup batch completed for detail label and right-alignment style drift in `Pages/PPDM39/WorkOrder/WorkOrderDetail.razor`, `Pages/PPDM39/HSE/IncidentDetail.razor`, and `Pages/PPDM39/Setup/DatabaseSetupWizard.razor`.
- W10-10 fourth cleanup batch completed for dashboard and data-management static style drift in `Pages/PPDM39/WorkOrder/WorkOrderDashboard.razor`, `Pages/PPDM39/DataManagementHub.razor`, `Pages/PPDM39/DataManagement/PPDMTablePage.razor`, `Pages/PPDM39/DataManagement/WellDataManagement.razor`, and `Pages/PPDM39/DataManagement/BusinessAssociateManagement.razor`.
- W10-10 fifth cleanup batch completed for static style cleanup in `Pages/PPDM39/DataManagement/SchemaManagementPage.razor`, `Components/BusinessProcess/ObligationCalendar.razor`, and `Components/Exploration/ProspectCard.razor`.
- W10-10 sixth cleanup batch completed across shared/data/calendar components in `Components/Data/SchemaModuleManager.razor`, `Components/Data/ReferenceValuesManager.razor`, `Components/Data/CategoryTableBrowser.razor`, `Components/PPDMTreeView.razor`, `Components/Data/PPDMTableManager.razor`, and `Pages/PPDM39/WorkOrder/WorkOrderCalendar.razor`.
- W10-10 seventh cleanup adjustment removed the remaining static tree-item and data-grid header style usage in `Components/PPDMTreeView.razor` and `Components/Data/PPDMTableManager.razor`.
- W10-10 final refactor pass replaced the remaining inline style attributes with computed class maps and attribute-dictionary helpers in `Pages/PPDM39/WorkOrder/WorkOrderCalendar.razor` and `Components/Shared/PPDMMapView.razor`, and promoted shared classes into `wwwroot/css/site.css`.
- Inline-style scan trend in Razor files improved from 94 matches to 0 matched `style="..."` occurrences after batches 1-8.
- Explicit exceptions retained for W10-10 closure: exactly three runtime-computed style dictionary paths remain and are intentional: calendar slot geometry (`left/width`) in `Pages/PPDM39/WorkOrder/WorkOrderCalendar.razor`, map height fallback for non-standard height values, and map legend fallback for non-standard custom colors in `Components/Shared/PPDMMapView.razor`.
- W10-10 is closed as Done: static style drift has been removed from Razor markup and dynamic rendering exceptions are now narrow, intentional, and documented.
- W10-06/10.6 is now closed as Done: tracker-close sequencing has been completed, W10-10 completion and exception notes are finalized, and the current exception register is explicitly limited to the three runtime-computed style dictionary paths documented above.

---

## Exit Gate

Only validated paths can move to retirement in Pass B.
