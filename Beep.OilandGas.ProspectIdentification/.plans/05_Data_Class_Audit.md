# Sheet 5: Data Class Audit

## Scope

This sheet audits every C# file under:

- `Data/ProspectIdentification`
- `Data/ProspectIdentification/Domain`
- `Data/Exploration`
- `Data/Evaluation`

## Summary

- Total audited files under `Data`: 85
- `Data/ProspectIdentification` including `Domain`: 45
- `Data/Exploration`: 10
- `Data/Evaluation`: 30

## Category Counts

- Local storage-backed table shapes: 24
- Workflow/progress storage shapes: 2
- Reference/lookup storage shapes: 2
- Domain aggregate/projection shapes: 18
- Request/response contracts: 20
- Evaluation/reporting projections: 19

## What We Actually Need Now

- We do not need all 85 files for the current live ProspectIdentification slice.
- The current live path is concentrated around:
	- `PROSPECT`
	- a small set of domain projections like `Prospect`, `ProspectEvaluation`, and `ProspectRanking`
	- request/response contracts already used by the active or parallel service surfaces
- The one clearly justified workflow-progress persistence shape is `PROSPECT_WORKFLOW_STAGE`.
- Many geology, seismic, economic, and portfolio classes are planning-stage or future-feature shapes, not currently justified by live persistence or workflow state.

## Recommended Organization Model

### Core

- `PROSPECT.cs`
- `PROSPECT_WORKFLOW_STAGE.cs`
- `Prospect.cs`
- `ProspectEvaluation.cs`
- `ProspectRanking.cs`
- `ProspectRequest.cs`
- `ProspectResponse.cs`

Core should stay small. It is for canonical persistence and the shared aggregate types reused across more than one workflow.

### `LEAD_TO_PROSPECT`

- `LEAD.cs`
- `CreateProspect.cs`
- `UpdateProspect.cs`
- `ProspectValidation.cs`

### `PROSPECT_TO_DISCOVERY`

- `PROSPECT_DISCOVERY.cs`
- `PROSPECT_WELL.cs`
- `PROSPECT_VOLUME_ESTIMATE.cs`
- `PROSPECT_RISK_ASSESSMENT.cs`
- `PROSPECT_ANALOG.cs`
- `ProspectRiskAssessmentRequest.cs`
- `RiskAssessment.cs`

### `GATE_EXPLORATION_REVIEW`

- `ProspectComparison.cs`
- `ProspectComparisonRequest.cs`
- `ProspectRankingRequest.cs`
- `ProspectReport.cs`
- `ProspectReportRequest.cs`
- `EconomicEvaluation.cs`

### Seismic evidence process

- `SeismicSurveyRequest.cs`
- `SeismicSurveyResponse.cs`
- `SeismicLineRequest.cs`
- `SeismicLineResponse.cs`
- `SeismicInterpretationAnalysis.cs`

### Deferred or future process slices

- budget, permitting, migration, source-rock, trap, portfolio, and most sensitivity/probabilistic classes should stay deferred until they are backed by a real endpoint or workflow.

## Highest-Priority Consolidation Targets

- `Data/ProspectIdentification/PROSPECT.cs`
	- confirmed canonical owner for the `PROSPECT` table in ProspectIdentification
	- follow-up is to retire the unused duplicate shared definition in `Beep.OilandGas.PPDM39.Models.PROSPECT`
- `R_LEAD_STATUS.cs`
- `R_PLAY_TYPE.cs`
	- local reference/lookup classes should not become a second reference-data system when LOV/shared-reference infrastructure already exists
- local seismic and well link classes that may be better expressed through shared PPDM tables plus projections instead of bespoke storage classes

## Audit Table: Data/ProspectIdentification Root

