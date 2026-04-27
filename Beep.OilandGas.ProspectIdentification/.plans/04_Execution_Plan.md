# Refactor Plan: Prospect Identification PPDM Alignment

## Current State

The project owns its module registration and a live API-facing prospect service, but the exploration module was still carrying pool-development tables and the broader project service surface remains split between a small live contract and several larger, mostly parallel interfaces. **`SeismicAnalysisService`** and **`ProspectEvaluationService`** already use **`PPDMGenericRepository`** for **`SEIS_ACQTN_SURVEY`** and **`PROSPECT`** (field scoping uses **`PRIMARY_FIELD_ID` / `FIELD_ID`** on the prospect row, not a `FIELD` table stand-in). Optional next depth is **`SEIS_LINE`**, **`PROSPECT_SEIS_SURVEY`**, and other evidence links. The project currently contains 95 C# files, including 45 under `Data/ProspectIdentification`, and there is a confirmed duplicate `PROSPECT` model between the local data folder and the shared PPDM package. The current data planning also classifies files by technical type, but not yet by the workflow or process that owns them.

## Target State

Prospect identification should be anchored on real exploration PPDM tables, with a clear distinction between core canonical data classes and workflow/process-owned projections. The live service and lifecycle workflows should share a single exploration vocabulary: `LEAD`, `PROSPECT`, seismic evidence, exploratory wells, and the seeded exploration process IDs. `PROSPECT` should end up with one canonical table-model owner, with any extra business or analytics fields moved out to projections or mapping layers. The active data surface should be organized as a small core plus workflow/process slices such as lead conversion, discovery handoff, gate review, and seismic evidence.

## Affected Files

| File | Change Type | Dependencies |
|------|-------------|--------------|
| `ProspectIdentification/Modules/ExplorationModule.cs` | modify | blocked by actual exploration table ownership |
| `ProspectIdentification/Services/ProspectIdentificationService.cs` | modify | consumed by ApiService registration and controller |
| `ProspectIdentification/Data/ProspectIdentification/PROSPECT.cs` | future modify or retire | currently duplicates shared PPDM `PROSPECT` ownership |
| `ProspectIdentification/Services/SeismicAnalysisService.cs` | optional extend | PPDM surveys via repo; add `SEIS_LINE` / prospect–survey link tables when workflows need them |
| `ProspectIdentification/Services/ProspectEvaluationService.cs` | optional extend | PPDM `PROSPECT` + survey count; enrich with `PROSPECT_SEIS_SURVEY` / analog tables when wired |
| `ProspectIdentification/.plans/*.md` | create | documents sequencing and remaining work |
| `ProspectIdentification/.plans/06_ExplorationModule_Approval_Sheet.md` | create | blocks any later module-list edit |
| `ProspectIdentification/.plans/07_Target_Slice_Folder_Class_Mapping.md` | create | defines exact target folder/class placement |
| `ProspectIdentification/.plans/08_Current_Class_Disposition_Matrix.md` | create | defines final keep-now, stage-later, defer classification |
| `ProspectIdentification/.plans/09_Implementation_Trigger_Sheet.md` | create | defines when stage-later slices may move into active code |
| `ProspectIdentification/.plans/10_IExplorationApplicationService_Region_Map.md` | create | maps each roadmap `#region` to deferred vs live substitute |
| `ProspectIdentification/.plans/11_ProspectIdentificationApi_Workflow_Alignment.md` | create | maps HTTP routes to lifecycle workflow ownership |
| `ProspectIdentification/.plans/12_Phase4_Data_Ownership_And_Handoffs.md` | create | workflow IDs → data slices; step handoffs; `ProcessInstance` entity rules |

## Execution Plan

### Phase 0: Exploration Business Process Foundation

- [x] Start the plan from the real exploration business flow: lead screening, prospect maturation, evidence gathering, risk and volume assessment, gate review, well program, and outcome capture.
- [x] Define the minimum data-model families needed for that flow: core business objects, evidence and support objects, workflow state, and handoff or outcome objects.
- [x] Record the rule that classes without an active workflow or endpoint stay deferred.

### Phase 1: Module And Live Service

