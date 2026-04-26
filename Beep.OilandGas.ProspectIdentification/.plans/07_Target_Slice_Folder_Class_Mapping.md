# Sheet 7: Target Slice Folder And Class Mapping

## Purpose

Define the exact target folder layout and the exact class placement for each exploration slice.

## Target Root Layout

```text
Beep.OilandGas.ProspectIdentification/
  Data/
    Core/
    LeadToProspect/
    ProspectToDiscovery/
    GateReviewAndRanking/
    SeismicEvidence/
    Deferred/
```

## Placement Rules

- `Core` holds canonical persisted exploration state and the smallest reusable aggregate models used across slices.
- `LeadToProspect` holds lead intake, screening, and conversion models.
- `ProspectToDiscovery` holds prospect maturation, risk, volume, analog, and prospect-to-well linkage models.
- `GateReviewAndRanking` holds ranking, comparison, reporting, and decision-support projections.
- `SeismicEvidence` holds seismic request/response and interpretation models.
- `Deferred` holds classes that do not belong in the first live slice and do not yet have an active workflow or endpoint.

## Exact Target Mapping By Slice

### `Core`

Target folder:
`Data/Core/`

Exact class mapping:

| Current class | Current folder | Target folder | Notes |
|------|------|------|------|
| `PROSPECT.cs` | `Data/ProspectIdentification/` | `Data/Core/` | canonical local prospect table shape |
| `PROSPECT_WORKFLOW_STAGE.cs` | `Data/ProspectIdentification/` | `Data/Core/` | persisted workflow state |
| `Prospect.cs` | `Data/ProspectIdentification/Domain/` | `Data/Core/` | shared aggregate view |
| `ProspectEvaluation.cs` | `Data/ProspectIdentification/Domain/` | `Data/Core/` | shared aggregate evaluation view |
| `ProspectRanking.cs` | `Data/ProspectIdentification/Domain/` | `Data/Core/` | reusable ranking aggregate |
| `ProspectRequest.cs` | `Data/Exploration/` | `Data/Core/` | live request contract |
| `ProspectResponse.cs` | `Data/Exploration/` | `Data/Core/` | live response contract |

Shared PPDM classes that stay external and are not moved into local folders:

- `SEIS_ACQTN_SURVEY`
- `SEIS_LINE`
- `WELL`
- `WELL_STATUS`

### `LeadToProspect`

Target folder:
`Data/LeadToProspect/`

Exact class mapping:

| Current class | Current folder | Target folder | Notes |
|------|------|------|------|
| `LEAD.cs` | `Data/ProspectIdentification/` | `Data/LeadToProspect/` | only active when lead screening is live |
| `PLAY.cs` | `Data/ProspectIdentification/` | `Data/LeadToProspect/` | keep here if play is part of lead screening |
| `CreateProspect.cs` | `Data/ProspectIdentification/Domain/` | `Data/LeadToProspect/` | conversion command/projection |
| `UpdateProspect.cs` | `Data/ProspectIdentification/Domain/` | `Data/LeadToProspect/` | prospect update command/projection |

Classes intentionally not kept in `Core` from this slice:

- `LEAD`
- `PLAY`

Reason:

- they belong to lead screening, not to the minimum shared live prospect core.

### `ProspectToDiscovery`

Target folder:
`Data/ProspectToDiscovery/`

Exact class mapping:

| Current class | Current folder | Target folder | Notes |
|------|------|------|------|
| `PROSPECT_ANALOG.cs` | `Data/ProspectIdentification/` | `Data/ProspectToDiscovery/` | analog support |
| `PROSPECT_DISCOVERY.cs` | `Data/ProspectIdentification/` | `Data/ProspectToDiscovery/` | discovery outcome linkage |
| `PROSPECT_RISK_ASSESSMENT.cs` | `Data/ProspectIdentification/` | `Data/ProspectToDiscovery/` | persisted risk support only if workflow is active |
| `PROSPECT_SEIS_SURVEY.cs` | `Data/ProspectIdentification/` | `Data/ProspectToDiscovery/` | prospect-to-seismic linkage |
| `PROSPECT_VOLUME_ESTIMATE.cs` | `Data/ProspectIdentification/` | `Data/ProspectToDiscovery/` | persisted volume support only if needed |
| `PROSPECT_WELL.cs` | `Data/ProspectIdentification/` | `Data/ProspectToDiscovery/` | prospect-to-well linkage |
| `ExploratoryWellRequest.cs` | `Data/Exploration/` | `Data/ProspectToDiscovery/` | discovery or drill-candidate request |
| `ExploratoryWellResponse.cs` | `Data/Exploration/` | `Data/ProspectToDiscovery/` | discovery or drill-candidate response |
| `RiskAssessmentRequest.cs` | `Data/Exploration/` | `Data/ProspectToDiscovery/` | workflow request |
| `RiskAssessmentResponse.cs` | `Data/Exploration/` | `Data/ProspectToDiscovery/` | workflow response |
| `ProspectRiskAssessmentRequest.cs` | `Data/Evaluation/` | `Data/ProspectToDiscovery/` | prospect-specific risk input |
| `RiskAssessment.cs` | `Data/Evaluation/` | `Data/ProspectToDiscovery/` | transient risk result |
| `ResourceEstimationResult.cs` | `Data/ProspectIdentification/Domain/` | `Data/ProspectToDiscovery/` | estimation result |
| `VolumetricAnalysis.cs` | `Data/Evaluation/` | `Data/ProspectToDiscovery/` | transient volume analysis |
| `VolumetricAnalysisRequest.cs` | `Data/Evaluation/` | `Data/ProspectToDiscovery/` | transient volume request |

