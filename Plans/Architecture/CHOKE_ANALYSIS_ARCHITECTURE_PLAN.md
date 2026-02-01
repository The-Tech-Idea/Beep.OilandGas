# Beep.OilandGas Choke Analysis - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned choke analysis platform for evaluating choke performance, pressure drop, and production optimization.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.ChokeAnalysis` as the system of record; services orchestrate analysis runs and store results with full metadata.

**Scope**: Choke performance evaluation and optimization for producing wells and facilities.

---

## Architecture Principles

### 1) Reproducible Analysis
- Preserve choke test inputs, assumptions, and correlations.
- Results are versioned and auditable.

### 2) System Integrity
- Link choke settings to production response and constraints.

### 3) PPDM39 Alignment
- Persist choke analysis data in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: daily production and constraints.
- **NodalAnalysis**: system modeling inputs.
- **EconomicAnalysis**: rate/pressure optimization impacts.

---

## Target Project Structure

```
Beep.OilandGas.ChokeAnalysis/
├── Services/
│   ├── ChokeAnalysisService.cs (orchestrator)
│   ├── TestService.cs
│   └── OptimizationService.cs
├── Calculations/
│   ├── PressureDropCalculator.cs
│   ├── ChokeCorrelationLibrary.cs
│   └── PerformanceCurveBuilder.cs
├── Validation/
│   ├── TestValidator.cs
│   └── CorrelationValidator.cs
└── Exceptions/
    ├── ChokeAnalysisException.cs
    └── CorrelationException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.ChokeAnalysis`:

### Core Choke Analysis
- CHOKE_TEST
- CHOKE_SETTING
- CHOKE_ANALYSIS_RUN
- CHOKE_ANALYSIS_RESULT

### Performance
- CHOKE_CORRELATION
- PRESSURE_DROP_RESULT
- CHOKE_PERFORMANCE_CURVE

---

## Service Interface Standards

```csharp
public interface IChokeAnalysisService
{
    Task<CHOKE_TEST> RecordTestAsync(CHOKE_TEST test, string userId);
    Task<CHOKE_ANALYSIS_RUN> RunAnalysisAsync(string testId, string userId);
    Task<CHOKE_ANALYSIS_RESULT> GetResultAsync(string runId);
    Task<CHOKE_SETTING> RecommendSettingAsync(string runId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement choke test and analysis entities.
- Create ChokeAnalysisService and validators.

### Phase 2: Calculations + Correlations (Weeks 2-3)
- Pressure drop calculations and correlation library.

### Phase 3: Integration (Week 4)
- Integrate with ProductionOperations and NodalAnalysis.

---

## Best Practices Embedded

- **Correlation transparency**: correlation selection stored with results.
- **Optimization loop**: recommended settings tracked to outcomes.
- **Auditability**: inputs and outputs preserved with run metadata.

---

## API Endpoint Sketch

```
/api/choke/
├── /tests
│   ├── POST
│   └── GET /{wellId}
├── /runs
│   ├── POST /run/{testId}
│   └── GET /{id}
└── /settings
    └── POST /recommend/{runId}
```

---

## Success Criteria

- PPDM-aligned choke entities persist all analysis data.
- Results are reproducible with audit trails.
- Integration with operations and nodal analysis is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
