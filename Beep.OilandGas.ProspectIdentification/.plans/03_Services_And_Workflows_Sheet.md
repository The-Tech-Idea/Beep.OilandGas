# Sheet 3: Services And Workflows

## Service Boundary

### Live service surfaces

- `Beep.OilandGas.Models.Core.Interfaces.IProspectIdentificationService`
  - Current live API contract.
  - Used by `Beep.OilandGas.ApiService/Controllers/Operations/ProspectIdentificationController.cs`.
- `Beep.OilandGas.LifeCycle.Services.Exploration.PPDMExplorationService`
  - Field-scoped exploration CRUD and evidence access.
  - Owns `PROSPECT`, `SEIS_ACQTN_SURVEY`, `SEIS_LINE`, and `WELL` access in the lifecycle boundary.
- `Beep.OilandGas.LifeCycle.Services.Exploration.Processes.ExplorationProcessService`
  - Orchestrates exploration workflows against the process engine.

### Parallel / legacy project-local surfaces

- `ProspectIdentification/Services/IProspectIdentificationService.cs`
- `ProspectIdentification/Services/ISeismicAnalysisService.cs`
- `ProspectIdentification/Services/IProspectEvaluationService.cs`
- `ProspectIdentification/Services/SeismicAnalysisService.cs`
- `ProspectIdentification/Services/ProspectEvaluationService.cs`

These files describe a much broader service stack than the one currently registered in the API. They should be treated as secondary planning surfaces until they are reduced to implemented, PPDM-backed operations.

## Confirmed Model Duplication Boundary

- The live service currently maps the local `Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT` shape.
- `PROSPECT` ownership is now defined as local to ProspectIdentification, not shared PPDM.
- The duplicate shared `Beep.OilandGas.PPDM39.Models.PROSPECT` definition should be treated as a retirement target, not as an active canonical model.
- Services touching `PROSPECT` should use the local ProspectIdentification model explicitly.

## Workflow Inventory

### Seeded exploration workflows already in lifecycle

- `LEAD_TO_PROSPECT`
- `PROSPECT_TO_DISCOVERY`
- `DISCOVERY_TO_DEVELOPMENT`
- `GATE_EXPLORATION_REVIEW`

## Proposed Data Ownership By Workflow

### Core reusable data

- `PROSPECT`
- `PROSPECT_WORKFLOW_STAGE`
- `Prospect`
- `ProspectEvaluation`
- `ProspectRanking`
- `ProspectRequest`
- `ProspectResponse`

This core set should remain small and should only contain canonical storage or cross-workflow aggregate shapes.

Module implication:

- only `PROSPECT` and `PROSPECT_WORKFLOW_STAGE` belong in the planned minimum live module entity list from this group.

### `LEAD_TO_PROSPECT`

- `LEAD`
- `CreateProspect`
- `UpdateProspect`
- any lead-conversion or intake-validation shapes

Module implication:

- `LEAD` is a planned module entity only if lead screening is active in the live slice.
- `CreateProspect` and `UpdateProspect` are process-owned shapes, not module entity types.

### `PROSPECT_TO_DISCOVERY`

- `PROSPECT_DISCOVERY`
- `PROSPECT_WELL`
- `PROSPECT_VOLUME_ESTIMATE`
- `PROSPECT_RISK_ASSESSMENT`
- `PROSPECT_ANALOG`
- `ProspectRiskAssessmentRequest`
- `RiskAssessment`

Module implication:

- these are planned second-wave module additions, not minimum live module members.

### `GATE_EXPLORATION_REVIEW`

- `ProspectComparison`
- `ProspectComparisonRequest`
- `ProspectRankingRequest`
- `ProspectReport`
- `ProspectReportRequest`
- `ProspectValidation`
- `EconomicEvaluation`

Module implication:

- this slice should stay mostly transient and should not expand the module entity list unless a persistent decision-history requirement is approved.

