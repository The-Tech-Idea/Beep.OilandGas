# Sheet 8: Current Class Disposition Matrix

## Purpose

Assign every current ProspectIdentification data class to exactly one disposition:

- `Keep Now`
- `Stage Later`
- `Defer`

This sheet removes folder ambiguity by also naming the target slice and target folder for every class.

## Disposition Rules

- `Keep Now` means part of the current approved live exploration slice.
- `Stage Later` means not in the first live slice, but explicitly reserved for a named workflow activation.
- `Defer` means outside the active and second-wave slices until a later business process or endpoint exists.

## Matrix

| Current class | Current folder | Disposition | Target slice | Target folder |
|------|------|------|------|------|
| `PROSPECT.cs` | `Data/ProspectIdentification/` | Keep Now | `Core` | `Data/Core/` |
| `PROSPECT_WORKFLOW_STAGE.cs` | `Data/ProspectIdentification/` | Keep Now | `Core` | `Data/Core/` |
| `Prospect.cs` | `Data/ProspectIdentification/Domain/` | Keep Now | `Core` | `Data/Core/` |
| `ProspectEvaluation.cs` | `Data/ProspectIdentification/Domain/` | Keep Now | `Core` | `Data/Core/` |
| `ProspectRanking.cs` | `Data/ProspectIdentification/Domain/` | Keep Now | `Core` | `Data/Core/` |
| `ProspectRequest.cs` | `Data/Exploration/` | Keep Now | `Core` | `Data/Core/` |
| `ProspectResponse.cs` | `Data/Exploration/` | Keep Now | `Core` | `Data/Core/` |
| `LEAD.cs` | `Data/ProspectIdentification/` | Stage Later | `LeadToProspect` | `Data/LeadToProspect/` |
| `PLAY.cs` | `Data/ProspectIdentification/` | Stage Later | `LeadToProspect` | `Data/LeadToProspect/` |
| `CreateProspect.cs` | `Data/ProspectIdentification/Domain/` | Stage Later | `LeadToProspect` | `Data/LeadToProspect/` |
| `UpdateProspect.cs` | `Data/ProspectIdentification/Domain/` | Stage Later | `LeadToProspect` | `Data/LeadToProspect/` |
| `PROSPECT_ANALOG.cs` | `Data/ProspectIdentification/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `PROSPECT_DISCOVERY.cs` | `Data/ProspectIdentification/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `PROSPECT_RISK_ASSESSMENT.cs` | `Data/ProspectIdentification/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `PROSPECT_SEIS_SURVEY.cs` | `Data/ProspectIdentification/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `PROSPECT_VOLUME_ESTIMATE.cs` | `Data/ProspectIdentification/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `PROSPECT_WELL.cs` | `Data/ProspectIdentification/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `ExploratoryWellRequest.cs` | `Data/Exploration/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `ExploratoryWellResponse.cs` | `Data/Exploration/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `RiskAssessmentRequest.cs` | `Data/Exploration/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `RiskAssessmentResponse.cs` | `Data/Exploration/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `ProspectRiskAssessmentRequest.cs` | `Data/Evaluation/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `RiskAssessment.cs` | `Data/Evaluation/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `ResourceEstimationResult.cs` | `Data/ProspectIdentification/Domain/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `VolumetricAnalysis.cs` | `Data/Evaluation/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `VolumetricAnalysisRequest.cs` | `Data/Evaluation/` | Stage Later | `ProspectToDiscovery` | `Data/ProspectToDiscovery/` |
| `PROSPECT_RANKING.cs` | `Data/ProspectIdentification/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `EconomicEvaluation.cs` | `Data/Evaluation/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `EconomicEvaluationRequest.cs` | `Data/Evaluation/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `ProspectComparison.cs` | `Data/Evaluation/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `ProspectComparisonRequest.cs` | `Data/Evaluation/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `ProspectRankingRequest.cs` | `Data/Evaluation/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `ProspectReport.cs` | `Data/Evaluation/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `ProspectReportRequest.cs` | `Data/Evaluation/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `ProspectRiskAnalysisResult.cs` | `Data/ProspectIdentification/Domain/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `ProspectValidation.cs` | `Data/Evaluation/` | Stage Later | `GateReviewAndRanking` | `Data/GateReviewAndRanking/` |
| `SeismicSurveyRequest.cs` | `Data/Exploration/` | Stage Later | `SeismicEvidence` | `Data/SeismicEvidence/` |
| `SeismicSurveyResponse.cs` | `Data/Exploration/` | Stage Later | `SeismicEvidence` | `Data/SeismicEvidence/` |
| `SeismicLineRequest.cs` | `Data/Exploration/` | Stage Later | `SeismicEvidence` | `Data/SeismicEvidence/` |
| `SeismicLineResponse.cs` | `Data/Exploration/` | Stage Later | `SeismicEvidence` | `Data/SeismicEvidence/` |
| `SeismicInterpretationAnalysis.cs` | `Data/ProspectIdentification/Domain/` | Stage Later | `SeismicEvidence` | `Data/SeismicEvidence/` |
| `SeismicSurvey.cs` | `Data/ProspectIdentification/Domain/` | Stage Later | `SeismicEvidence` | `Data/SeismicEvidence/` |
| `EXPLORATION_BUDGET.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `EXPLORATION_COSTS.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `EXPLORATION_PROGRAM.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `EXPLORATION_PERMIT.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_BA.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_ECONOMIC.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_FIELD.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_HISTORY.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_MIGRATION.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_PLAY.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_PORTFOLIO.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_RESERVOIR.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_SOURCE_ROCK.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `PROSPECT_TRAP.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `R_LEAD_STATUS.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `R_PLAY_TYPE.cs` | `Data/ProspectIdentification/` | Defer | `Deferred` | `Data/Deferred/` |
| `CriteriaScoring.cs` | `Data/ProspectIdentification/Domain/` | Defer | `Deferred` | `Data/Deferred/` |
| `EconomicViabilityAnalysis.cs` | `Data/ProspectIdentification/Domain/` | Defer | `Deferred` | `Data/Deferred/` |
| `Fault.cs` | `Data/ProspectIdentification/Domain/` | Defer | `Deferred` | `Data/Deferred/` |
| `Horizon.cs` | `Data/ProspectIdentification/Domain/` | Defer | `Deferred` | `Data/Deferred/` |
| `MigrationPathAnalysis.cs` | `Data/ProspectIdentification/Domain/` | Defer | `Deferred` | `Data/Deferred/` |
| `PortfolioOptimizationResult.cs` | `Data/ProspectIdentification/Domain/` | Defer | `Deferred` | `Data/Deferred/` |
| `RiskCategory.cs` | `Data/ProspectIdentification/Domain/` | Defer | `Deferred` | `Data/Deferred/` |
| `SealSourceAssessment.cs` | `Data/ProspectIdentification/Domain/` | Defer | `Deferred` | `Data/Deferred/` |
| `TrapGeometryAnalysis.cs` | `Data/ProspectIdentification/Domain/` | Defer | `Deferred` | `Data/Deferred/` |
| `AnalogProspect.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `AnalogSearchRequest.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `PeerReview.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `PeerReviewRequest.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `PlayAnalysis.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `PlayAnalysisRequest.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `PlayStatistics.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `PortfolioReport.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `PortfolioReportRequest.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `PortfolioSummary.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `ProbabilisticAssessment.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `ProbabilisticAssessmentRequest.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `ProspectEvaluationRequest.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `ProspectStatus.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `ResourceEstimate.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `ResourceEstimateRequest.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `SensitivityAnalysis.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |
| `SensitivityAnalysisRequest.cs` | `Data/Evaluation/` | Defer | `Deferred` | `Data/Deferred/` |

## Final No-Ambiguity Summary

### Keep Now

- exactly 7 local classes in `Core`

### Stage Later

- exactly the classes listed under `LeadToProspect`, `ProspectToDiscovery`, `GateReviewAndRanking`, and `SeismicEvidence`

### Defer

- all remaining current classes listed in the `Deferred` rows above

There are no unassigned current classes left after this matrix.