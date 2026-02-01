# Beep.OilandGas Production Operations - Architecture Plan

## Executive Summary

**Goal**: Implement a PPDM-aligned production operations platform covering daily production tracking, well surveillance, maintenance, and operational KPIs.

**Key Principle**: Use **Data Classes Only** in `Beep.OilandGas.Models.Data.ProductionOperations` as the system of record; services orchestrate surveillance, optimization, and work management.

**Scope**: Producing wells and facilities from first production through late-life operations.

---

## Architecture Principles

### 1) Operational Continuity
- Track daily production, downtime, and well status changes.
- Preserve a consistent operational history per well and facility.

### 2) Surveillance + Optimization
- Integrate well tests, nodal analysis, and artificial lift diagnostics.
- Detect underperformance and trigger work orders.

### 3) PPDM39 Alignment
- Use PPDM entities for well, facility, production, and status history.

### 4) Cross-Project Integration
- **ProductionAccounting**: volumes and allocation.
- **WellTestAnalysis**: test data and pressure analysis.
- **NodalAnalysis / Artificial Lift**: optimization inputs.
- **Maintenance / Workovers**: if present in future scope.

---

## Target Project Structure

```
Beep.OilandGas.ProductionOperations/
├── Services/
│   ├── ProductionOperationsService.cs (orchestrator)
│   ├── DailyProductionService.cs
│   ├── WellStatusService.cs
│   ├── SurveillanceService.cs
│   ├── OptimizationService.cs
│   └── DowntimeService.cs
├── Workflows/
│   ├── SurveillanceWorkflow.cs
│   └── DowntimeApprovalWorkflow.cs
├── Validation/
│   ├── ProductionValidator.cs
│   ├── StatusValidator.cs
│   └── DowntimeValidator.cs
├── Integration/
│   ├── AccountingBridge.cs
│   ├── WellTestBridge.cs
│   └── NodalAnalysisBridge.cs
└── Exceptions/
    ├── ProductionOperationsException.cs
    └── SurveillanceException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.ProductionOperations`:

### Core Production
- DAILY_PRODUCTION
- PRODUCTION_TEST
- WELL_STATUS_HISTORY
- PRODUCTION_ALLOCATION_REFERENCE

### Surveillance + Optimization
- SURVEILLANCE_EVENT
- PRODUCTION_CONSTRAINT
- OPTIMIZATION_RECOMMENDATION
- WORKOVER_CANDIDATE

### Downtime + Maintenance
- DOWNTIME_EVENT
- DOWNTIME_REASON
- MAINTENANCE_REQUEST

### Facilities
- FACILITY_PRODUCTION
- SEPARATOR_TEST
- METER_READING

---

## Service Interface Standards

```csharp
public interface IProductionOperationsService
{
    Task<DAILY_PRODUCTION> RecordDailyProductionAsync(DAILY_PRODUCTION record, string userId);
    Task<WELL_STATUS_HISTORY> UpdateWellStatusAsync(WELL_STATUS_HISTORY status, string userId);
    Task<PRODUCTION_TEST> RecordWellTestAsync(PRODUCTION_TEST test, string userId);
    Task<bool> SubmitDowntimeAsync(DOWNTIME_EVENT downtime, string userId);
}
```

---

## Implementation Phases

### Phase 1: Data Model + Core Services (Week 1)
- Implement daily production and status history entities.
- Create ProductionOperationsService and validators.

### Phase 2: Surveillance + Optimization (Weeks 2-3)
- Surveillance events and constraints.
- Optimization recommendations tied to analysis modules.

### Phase 3: Downtime + Maintenance (Week 4)
- Downtime tracking and approval workflow.
- Maintenance request linkage.

### Phase 4: Integration (Week 5)
- Accounting integration for volumes and allocation.
- Well test and nodal analysis integration.

---

## Best Practices Embedded

- **Operational integrity**: daily reporting for all producing wells.
- **Surveillance discipline**: consistent tests and diagnostics.
- **Loss visibility**: downtime tracked with reason codes.
- **Optimization loop**: recommendations tracked to outcomes.

---

## API Endpoint Sketch

```
/api/production-ops/
├── /daily
│   ├── POST
│   └── GET /{wellId}
├── /status
│   ├── POST
│   └── GET /{wellId}
├── /tests
│   ├── POST
│   └── GET /{wellId}
└── /downtime
    ├── POST
    └── GET /{wellId}
```

---

## Success Criteria

- PPDM-aligned production entities persist all operational data.
- Surveillance and optimization workflows link to analysis modules.
- Downtime and status history are auditable.
- No DTOs used for persisted production state.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
