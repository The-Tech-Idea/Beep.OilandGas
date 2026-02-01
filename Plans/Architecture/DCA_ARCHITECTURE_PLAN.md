# Beep.OilandGas DCA - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned decline curve analysis (DCA) platform to generate reliable production forecasts and reserves estimates with transparent inputs and assumptions.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.DCA` as the system of record; services orchestrate analysis runs and store results with full metadata.

**Scope**: Engineering forecasting for producing wells and development scenarios; integrates with ProductionForecasting and EconomicAnalysis.

---

## Architecture Principles

### 1) Reproducible Analysis
- Every DCA run must retain input data and method parameters.
- Results are versioned and auditable.

### 2) Method Transparency
- Support Arps, exponential, hyperbolic, and harmonic decline methods.
- Store fit diagnostics and error metrics.

### 3) PPDM39 Alignment
- Persist DCA models and outputs in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: historical production inputs.
- **ProductionForecasting**: forecast aggregation.
- **EconomicAnalysis**: reserves and cash flow inputs.

---

## Target Project Structure

```
Beep.OilandGas.DCA/
├── Services/
│   ├── DcaService.cs (orchestrator)
│   ├── DeclineCurveFitService.cs
│   ├── ReservesService.cs
│   └── ScenarioService.cs
├── Calculations/
│   ├── ArpsCalculator.cs
│   ├── ExponentialCalculator.cs
│   ├── HyperbolicCalculator.cs
│   └── ErrorMetrics.cs
├── Validation/
│   ├── InputValidator.cs
│   └── FitValidator.cs
└── Exceptions/
    ├── DcaException.cs
    └── FitException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.DCA`:

### Core DCA
- DCA_MODEL
- DCA_INPUT
- DCA_RUN
- DCA_RESULT
- DCA_ASSUMPTION

### Fit Diagnostics
- DCA_FIT_METRIC
- DCA_ERROR_METRIC
- DCA_RESIDUAL

### Reserves
- RESERVES_ESTIMATE
- EUR_ESTIMATE
- RESERVES_CLASSIFICATION

---

## Service Interface Standards

```csharp
public interface IDcaService
{
    Task<DCA_MODEL> CreateModelAsync(DCA_MODEL model, string userId);
    Task<DCA_RUN> RunAnalysisAsync(string modelId, string userId);
    Task<DCA_RESULT> GetResultAsync(string runId);
    Task<RESERVES_ESTIMATE> GenerateReservesAsync(string runId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement DCA model, input, run, and result entities.
- Create DcaService and validators.

### Phase 2: Fit Calculations (Weeks 2-3)
- Implement decline curve fit methods and error metrics.

### Phase 3: Reserves + Integration (Week 4)
- Reserves estimates and reporting.
- Integration with forecasting and economics.

---

## Best Practices Embedded

- **Transparent fits**: inputs and error metrics preserved.
- **Repeatable analysis**: run metadata stored with results.
- **Reserves discipline**: classification tracked with assumptions.

---

## API Endpoint Sketch

```
/api/dca/
├── /models
│   ├── POST
│   └── GET /{id}
├── /runs
│   ├── POST /run/{modelId}
│   └── GET /{id}
└── /reserves
    └── POST /generate/{runId}
```

---

## Success Criteria

- PPDM-aligned DCA entities persist all analysis inputs and outputs.
- Forecasts and reserves are reproducible with audit trails.
- Integration with forecasting and economics is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
