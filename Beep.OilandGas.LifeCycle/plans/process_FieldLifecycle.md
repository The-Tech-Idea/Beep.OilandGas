# Field Lifecycle Process - Implementation Plan

## Overview

This document defines the complete lifecycle process for Fields, tracking state transitions across all phases.

## Current State

### What Exists
- ✅ Basic CRUD for FIELD table
- ✅ FieldOrchestrator for field coordination
- ✅ Phase services (Exploration, Development, Production, Decommissioning)
- ❌ **Missing**: Field lifecycle state machine, phase transition management, field status tracking

### What Needs to Be Built
1. Field lifecycle state machine
2. Phase transition validation
3. Field status tracking
4. Integration with phase processes

## Field Lifecycle States

### State Definitions

```
EXPLORATION → DEVELOPMENT → PRODUCTION → DECLINE → DECOMMISSIONING → DECOMMISSIONED
     ↓              ↓            ↓           ↓
  REJECTED      REJECTED    REJECTED    REJECTED (terminal)
```

### State Descriptions

**EXPLORATION**: Field is in exploration phase
- Activities: Prospect evaluation, seismic surveys, exploratory drilling
- Exit: Discovery made and development approved

**DEVELOPMENT**: Field is in development phase
- Activities: Pool definition, facility construction, well drilling
- Exit: Development complete, production started

**PRODUCTION**: Field is in production phase
- Activities: Production operations, well management, optimization
- Exit: Production declined, decommissioning decision

**DECLINE**: Field production is declining
- Activities: Decline management, workover decisions, optimization
- Exit: Decommissioning decision or production recovery

**DECOMMISSIONING**: Field is being decommissioned
- Activities: Well abandonment, facility decommissioning, site restoration
- Exit: Decommissioning complete

**DECOMMISSIONED**: Field decommissioning complete
- Terminal state

## Phase Transitions

### Allowed Transitions

1. **EXPLORATION → DEVELOPMENT**
   - Condition: Discovery made, development approved
   - Process: Discovery to Development Decision Process

2. **EXPLORATION → REJECTED**
   - Condition: No commercial discovery

3. **DEVELOPMENT → PRODUCTION**
   - Condition: First production achieved
   - Process: Development completion, production startup

4. **DEVELOPMENT → REJECTED**
   - Condition: Development not economic

5. **PRODUCTION → DECLINE**
   - Condition: Production decline detected
   - Process: Decline Management Process

6. **PRODUCTION → DECOMMISSIONING**
   - Condition: Early decommissioning decision

7. **DECLINE → PRODUCTION**
   - Condition: Production recovery (rare)

8. **DECLINE → DECOMMISSIONING**
   - Condition: Decommissioning decision made
   - Process: Decommissioning Process

9. **DECOMMISSIONING → DECOMMISSIONED**
   - Condition: All decommissioning activities complete
   - Process: Decommissioning Process completion

## Service Implementation

### FieldLifecycleService
**Location**: `Beep.OilandGas.LifeCycle/Services/FieldLifecycle/FieldLifecycleService.cs`

#### Methods

```csharp
// State Management
Task<bool> TransitionFieldPhaseAsync(string fieldId, string targetPhase, string userId);
Task<List<string>> GetAvailablePhaseTransitionsAsync(string fieldId);
Task<bool> CanTransitionPhaseAsync(string fieldId, string targetPhase);
Task<string> GetCurrentFieldPhaseAsync(string fieldId);
Task<FieldPhaseStatus> GetFieldPhaseStatusAsync(string fieldId);

// Phase Process Integration
Task<bool> StartExplorationPhaseAsync(string fieldId, string userId);
Task<bool> StartDevelopmentPhaseAsync(string fieldId, string userId);
Task<bool> StartProductionPhaseAsync(string fieldId, string userId);
Task<bool> StartDeclinePhaseAsync(string fieldId, string userId);
Task<bool> StartDecommissioningPhaseAsync(string fieldId, string userId);

// Phase Validation
Task<ValidationResult> ValidatePhaseTransitionAsync(string fieldId, string fromPhase, string toPhase);
Task<bool> ValidatePhaseCompletionAsync(string fieldId, string phase);
```

### FieldStateMachine
**Location**: `Beep.OilandGas.LifeCycle/Services/FieldLifecycle/FieldStateMachine.cs`

State machine engine for field phase transitions.

## Integration with Phase Services

### Exploration Service
- Field starts in EXPLORATION phase
- Discovery triggers transition to DEVELOPMENT

### Development Service
- Field in DEVELOPMENT phase
- First production triggers transition to PRODUCTION

### Production Service
- Field in PRODUCTION phase
- Decline detection triggers transition to DECLINE

### Decommissioning Service
- Field transitions to DECOMMISSIONING phase
- Completion triggers transition to DECOMMISSIONED

## PPDM Tables

### Existing Tables (Use These)
- `FIELD` - Field data

### New Tables Needed

#### FIELD_PHASE Table
```sql
CREATE TABLE FIELD_PHASE (
    FIELD_PHASE_ID VARCHAR(50) PRIMARY KEY,
    FIELD_ID VARCHAR(50),
    PHASE VARCHAR(50), -- EXPLORATION, DEVELOPMENT, PRODUCTION, DECLINE, DECOMMISSIONING, DECOMMISSIONED
    PHASE_START_DATE DATETIME,
    PHASE_END_DATE DATETIME,
    PHASE_STATUS VARCHAR(50), -- ACTIVE, COMPLETED, REJECTED
    TRANSITION_REASON TEXT,
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50)
);
```

## DTOs Required

```csharp
public class FieldPhaseTransitionRequest { }
public class FieldPhaseTransitionResponse { }
public class FieldPhaseStatus { }
```

## MVVM Structure

### ViewModels (Future)
- `FieldLifecycleViewModel.cs` - Field lifecycle management UI
- `FieldPhaseTransitionViewModel.cs` - Phase transition UI

## Implementation Steps

1. Create FieldStateMachine class
2. Implement FieldLifecycleService
3. Define phase transition rules
4. Integrate with phase services
5. Create phase transition validation
6. Add phase history tracking
7. Update FieldOrchestrator to use lifecycle service

