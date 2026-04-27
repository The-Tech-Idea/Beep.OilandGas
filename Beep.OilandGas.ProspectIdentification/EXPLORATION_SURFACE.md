# Exploration surface inventory (Phase 0.1)

| Surface | Role |
|---------|------|
| `Beep.OilandGas.Models.Core.Interfaces.IProspectIdentificationService` | **Live** DI contract; file: `Core/Interfaces/IProspectIdentificationService.cs` in this project |
| `IProspectTechnicalMaturationService`, `IProspectRiskEconomicAnalysisService`, `IProspectPortfolioOptimizationService` | **Live** DI (Api `Program.cs`); same scoped `ProspectIdentificationService` instance — align with partials `*.TechnicalMaturation.cs`, `*.RiskAndEconomics.cs`, `*.ProspectsAndPortfolio.cs` |
| `Beep.OilandGas.ProspectIdentification.Services.ProspectIdentificationService` | **Live** implementation (partial class: `ProspectIdentificationService.cs` core + `*.ProspectsAndPortfolio.cs`, `*.TechnicalMaturation.cs`, `*.RiskAndEconomics.cs`); registered in `ApiService/Program.cs` |
| *(removed)* `IExplorationApplicationService` | **Removed** (2026-04-27): obsolete roadmap interface + embedded DTOs lived in `Services/IExplorationApplicationService.cs`; zero in-repo consumers — **historical region map:** `.plans/10_IExplorationApplicationService_Region_Map.md` |
| `Beep.OilandGas.ApiService/Controllers/Operations/ProspectIdentificationController*.cs` | HTTP API for prospect CRUD/evaluate/rank; **`workflow/*`** POSTs call **`IProspectTechnicalMaturationService`**, **`IProspectRiskEconomicAnalysisService`**, **`IProspectPortfolioOptimizationService`** (request bodies in **`Data/Contracts/ProspectAnalysisWorkflowRequests.cs`**) — **vs lifecycle workflows:** `.plans/11_ProspectIdentificationApi_Workflow_Alignment.md` |
| `Beep.OilandGas.ApiService/Controllers/Field/ExplorationController.cs` | Field-scoped exploration; **`EnsureWorkflowProcessMatchesCurrentFieldAsync`** (uses **`IsProcessInstanceInFieldAsync`**) for **evaluate / approve / reject / promote** and all **`ExplorationWorkflowStepRequest`** step POSTs; **start** workflows validate **lead / prospect / discovery** in current field (**404** if not); lead→prospect uses **`EnsureLeadInFieldForWorkflowStartAsync`** (single check + persist unset **`LEAD.FIELD_ID`**); prospect→discovery **`POST …/workflows/prospect-to-discovery/prospect-readiness`** completes seed step **`PROSPECT_CREATION`** (display **Prospect Readiness**) without **`ILeadExplorationService`**; **`CancellationToken`** on workflow actions |
| `Beep.OilandGas.ProspectIdentification/Services/PPDMExplorationService.cs` | Field-scoped `PROSPECT`, wells, seismic (`IFieldExplorationService`); shared **`LEAD`** load for **`IsLeadInFieldAsync`**, **`EnsureLeadInFieldForWorkflowStartAsync`**, **`UpdateLeadStatusAsync`**; **`IsProspectDiscoveryInFieldAsync`** ties **`PROSPECT_DISCOVERY`** to field via parent **`PROSPECT`** |
| `Beep.OilandGas.LifeCycle/Services/Exploration/Processes/ExplorationProcessService.cs` | Process engine orchestration; literals from **`ExplorationReferenceCodes`** (process names, **`EXPLORATION`** type, entity types, step IDs, outcomes); calls `ILeadExplorationService` after **`PROSPECT_CREATION`** succeeds; **all public step methods** take optional **`CancellationToken`** (cooperative; `IProcessService` unchanged) |
| `Beep.OilandGas.ProspectIdentification/ExplorationReferenceCodes*.cs` | Partial **`ExplorationReferenceCodes`**: **`ExplorationCategoryToken`**, **`FieldLifecyclePhaseExploration`** (new-field phase; **`FieldLifecycleService`**), module id, process type, **`WELL.WELL_TYPE`** exploratory filter, lead statuses; **`ProcessEngine`** partial — process names/IDs, entity types, steps, outcomes (sync with **`ProcessDefinitionInitializer`**) |
| `Beep.OilandGas.ApiService.Tests` (`ExplorationProcessServiceTests`, `LeadExplorationServiceTests`) | References **`ProspectIdentification`** for **`ExplorationReferenceCodes`** in mocks (avoids drift from production literals) |
| `Beep.OilandGas.ProspectIdentification.Tests` | **`ProspectIdentificationService`** mapping + analysis + **validation**; **`ExplorationReferenceCodes`** gate / process / **prospect→discovery step chain** drift guards (**26** tests) |
| `Beep.OilandGas.ProspectIdentification/Data/Contracts/ExploratoryWellRequest.cs` | Default **`WellType`** uses **`ExplorationReferenceCodes.PpdmWellTypeExploration`** (PPDM exploratory well) |
| `Beep.OilandGas.Models.Core.Interfaces.ILeadExplorationService` | After `PROSPECT_CREATION`, creates field `PROSPECT` with `LEAD_ID` (`LifeCycle/Services/Exploration/LeadExplorationService.cs`; DI in Api `Program.cs`) |
| `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.cs` | Seeds exploration process definitions |
| `Beep.OilandGas.ProspectIdentification/Modules/ExplorationModule.cs` | Schema entity list; module seed runs analysis reference import then **idempotent `R_LEAD_STATUS`** (`ACTIVE`, `PROSPECT`, `CLOSED`) for `LEAD.LEAD_STATUS` / promotion |
| `Beep.OilandGas.ProspectIdentification/LeadExplorationWorkflowOptions.cs` | **`Exploration:LeadWorkflow`** — `PromotedLeadStatusCode` written to `LEAD.LEAD_STATUS` after a new field `PROSPECT` is created from the lead workflow (must match `R_LEAD_STATUS`; default aligns with seed) |

