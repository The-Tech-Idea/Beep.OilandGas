# Beep.OilandGas Hydraulic Pumps - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned hydraulic pumping platform supporting design, surveillance, and optimization of hydraulic lift systems.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.HydraulicPumps` as the system of record; services orchestrate design, diagnostics, and optimization.

**Scope**: Hydraulic lift operations and performance management.

---

## Architecture Principles

### 1) Design-to-Operations Continuity
- Preserve design parameters and link to operating data.
- Capture pump performance tests and diagnostics.

### 2) Optimization Discipline
- Track recommendations, setpoints, and outcomes.

### 3) PPDM39 Alignment
- Persist hydraulic pump data in PPDM-aligned entities.

### 4) Cross-Project Integration
- **NodalAnalysis**: inflow/outflow modeling.
- **ProductionOperations**: surveillance data.
- **EconomicAnalysis**: cost/benefit of lift changes.

---

## Target Project Structure

```
Beep.OilandGas.HydraulicPumps/
├── Services/
│   ├── HydraulicPumpsService.cs (orchestrator)
│   ├── PumpDesignService.cs
│   ├── DiagnosticsService.cs
│   ├── OptimizationService.cs
│   └── SurveillanceService.cs
├── Calculations/
│   ├── PumpSizingCalculator.cs
│   ├── PressureLossCalculator.cs
│   └── EfficiencyAnalyzer.cs
├── Validation/
│   ├── DesignValidator.cs
│   └── DiagnosticsValidator.cs
└── Exceptions/
    ├── HydraulicPumpsException.cs
    └── DiagnosticsException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.HydraulicPumps`:

### Core Hydraulic Lift
- HYDRAULIC_PUMP_DESIGN
- HYDRAULIC_PUMP_TEST
- HYDRAULIC_PUMP_DIAGNOSTIC
- HYDRAULIC_LIFT_SURVEILLANCE
- HYDRAULIC_OPTIMIZATION_RUN

### Operations
- POWER_FLUID_RATE
- PUMP_INTAKE_PRESSURE
- SURFACE_PRESSURE
- FLOWING_PRESSURE

---

## Service Interface Standards

```csharp
public interface IHydraulicPumpsService
{
    Task<HYDRAULIC_PUMP_DESIGN> CreateDesignAsync(HYDRAULIC_PUMP_DESIGN design, string userId);
    Task<HYDRAULIC_PUMP_TEST> RecordTestAsync(HYDRAULIC_PUMP_TEST test, string userId);
    Task<HYDRAULIC_PUMP_DIAGNOSTIC> AnalyzeDiagnosticsAsync(string testId, string userId);
    Task<HYDRAULIC_OPTIMIZATION_RUN> OptimizeAsync(string wellId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement hydraulic pump design, test, and diagnostic entities.
- Create HydraulicPumpsService and validators.

### Phase 2: Diagnostics + Optimization (Weeks 2-3)
- Pump performance analysis and recommendation logic.

### Phase 3: Integration (Week 4)
- Integrate with NodalAnalysis and ProductionOperations.

---

## Best Practices Embedded

- **Diagnostics rigor**: tests and interpretations preserved.
- **Optimization loop**: recommendations tracked to outcomes.
- **Design traceability**: design parameters stored with operations.

---

## API Endpoint Sketch

```
/api/hydraulic-pumps/
├── /designs
│   ├── POST
│   └── GET /{id}
├── /tests
│   ├── POST
│   └── GET /{wellId}
└── /optimization
    └── POST /run/{wellId}
```

---

## Success Criteria

- PPDM-aligned hydraulic pump entities persist design and diagnostic data.
- Optimization runs are reproducible with audit trails.
- Integration with nodal and production workflows is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
