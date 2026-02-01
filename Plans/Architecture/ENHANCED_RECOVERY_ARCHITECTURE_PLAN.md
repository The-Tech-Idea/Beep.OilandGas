# Beep.OilandGas Enhanced Recovery - Architecture Plan

## Executive Summary

**Goal**: Build a PPDM-aligned enhanced recovery (EOR) platform that manages injection programs, pattern optimization, and production response tracking.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.EnhancedRecovery` as the system of record; services orchestrate injection operations and performance analysis.

**Scope**: Secondary and tertiary recovery (waterflood, gas injection, chemical, thermal).

---

## Architecture Principles

### 1) Injection-to-Response Traceability
- Each injection program links to performance metrics and response analysis.
- Preserve assumptions and surveillance data for pilots and full-field rollouts.

### 2) Surveillance Driven Optimization
- Track pattern performance and sweep efficiency.
- Recommendations are recorded and auditable.

### 3) PPDM39 Alignment
- Store EOR assets and operations in PPDM-style entities.

### 4) Cross-Project Integration
- **ProductionOperations**: surveillance data and operational constraints.
- **ProductionForecasting**: forecast impacts of EOR programs.
- **EconomicAnalysis**: EOR project economics.
- **PermitsAndApplications**: injection approvals.

---

## Target Project Structure

```
Beep.OilandGas.EnhancedRecovery/
├── Services/
│   ├── EnhancedRecoveryService.cs (orchestrator)
│   ├── InjectionManagementService.cs
│   ├── PatternOptimizationService.cs
│   ├── SurveillanceService.cs
│   └── EorEconomicsService.cs
├── Workflows/
│   ├── EorPilotWorkflow.cs
│   └── EorScaleUpWorkflow.cs
├── Validation/
│   ├── InjectionValidator.cs
│   ├── PatternValidator.cs
│   └── SurveillanceValidator.cs
└── Exceptions/
    ├── EnhancedRecoveryException.cs
    └── InjectionException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.EnhancedRecovery`:

### Core EOR
- ENHANCED_RECOVERY_PROJECT
- INJECTION_PROGRAM
- INJECTION_WELL
- INJECTANT
- INJECTION_SCHEDULE

### Pattern + Surveillance
- INJECTION_PATTERN
- SWEEP_EFFICIENCY
- RESERVOIR_SURVEILLANCE
- PRODUCTION_RESPONSE
- PRESSURE_RESPONSE

### Pilots + Tests
- EOR_PILOT_TEST
- PILOT_RESULT
- SCALE_UP_DECISION

---

## Service Interface Standards

```csharp
public interface IEnhancedRecoveryService
{
    Task<ENHANCED_RECOVERY_PROJECT> CreateProjectAsync(ENHANCED_RECOVERY_PROJECT project, string userId);
    Task<INJECTION_PROGRAM> StartInjectionAsync(INJECTION_PROGRAM program, string userId);
    Task<PRODUCTION_RESPONSE> RecordResponseAsync(PRODUCTION_RESPONSE response, string userId);
    Task<bool> EvaluateScaleUpAsync(string projectId, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement EOR project, injection program, and response entities.
- Create EnhancedRecoveryService and validators.

### Phase 2: Pattern + Surveillance (Weeks 2-3)
- Pattern optimization and surveillance workflows.

### Phase 3: Pilot + Scale-Up (Week 4)
- Pilot design, evaluation, and scale-up decision support.

### Phase 4: Integration (Week 5)
- Integrate with ProductionOperations, Forecasting, Economics, and Permits.

---

## Best Practices Embedded

- **Pilot-first discipline**: pilots are evaluated before scale-up.
- **Response traceability**: injection linked to production response.
- **Optimization loop**: recommendations tracked to outcomes.

---

## API Endpoint Sketch

```
/api/eor/
├── /projects
│   ├── POST
│   └── GET /{id}
├── /injection
│   ├── POST /start
│   └── GET /{projectId}
├── /response
│   ├── POST
│   └── GET /{projectId}
└── /scale-up
    └── POST /evaluate/{projectId}
```

---

## Success Criteria

- PPDM-aligned EOR entities persist injection and response data.
- EOR pilots and scale-up decisions are auditable.
- Integration with operations and economics is reliable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
