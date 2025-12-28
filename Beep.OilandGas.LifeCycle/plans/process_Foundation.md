# Process Foundation - Implementation Plan

## Overview

This document defines the foundation for process/workflow management in the LifeCycle system. It establishes the core models, services, and state machine infrastructure that all lifecycle processes will build upon.

## Current State

### What Exists
- ✅ Basic CRUD operations in all phase services
- ✅ Permit status tracking (basic)
- ✅ Well status tracking (basic via WELL_STATUS table)
- ❌ **Missing**: Process workflow engine, state machines, process orchestration

### What Needs to Be Built
1. Process definition models
2. Process instance tracking
3. State machine engine
4. Process validation framework
5. Process history tracking
6. Base process service

## Process Foundation Architecture

### 1. Process Models

#### 1.1 ProcessDefinition
**Location**: `Beep.OilandGas.LifeCycle/Models/Processes/ProcessDefinition.cs`

```csharp
public class ProcessDefinition
{
    public string ProcessId { get; set; }
    public string ProcessName { get; set; }
    public string ProcessType { get; set; } // EXPLORATION, DEVELOPMENT, PRODUCTION, DECOMMISSIONING
    public string EntityType { get; set; } // WELL, FIELD, RESERVOIR, PROSPECT, POOL, FACILITY
    public string Description { get; set; }
    public List<ProcessStepDefinition> Steps { get; set; }
    public Dictionary<string, ProcessTransition> Transitions { get; set; }
    public Dictionary<string, object> Configuration { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
}
```

#### 1.2 ProcessStepDefinition
**Location**: `Beep.OilandGas.LifeCycle/Models/Processes/ProcessStepDefinition.cs`

```csharp
public class ProcessStepDefinition
{
    public string StepId { get; set; }
    public string StepName { get; set; }
    public int SequenceNumber { get; set; }
    public string StepType { get; set; } // ACTION, APPROVAL, VALIDATION, NOTIFICATION
    public bool IsRequired { get; set; }
    public bool RequiresApproval { get; set; }
    public List<string> RequiredRoles { get; set; }
    public Dictionary<string, ValidationRule> ValidationRules { get; set; }
    public Dictionary<string, object> StepConfiguration { get; set; }
    public string NextStepId { get; set; }
    public List<string> ConditionalNextSteps { get; set; } // Based on step outcome
}
```

#### 1.3 ProcessInstance
**Location**: `Beep.OilandGas.LifeCycle/Models/Processes/ProcessInstance.cs`

```csharp
public class ProcessInstance
{
    public string InstanceId { get; set; }
    public string ProcessId { get; set; }
    public string EntityId { get; set; } // Well, Field, Reservoir, etc.
    public string EntityType { get; set; }
    public string FieldId { get; set; }
    public string CurrentState { get; set; }
    public string CurrentStepId { get; set; }
    public ProcessStatus Status { get; set; } // NOT_STARTED, IN_PROGRESS, COMPLETED, FAILED, CANCELLED
    public DateTime StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string StartedBy { get; set; }
    public Dictionary<string, object> ProcessData { get; set; }
    public List<ProcessStepInstance> StepInstances { get; set; }
    public List<ProcessHistoryEntry> History { get; set; }
}
```

#### 1.4 ProcessStepInstance
**Location**: `Beep.OilandGas.LifeCycle/Models/Processes/ProcessStepInstance.cs`

```csharp
public class ProcessStepInstance
{
    public string StepInstanceId { get; set; }
    public string InstanceId { get; set; }
    public string StepId { get; set; }
    public StepStatus Status { get; set; } // PENDING, IN_PROGRESS, COMPLETED, FAILED, SKIPPED
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string CompletedBy { get; set; }
    public Dictionary<string, object> StepData { get; set; }
    public List<ApprovalRecord> Approvals { get; set; }
    public List<ValidationResult> ValidationResults { get; set; }
    public string Outcome { get; set; } // SUCCESS, FAILED, CONDITIONAL
    public string Notes { get; set; }
}
```

#### 1.5 ProcessState
**Location**: `Beep.OilandGas.LifeCycle/Models/Processes/ProcessState.cs`

