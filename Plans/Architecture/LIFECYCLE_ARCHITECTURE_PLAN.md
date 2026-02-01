# Beep.OilandGas LifeCycle - Architecture Plan

## Executive Summary

**Goal**: Provide the central lifecycle orchestration engine that governs stage-gate workflows, entity state transitions, and cross-project integration from discovery to abandonment.

**Key Principle**: Use **PPDM-aligned process and status tables** as the system of record; lifecycle services coordinate transitions and enforce validation across phase services.

**Scope**: Cross-cutting orchestration spanning ProspectIdentification, DevelopmentPlanning, DrillingAndConstruction, ProductionOperations, EnhancedRecovery, and Decommissioning.

---

## Architecture Principles

### 1) Process and Entity State Consistency
- Process status and entity status must always remain consistent.
- All state transitions are validated and auditable.

### 2) Stage-Gate Governance
- Stage gates are explicit, versioned, and approval-driven.
- Gate outcomes drive readiness and handoff tasks.

### 3) PPDM39 Alignment
- Use PPDM-style process workflow tables with audit columns.
- Status tables mirror entity lifecycle states.

### 4) Integration First
- Orchestrate phase services rather than duplicating logic.
- Maintain a single orchestration path for lifecycle transitions.

---

## Target Project Structure

```
Beep.OilandGas.LifeCycle/
├── Services/
│   ├── FieldOrchestrator.cs
│   ├── Processes/
│   │   ├── ProcessDefinitionInitializer.cs
│   │   ├── ProcessIntegrationHelper.cs
│   │   ├── ProcessValidator.cs
│   │   └── ProcessServiceExtensions.cs
│   ├── Exploration/Processes/*.cs
│   ├── Development/Processes/*.cs
│   ├── Production/Processes/*.cs
│   └── Decommissioning/Processes/*.cs
├── DTOs/
│   └── Process DTOs only (no domain DTOs)
├── Scripts/
│   └── ProcessWorkflowTables.sql
└── Exceptions/
    ├── LifeCycleException.cs
    └── ProcessValidationException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.LifeCycle`:

### Core Process Workflow
- PROCESS_DEFINITION
- PROCESS_STEP
- PROCESS_INSTANCE
- PROCESS_STEP_INSTANCE
- PROCESS_STATUS_HISTORY

### Entity Lifecycle States
- ENTITY_LIFECYCLE_STATE
- ENTITY_STATE_TRANSITION
- ENTITY_STATE_HISTORY

### Governance
- STAGE_GATE
- STAGE_GATE_DECISION
- APPROVAL_RECORD

---

## Service Interface Standards

```csharp
public interface ILifeCycleService
{
    Task<PROCESS_INSTANCE> StartProcessAsync(string processKey, string entityId, string userId);
    Task<PROCESS_STEP_INSTANCE> AdvanceStepAsync(string processInstanceId, string stepKey, string userId);
    Task<bool> TransitionEntityStateAsync(string entityId, string newState, string userId);
    Task<bool> RecordGateDecisionAsync(string gateId, string decision, string userId);
}
```

---

## Implementation Phases

### Phase 1: Workflow Tables + Definitions (Week 1)
- Verify process and status tables.
- Define standard lifecycle processes.

### Phase 2: Phase Orchestration (Week 2)
- Exploration, Development, Production, and Decommissioning process services.
- Integrate with phase service boundaries.

### Phase 3: Validation + Approvals (Week 3)
- Enforce validation rules for each step.
- Gate approvals and audit trails.

### Phase 4: Integration (Week 4)
- Ensure all phase projects call lifecycle orchestration for transitions.

---

## Best Practices Embedded

- **Single orchestration path**: lifecycle state changes via one service.
- **Auditable gates**: decisions stored with approver and timestamp.
- **Process transparency**: process definitions are data-driven.

---

## API Endpoint Sketch

```
/api/lifecycle/
├── /processes
│   ├── POST /start
│   ├── POST /advance
│   └── GET /{id}
├── /states
│   ├── POST /transition
│   └── GET /history/{entityId}
└── /gates
    ├── POST /decision
    └── GET /{gateId}
```

---

## Success Criteria

- Lifecycle transitions are centralized and auditable.
- All phase services integrate with lifecycle orchestration.
- Process definitions are data-driven and PPDM-aligned.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
