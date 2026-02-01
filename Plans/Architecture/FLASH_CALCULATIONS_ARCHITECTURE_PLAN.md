# Beep.OilandGas Flash Calculations - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned flash calculations platform for phase behavior and separator modeling used in production operations and facility design.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.FlashCalculations` as the system of record; services orchestrate flash calculation runs and store results with full metadata.

**Scope**: Surface and subsurface flash calculations, separator train analysis.

---

## Architecture Principles

### 1) Reproducible Calculations
- Preserve input composition, conditions, and correlation choices.
- Results are versioned and auditable.

### 2) Operational Reuse
- Provide standard flash calculations reused across production systems.

### 3) PPDM39 Alignment
- Persist flash calculation models in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: separator performance and facility constraints.
- **PipelineAnalysis**: phase behavior impacts.
- **EconomicAnalysis**: shrink and sales gas volumes.

---

## Target Project Structure

```
Beep.OilandGas.FlashCalculations/
├── Services/
│   ├── FlashCalculationService.cs (orchestrator)
│   ├── CompositionService.cs
│   └── SeparatorTrainService.cs
├── Calculations/
│   ├── FlashCalculator.cs
│   ├── PhaseEquilibriumSolver.cs
│   └── ShrinkCalculator.cs
├── Validation/
│   ├── CompositionValidator.cs
│   └── ConditionValidator.cs
└── Exceptions/
    ├── FlashCalculationsException.cs
    └── PhaseEquilibriumException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.FlashCalculations`:

### Core Flash
- FLASH_MODEL
- FLASH_INPUT
- FLASH_RUN
- FLASH_RESULT

### Composition + Conditions
- COMPONENT_COMPOSITION
- TEMPERATURE_PRESSURE_CONDITION
- SEPARATOR_STAGE
- SHRINK_FACTOR

---

## Service Interface Standards

```csharp
public interface IFlashCalculationService
{
    Task<FLASH_MODEL> CreateModelAsync(FLASH_MODEL model, string userId);
    Task<FLASH_RUN> RunAsync(string modelId, string userId);
    Task<FLASH_RESULT> GetResultAsync(string runId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement flash model, input, and result entities.
- Create FlashCalculationService and validators.

### Phase 2: Calculations + Separator Trains (Weeks 2-3)
- Flash calculations and shrink factor logic.

### Phase 3: Integration (Week 4)
- Integrate with production ops and pipeline analysis.

---

## Best Practices Embedded

- **Composition traceability**: inputs stored with results.
- **Reproducibility**: run metadata retained.
- **Operational reuse**: standardized flash results for facilities.

---

## API Endpoint Sketch

```
/api/flash/
├── /models
│   ├── POST
│   └── GET /{id}
├── /runs
│   ├── POST /run/{modelId}
│   └── GET /{id}
└── /results
    └── GET /{runId}
```

---

## Success Criteria

- PPDM-aligned flash entities persist all models and results.
- Flash results are reproducible with audit trails.
- Integration with production and pipeline workflows is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