| File | Category | Usage Signal | Recommendation |
|------|----------|--------------|----------------|
| `EXPLORATION_BUDGET.cs` | Local storage-backed table shape | No obvious current usage | Defer until budget workflow/API exists |
| `EXPLORATION_COSTS.cs` | Local storage-backed table shape | No obvious current usage | Defer or move toward accounting ownership |
| `EXPLORATION_PERMIT.cs` | Local storage-backed table shape | No obvious current usage | Defer until permitting workflow/API exists |
| `EXPLORATION_PROGRAM.cs` | Local storage-backed table shape | No obvious current usage | Defer until program management workflow/API exists |
| `LEAD.cs` | Local storage-backed table shape | No obvious current usage | Keep only if exploration workflow will persist leads; otherwise defer |
| `PLAY.cs` | Local storage-backed table shape | No obvious current usage | Keep only if play inventory is truly project-owned; otherwise review shared ownership |
| `PROSPECT.cs` | Local storage-backed table shape | Used by live `ProspectIdentificationService` | Keep now, but consolidate with shared PPDM `PROSPECT` |
| `PROSPECT_ANALOG.cs` | Local storage-backed table shape | No obvious current usage | Defer until analog-search workflow exists |
| `PROSPECT_BA.cs` | Local storage-backed table shape | No obvious current usage | Defer until BA/ownership workflow exists |
| `PROSPECT_DISCOVERY.cs` | Local storage-backed table shape | No obvious current usage | Defer until discovery handoff is implemented |
| `PROSPECT_ECONOMIC.cs` | Local storage-backed table shape | No obvious current usage | Defer or move toward economic/accounting ownership |
| `PROSPECT_FIELD.cs` | Local storage-backed table shape | No obvious current usage | Remove or defer; likely redundant with `PRIMARY_FIELD_ID` / `FIELD_ID` |
| `PROSPECT_HISTORY.cs` | Workflow/progress storage shape | No obvious current usage | Remove or defer; overlaps with common audit columns |
| `PROSPECT_MIGRATION.cs` | Local storage-backed table shape | No obvious current usage | Defer until migration-analysis feature exists |
| `PROSPECT_PLAY.cs` | Local storage-backed table shape | No obvious current usage | Keep only if prospect-play many-to-many linkage is required |
| `PROSPECT_PORTFOLIO.cs` | Local storage-backed table shape | No obvious current usage | Defer until portfolio persistence is real |
| `PROSPECT_RANKING.cs` | Local storage-backed table shape | Ranking is currently in-memory, not persisted | Rename/re-scope as projection unless persisted history is required |
| `PROSPECT_RESERVOIR.cs` | Local storage-backed table shape | No obvious current usage | Defer until reservoir characterization feature exists |
| `PROSPECT_RISK_ASSESSMENT.cs` | Local storage-backed table shape | No obvious current usage | Defer until persisted risk workflow is implemented |
| `PROSPECT_SEIS_SURVEY.cs` | Local storage-backed table shape | No obvious current usage | Defer; prefer shared seismic tables plus projections |
| `PROSPECT_SOURCE_ROCK.cs` | Local storage-backed table shape | No obvious current usage | Defer until source-rock workflow exists |
| `PROSPECT_TRAP.cs` | Local storage-backed table shape | No obvious current usage | Defer until trap-geometry workflow exists |
| `PROSPECT_VOLUME_ESTIMATE.cs` | Local storage-backed table shape | No obvious current usage | Keep only if probabilistic/p10-p50-p90 storage is required |
| `PROSPECT_WELL.cs` | Local storage-backed table shape | No obvious current usage | Defer until prospect-to-well linkage is implemented |
| `PROSPECT_WORKFLOW_STAGE.cs` | Workflow/progress storage shape | No obvious current usage yet, but justified by lifecycle workflow stages | Keep as the primary workflow-progress persistence candidate |
| `R_LEAD_STATUS.cs` | Reference/lookup storage shape | No obvious current usage | Replace with shared LOV/reference-data handling |
| `R_PLAY_TYPE.cs` | Reference/lookup storage shape | No obvious current usage | Replace with shared LOV/reference-data handling |

## Audit Table: Data/ProspectIdentification/Domain

