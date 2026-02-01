# Beep.OilandGas Compressor Analysis - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned compressor analysis platform for evaluating compressor performance, efficiency, and operational constraints.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.CompressorAnalysis` as the system of record; services orchestrate analysis runs and store results with full metadata.

**Scope**: Compressor performance and optimization for production facilities.

---

## Architecture Principles

### 1) Reproducible Analysis
- Preserve test inputs, operating conditions, and correlation choices.
- Results are versioned and auditable.

### 2) Operational Integrity
- Link compressor performance to facility throughput constraints.

### 3) PPDM39 Alignment
- Persist compressor analysis data in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: facility constraints and downtime.
- **EconomicAnalysis**: cost/benefit of optimization.
- **PipelineAnalysis**: downstream constraints.

---

## Target Project Structure

```
Beep.OilandGas.CompressorAnalysis/
├── Services/
│   ├── CompressorAnalysisService.cs (orchestrator)
│   ├── TestService.cs
│   └── OptimizationService.cs
├── Calculations/
│   ├── CompressorMapCalculator.cs
│   ├── EfficiencyCalculator.cs
│   └── SurgeMarginCalculator.cs
├── Validation/
│   ├── TestValidator.cs
│   └── OperatingWindowValidator.cs
└── Exceptions/
    ├── CompressorAnalysisException.cs
    └── OperatingWindowException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.CompressorAnalysis`:

### Core Compressor Analysis
- COMPRESSOR_TEST
- COMPRESSOR_ANALYSIS_RUN
- COMPRESSOR_ANALYSIS_RESULT
- COMPRESSOR_PERFORMANCE_CURVE

### Performance + Constraints
- SURGE_MARGIN
- OPERATING_WINDOW
- ENERGY_CONSUMPTION
- EFFICIENCY_RESULT

---

## Service Interface Standards

```csharp
public interface ICompressorAnalysisService
{
    Task<COMPRESSOR_TEST> RecordTestAsync(COMPRESSOR_TEST test, string userId);
    Task<COMPRESSOR_ANALYSIS_RUN> RunAnalysisAsync(string testId, string userId);
    Task<COMPRESSOR_ANALYSIS_RESULT> GetResultAsync(string runId);
    Task<OPERATING_WINDOW> RecommendOperatingWindowAsync(string runId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement compressor test and analysis entities.
- Create CompressorAnalysisService and validators.

### Phase 2: Calculations + Constraints (Weeks 2-3)
- Compressor map, efficiency, and surge calculations.

### Phase 3: Integration (Week 4)
- Integrate with ProductionOperations and PipelineAnalysis.

---

## Best Practices Embedded

- **Efficiency visibility**: performance curves and energy use captured.
- **Operating safety**: surge margin tracked and enforced.
- **Auditability**: inputs and outputs preserved with run metadata.

---

## API Endpoint Sketch

```
/api/compressor/
├── /tests
│   ├── POST
│   └── GET /{facilityId}
├── /runs
│   ├── POST /run/{testId}
│   └── GET /{id}
└── /operating-window
    └── POST /recommend/{runId}
```

---

## Success Criteria

- PPDM-aligned compressor entities persist all analysis data.
- Results are reproducible with audit trails.
- Integration with operations and pipeline analysis is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