### `GateReviewAndRanking`

Target folder:
`Data/GateReviewAndRanking/`

Exact class mapping:

| Current class | Current folder | Target folder | Notes |
|------|------|------|------|
| `PROSPECT_RANKING.cs` | `Data/ProspectIdentification/` | `Data/GateReviewAndRanking/` | keep only if a persisted ranking-history requirement is approved |
| `EconomicEvaluation.cs` | `Data/Evaluation/` | `Data/GateReviewAndRanking/` | transient decision-support result |
| `EconomicEvaluationRequest.cs` | `Data/Evaluation/` | `Data/GateReviewAndRanking/` | request for economics run |
| `ProspectComparison.cs` | `Data/Evaluation/` | `Data/GateReviewAndRanking/` | transient comparison result |
| `ProspectComparisonRequest.cs` | `Data/Evaluation/` | `Data/GateReviewAndRanking/` | comparison request |
| `ProspectRankingRequest.cs` | `Data/Evaluation/` | `Data/GateReviewAndRanking/` | ranking request |
| `ProspectReport.cs` | `Data/Evaluation/` | `Data/GateReviewAndRanking/` | decision/report output |
| `ProspectReportRequest.cs` | `Data/Evaluation/` | `Data/GateReviewAndRanking/` | report request |
| `ProspectRiskAnalysisResult.cs` | `Data/ProspectIdentification/Domain/` | `Data/GateReviewAndRanking/` | decision-support risk view |
| `ProspectValidation.cs` | `Data/Evaluation/` | `Data/GateReviewAndRanking/` | validation result |

### `SeismicEvidence`

Target folder:
`Data/SeismicEvidence/`

Exact class mapping:

| Current class | Current folder | Target folder | Notes |
|------|------|------|------|
| `SeismicSurveyRequest.cs` | `Data/Exploration/` | `Data/SeismicEvidence/` | seismic process request |
| `SeismicSurveyResponse.cs` | `Data/Exploration/` | `Data/SeismicEvidence/` | seismic process response |
| `SeismicLineRequest.cs` | `Data/Exploration/` | `Data/SeismicEvidence/` | line-level request |
| `SeismicLineResponse.cs` | `Data/Exploration/` | `Data/SeismicEvidence/` | line-level response |
| `SeismicInterpretationAnalysis.cs` | `Data/ProspectIdentification/Domain/` | `Data/SeismicEvidence/` | interpreted evidence output |
| `SeismicSurvey.cs` | `Data/ProspectIdentification/Domain/` | `Data/SeismicEvidence/` | local projection only if still needed beyond shared PPDM seismic models |

## Exact Deferred Mapping

Target folder:
`Data/Deferred/`

These classes are explicitly outside the first live slice and outside the current active workflow set.

| Current class | Current folder | Target folder | Reason |
|------|------|------|------|
| `EXPLORATION_BUDGET.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | no active budget workflow |
| `EXPLORATION_COSTS.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | no active cost workflow |
| `EXPLORATION_PROGRAM.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | later-stage planning workflow |
| `EXPLORATION_PERMIT.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | later-stage permitting workflow |
| `PROSPECT_BA.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | no active BA ownership workflow |
| `PROSPECT_FIELD.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | likely redundant with field ids |
| `PROSPECT_HISTORY.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | overlaps with audit/common columns |
| `PROSPECT_PLAY.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | no active linkage workflow |
| `PROSPECT_PORTFOLIO.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | no active portfolio persistence workflow |
| `PROSPECT_SOURCE_ROCK.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | no active source-rock workflow |
| `PROSPECT_TRAP.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | no active trap workflow |
| `PROSPECT_RESERVOIR.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | no active reservoir workflow |
| `PROSPECT_MIGRATION.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | no active migration workflow |
| `R_LEAD_STATUS.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | replace with shared LOV handling |
| `R_PLAY_TYPE.cs` | `Data/ProspectIdentification/` | `Data/Deferred/` | replace with shared LOV handling |

## Minimum Live Slice Summary

### Keep active now

- `Data/Core/PROSPECT.cs`
- `Data/Core/PROSPECT_WORKFLOW_STAGE.cs`
- `Data/Core/Prospect.cs`
- `Data/Core/ProspectEvaluation.cs`
- `Data/Core/ProspectRanking.cs`
- `Data/Core/ProspectRequest.cs`
- `Data/Core/ProspectResponse.cs`

plus shared PPDM evidence models:

- `SEIS_ACQTN_SURVEY`
- `SEIS_LINE`
- `WELL`
- `WELL_STATUS`

### Stage-gated additions only after approval

- everything in `LeadToProspect/`
- everything in `ProspectToDiscovery/`
- everything in `GateReviewAndRanking/`
- everything in `SeismicEvidence/`

unless that slice is explicitly activated by a real API, workflow, or UI process.