| File | Category | Usage Signal | Recommendation |
|------|----------|--------------|----------------|
| `CreateProspect.cs` | Domain aggregate/projection | Used by `ProspectEvaluationService` | Keep |
| `CriteriaScoring.cs` | Domain aggregate/projection | No obvious current usage | Defer until scoring workflow is implemented |
| `EconomicViabilityAnalysis.cs` | Domain aggregate/projection | No obvious current usage | Defer or move toward economics ownership |
| `Fault.cs` | Domain aggregate/projection | No obvious current usage | Defer |
| `Horizon.cs` | Domain aggregate/projection | No obvious current usage | Defer |
| `MigrationPathAnalysis.cs` | Domain aggregate/projection | No obvious current usage | Defer |
| `PortfolioOptimizationResult.cs` | Domain aggregate/projection | No obvious current usage | Defer |
| `Prospect.cs` | Domain aggregate/projection | Used by live and parallel prospect services as an aggregate result | Keep |
| `ProspectEvaluation.cs` | Domain aggregate/projection | Used by live and parallel prospect services | Keep |
| `ProspectRanking.cs` | Domain aggregate/projection | Used by live ranking logic | Keep |
| `ProspectRiskAnalysisResult.cs` | Domain aggregate/projection | Used by parallel service logic | Keep but clarify as transient result |
| `ResourceEstimationResult.cs` | Domain aggregate/projection | No obvious current usage | Defer until estimation workflow is implemented |
| `RiskCategory.cs` | Domain aggregate/projection | No obvious current usage | Defer |
| `SealSourceAssessment.cs` | Domain aggregate/projection | No obvious current usage | Defer |
| `SeismicInterpretationAnalysis.cs` | Domain aggregate/projection | No obvious current usage | Defer |
| `SeismicSurvey.cs` | Domain aggregate/projection | No obvious current usage | Defer; prefer shared PPDM seismic model where possible |
| `TrapGeometryAnalysis.cs` | Domain aggregate/projection | No obvious current usage | Defer |
| `UpdateProspect.cs` | Domain aggregate/projection | Used by `ProspectEvaluationService` | Keep |

## Audit Table: Data/Exploration

| File | Category | Usage Signal | Recommendation |
|------|----------|--------------|----------------|
| `ExploratoryWellRequest.cs` | Request/response contract | No obvious current usage | Defer or remove until exploratory-well API exists |
| `ExploratoryWellResponse.cs` | Request/response contract | No obvious current usage | Defer or remove until exploratory-well API exists |
| `ProspectRequest.cs` | Request/response contract | Active contract pattern for prospect create/update flows | Keep |
| `ProspectResponse.cs` | Request/response contract | Active contract pattern for prospect query/create flows | Keep |
| `RiskAssessmentRequest.cs` | Request/response contract | No obvious current usage | Defer until risk-assessment endpoint is implemented |
| `RiskAssessmentResponse.cs` | Request/response contract | No obvious current usage | Defer until risk-assessment endpoint is implemented |
| `SeismicLineRequest.cs` | Request/response contract | No obvious current usage | Defer or move to seismic-specific slice |
| `SeismicLineResponse.cs` | Request/response contract | No obvious current usage | Defer or move to seismic-specific slice |
| `SeismicSurveyRequest.cs` | Request/response contract | No obvious current usage | Defer or move to seismic-specific slice |
| `SeismicSurveyResponse.cs` | Request/response contract | No obvious current usage | Defer or move to seismic-specific slice |

## Audit Table: Data/Evaluation

