# Beep.OilandGas Economic Analysis - Architecture Plan

## Executive Summary

**Goal**: Deliver a PPDM-aligned economic analysis platform that produces transparent cash flow models, fiscal calculations, and scenario comparisons for all lifecycle phases.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.EconomicAnalysis` as the system of record; services orchestrate calculations and scenario management.

**Scope**: Economics for prospect screening, development planning, production optimization, and abandonment.

---

## Architecture Principles

### 1) Transparent Assumptions
- Every economic result must retain assumptions, price decks, and fiscal terms.
- Scenario runs are versioned and auditable.

### 2) Separation of Inputs vs Outputs
- Inputs (prices, costs, production profiles) are immutable per scenario.
- Outputs (NPV, IRR, cash flow) are calculated and stored with run metadata.

### 3) PPDM39 Alignment
- Store models and results using PPDM-aligned entities and audit columns.

### 4) Cross-Project Integration
- **ProspectIdentification**: screening economics.
- **DevelopmentPlanning**: FDP cost/economic evaluation.
- **ProductionForecasting**: production volumes input.
- **ProductionAccounting**: actuals for reconciliation.

---

## Target Project Structure

```
Beep.OilandGas.EconomicAnalysis/
├── Services/
│   ├── EconomicAnalysisService.cs (orchestrator)
│   ├── CashFlowService.cs
│   ├── FiscalService.cs
│   ├── PriceDeckService.cs
│   ├── ScenarioService.cs
│   └── SensitivityService.cs
├── Calculations/
│   ├── NpvCalculator.cs
│   ├── IrrCalculator.cs
│   ├── TaxCalculator.cs
│   └── RoyaltyCalculator.cs
├── Validation/
│   ├── EconomicModelValidator.cs
│   └── AssumptionValidator.cs
└── Exceptions/
    ├── EconomicAnalysisException.cs
    └── FiscalException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.EconomicAnalysis`:

### Core Economics
- ECONOMIC_MODEL
- ECONOMIC_SCENARIO
- ECONOMIC_ASSUMPTION
- CASH_FLOW
- DISCOUNT_RATE

### Prices + Costs
- PRICE_DECK
- PRICE_DECK_ITEM
- COST_ESTIMATE
- REVENUE_FORECAST
- OPEX_FORECAST
- CAPEX_FORECAST

### Fiscal + Risk
- FISCAL_TERM
- TAX_REGIME
- ROYALTY_TERM
- SENSITIVITY_RUN
- MONTE_CARLO_RUN

---

## Service Interface Standards

```csharp
public interface IEconomicAnalysisService
{
    Task<ECONOMIC_MODEL> CreateModelAsync(ECONOMIC_MODEL model, string userId);
    Task<ECONOMIC_SCENARIO> RunScenarioAsync(string modelId, ECONOMIC_SCENARIO scenario, string userId);
    Task<CASH_FLOW> GetCashFlowAsync(string scenarioId);
    Task<bool> RunSensitivityAsync(string scenarioId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement economic model, scenario, and cash flow entities.
- Create EconomicAnalysisService and validators.

### Phase 2: Pricing + Fiscal Calculations (Weeks 2-3)
- Price deck management and fiscal term application.

### Phase 3: Sensitivity + Risk (Week 4)
- Sensitivity and Monte Carlo runs.

### Phase 4: Integration (Week 5)
- Inputs from ProductionForecasting and DevelopmentPlanning.
- Actuals reconciliation with ProductionAccounting.

---

## Best Practices Embedded

- **Assumption transparency**: all economic results traceable to inputs.
- **Scenario discipline**: base/low/high with controlled changes.
- **Fiscal accuracy**: tax and royalty calculations versioned by regime.

---

## API Endpoint Sketch

```
/api/economics/
├── /models
│   ├── POST
│   └── GET /{id}
├── /scenarios
│   ├── POST /run/{modelId}
│   └── GET /{id}
├── /cashflow
│   ├── GET /{scenarioId}
└── /sensitivity
    └── POST /run/{scenarioId}
```

---

## Success Criteria

- PPDM-aligned economic entities persist all inputs and outputs.
- Scenario results are reproducible with full audit trails.
- Economics integrates cleanly with forecasting and accounting.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
