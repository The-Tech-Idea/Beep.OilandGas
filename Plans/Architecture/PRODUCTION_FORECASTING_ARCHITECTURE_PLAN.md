# Beep.OilandGas Production Forecasting - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned forecasting system that produces deterministic and probabilistic production forecasts from reservoir, decline curve, and type curve inputs.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.ProductionForecasting` as the system of record; services orchestrate forecasting workflows and scenario management.

**Scope**: Forecasting for prospect screening, development planning, production optimization, and economic analysis.

---

## Architecture Principles

### 1) Reproducible Forecasts
- Forecast outputs must retain full input assumptions and method metadata.
- Scenario runs are versioned and auditable.

### 2) Method Transparency
- Support decline-curve, type-curve, and model-based forecasting.
- Store method parameters and constraints.

### 3) PPDM39 Alignment
- Persist forecasts as PPDM-style entities with audit columns.

### 4) Cross-Project Integration
- **ProspectIdentification** and **DevelopmentPlanning** for plan economics.
- **ProductionOperations** for actuals and surveillance.
- **EconomicAnalysis** for cash flow modeling.

---

## Target Project Structure

```
Beep.OilandGas.ProductionForecasting/
├── Services/
│   ├── ForecastingService.cs (orchestrator)
│   ├── DeclineCurveService.cs
│   ├── TypeCurveService.cs
│   ├── ScenarioService.cs
│   └── ConstraintService.cs
├── Calculations/
│   ├── ArpsDeclineCalculator.cs
│   ├── ForecastAggregator.cs
│   └── ProbabilisticRunner.cs
├── Validation/
│   ├── ForecastValidator.cs
│   └── InputValidator.cs
└── Exceptions/
    ├── ForecastingException.cs
    └── DeclineCurveException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.ProductionForecasting`:

### Core Forecasting
- PRODUCTION_FORECAST
- FORECAST_SCENARIO
- FORECAST_INPUT
- FORECAST_OUTPUT
- FORECAST_ASSUMPTION

### Methods
- DECLINE_CURVE
- TYPE_CURVE
- RESERVOIR_MODEL_REFERENCE
- FORECAST_METHOD_PARAMETER

### Constraints
- FACILITY_CONSTRAINT
- OPERATING_CONSTRAINT
- FORECAST_LIMIT

---

## Service Interface Standards

```csharp
public interface IForecastingService
{
    Task<PRODUCTION_FORECAST> CreateForecastAsync(PRODUCTION_FORECAST forecast, string userId);
    Task<FORECAST_SCENARIO> RunScenarioAsync(string forecastId, FORECAST_SCENARIO scenario, string userId);
    Task<FORECAST_OUTPUT> GetForecastOutputAsync(string scenarioId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement forecast, scenario, and output entities.
- Create ForecastingService and validators.

### Phase 2: Methods + Calculations (Weeks 2-3)
- Decline curve and type curve calculations.
- Probabilistic runs (P10/P50/P90).

### Phase 3: Constraints + Integration (Week 4)
- Apply facility and operating constraints.
- Integrate with EconomicAnalysis and DevelopmentPlanning.

---

## Best Practices Embedded

- **Forecast reproducibility**: inputs and methods stored with outputs.
- **Scenario discipline**: controlled variations for sensitivities.
- **Method transparency**: parameters and constraint logic retained.

---

## API Endpoint Sketch

```
/api/forecasting/
├── /forecasts
│   ├── POST
│   └── GET /{id}
├── /scenarios
│   ├── POST /run/{forecastId}
│   └── GET /{id}
└── /outputs
    └── GET /{scenarioId}
```

---

## Success Criteria

- PPDM-aligned forecast entities persist all forecast data.
- Scenario outputs are reproducible with audit trails.
- Forecasts integrate with economics and planning workflows.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
