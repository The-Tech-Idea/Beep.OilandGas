# Beep.OilandGas Nodal Analysis - Architecture Plan

## Executive Summary

**Goal**: Deliver a PPDM-aligned nodal analysis platform to evaluate well performance, system constraints, and optimization opportunities.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.NodalAnalysis` as the system of record; services orchestrate model runs and store results with full metadata.

**Scope**: Production optimization and artificial lift design support.

---

## Architecture Principles

### 1) Reproducible Models
- Each nodal model stores inputs, correlations, and tuning factors.
- Results are versioned with run metadata.

### 2) System Consistency
- Separate inflow (IPR) and outflow (VLP) components with clear linkage.
- Preserve constraints and operating envelopes.

### 3) PPDM39 Alignment
- Persist nodal analysis models and results in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: current operating data.
- **WellTestAnalysis**: productivity and skin inputs.
- **Artificial Lift** (GasLift, Pumping): design and optimization.

---

## Target Project Structure

```
Beep.OilandGas.NodalAnalysis/
├── Services/
│   ├── NodalAnalysisService.cs (orchestrator)
│   ├── InflowService.cs
│   ├── OutflowService.cs
│   ├── OptimizationService.cs
│   └── ScenarioService.cs
├── Calculations/
│   ├── IprCalculator.cs
│   ├── VlpCalculator.cs
│   └── IntersectionSolver.cs
├── Validation/
│   ├── NodalInputValidator.cs
│   └── CorrelationValidator.cs
└── Exceptions/
    ├── NodalAnalysisException.cs
    └── CorrelationException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.NodalAnalysis`:

### Core Nodal
- NODAL_MODEL
- NODAL_INPUT
- NODAL_RUN
- NODAL_RESULT
- NODAL_ASSUMPTION

### Inflow/Outflow
- IPR_MODEL
- VLP_MODEL
- CORRELATION_REFERENCE
- TUNING_FACTOR

### Constraints
- OPERATING_ENVELOPE
- SYSTEM_CONSTRAINT
- RECOMMENDED_SETPOINT

---

## Service Interface Standards

```csharp
public interface INodalAnalysisService
{
    Task<NODAL_MODEL> CreateModelAsync(NODAL_MODEL model, string userId);
    Task<NODAL_RUN> RunAnalysisAsync(string modelId, string userId);
    Task<NODAL_RESULT> GetResultAsync(string runId);
    Task<RECOMMENDED_SETPOINT> RecommendSetpointAsync(string runId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement nodal model, inputs, runs, and results.
- Create NodalAnalysisService and validators.

### Phase 2: Calculations (Weeks 2-3)
- IPR/VLP curves and intersection solver.
- Constraint handling.

### Phase 3: Integration (Week 4)
- Integrate with WellTestAnalysis and Artificial Lift projects.

---

## Best Practices Embedded

- **Model transparency**: correlations and tuning factors stored.
- **Optimization discipline**: constraints and setpoints recorded.
- **Reproducibility**: run metadata retained.

---

## API Endpoint Sketch

```
/api/nodal/
├── /models
│   ├── POST
│   └── GET /{id}
├── /runs
│   ├── POST /run/{modelId}
│   └── GET /{id}
└── /setpoints
    └── POST /recommend/{runId}
```

---

## Success Criteria

- PPDM-aligned nodal entities persist all models and results.
- Nodal outputs are reproducible with audit trails.
- Integration with artificial lift design is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
