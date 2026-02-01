# Beep.OilandGas Drilling and Construction - Architecture Plan

## Executive Summary

**Goal**: Deliver a PPDM-aligned drilling and construction system that executes approved development plans with rigorous well construction tracking, operational reporting, and cost control.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.DrillingAndConstruction` for all persisted drilling state; services orchestrate execution, reporting, and handoff to ProductionOperations.

**Scope**: Well construction and completions from spud to ready-to-produce (RTP).

---

## Architecture Principles

### 1) Operational Traceability
- Capture daily drilling activity, key events, and equipment status.
- Preserve audit trails for approvals and deviations.

### 2) Safety + Compliance
- Embed HSE checks and permit readiness status.
- Track well control events and safety observations.

### 3) PPDM39 Alignment
- Persist wellbore, casing, cement, and drilling events in PPDM-aligned tables.

### 4) Cross-Project Integration
- **DevelopmentPlanning**: drilling program and schedule.
- **PermitsAndApplications**: regulatory readiness.
- **ProductionOperations**: handoff to production and maintenance.
- **WellTestAnalysis**: post-completion testing data.

---

## Target Project Structure

```
Beep.OilandGas.DrillingAndConstruction/
├── Services/
│   ├── DrillingService.cs (orchestrator)
│   ├── RigScheduleService.cs
│   ├── WellConstructionService.cs
│   ├── CompletionService.cs
│   ├── DailyReportService.cs
│   └── SafetyService.cs
├── Workflows/
│   ├── DrillingExecutionWorkflow.cs
│   └── WellCompletionWorkflow.cs
├── Validation/
│   ├── DrillingPlanValidator.cs
│   ├── WellboreValidator.cs
│   └── ComplianceValidator.cs
├── Integration/
│   ├── DevelopmentPlanningBridge.cs
│   ├── PermittingBridge.cs
│   └── ProductionHandoffBridge.cs
└── Exceptions/
    ├── DrillingException.cs
    └── WellConstructionException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.DrillingAndConstruction`:

### Core Drilling
- DRILLING_PROGRAM
- DRILLING_EVENT
- DAILY_DRILLING_REPORT
- RIG
- RIG_ASSIGNMENT

### Well Construction
- WELLBORE
- CASING_STRING
- CEMENT_JOB
- MUD_REPORT
- BIT_RUN
- BHA_COMPONENT
- DIRECTIONAL_SURVEY

### Completion
- COMPLETION_EVENT
- PERFORATION
- STIMULATION_JOB
- WELL_TEST_REFERENCE

### Safety + Compliance
- HSE_OBSERVATION
- WELL_CONTROL_EVENT
- PERMIT_READINESS_STATUS

---

## Service Interface Standards

```csharp
public interface IDrillingService
{
    Task<DRILLING_PROGRAM> CreateProgramAsync(DRILLING_PROGRAM program, string userId);
    Task<DAILY_DRILLING_REPORT> SubmitDailyReportAsync(DAILY_DRILLING_REPORT report, string userId);
    Task<WELLBORE> UpdateWellboreAsync(WELLBORE wellbore, string userId);
    Task<bool> MarkReadyToProduceAsync(string wellId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement drilling program, daily report, and wellbore entities.
- Create DrillingService and validation.

### Phase 2: Construction Tracking (Weeks 2-3)
- Casing, cement, mud, and BHA tracking.
- Rig schedule and assignment.

### Phase 3: Completion Workflow (Weeks 4-5)
- Completion and stimulation tracking.
- Handoff to ProductionOperations.

### Phase 4: Safety + Compliance (Week 6)
- HSE observations and well control events.
- Permit readiness validation.

---

## Best Practices Embedded

- **Daily reporting discipline**: standardized DDR logs.
- **Well integrity**: casing/cement QA records.
- **Safety visibility**: HSE events captured and reviewed.
- **Operational readiness**: RTP gate enforced.

---

## API Endpoint Sketch

```
/api/drilling/
├── /programs
│   ├── POST
│   └── GET /{id}
├── /reports
│   ├── POST
│   └── GET /{wellId}
├── /wellbore
│   ├── PUT /{id}
│   └── POST /{id}/rtp
└── /safety
    ├── POST /events
    └── GET /{wellId}
```

---

## Success Criteria

- PPDM-aligned drilling entities persist operational state.
- Daily drilling reports and well construction data are auditable.
- RTP handoff integrates with ProductionOperations.
- No DTOs used for persisted drilling state.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
