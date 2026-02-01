# Beep.OilandGas Sucker Rod Pumping - Architecture Plan

## Executive Summary

**Goal**: Provide a PPDM-aligned sucker rod pumping (SRP) platform that supports design, surveillance, and optimization of rod pump systems.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.SuckerRodPumping` as the system of record; services orchestrate diagnostics, design, and optimization.

**Scope**: Rod pump selection, well diagnostics, and performance optimization.

---

## Architecture Principles

### 1) Design-to-Operations Continuity
- Preserve design parameters and link to operating data.
- Capture pump cards and diagnostic interpretations.

### 2) Optimization Discipline
- Track recommendations, setpoints, and outcomes.

### 3) PPDM39 Alignment
- Persist SRP data and diagnostics in PPDM-aligned entities.

### 4) Cross-Project Integration
- **NodalAnalysis**: inflow/outflow matching.
- **ProductionOperations**: surveillance data.
- **EconomicAnalysis**: cost/benefit analysis of changes.

---

## Target Project Structure

```
Beep.OilandGas.SuckerRodPumping/
├── Services/
│   ├── SuckerRodPumpingService.cs (orchestrator)
│   ├── PumpDesignService.cs
│   ├── DiagnosticsService.cs
│   ├── OptimizationService.cs
│   └── SurveillanceService.cs
├── Calculations/
│   ├── RodStringDesignCalculator.cs
│   ├── PumpCardAnalyzer.cs
│   └── TorqueAnalyzer.cs
├── Validation/
│   ├── DesignValidator.cs
│   └── DiagnosticsValidator.cs
└── Exceptions/
    ├── SuckerRodPumpingException.cs
    └── DiagnosticsException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.SuckerRodPumping`:

### Core SRP
- SUCKER_ROD_PUMP_DESIGN
- ROD_STRING
- PUMP_CARD
- PUMP_DIAGNOSTIC
- SRP_OPTIMIZATION_RUN

### Operations
- STROKE_LENGTH
- STROKES_PER_MINUTE
- POLISHED_ROD_LOAD
- DYNAMOMETER_CARD

---

## Service Interface Standards

```csharp
public interface ISuckerRodPumpingService
{
    Task<SUCKER_ROD_PUMP_DESIGN> CreateDesignAsync(SUCKER_ROD_PUMP_DESIGN design, string userId);
    Task<PUMP_CARD> RecordPumpCardAsync(PUMP_CARD card, string userId);
    Task<PUMP_DIAGNOSTIC> AnalyzeDiagnosticsAsync(string cardId, string userId);
    Task<SRP_OPTIMIZATION_RUN> OptimizeAsync(string wellId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement SRP design, pump card, and diagnostics entities.
- Create SuckerRodPumpingService and validators.

### Phase 2: Diagnostics + Optimization (Weeks 2-3)
- Pump card analysis and recommendation logic.

### Phase 3: Integration (Week 4)
- Integrate with NodalAnalysis and ProductionOperations.

---

## Best Practices Embedded

- **Diagnostics rigor**: pump cards and interpretations preserved.
- **Optimization loop**: recommendations tracked to outcomes.
- **Design traceability**: design parameters stored with operations.

---

## API Endpoint Sketch

```
/api/srp/
├── /designs
│   ├── POST
│   └── GET /{id}
├── /cards
│   ├── POST
│   └── GET /{wellId}
└── /optimization
    └── POST /run/{wellId}
```

---

## Success Criteria

- PPDM-aligned SRP entities persist design and diagnostic data.
- Optimization runs are reproducible with audit trails.
- Integration with nodal and production workflows is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