- [x] Approve the planned minimum live module list in the plan before any code edit to `ExplorationModule.cs`.
- [x] Correct the module boundary rule: `ExplorationModule.cs` is the full project table registry, not the minimum live runtime slice.
- [x] Keep `ExplorationModule.cs` on the full project table registry while using the runtime slice only for service/workflow/data ownership.
- [ ] Plan second-wave additions only after workflow activation: `LEAD`, `PLAY`, `PROSPECT_DISCOVERY`, `PROSPECT_WELL`, `PROSPECT_RISK_ASSESSMENT`, `PROSPECT_VOLUME_ESTIMATE`, `PROSPECT_ANALOG`, and `PROSPECT_SEIS_SURVEY`.
- [ ] Plan exclusions from the live module list: `EXPLORATION_PROGRAM`, `EXPLORATION_PERMIT`, `PROSPECT_HISTORY`, `PROSPECT_BA`, and `PROSPECT_PLAY`.
- [x] Align `ProspectIdentificationService.cs` to the prospect record field/status/resource conventions already used by the project data shape.
- [x] Verify the touched files with focused diagnostics.

Implementation note:

- `Beep.OilandGas.ProspectIdentification.csproj` builds clean after restoring the full module registry.
- `Beep.OilandGas.ApiService.csproj` consumer validation is currently blocked by a running `Beep.OilandGas.ApiService` process holding output DLL locks, not by a compile error in the ProspectIdentification slice.

### Phase 2: Data Folder Rationalization

- [x] Inventory the current 95-file project surface and classify the 45 files under `Data/ProspectIdentification` into canonical PPDM table shapes versus projections.
- [x] Extend that audit across the full `Data` folder and record the results in `05_Data_Class_Audit.md`.
- [ ] Define the target organization as a small `Core` set plus workflow/process-owned slices.
- [ ] Map each active class to one of: `Core`, `LeadToProspect`, `ProspectToDiscovery`, `GateReviewAndRanking`, or `SeismicEvidence`.
- [ ] Resolve the confirmed duplicate `PROSPECT` ownership by either adopting the shared PPDM model plus mapping or renaming/shrinking the local shape into an explicit wrapper or projection.
- [ ] Keep only the storage-backed PPDM table classes that truly must remain project-local inside the `Core` set.
- [ ] Move active request/response and aggregate models under their owning workflow/process slice instead of leaving them in flat `Exploration` and `Evaluation` buckets.
- [ ] Leave future-only classes deferred until they are backed by a real endpoint or workflow.
- [ ] Audit other local PPDM-style classes against the shared PPDM package before preserving them in the project.

Implementation note:

- `ProspectIdentificationService` is now split into partial classes by concern:
  - `Services/ProspectIdentificationService.cs` (constructor/core helpers)
  - `Services/ProspectIdentificationService.ProspectsAndPortfolio.cs`
  - `Services/ProspectIdentificationService.TechnicalMaturation.cs`
  - `Services/ProspectIdentificationService.RiskAndEconomics.cs`
- Workflow-scoped interfaces were added in `Core/Interfaces` and mapped in DI to the same scoped service instance:
  - `IProspectTechnicalMaturationService`
  - `IProspectRiskEconomicAnalysisService`
  - `IProspectPortfolioOptimizationService`
- Exploration module seed baseline is expanded (2026-04-27):
  - `R_LEAD_STATUS` defaults for lead lifecycle transitions
  - `R_PLAY_TYPE` defaults for play screening/classification
  - `R_EXPLORATION_REFERENCE_CODE` for workflow/process/entity/step/outcome/category token catalogs used by controllers/process services.

### Phase 3: Service Convergence

- [x] `SeismicAnalysisService` uses **`PPDMGenericRepository`** for **`SEIS_ACQTN_SURVEY`** (and **`PROSPECT`** for create validation). Synthetic attribute/AVO/report paths remain stubs. **Optional:** add **`SEIS_LINE`**, **`PROSPECT_SEIS_SURVEY`**, and richer evidence persistence.
- [x] `ProspectEvaluationService` reads/writes **`PROSPECT`** via **`PPDMGenericRepository`** and counts active surveys on **`SEIS_ACQTN_SURVEY`** (`AREA_TYPE` = `PROSPECT`). **Optional:** link rows in **`PROSPECT_SEIS_SURVEY`** and related evidence tables instead of inferring only from `AREA_ID`.
- [x] Introduce workflow-scoped analysis interfaces and wire them in DI to implemented operations on the existing service.
- [x] Move selected `ProspectIdentificationController` operations to workflow-scoped interfaces (maturation, risk/economics, portfolio optimization) to narrow controller dependencies.
- [x] Map each former `IExplorationApplicationService` `#region` to deferred, partial overlap, or live substitute — see **`10_IExplorationApplicationService_Region_Map.md`** (file **removed** 2026-04-27).