```csharp
public class ProcessState
{
    public string StateId { get; set; }
    public string StateName { get; set; }
    public string StateType { get; set; } // INITIAL, INTERMEDIATE, FINAL, ERROR
    public string Description { get; set; }
    public Dictionary<string, object> StateProperties { get; set; }
    public List<string> AllowedTransitions { get; set; }
    public bool IsTerminal { get; set; }
}
```

#### 1.6 ProcessTransition
**Location**: `Beep.OilandGas.LifeCycle/Models/Processes/ProcessTransition.cs`

```csharp
public class ProcessTransition
{
    public string TransitionId { get; set; }
    public string FromStateId { get; set; }
    public string ToStateId { get; set; }
    public string Trigger { get; set; } // Event that triggers transition
    public TransitionCondition Condition { get; set; }
    public List<TransitionAction> Actions { get; set; }
    public bool RequiresApproval { get; set; }
    public List<string> RequiredRoles { get; set; }
}
```

#### 1.7 ProcessHistory
**Location**: `Beep.OilandGas.LifeCycle/Models/Processes/ProcessHistory.cs`

```csharp
public class ProcessHistoryEntry
{
    public string HistoryId { get; set; }
    public string InstanceId { get; set; }
    public string StepInstanceId { get; set; }
    public string Action { get; set; } // STATE_CHANGE, STEP_STARTED, STEP_COMPLETED, APPROVAL_REQUESTED, etc.
    public string PreviousState { get; set; }
    public string NewState { get; set; }
    public DateTime Timestamp { get; set; }
    public string PerformedBy { get; set; }
    public string Notes { get; set; }
    public Dictionary<string, object> ActionData { get; set; }
}
```

### 2. Process Service Base

#### 2.1 IProcessService Interface
**Location**: `Beep.OilandGas.LifeCycle/Services/Processes/IProcessService.cs`

```csharp
public interface IProcessService
{
    // Process Definition Management
    Task<ProcessDefinition> GetProcessDefinitionAsync(string processId);
    Task<List<ProcessDefinition>> GetProcessDefinitionsByTypeAsync(string processType);
    Task<ProcessDefinition> CreateProcessDefinitionAsync(ProcessDefinition definition, string userId);
    
    // Process Instance Management
    Task<ProcessInstance> StartProcessAsync(string processId, string entityId, string entityType, string fieldId, string userId);
    Task<ProcessInstance> GetProcessInstanceAsync(string instanceId);
    Task<List<ProcessInstance>> GetProcessInstancesForEntityAsync(string entityId, string entityType);
    Task<ProcessInstance> GetCurrentProcessForEntityAsync(string entityId, string entityType);
    
    // Process Execution
    Task<bool> ExecuteStepAsync(string instanceId, string stepId, Dictionary<string, object> stepData, string userId);
    Task<bool> CompleteStepAsync(string instanceId, string stepId, string outcome, string userId);
    Task<bool> SkipStepAsync(string instanceId, string stepId, string reason, string userId);
    Task<bool> RollbackStepAsync(string instanceId, string stepId, string reason, string userId);
    
    // State Management
    Task<bool> TransitionStateAsync(string instanceId, string targetState, string userId);
    Task<List<string>> GetAvailableTransitionsAsync(string instanceId);
    Task<bool> CanTransitionAsync(string instanceId, string targetState);
    
    // Process Status
    Task<ProcessStatus> GetProcessStatusAsync(string instanceId);
    Task<List<ProcessStepInstance>> GetCompletedStepsAsync(string instanceId);
    Task<List<ProcessStepInstance>> GetPendingStepsAsync(string instanceId);
    
    // Process History
    Task<List<ProcessHistoryEntry>> GetProcessHistoryAsync(string instanceId);
    Task<ProcessHistoryEntry> AddHistoryEntryAsync(string instanceId, ProcessHistoryEntry entry);
    
    // Validation
    Task<ValidationResult> ValidateStepAsync(string instanceId, string stepId, Dictionary<string, object> stepData);
    Task<bool> ValidateProcessCompletionAsync(string instanceId);
}
```

#### 2.2 ProcessServiceBase
**Location**: `Beep.OilandGas.LifeCycle/Services/Processes/ProcessServiceBase.cs`

