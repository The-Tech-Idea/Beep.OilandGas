# Beep.OilandGas Well Test Analysis - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned well test analysis platform for pressure-transient analysis (PTA), rate-transient analysis (RTA), and test interpretation with transparent assumptions.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.WellTestAnalysis` as the system of record; services orchestrate test processing and interpretation workflows.

**Scope**: Diagnostic and interpretive test analysis supporting production optimization and reservoir understanding.

---

## Architecture Principles

### 1) Traceable Interpretations
- Preserve raw test data, processed data, and interpretation outputs.
- Store model assumptions and diagnostic plots metadata.

### 2) Repeatable Analysis
- Tests are versioned with method parameters and analyst approvals.

### 3) PPDM39 Alignment
- Persist test results and interpretations in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: test scheduling and surveillance.
- **NodalAnalysis**: input for well performance models.
- **EconomicAnalysis**: productivity impact scenarios.

---

## Target Project Structure

```
Beep.OilandGas.WellTestAnalysis/
├── Services/
│   ├── WellTestService.cs (orchestrator)
│   ├── PtaService.cs
│   ├── RtaService.cs
│   └── InterpretationService.cs
├── Calculations/
│   ├── DerivativeCalculator.cs
│   ├── HornerAnalysis.cs
│   └── FlowRegimeClassifier.cs
├── Validation/
│   ├── TestDataValidator.cs
│   └── InterpretationValidator.cs
└── Exceptions/
    ├── WellTestException.cs
    └── InterpretationException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.WellTestAnalysis`:

### Core Test Data
- WELL_TEST
- WELL_TEST_DATA
- PRESSURE_TRANSIENT_RESULT
- RATE_TRANSIENT_RESULT

### Interpretation
- WELL_TEST_INTERPRETATION
- FLOW_REGIME
- RESERVOIR_PARAMETER
- SKIN_FACTOR

### Diagnostics
- DERIVATIVE_PLOT
- HORNER_PLOT
- QUALITY_FLAG

---

## Service Interface Standards

```csharp
public interface IWellTestService
{
    Task<WELL_TEST> CreateTestAsync(WELL_TEST test, string userId);
    Task<PRESSURE_TRANSIENT_RESULT> RunPtaAsync(string testId, string userId);
    Task<RATE_TRANSIENT_RESULT> RunRtaAsync(string testId, string userId);
    Task<WELL_TEST_INTERPRETATION> PublishInterpretationAsync(string testId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement test, data, and interpretation entities.
- Create WellTestService and validators.

### Phase 2: PTA + RTA Calculations (Weeks 2-3)
- Implement derivative analysis, Horner plots, flow regimes.

### Phase 3: Integration (Week 4)
- Integrate with ProductionOperations and NodalAnalysis.

---

## Best Practices Embedded

- **Interpretation transparency**: assumptions stored with results.
- **Diagnostic rigor**: derivative and flow regime artifacts preserved.
- **Auditability**: analyst approvals and versioned interpretations.

---

## API Endpoint Sketch

```
/api/well-tests/
├── /tests
│   ├── POST
│   └── GET /{id}
├── /pta
│   └── POST /run/{testId}
├── /rta
│   └── POST /run/{testId}
└── /interpretation
    └── POST /publish/{testId}
```

---

## Success Criteria

- PPDM-aligned test entities persist all data and interpretations.
- PTA/RTA results are reproducible with audit trails.
- Integration with production and nodal analysis is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