### Phase 3.1: Endpoint Consumption Narrowing (execution-ready)

- [x] Add/extend endpoints that consume `IProspectTechnicalMaturationService` directly for:
  - seismic interpretation analysis
  - resource estimation
  - trap/migration/seal-source analysis
- [x] Add/extend endpoints that consume `IProspectRiskEconomicAnalysisService` directly for:
  - risk assessment
  - economic viability screening
- [x] Add/extend endpoints that consume `IProspectPortfolioOptimizationService` directly for portfolio optimization on ranked prospects.
- [x] Keep existing compatibility routes stable; do not break the current `IProspectIdentificationService` routes.
- [x] Add controller tests for workflow endpoints (delegate + validation); extend for full OpenAPI examples when routes stabilize.

### Phase 4: Workflow Alignment

- [x] Map prospect **HTTP surface** to seeded lifecycle workflow IDs (`LEAD_TO_PROSPECT`, `PROSPECT_TO_DISCOVERY`, `GATE_EXPLORATION_REVIEW`) — see **`11_ProspectIdentificationApi_Workflow_Alignment.md`** (field **`ExplorationController`** owns instance advance; **`ProspectIdentificationController`** = data/analysis helpers).
- [x] Use those same workflow IDs to drive **data-class ownership / folder slices** — see **`12_Phase4_Data_Ownership_And_Handoffs.md`** §1 (ties to **`02_Data_Folder_Sheet.md`** target slices).
- [x] Add explicit **workflow handoff** documentation for risk, volume, economics, discovery — **`12_Phase4_Data_Ownership_And_Handoffs.md`** §2 (maps **`ExplorationController`** routes → **`ExplorationProcessService`** → step IDs).
- [x] Document **`ProcessInstance.EntityId` / `EntityType` / `FieldId`** rules so anchors stay real **`LEAD`** / **`PROSPECT`** / discovery ids — **`12_Phase4_Data_Ownership_And_Handoffs.md`** §3.

### Phase 5: Tests And Cleanup

- [x] Add focused tests for prospect service **mapping and deterministic analysis** (`Beep.OilandGas.ProspectIdentification.Tests`: `ProspectIdentificationServiceMappingTests`, `ProspectIdentificationServiceAnalysisTests`). **Integration** tests for `PPDMGenericRepository` CRUD remain optional.
- [x] Add controller tests for the prospect compatibility routes (`ProspectIdentificationControllerCompatibilityTests.cs`).
- [x] **Removed** roadmap-only **`IExplorationApplicationService`** + embedded DTOs (`Services/IExplorationApplicationService.cs`) after grep showed no in-repo / BeepDM consumers (2026-04-27).

## Rollback Plan

If a later service conversion fails:

1. Keep the current live API service on the small `Beep.OilandGas.Models.Core.Interfaces.IProspectIdentificationService` contract.
2. Revert individual service rewrites without touching approved planning docs.
3. Preserve lifecycle workflow definitions and field-scoped exploration services as the stable anchor.

If planning changes are not approved:

1. Leave `ExplorationModule.cs` unchanged.
2. Treat the module class layout as unresolved and do not trim or add entity types in code.

## Risks

- The project has multiple overlapping prospect model and service surfaces; refactors must avoid changing the live API contract accidentally.
- The confirmed duplicate `PROSPECT` model makes imports, ownership, and future persistence mapping easy to drift.
- `SeismicAnalysisService` and `ProspectEvaluationService` are PPDM-repository-based for surveys/prospects; optional evidence tables (`SEIS_LINE`, `PROSPECT_SEIS_SURVEY`) can still drift from lifecycle boundaries if added without following **`12_`** slice rules.
- Prospect-identification test coverage is currently missing.