Base implementation with common process management logic:
- Process instance creation
- Step execution orchestration
- State transition management
- History tracking
- Validation framework

#### 2.3 ProcessStateMachine
**Location**: `Beep.OilandGas.LifeCycle/Services/Processes/ProcessStateMachine.cs`

State machine engine that:
- Manages state definitions
- Validates state transitions
- Executes transition actions
- Tracks state history

#### 2.4 ProcessValidator
**Location**: `Beep.OilandGas.LifeCycle/Services/Processes/ProcessValidator.cs`

Validation framework that:
- Validates step data against rules
- Checks required approvals
- Validates business rules
- Ensures data completeness

### 3. PPDM Tables for Process Tracking

#### 3.1 PROCESS_DEFINITION Table
```sql
CREATE TABLE PROCESS_DEFINITION (
    PROCESS_DEFINITION_ID VARCHAR(50) PRIMARY KEY,
    PROCESS_NAME VARCHAR(200) NOT NULL,
    PROCESS_TYPE VARCHAR(50), -- EXPLORATION, DEVELOPMENT, PRODUCTION, DECOMMISSIONING
    ENTITY_TYPE VARCHAR(50), -- WELL, FIELD, RESERVOIR, PROSPECT, POOL, FACILITY
    DESCRIPTION TEXT,
    PROCESS_CONFIG_JSON TEXT, -- JSON with steps, transitions, configuration
    ACTIVE_IND VARCHAR(1) DEFAULT 'Y',
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50),
    ROW_CHANGED_DATE DATETIME,
    ROW_CHANGED_BY VARCHAR(50)
);
```

#### 3.2 PROCESS_INSTANCE Table
```sql
CREATE TABLE PROCESS_INSTANCE (
    PROCESS_INSTANCE_ID VARCHAR(50) PRIMARY KEY,
    PROCESS_DEFINITION_ID VARCHAR(50),
    ENTITY_ID VARCHAR(50), -- Well, Field, Reservoir, etc.
    ENTITY_TYPE VARCHAR(50),
    FIELD_ID VARCHAR(50),
    CURRENT_STATE VARCHAR(100),
    CURRENT_STEP_ID VARCHAR(50),
    STATUS VARCHAR(50), -- NOT_STARTED, IN_PROGRESS, COMPLETED, FAILED, CANCELLED
    START_DATE DATETIME,
    COMPLETION_DATE DATETIME,
    STARTED_BY VARCHAR(50),
    PROCESS_DATA_JSON TEXT, -- JSON with process-specific data
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50),
    ROW_CHANGED_DATE DATETIME,
    ROW_CHANGED_BY VARCHAR(50)
);
```

#### 3.3 PROCESS_STEP_INSTANCE Table
```sql
CREATE TABLE PROCESS_STEP_INSTANCE (
    PROCESS_STEP_INSTANCE_ID VARCHAR(50) PRIMARY KEY,
    PROCESS_INSTANCE_ID VARCHAR(50),
    STEP_ID VARCHAR(50),
    SEQUENCE_NUMBER INT,
    STATUS VARCHAR(50), -- PENDING, IN_PROGRESS, COMPLETED, FAILED, SKIPPED
    START_DATE DATETIME,
    COMPLETION_DATE DATETIME,
    COMPLETED_BY VARCHAR(50),
    STEP_DATA_JSON TEXT,
    OUTCOME VARCHAR(50), -- SUCCESS, FAILED, CONDITIONAL
    NOTES TEXT,
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50),
    ROW_CHANGED_DATE DATETIME,
    ROW_CHANGED_BY VARCHAR(50)
);
```

#### 3.4 PROCESS_HISTORY Table
```sql
CREATE TABLE PROCESS_HISTORY (
    PROCESS_HISTORY_ID VARCHAR(50) PRIMARY KEY,
    PROCESS_INSTANCE_ID VARCHAR(50),
    PROCESS_STEP_INSTANCE_ID VARCHAR(50),
    ACTION VARCHAR(100), -- STATE_CHANGE, STEP_STARTED, STEP_COMPLETED, etc.
    PREVIOUS_STATE VARCHAR(100),
    NEW_STATE VARCHAR(100),
    ACTION_DATE DATETIME,
    PERFORMED_BY VARCHAR(50),
    NOTES TEXT,
    ACTION_DATA_JSON TEXT,
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50)
);
```