## Configuration (Api)

- **`Exploration:LeadWorkflow:PromotedLeadStatusCode`** — see `Beep.OilandGas.ApiService/appsettings.json` (overridable per environment). Bound in `Program.cs` via `Configure<LeadExplorationWorkflowOptions>`.

## Process ID cross-reference (Phase 0.3)

| Internal `ProcessId` / name | Business catalog (informative) |
|------------------------------|--------------------------------|
| `LEAD_TO_PROSPECT` / LeadToProspect | `EXP-LEAD-ASSESS` (Lead-to-Prospect Assessment) |
| `PROSPECT_TO_DISCOVERY` / ProspectToDiscovery | `EXP-PROSPECT-DISC` (Prospect-to-Discovery Evaluation) |
| `DISCOVERY_TO_DEVELOPMENT` / DiscoveryToDevelopment | Handoff toward development / FID patterns |
| `GATE_EXPLORATION_REVIEW` | `EXP-PROSPECT-APPROVE` (Prospect Investment Approval) — constants: **`ExplorationReferenceCodes.ProcessIdGateExplorationReview`**, **`StepGateExploration*`** |

Catalog detail: `Plans/BusinessProcessesPlan/Phase2_ServiceLayer/03_ProcessDefinitionCatalog.md`.

**Phase 4 (ownership + handoffs):** `.plans/12_Phase4_Data_Ownership_And_Handoffs.md` (workflow slices, `ExplorationController` step POSTs → `ExplorationProcessService`, `ProcessInstance.EntityId` rules). Complements `.plans/11_ProspectIdentificationApi_Workflow_Alignment.md`.
