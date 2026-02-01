# Beep.OilandGas Prospect Identification - Architecture Plan

## Executive Summary

**Goal**: Strengthen the `Beep.OilandGas.ProspectIdentification` project to manage the discovery phase with industry best practices (stage-gate prospect maturation, data governance, risk/volumetric/economic evaluation, and portfolio optimization) while staying PPDM39-aligned and consistent with the solution architecture.

**Key Principle**: Use **PPDM-aligned Data Entities** as the system of record (no long-lived DTOs for core domain state). Services orchestrate workflows and calculations, while all persisted entities live in `Beep.OilandGas.Models.Data.ProspectIdentification`.

**Scope**: Discovery through prospect maturity handoff to DevelopmentPlanning/Drilling. This plan feeds lifecycle orchestration in `Beep.OilandGas.LifeCycle`.

---

## Architecture Principles

### 1) Data Model Authority
- **Single source of truth**: `Beep.OilandGas.Models.Data.ProspectIdentification` (PPDM-aligned, ALL_CAPS entities).
- Minimize DTOs: transient request/response types only at API boundary.
- Preserve data provenance: each interpretation, risk assessment, and volumetric evaluation must store inputs, assumptions, and version metadata.

### 2) Stage-Gate Workflow (Discovery Best Practice)
- **Gate 0**: Data acquisition & QC
- **Gate 1**: Lead identification (seismic + geology)
- **Gate 2**: Prospect definition (trap/seal/source/reservoir)
- **Gate 3**: Risking + volumetrics
- **Gate 4**: Economic screening & portfolio ranking
- **Gate 5**: Candidate for development (handoff)

### 3) PPDM39 Alignment
- Map each discovery artifact to PPDM entities (e.g., BASIN/PLAY/PROSPECT/SEIS_SURVEY/WELL/RESERVOIR).
- Use `PPDMGenericRepository` for CRUD and `PPDMMappingService` for conversions.

### 4) Traceable Decisions
- Every evaluation step produces auditable records (inputs, assumptions, outputs, and reviewer approvals).
- Maintain an evaluation history per prospect (versioned).

### 5) Cross-Project Integration
- **EconomicAnalysis**: NPV/IRR and scenario economics.
- **DevelopmentPlanning**: prospect handoff package.
- **PPDM39 DataManagement**: authoritative storage and metadata.
- **LifeCycle**: gate transitions and orchestration.

---

## Target Project Structure

```
Beep.OilandGas.ProspectIdentification/
├── Services/
│   ├── ProspectIdentificationService.cs (orchestrator)
│   ├── ProspectEvaluationService.cs (risk/volumetrics/economics)
│   ├── SeismicAnalysisService.cs (interpretation helpers)
│   ├── DataIntegrationService.cs (ingest/QC)
│   ├── PortfolioService.cs (portfolio optimization)
│   └── DecisionSupportService.cs (recommendations, VOI)
├── Workflows/
│   ├── ProspectStageGateWorkflow.cs
│   └── ProspectPromotionRules.cs
├── Calculations/
│   ├── VolumetricsCalculator.cs
│   ├── RiskingCalculator.cs
│   ├── PlayChanceModel.cs
│   └── MonteCarloRunner.cs
├── Validation/
│   ├── DataQualityValidator.cs
│   ├── ProspectValidator.cs
│   └── RiskModelValidator.cs
├── Integration/
│   ├── EconomicAnalysisBridge.cs
│   └── DevelopmentPlanningBridge.cs
└── Exceptions/
    ├── ProspectIdentificationException.cs
    └── DataQualityException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.ProspectIdentification` (ALL_CAPS, PPDM style):

### Core Discovery Entities
- PROSPECT_IDENTIFICATION_PROJECT
- PROSPECT
- LEAD
- PLAY
- BASIN
- PROSPECT_EVALUATION
- PROSPECT_RISKING
- PROSPECT_VOLUMETRICS
- PROSPECT_ECONOMIC_SCREEN
- PROSPECT_DECISION

### Data Acquisition & Interpretation
- SEISMIC_SURVEY_REFERENCE
- SEISMIC_DATA_IMPORT
- WELL_DATA_IMPORT
- DATA_QUALITY_CHECK
- GEOLOGIC_INTERPRETATION
- STRUCTURAL_MAP
- STRATIGRAPHIC_MODEL

### Portfolio & Scheduling
- PROSPECT_PORTFOLIO
- PORTFOLIO_OPTIMIZATION_RUN
- DRILLING_SCHEDULE
- SCENARIO_ANALYSIS
- VALUE_OF_INFORMATION

### Governance & Audit
- EVALUATION_STAGE_HISTORY
- REVIEW_APPROVAL
- DATA_PROVENANCE
- ASSUMPTION_SET

---

## Service Interface Standards

Match the Accounting pattern: async CRUD + business operations.

```csharp
public interface IProspectIdentificationService
{
    Task<PROSPECT_IDENTIFICATION_PROJECT> CreateProjectAsync(PROSPECT_IDENTIFICATION_PROJECT project, string userId);
    Task<PROSPECT_IDENTIFICATION_PROJECT?> GetProjectAsync(string projectId);
    Task<PROSPECT> CreateProspectAsync(PROSPECT prospect, string userId);
    Task<PROSPECT_EVALUATION> EvaluateProspectAsync(string prospectId, string userId);
    Task<PROSPECT_PORTFOLIO> OptimizePortfolioAsync(string projectId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model & PPDM Alignment (Week 1)
- Audit existing DTO-heavy services and map them to Data entities.
- Create missing PPDM-aligned entities.
- Establish ID conventions and audit columns.

### Phase 2: Workflow Orchestration (Week 2)
- Implement stage-gate workflow in LifeCycle.
- Add promotion rules and status tracking.

### Phase 3: Evaluation Engine (Weeks 3-4)
- Volumetrics (deterministic + Monte Carlo).
- Risking (play chance + prospect chance).
- Standardized assumptions and uncertainty handling.

### Phase 4: Portfolio & Economics (Week 5)
- Integrate EconomicAnalysis for NPV/IRR.
- Portfolio ranking and drill scheduling logic.

### Phase 5: Governance & Reporting (Week 6)
- Data quality validation, audit trails, approvals.
- Reporting and export packages for handoff.

### Phase 6: Testing & Performance (Week 7)
- Unit tests for calculations.
- Integration tests for stage-gate workflow.
- Benchmark for large portfolios.

---

## Best Practices Embedded

- **Stage-Gate discipline**: gate outcomes are explicit and auditable.
- **Risking transparency**: chance factors stored and traceable.
- **Volumetric rigor**: P10/P50/P90 outputs with assumptions.
- **Portfolio balance**: value vs risk optimization.
- **Data governance**: provenance, QA, and reviewer sign-off.

---

## API Endpoint Sketch

```
/api/prospect/
├── /projects
│   ├── POST
│   ├── GET /{id}
│   └── PATCH /{id}/status
├── /prospects
│   ├── POST
│   ├── GET /{id}
│   └── POST /{id}/evaluate
├── /evaluation
│   ├── POST /pipeline
│   └── GET /history/{prospectId}
├── /portfolio
│   ├── POST /optimize
│   └── GET /{projectId}
└── /reports
    ├── /inventory/{projectId}
    └── /executive/{projectId}
```

---

## Success Criteria

- PPDM-aligned entities persist all discovery artifacts.
- Stage-gate workflow fully integrated with LifeCycle services.
- Evaluation pipeline produces auditable results (risk, volumetrics, economics).
- Portfolio optimization and handoff packages are repeatable and traceable.
- Unit test coverage > 80% for calculations and workflows.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