### Seismic evidence gathering

- `SeismicSurveyRequest`
- `SeismicSurveyResponse`
- `SeismicLineRequest`
- `SeismicLineResponse`
- `SeismicInterpretationAnalysis`

These shapes support a bounded evidence-gathering process and should not be treated as core prospect entities.

Module implication:

- keep shared PPDM seismic tables in the module boundary.
- keep request/response and interpretation classes out of the module entity list.

## Planned Minimum Live Exploration Slice

### Planned module entity list for the live slice

- `PROSPECT`
- `PROSPECT_WORKFLOW_STAGE`
- `SEIS_ACQTN_SURVEY`
- `SEIS_LINE`
- `WELL`
- `WELL_STATUS`

### Planned next-wave additions only after workflow activation

- `LEAD`
- `PLAY`
- `PROSPECT_DISCOVERY`
- `PROSPECT_WELL`
- `PROSPECT_RISK_ASSESSMENT`
- `PROSPECT_VOLUME_ESTIMATE`
- `PROSPECT_ANALOG`
- `PROSPECT_SEIS_SURVEY`

### Planned exclusions from the live module list

- `EXPLORATION_PROGRAM`
- `EXPLORATION_PERMIT`
- `PROSPECT_HISTORY`
- `PROSPECT_BA`
- `PROSPECT_PLAY`

These stay out until a real workflow and endpoint prove they belong in the active slice.

## Required Mapping Between Services And PPDM Tables

### Prospect inventory and screening

- `PROSPECT`
- `PLAY`
- `LEAD`
- `PROSPECT_HISTORY`
- `PROSPECT_BA`

### Seismic evidence and interpretation support

- `SEIS_ACQTN_SURVEY`
- `SEIS_LINE`
- `PROSPECT_SEIS_SURVEY`

### Well calibration and drill candidate support

- `WELL`
- `WELL_STATUS`
- `PROSPECT_WELL`

### Volumetrics, risk, and discovery tracking

- `PROSPECT_VOLUME_ESTIMATE`
- `PROSPECT_RISK_ASSESSMENT`
- `PROSPECT_DISCOVERY`
- `PROSPECT_ANALOG`

## Service Revision Rules

- Keep the live prospect service on `PPDMGenericRepository`.
- Organize data classes by owning workflow/process, with only a small reusable core set left outside those slices.
- Prefer the project prospect table shape over `FIELD` stand-ins for prospect identity.
- Do not widen the local `PROSPECT` further. New computed or compatibility data belongs in projections, request/response models, or domain results.
- Keep field scoping explicit through `PRIMARY_FIELD_ID` / `FIELD_ID` on the prospect record.
- Use workflow IDs that already exist in lifecycle instead of creating a second workflow vocabulary in the ProspectIdentification project.
- When converting `SeismicAnalysisService`, prefer `SEIS_ACQTN_SURVEY`, `SEIS_LINE`, and linked `WELL` evidence over `FIELD` plus `SEIS_SET` assumptions.
- When converting `ProspectEvaluationService`, stop treating `FIELD` as the prospect root entity; evaluate the actual prospect and attach evidence from seismic, analog, and well tables.
- If a class cannot be assigned to core or a named workflow/process, it should be treated as deferred instead of remaining in the active data surface.
- Resolve `PROSPECT` model ownership before expanding DI onto the broader project-local service interfaces.

## Current Status

- [x] Live prospect service aligned to the project prospect record shape for field, status, description, and estimated resources.
- [x] Existing lifecycle workflows identified and documented.
- [ ] `PROSPECT` still has split ownership between the local project data folder and the shared PPDM package.
- [ ] `SeismicAnalysisService` still needs PPDMGenericRepository conversion.
- [ ] `ProspectEvaluationService` still needs PPDM-first prospect/evidence mapping.
- [ ] No focused tests yet cover the live prospect service or workflow orchestration.