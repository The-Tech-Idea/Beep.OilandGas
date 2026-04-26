# Phase 10 Deferred Exceptions and Backlog

> Published for W10-07
> Date: 2026-04-23
> Purpose: capture intentional post-Phase-10 exceptions so remaining debt is explicit and does not get confused with regression.

## Confirmed Deferred Exceptions

| Area | Status | Why deferred | Evidence |
|------|--------|--------------|----------|
| Drawing sample host build failure | Deferred blocker | The retained drawing sample page still depends on namespaces and types not currently resolving from `Beep.OilandGas.Drawing`; this predates the Phase 10 route and dependency cleanup and remains the only web build blocker after W10-04. | `Beep.OilandGas.Web/Pages/PPDM39/DataManagement/DrawingSampleHost.razor.cs` and `dotnet build Beep.OilandGas.Web/Beep.OilandGas.Web.csproj --no-dependencies` returning only drawing-sample errors. |
| Welcome page retained outside canonical runtime entry | Intentional compatibility | `Pages/Index.razor` was moved to `/welcome` to resolve duplicate `/` ownership. It remains available as a non-canonical informational entry rather than being removed in Phase 10. | `Beep.OilandGas.Web/Pages/Index.razor` and `Beep.OilandGas.Web/Pages/Landing.razor`. |
| Template compatibility redirects | Intentional compatibility | Old/template routes are still redirected instead of removed to avoid breaking bookmarks while keeping the canonical runtime route tree clean. | `/counter`, `/fetchdata`, `/data/database-setup`, `/ppdm39/database-setup`. |
| PermitsAndApplications project build errors | Deferred blocker | The PermitsAndApplications project has 414+ pre-existing compilation errors (type mismatches, missing members, interface implementation gaps, data mapping errors) that prevent the project from building. This blocks the API service from referencing permit types and prevents the permit surfacing work (W12-02) from completing end-to-end. Table classes (`PERMIT_APPLICATION`, `PERMIT_STATUS_HISTORY`, `DRILLING_PERMIT_APPLICATION`, `ENVIRONMENTAL_PERMIT_APPLICATION`, `INJECTION_PERMIT_APPLICATION`, `JURISDICTION_REQUIREMENTS`, `MIT_RESULT`, `REQUIRED_FORM`, `APPLICATION_ATTACHMENT`) and module setup are created correctly; the pre-existing service/interface/data-mapping errors need a separate cleanup pass. | `dotnet build Beep.OilandGas.PermitsAndApplications/Beep.OilandGas.PermitsAndApplications.csproj` returns 414+ errors across services, interfaces, data mapping, and validation files. |
| Cross-module workflow integration tests | Deferred (blocked) | Integration test file `CrossModuleWorkflowTests.cs` is created with 5 workflow chain tests (intervention→work order→AFE, lease→compliance→FDP, EOR→pilot economics→evaluation, late-life→decommissioning→compliance/finance, well design→construction/drilling). Tests cannot run because the test project depends on `ApiService` which transitively depends on the broken `PermitsAndApplications` project. | `Beep.OilandGas.ApiService.Tests/CrossModuleWorkflowTests.cs` created; `dotnet build Beep.OilandGas.ApiService.Tests` fails due to transitive PermitsAndApplications errors. |

## Deferred Build-Out Gaps

| Domain gap | Current treatment | Reason not treated as Phase 10 regression |
|------------|-------------------|-------------------------------------------|
| Permit surfacing breadth | Deferred follow-up | Permit/compliance validation rules and backend support exist, but first-class page/API surfacing remains thinner than the service footprint. |
| Finance UI breadth vs service depth | Deferred follow-up | Accounting and finance services are broader than the currently surfaced workflow pages; this is a coverage backlog item, not a newly introduced defect. |
| Engineering modules beyond exposed workbenches | Deferred follow-up | Several engineering/calculation modules remain backend or support libraries without first-class workflow surfaces; Phase 10 only retired accidental web coupling and preserved active exposed seams. |
| Thin operational domains | Deferred follow-up | `ProductionOperations`, `EnhancedRecovery`, and `DrillingAndConstruction` still have maturity gaps in implementation depth; these were already classified as thin or partial domains during earlier phases. |

## Completed Phase 10 Retirements That Close Prior Ambiguity

| Item | Outcome |
|------|---------|
| Duplicate runtime layout layers | Closed; runtime shell reduced to `MainLayout` plus `AccountLayout`. |
| Root-route duplication | Closed; `/` is owned only by `Pages/Landing.razor`. |
| Hidden legacy nav drift | Closed; stale hidden nav block removed and nav links now all resolve to pages. |
| Deprecated setup entry routes | Closed through redirect compatibility to `/ppdm39/setup`. |
| Web direct project overreach | Closed; 26 unused direct references removed and remaining graph reduced to explicit owners. |

## Recommended Post-Phase-10 Follow-Up Order

1. Resolve `DrawingSampleHost` and `Beep.OilandGas.Drawing` type/namespace drift so the web project can build cleanly without the known exception.
2. Expand permit/compliance workflow surfacing where backend validation and document-generation support already exists.
3. Broaden finance UI coverage to match the service-heavy accounting footprint.
4. Reassess remaining template compatibility redirects after downstream consumers have moved to canonical routes.
