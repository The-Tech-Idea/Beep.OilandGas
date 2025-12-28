# Well Lifecycle Process - Implementation Plan

## Overview

This document defines the complete lifecycle process for Wells, tracking state transitions from planning through abandonment.

## Current State

### What Exists
- ✅ Basic CRUD for WELL table
- ✅ Basic well status tracking (WELL_STATUS table)
- ✅ Well status methods in PPDMCalculationService
- ❌ **Missing**: Well lifecycle state machine, state transition validation, lifecycle process orchestration

### What Needs to Be Built
1. Well lifecycle state machine
2. State transition validation
3. Lifecycle process orchestration
4. Integration with phase-specific processes

## Well Lifecycle States

### State Definitions

```
PLANNED → DRILLING → COMPLETED → PRODUCING → WORKOVER → SUSPENDED → ABANDONED
   ↓         ↓           ↓           ↓          ↓           ↓
REJECTED  REJECTED   REJECTED   REJECTED   REJECTED   REJECTED (terminal)
```

### State Descriptions

**PLANNED**: Well is planned but not yet drilled
- Entry: Well created in planning phase
- Exit: Drilling permit obtained, drilling started

**DRILLING**: Well is being drilled
- Entry: Drilling started
- Exit: Drilling completed or failed

**COMPLETED**: Well is completed but not yet producing
- Entry: Drilling completed, completion done
- Exit: Production started or well failed

**PRODUCING**: Well is in active production
- Entry: Production started
- Exit: Production stopped, workover needed, or abandonment

**WORKOVER**: Well is undergoing workover operations
- Entry: Workover started
- Exit: Workover completed (return to PRODUCING) or failed

**SUSPENDED**: Well production is suspended
- Entry: Production suspended
- Exit: Production restarted or abandonment

**ABANDONED**: Well is abandoned
- Entry: Abandonment completed
- Exit: Terminal state

## State Transitions

### Allowed Transitions

1. **PLANNED → DRILLING**
   - Condition: Drilling permit obtained
   - Process: Well Development Process (Drilling step)

2. **DRILLING → COMPLETED**
   - Condition: Drilling successful
   - Process: Well Development Process (Completion step)

3. **DRILLING → REJECTED**
   - Condition: Drilling failed
   - Process: Well Development Process (Failure)

4. **COMPLETED → PRODUCING**
   - Condition: Production approved and started
   - Process: Well Production Startup Process

5. **COMPLETED → REJECTED**
   - Condition: Completion failed or not economic

6. **PRODUCING → WORKOVER**
   - Condition: Workover decision made
   - Process: Workover Process

7. **PRODUCING → SUSPENDED**
   - Condition: Production suspended (temporary)

8. **PRODUCING → ABANDONED**
   - Condition: Abandonment decision made
   - Process: Well Abandonment Process

9. **WORKOVER → PRODUCING**
   - Condition: Workover successful
   - Process: Workover Process (Production restart)

10. **WORKOVER → ABANDONED**
    - Condition: Workover failed or not economic

11. **SUSPENDED → PRODUCING**
    - Condition: Production restarted

12. **SUSPENDED → ABANDONED**
    - Condition: Abandonment decision made

## Service Implementation

### WellLifecycleService
**Location**: `Beep.OilandGas.LifeCycle/Services/WellLifecycle/WellLifecycleService.cs`

#### Methods

```csharp
// State Management
Task<bool> TransitionWellStateAsync(string wellId, string targetState, string userId);
Task<List<string>> GetAvailableTransitionsAsync(string wellId);
Task<bool> CanTransitionAsync(string wellId, string targetState);
Task<string> GetCurrentWellStateAsync(string wellId);
Task<List<WellStateHistory>> GetWellStateHistoryAsync(string wellId);

// Lifecycle Process Integration
Task<bool> StartDrillingProcessAsync(string wellId, string fieldId, string userId);
Task<bool> StartCompletionProcessAsync(string wellId, string fieldId, string userId);
Task<bool> StartProductionProcessAsync(string wellId, string fieldId, string userId);
Task<bool> StartWorkoverProcessAsync(string wellId, string fieldId, string userId);
Task<bool> StartAbandonmentProcessAsync(string wellId, string fieldId, string userId);

// State Validation
Task<ValidationResult> ValidateStateTransitionAsync(string wellId, string fromState, string toState);
Task<bool> ValidateWellStateAsync(string wellId, string requiredState);
```

### WellStateMachine
**Location**: `Beep.OilandGas.LifeCycle/Services/WellLifecycle/WellStateMachine.cs`

State machine engine for well state transitions.

## Integration with Phase Processes

### Exploration Phase
- Well created in PLANNED state
- Exploratory wells follow exploration process

### Development Phase
- Well transitions PLANNED → DRILLING → COMPLETED
- Uses Well Development Process

### Production Phase
- Well transitions COMPLETED → PRODUCING
- Uses Well Production Startup Process
- Well transitions PRODUCING → WORKOVER → PRODUCING
- Uses Workover Process

### Decommissioning Phase
- Well transitions PRODUCING/SUSPENDED → ABANDONED
- Uses Well Abandonment Process

## PPDM Tables

### Existing Tables (Use These)
- `WELL` - Well data
- `WELL_STATUS` - Well status history

### Enhancements Needed
- Add state machine tracking to WELL_STATUS
- Add transition history tracking

## DTOs Required

```csharp
public class WellStateTransitionRequest { }
public class WellStateTransitionResponse { }
public class WellStateHistoryRequest { }
public class WellStateHistoryResponse { }
```

## MVVM Structure

### ViewModels (Future)
- `WellLifecycleViewModel.cs` - Well lifecycle state management UI
- `WellStateTransitionViewModel.cs` - State transition UI

## Implementation Steps

1. Create WellStateMachine class
2. Implement WellLifecycleService
3. Define state transition rules
4. Integrate with phase-specific processes
5. Create state transition validation
6. Add state history tracking

