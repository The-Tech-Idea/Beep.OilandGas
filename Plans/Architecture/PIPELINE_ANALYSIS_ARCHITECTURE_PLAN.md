# Beep.OilandGas Pipeline Analysis - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned pipeline analysis platform for hydraulics, capacity evaluation, and integrity-related constraints impacting production systems.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.PipelineAnalysis` as the system of record; services orchestrate analysis runs and store results with full metadata.

**Scope**: Flow assurance and capacity constraints for production and compression systems.

---

## Architecture Principles

### 1) Reproducible Analysis
- Preserve pipeline configuration, fluid properties, and boundary conditions.
- Results are versioned and auditable.

### 2) Operational Integrity
- Link pipeline constraints to facility and production limits.

### 3) PPDM39 Alignment
- Persist pipeline models and analysis data in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: facility throughput and constraints.
- **CompressorAnalysis**: upstream compression impacts.
- **GasProperties/OilProperties**: fluid model inputs.

---

## Target Project Structure

```
Beep.OilandGas.PipelineAnalysis/
├── Services/
│   ├── PipelineAnalysisService.cs (orchestrator)
│   ├── HydraulicModelService.cs
│   └── ConstraintService.cs
├── Calculations/
│   ├── PressureDropCalculator.cs
│   ├── HeatLossCalculator.cs
│   └── FlowRegimeCalculator.cs
├── Validation/
│   ├── ModelValidator.cs
│   └── BoundaryConditionValidator.cs
└── Exceptions/
    ├── PipelineAnalysisException.cs
    └── HydraulicModelException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.PipelineAnalysis`:

### Core Pipeline
- PIPELINE_MODEL
- PIPELINE_SEGMENT
- PIPELINE_COMPONENT
- PIPELINE_ANALYSIS_RUN
- PIPELINE_ANALYSIS_RESULT

### Constraints
- CAPACITY_CONSTRAINT
- PRESSURE_CONSTRAINT
- FLOW_ASSURANCE_RISK

---

## Service Interface Standards

```csharp
public interface IPipelineAnalysisService
{
    Task<PIPELINE_MODEL> CreateModelAsync(PIPELINE_MODEL model, string userId);
    Task<PIPELINE_ANALYSIS_RUN> RunAnalysisAsync(string modelId, string userId);
    Task<PIPELINE_ANALYSIS_RESULT> GetResultAsync(string runId);
    Task<CAPACITY_CONSTRAINT> RecommendConstraintAsync(string runId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement pipeline model and analysis entities.
- Create PipelineAnalysisService and validators.

### Phase 2: Hydraulics + Constraints (Weeks 2-3)
- Pressure drop and flow regime calculations.

### Phase 3: Integration (Week 4)
- Integrate with ProductionOperations and CompressorAnalysis.

---

## Best Practices Embedded

- **Model transparency**: boundary conditions stored with runs.
- **Constraint visibility**: capacity limits recorded with impact.
- **Auditability**: inputs and outputs preserved with run metadata.

---

## API Endpoint Sketch

```
/api/pipeline/
├── /models
│   ├── POST
│   └── GET /{id}
├── /runs
│   ├── POST /run/{modelId}
│   └── GET /{id}
└── /constraints
    └── POST /recommend/{runId}
```

---

## Success Criteria

- PPDM-aligned pipeline entities persist all analysis data.
- Results are reproducible with audit trails.
- Integration with compression and production workflows is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
