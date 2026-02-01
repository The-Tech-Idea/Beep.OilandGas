# Beep.OilandGas Gas Lift - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned gas lift management platform for design, surveillance, and optimization of gas lift systems.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.GasLift` as the system of record; services orchestrate design, injection allocation, and performance analysis.

**Scope**: Gas lift design, operations, and optimization for producing wells.

---

## Architecture Principles

### 1) Design-to-Operations Continuity
- Preserve design parameters and link to operating data.
- Capture valve performance and injection profiles.

### 2) Optimization and Allocation
- Track gas availability, allocation rules, and optimization results.

### 3) PPDM39 Alignment
- Persist gas lift designs and results in PPDM-aligned entities.

### 4) Cross-Project Integration
- **NodalAnalysis**: inflow/outflow and system modeling.
- **ProductionOperations**: daily performance and downtime.
- **EconomicAnalysis**: cost of injected gas and uplift.

---

## Target Project Structure

```
Beep.OilandGas.GasLift/
├── Services/
│   ├── GasLiftService.cs (orchestrator)
│   ├── DesignService.cs
│   ├── InjectionAllocationService.cs
│   ├── SurveillanceService.cs
│   └── OptimizationService.cs
├── Calculations/
│   ├── ValveSpacingCalculator.cs
│   ├── InjectionRateCalculator.cs
│   └── PerformanceCurveBuilder.cs
├── Validation/
│   ├── DesignValidator.cs
│   └── SurveillanceValidator.cs
└── Exceptions/
    ├── GasLiftException.cs
    └── DesignException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.GasLift`:

### Core Gas Lift
- GAS_LIFT_DESIGN
- GAS_LIFT_VALVE
- GAS_LIFT_INJECTION_PROFILE
- GAS_LIFT_SURVEILLANCE
- GAS_LIFT_OPTIMIZATION_RUN

### Allocation + Constraints
- INJECTION_ALLOCATION
- GAS_SUPPLY_CONSTRAINT
- LIFT_GAS_AVAILABILITY

### Performance
- VALVE_PERFORMANCE
- WELL_LIFT_RESPONSE
- OPERATING_LIMIT

---

## Service Interface Standards

```csharp
public interface IGasLiftService
{
    Task<GAS_LIFT_DESIGN> CreateDesignAsync(GAS_LIFT_DESIGN design, string userId);
    Task<GAS_LIFT_INJECTION_PROFILE> AllocateInjectionAsync(string fieldId, string userId);
    Task<GAS_LIFT_SURVEILLANCE> RecordSurveillanceAsync(GAS_LIFT_SURVEILLANCE data, string userId);
    Task<GAS_LIFT_OPTIMIZATION_RUN> OptimizeAsync(string fieldId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement design, valve, and injection profile entities.
- Create GasLiftService and validators.

### Phase 2: Surveillance + Optimization (Weeks 2-3)
- Surveillance data management and optimization logic.

### Phase 3: Integration (Week 4)
- Integrate with NodalAnalysis and ProductionOperations.

---

## Best Practices Embedded

- **Design traceability**: design inputs stored and linked to operations.
- **Allocation transparency**: allocation logic and constraints recorded.
- **Optimization loop**: results tracked to performance changes.

---

## API Endpoint Sketch

```
/api/gas-lift/
├── /designs
│   ├── POST
│   └── GET /{id}
├── /allocation
│   └── POST /run/{fieldId}
├── /surveillance
│   ├── POST
│   └── GET /{wellId}
└── /optimization
    └── POST /run/{fieldId}
```

---

## Success Criteria

- PPDM-aligned gas lift entities persist design and surveillance data.
- Allocation and optimization runs are reproducible with audit trails.
- Integration with nodal and production workflows is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
