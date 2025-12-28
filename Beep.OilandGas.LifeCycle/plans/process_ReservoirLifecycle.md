# Reservoir Lifecycle Process - Implementation Plan

## Overview

This document defines the complete lifecycle process for Reservoirs, tracking state transitions from discovery through abandonment.

## Current State

### What Exists
- ✅ Basic CRUD for RESERVE_ENTITY table (reservoir data)
- ✅ Pool management (pools contain reservoirs)
- ❌ **Missing**: Reservoir lifecycle state machine, reservoir state tracking, lifecycle process orchestration

### What Needs to Be Built
1. Reservoir lifecycle state machine
2. State transition validation
3. Lifecycle process orchestration
4. Integration with exploration, development, and production processes

## Reservoir Lifecycle States

### State Definitions

```
DISCOVERED → APPRAISED → DEVELOPED → PRODUCING → DEPLETED → ABANDONED
     ↓            ↓           ↓           ↓
  REJECTED    REJECTED   REJECTED   REJECTED (terminal)
```

### State Descriptions

**DISCOVERED**: Reservoir has been discovered
- Entry: Discovery recorded
- Exit: Appraisal completed

**APPRAISED**: Reservoir has been appraised
- Entry: Appraisal activities completed
- Exit: Development approved

**DEVELOPED**: Reservoir is being developed
- Entry: Development started
- Exit: Production started

**PRODUCING**: Reservoir is in production
- Entry: Production started
- Exit: Reservoir depleted or abandoned

**DEPLETED**: Reservoir is depleted
- Entry: Production declined, reserves depleted
- Exit: Abandonment

**ABANDONED**: Reservoir is abandoned
- Entry: Abandonment completed
- Exit: Terminal state

## State Transitions

### Allowed Transitions

1. **DISCOVERED → APPRAISED**
   - Condition: Appraisal completed
   - Process: Discovery to Development Decision Process (Appraisal step)

2. **DISCOVERED → REJECTED**
   - Condition: Not commercial

3. **APPRAISED → DEVELOPED**
   - Condition: Development approved
   - Process: Development Process

4. **APPRAISED → REJECTED**
   - Condition: Not economic for development

5. **DEVELOPED → PRODUCING**
   - Condition: Production started
   - Process: Production Startup Process

6. **DEVELOPED → REJECTED**
   - Condition: Development failed

7. **PRODUCING → DEPLETED**
   - Condition: Reserves depleted, production uneconomic
   - Process: Decline Management Process

8. **PRODUCING → ABANDONED**
   - Condition: Early abandonment decision

9. **DEPLETED → ABANDONED**
   - Condition: Abandonment decision
   - Process: Decommissioning Process

## Service Implementation

### ReservoirLifecycleService
**Location**: `Beep.OilandGas.LifeCycle/Services/ReservoirLifecycle/ReservoirLifecycleService.cs`

#### Methods

```csharp
// State Management
Task<bool> TransitionReservoirStateAsync(string reservoirId, string targetState, string userId);
Task<List<string>> GetAvailableTransitionsAsync(string reservoirId);
Task<bool> CanTransitionAsync(string reservoirId, string targetState);
Task<string> GetCurrentReservoirStateAsync(string reservoirId);
Task<List<ReservoirStateHistory>> GetReservoirStateHistoryAsync(string reservoirId);

// Lifecycle Process Integration
Task<bool> RecordDiscoveryAsync(string reservoirId, string fieldId, DiscoveryRequest data, string userId);
Task<bool> CompleteAppraisalAsync(string reservoirId, string fieldId, string userId);
Task<bool> StartDevelopmentAsync(string reservoirId, string fieldId, string userId);
Task<bool> StartProductionAsync(string reservoirId, string fieldId, string userId);
Task<bool> MarkDepletedAsync(string reservoirId, string fieldId, string userId);
Task<bool> AbandonReservoirAsync(string reservoirId, string fieldId, string userId);
```

### ReservoirStateMachine
**Location**: `Beep.OilandGas.LifeCycle/Services/ReservoirLifecycle/ReservoirStateMachine.cs`

State machine engine for reservoir state transitions.

## Integration with Phase Processes

### Exploration Phase
- Reservoir DISCOVERED state
- Appraisal activities

### Development Phase
- Reservoir DEVELOPED state
- Development activities

### Production Phase
- Reservoir PRODUCING state
- Production operations

### Decommissioning Phase
- Reservoir ABANDONED state
- Abandonment activities

## PPDM Tables

### Existing Tables (Use These)
- `RESERVE_ENTITY` - Reservoir/reserve data
- `POOL` - Pool data (pools contain reservoirs)

### New Tables Needed

#### RESERVOIR_STATUS Table
```sql
CREATE TABLE RESERVOIR_STATUS (
    RESERVOIR_STATUS_ID VARCHAR(50) PRIMARY KEY,
    RESERVE_ENTITY_ID VARCHAR(50), -- or POOL_ID
    STATUS VARCHAR(50), -- DISCOVERED, APPRAISED, DEVELOPED, PRODUCING, DEPLETED, ABANDONED, REJECTED
    STATUS_DATE DATETIME,
    STATUS_CHANGED_BY VARCHAR(50),
    NOTES TEXT,
    ROW_CREATED_DATE DATETIME
);
```

## DTOs Required

```csharp
public class ReservoirStateTransitionRequest { }
public class ReservoirStateTransitionResponse { }
public class ReservoirStateHistoryRequest { }
public class ReservoirStateHistoryResponse { }
```

## MVVM Structure

### ViewModels (Future)
- `ReservoirLifecycleViewModel.cs` - Reservoir lifecycle state management UI
- `ReservoirStateTransitionViewModel.cs` - State transition UI

## Implementation Steps

1. Create ReservoirStateMachine class
2. Implement ReservoirLifecycleService
3. Define state transition rules
4. Integrate with phase-specific processes
5. Create state transition validation
6. Add state history tracking

