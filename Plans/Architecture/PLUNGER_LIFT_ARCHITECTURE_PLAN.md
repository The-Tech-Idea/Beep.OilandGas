# Beep.OilandGas Plunger Lift - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned plunger lift platform for design, control, and optimization of plunger lift systems in gas and low-rate oil wells.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.PlungerLift` as the system of record; services orchestrate cycle analysis and optimization.

**Scope**: Plunger lift design, cycle control, and optimization.

---

## Architecture Principles

### 1) Cycle Traceability
- Preserve cycle data, operating conditions, and control parameters.
- Track adjustments and outcomes over time.

### 2) Optimization Discipline
- Recommendations recorded with impact on production and downtime.

### 3) PPDM39 Alignment
- Persist plunger lift data in PPDM-aligned entities.

### 4) Cross-Project Integration
- **ProductionOperations**: daily production and downtime inputs.
- **NodalAnalysis**: system modeling and constraints.
- **EconomicAnalysis**: cost/benefit of optimization.

---

## Target Project Structure

```
Beep.OilandGas.PlungerLift/
├── Services/
│   ├── PlungerLiftService.cs (orchestrator)
│   ├── CycleAnalysisService.cs
│   ├── OptimizationService.cs
│   └── SurveillanceService.cs
├── Calculations/
│   ├── CycleEfficiencyCalculator.cs
│   ├── ArrivalTimeAnalyzer.cs
│   └── LoadFactorCalculator.cs
├── Validation/
│   ├── CycleValidator.cs
│   └── ControlValidator.cs
└── Exceptions/
    ├── PlungerLiftException.cs
    └── CycleException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.PlungerLift`:

### Core Plunger Lift
- PLUNGER_LIFT_DESIGN
- PLUNGER_LIFT_CYCLE
- PLUNGER_LIFT_CONTROL
- PLUNGER_LIFT_SURVEILLANCE
- PLUNGER_LIFT_OPTIMIZATION_RUN

### Cycle Metrics
- ARRIVAL_TIME
- LIFT_EFFICIENCY
- LOAD_FACTOR
- CYCLE_DOWNTIME

---

## Service Interface Standards

```csharp
public interface IPlungerLiftService
{
    Task<PLUNGER_LIFT_DESIGN> CreateDesignAsync(PLUNGER_LIFT_DESIGN design, string userId);
    Task<PLUNGER_LIFT_CYCLE> RecordCycleAsync(PLUNGER_LIFT_CYCLE cycle, string userId);
    Task<PLUNGER_LIFT_OPTIMIZATION_RUN> OptimizeAsync(string wellId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement plunger lift design, cycle, and control entities.
- Create PlungerLiftService and validators.

### Phase 2: Cycle Analysis + Optimization (Weeks 2-3)
- Cycle efficiency and arrival time analysis.

### Phase 3: Integration (Week 4)
- Integrate with ProductionOperations and NodalAnalysis.

---

## Best Practices Embedded

- **Cycle visibility**: cycle data preserved with control changes.
- **Optimization loop**: recommendations tracked to outcomes.
- **Design traceability**: parameters stored with operations.

---

## API Endpoint Sketch

```
/api/plunger-lift/
├── /designs
│   ├── POST
│   └── GET /{id}
├── /cycles
│   ├── POST
│   └── GET /{wellId}
└── /optimization
    └── POST /run/{wellId}
```

---

## Success Criteria

- PPDM-aligned plunger lift entities persist design and cycle data.
- Optimization runs are reproducible with audit trails.
- Integration with nodal and production workflows is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