#### 3.5 PROCESS_APPROVAL Table
```sql
CREATE TABLE PROCESS_APPROVAL (
    PROCESS_APPROVAL_ID VARCHAR(50) PRIMARY KEY,
    PROCESS_STEP_INSTANCE_ID VARCHAR(50),
    APPROVAL_TYPE VARCHAR(50), -- STEP_APPROVAL, STATE_TRANSITION_APPROVAL
    REQUESTED_DATE DATETIME,
    REQUESTED_BY VARCHAR(50),
    APPROVED_DATE DATETIME,
    APPROVED_BY VARCHAR(50),
    APPROVAL_STATUS VARCHAR(50), -- PENDING, APPROVED, REJECTED
    APPROVAL_NOTES TEXT,
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50),
    ROW_CHANGED_DATE DATETIME,
    ROW_CHANGED_BY VARCHAR(50)
);
```

### 4. DTOs for Process Management

#### 4.1 Process DTOs
**Location**: `Beep.OilandGas.PPDM39/Core/DTOs/ProcessDTOs.cs`

```csharp
public class ProcessDefinitionRequest { }
public class ProcessDefinitionResponse { }
public class ProcessInstanceRequest { }
public class ProcessInstanceResponse { }
public class ProcessStepExecutionRequest { }
public class ProcessStepExecutionResponse { }
public class ProcessStateTransitionRequest { }
public class ProcessStateTransitionResponse { }
public class ProcessHistoryRequest { }
public class ProcessHistoryResponse { }
```

### 5. Implementation Steps

#### Step 1: Create Process Models
1. Create `Models/Processes/` directory
2. Implement all process model classes
3. Add XML documentation
4. Add validation attributes

#### Step 2: Create Process Service Base
1. Create `Services/Processes/` directory
2. Implement `IProcessService` interface
3. Implement `ProcessServiceBase` abstract class
4. Implement `ProcessStateMachine` class
5. Implement `ProcessValidator` class

#### Step 3: Create PPDM Tables
1. Create SQL scripts for process tables
2. Add table metadata
3. Create PPDM model classes (if needed)

#### Step 4: Create Process DTOs
1. Add process DTOs to `CalculationsDTOs.cs` or create new file
2. Add mapping logic

#### Step 5: Create Process Repository
1. Add process repository methods to appropriate service
2. Implement CRUD operations for process definitions and instances

### 6. MVVM Structure

#### 6.1 Process ViewModels (Future)
**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Processes/` (for Blazor integration)

- `ProcessDefinitionViewModel.cs` - Process definition UI model
- `ProcessInstanceViewModel.cs` - Process instance UI model
- `ProcessStepViewModel.cs` - Process step UI model
- `ProcessStateViewModel.cs` - Process state UI model

#### 6.2 Process UI Models
**Location**: `Beep.OilandGas.LifeCycle/Models/ProcessUI/`

- `ProcessStepUI.cs` - UI representation of process step
- `ProcessStatusUI.cs` - UI representation of process status
- `ProcessActionUI.cs` - UI representation of process actions

### 7. Integration Points

#### 7.1 Integration with Phase Services
- Exploration services will use process engine for Lead→Prospect→Discovery workflows
- Development services will use process engine for Pool/Facility/Well development
- Production services will use process engine for Production startup and operations
- Decommissioning services will use process engine for abandonment workflows

#### 7.2 Integration with State Machines
- Well lifecycle state machine
- Field lifecycle state machine
- Reservoir lifecycle state machine

### 8. Testing Strategy

1. Unit tests for process models
2. Unit tests for state machine engine
3. Unit tests for process validator
4. Integration tests for process execution
5. End-to-end tests for complete workflows

### 9. Next Steps

After foundation is complete:
1. Implement Exploration processes
2. Implement Development processes
3. Implement Production processes
4. Implement Decommissioning processes
5. Implement Entity lifecycle processes