| File | Category | Usage Signal | Recommendation |
|------|----------|--------------|----------------|
| `AnalogProspect.cs` | Evaluation/reporting projection | No obvious current usage | Defer |
| `AnalogSearchRequest.cs` | Request/response contract | No obvious current usage | Defer |
| `EconomicEvaluation.cs` | Evaluation/reporting projection | Returned by parallel evaluation service logic | Keep as transient result |
| `EconomicEvaluationRequest.cs` | Request/response contract | No obvious current usage | Defer until full economic endpoint exists |
| `PeerReview.cs` | Evaluation/reporting projection | No obvious current usage | Defer |
| `PeerReviewRequest.cs` | Request/response contract | No obvious current usage | Defer |
| `PlayAnalysis.cs` | Evaluation/reporting projection | No obvious current usage | Defer |
| `PlayAnalysisRequest.cs` | Request/response contract | No obvious current usage | Defer |
| `PlayStatistics.cs` | Evaluation/reporting projection | No obvious current usage | Defer |
| `PortfolioReport.cs` | Evaluation/reporting projection | No obvious current usage | Defer unless portfolio reporting is implemented |
| `PortfolioReportRequest.cs` | Request/response contract | No obvious current usage | Defer |
| `PortfolioSummary.cs` | Evaluation/reporting projection | No obvious current usage | Defer |
| `ProbabilisticAssessment.cs` | Evaluation/reporting projection | No obvious current usage | Defer |
| `ProbabilisticAssessmentRequest.cs` | Request/response contract | No obvious current usage | Defer |
| `ProspectComparison.cs` | Evaluation/reporting projection | Returned by parallel evaluation service logic | Keep as transient result |
| `ProspectComparisonRequest.cs` | Request/response contract | Used by parallel evaluation service surface | Keep |
| `ProspectEvaluationRequest.cs` | Request/response contract | No obvious current usage | Defer |
| `ProspectRankingRequest.cs` | Request/response contract | Used by ranking service surface | Keep |
| `ProspectReport.cs` | Evaluation/reporting projection | Returned by parallel evaluation service logic | Keep as transient result |
| `ProspectReportRequest.cs` | Request/response contract | Used by report-generation service surface | Keep |
| `ProspectRiskAssessmentRequest.cs` | Request/response contract | Used by parallel seismic/evaluation service surfaces | Keep |
| `ProspectStatus.cs` | Evaluation/reporting projection | No obvious current usage; status is string-based in current storage model | Remove or defer |
| `ProspectValidation.cs` | Evaluation/reporting projection | Returned by parallel validation logic | Keep as transient result |
| `ResourceEstimate.cs` | Evaluation/reporting projection | No obvious current usage | Defer |
| `ResourceEstimateRequest.cs` | Request/response contract | No obvious current usage | Defer |
| `RiskAssessment.cs` | Evaluation/reporting projection | Returned by parallel evaluation service logic | Keep as transient result |
| `SensitivityAnalysis.cs` | Evaluation/reporting projection | No obvious current usage | Defer |
| `SensitivityAnalysisRequest.cs` | Request/response contract | No obvious current usage | Defer |
| `VolumetricAnalysis.cs` | Evaluation/reporting projection | No obvious current usage | Defer |
| `VolumetricAnalysisRequest.cs` | Request/response contract | No obvious current usage | Defer |

## Final Verdict

### Keep Now In Core Or Active Process Slices

- `PROSPECT.cs`
- `PROSPECT_WORKFLOW_STAGE.cs`
- `CreateProspect.cs`
- `UpdateProspect.cs`
- `Prospect.cs`
- `ProspectEvaluation.cs`
- `ProspectRanking.cs`
- `ProspectRequest.cs`
- `ProspectResponse.cs`
- `ProspectComparison.cs`
- `ProspectComparisonRequest.cs`
- `ProspectRankingRequest.cs`
- `ProspectReport.cs`
- `ProspectReportRequest.cs`
- `ProspectRiskAssessmentRequest.cs`
- `ProspectValidation.cs`
- `EconomicEvaluation.cs`
- `RiskAssessment.cs`

### Keep But Re-scope Or Clarify

- `LEAD.cs`
- `PLAY.cs`
- `PROSPECT_VOLUME_ESTIMATE.cs`
- `PROSPECT_RANKING.cs`
- `ProspectRiskAnalysisResult.cs`

### Defer Until Real Workflow Or Endpoint Exists

- most of `EXPLORATION_*`
- most `PROSPECT_*` geology, seismic, reservoir, and portfolio files
- most of `Data/Evaluation`
- most of the seismic and exploratory-well request/response contracts

### Remove Or Replace

- `PROSPECT_HISTORY.cs`
- `PROSPECT_FIELD.cs` if no second field-association use case is confirmed
- `R_LEAD_STATUS.cs`
- `R_PLAY_TYPE.cs`
- `ProspectStatus.cs`

## Answer To The User Question

- No, we do not need all of these classes in the current slice.
- The better target organization is core data classes first, then workflow/process-owned slices.
- Only one file is clearly justified as workflow/progress persistence today: `PROSPECT_WORKFLOW_STAGE.cs`.
- The rest of the workflow/process-related shapes are mostly transient request/response or projection models, not storage models.
- The biggest immediate cleanup is to collapse duplicate `PROSPECT` ownership and then reduce speculative local storage classes to the subset backed by real services and real workflow state.