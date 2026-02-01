# Beep.OilandGas Development Planning - Architecture Plan

## Executive Summary

**Goal**: Build a PPDM-aligned development planning system that converts prospects into executable field development plans (FDPs), including well planning, facilities scope, schedule, CAPEX/OPEX, and risk management.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.DevelopmentPlanning` as the system of record; workflows orchestrate plan versioning, approvals, and handoffs to DrillingAndConstruction and ProductionOperations.

**Scope**: Concept select through final investment decision (FID); feeds drilling, facilities, and permitting.

---

## Architecture Principles

### 1) Plan Versioning and Governance
- Development plans are versioned with status (Draft, Reviewed, Approved, Superseded).
- All assumptions, inputs, and approvals are auditable.

### 2) Integrated Planning
- Couple wells, facilities, and infrastructure within a single plan.
- Support scenario comparisons (base, low, high).

### 3) PPDM39 Alignment
- Persist reservoir, well, and facility references in PPDM entities.
- Use PPDM IDs for wells, facilities, and areas.

### 4) Cross-Project Integration
- **EconomicAnalysis**: NPV/IRR for plan alternatives.
- **DrillingAndConstruction**: drilling schedule + AFEs.
- **PermitsAndApplications**: regulatory dependencies.
- **LifeCycle**: gate transitions and approvals.

---

## Target Project Structure

```
Beep.OilandGas.DevelopmentPlanning/
├── Services/
│   ├── DevelopmentPlanningService.cs (orchestrator)
│   ├── PlanVersionService.cs
│   ├── WellPlanningService.cs
│   ├── FacilitiesPlanningService.cs
│   ├── InfrastructurePlanningService.cs
│   └── ScheduleService.cs
├── Workflows/
│   ├── PlanApprovalWorkflow.cs
│   └── PlanScenarioWorkflow.cs
├── Validation/
│   ├── PlanValidator.cs
│   ├── ScheduleValidator.cs
│   └── CostValidator.cs
├── Integration/
│   ├── EconomicAnalysisBridge.cs
│   ├── DrillingBridge.cs
│   └── PermittingBridge.cs
└── Exceptions/
    ├── DevelopmentPlanningException.cs
    └── PlanApprovalException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.DevelopmentPlanning`:

### Core Planning
- DEVELOPMENT_PLAN
- DEVELOPMENT_PLAN_VERSION
- PLAN_SCENARIO
- PLAN_ASSUMPTION
- PLAN_APPROVAL

### Well + Reservoir Planning
- PLANNED_WELL
- WELL_TRAJECTORY_PLAN
- DRILLING_PROGRAM
- COMPLETION_CONCEPT
- RESERVOIR_DEVELOPMENT_AREA

### Facilities + Infrastructure
- FACILITY_PLAN
- SURFACE_LOCATION
- PIPELINE_CONCEPT
- UTILITIES_PLAN
- FLOWLINE_PLAN

### Costs + Schedule
- AFE_HEADER
- AFE_LINE_ITEM
- CAPEX_ESTIMATE
- OPEX_ESTIMATE
- PLAN_SCHEDULE
- MILESTONE

---

## Service Interface Standards

```csharp
public interface IDevelopmentPlanningService
{
    Task<DEVELOPMENT_PLAN> CreatePlanAsync(DEVELOPMENT_PLAN plan, string userId);
    Task<DEVELOPMENT_PLAN_VERSION> CreateVersionAsync(string planId, string userId);
    Task<PLAN_SCENARIO> AddScenarioAsync(string planId, PLAN_SCENARIO scenario, string userId);
    Task<bool> SubmitForApprovalAsync(string versionId, string userId);
    Task<bool> ApprovePlanAsync(string versionId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Plan Versioning (Week 1)
- Implement core plan/version entities and metadata.
- Add status and approval history.

### Phase 2: Core Services (Week 2)
- DevelopmentPlanningService and PlanVersionService.
- Validation of inputs and dependencies.

### Phase 3: Well + Facilities Planning (Weeks 3-4)
- Well/trajectory planning services.
- Facilities and infrastructure planning services.

### Phase 4: Cost + Schedule (Week 5)
- AFE, CAPEX/OPEX estimates, schedules.
- Scenario comparison logic.

### Phase 5: Integrations (Week 6)
- EconomicAnalysis, DrillingAndConstruction, Permits.
- LifeCycle stage-gate transitions.

---

## Best Practices Embedded

- **FDP governance**: formal approvals and version control.
- **Scenario-driven**: base/low/high planning.
- **Integrated scope**: wells + facilities + infra.
- **Transparent economics**: plan costs tied to AFE items.

---

## API Endpoint Sketch

```
/api/development/
├── /plans
│   ├── POST
│   ├── GET /{id}
│   └── POST /{id}/version
├── /scenarios
│   ├── POST
│   └── GET /{planId}
├── /approvals
│   ├── POST /submit/{versionId}
│   └── POST /approve/{versionId}
└── /schedule
    ├── POST
    └── GET /{planId}
```

---

## Success Criteria

- PPDM-aligned plan entities store all FDP data.
- Approved plans flow to drilling and permitting workflows.
- Scenario comparisons and economics are reproducible.
- No DTOs used for persisted plan state.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
