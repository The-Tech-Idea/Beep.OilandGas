# MVVM Architecture for Lifecycle Processes - Implementation Plan

## Overview

This document defines the MVVM (Model-View-ViewModel) architecture for implementing lifecycle processes in the Blazor UI layer. Process definitions remain in services, ViewModels orchestrate UI state and call services.

## Architecture Principles

### Separation of Concerns

**Models (Service Layer)**
- Process definitions
- Process instances
- Business logic
- Data access

**ViewModels (UI Layer - Future)**
- UI state management
- Process orchestration for UI
- Validation for UI
- Command handling

**Views (Blazor UI - Future)**
- User interface
- Data binding
- User interactions

## MVVM Structure

### 1. Process Models (Service Layer)

**Location**: `Beep.OilandGas.LifeCycle/Models/Processes/`

Already defined in process_Foundation.md:
- `ProcessDefinition.cs`
- `ProcessInstance.cs`
- `ProcessStep.cs`
- `ProcessState.cs`
- `ProcessTransition.cs`
- `ProcessHistory.cs`

### 2. Process ViewModels (UI Layer)

**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Processes/` (for future Blazor integration)

#### 2.1 ProcessViewModelBase
**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Processes/ProcessViewModelBase.cs`

```csharp
public abstract class ProcessViewModelBase : INotifyPropertyChanged
{
    protected readonly IProcessService _processService;
    protected readonly ILogger _logger;
    
    public ProcessInstance? CurrentInstance { get; set; }
    public ProcessDefinition? ProcessDefinition { get; set; }
    public ProcessStepInstance? CurrentStep { get; set; }
    public List<ProcessStepInstance> CompletedSteps { get; set; }
    public List<ProcessStepInstance> PendingSteps { get; set; }
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Commands
    public ICommand ExecuteStepCommand { get; }
    public ICommand CompleteStepCommand { get; }
    public ICommand SkipStepCommand { get; }
    public ICommand RollbackStepCommand { get; }
    
    // Methods
    public abstract Task LoadProcessAsync(string instanceId);
    public abstract Task ExecuteCurrentStepAsync();
    public abstract Task CompleteCurrentStepAsync();
    public abstract Task<bool> ValidateCurrentStepAsync();
}
```

#### 2.2 Exploration Process ViewModels

**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Exploration/`

- `LeadToProspectViewModel.cs` - Lead promotion process UI
- `ProspectEvaluationViewModel.cs` - Prospect evaluation process UI
- `DiscoveryRecordingViewModel.cs` - Discovery recording UI
- `DevelopmentDecisionViewModel.cs` - Development decision UI

#### 2.3 Development Process ViewModels

**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Development/`

- `PoolDefinitionViewModel.cs` - Pool definition process UI
- `FacilityDevelopmentViewModel.cs` - Facility development process UI
- `WellDevelopmentViewModel.cs` - Well development process UI
- `PipelineDevelopmentViewModel.cs` - Pipeline development process UI

#### 2.4 Production Process ViewModels

**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Production/`

- `WellStartupViewModel.cs` - Well production startup UI
- `ProductionOperationsViewModel.cs` - Production operations UI
- `DeclineManagementViewModel.cs` - Decline management UI
- `WorkoverViewModel.cs` - Workover process UI

#### 2.5 Decommissioning Process ViewModels

**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Decommissioning/`

- `WellAbandonmentViewModel.cs` - Well abandonment UI
- `FacilityDecommissioningViewModel.cs` - Facility decommissioning UI

#### 2.6 Lifecycle ViewModels

**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Lifecycle/`

- `WellLifecycleViewModel.cs` - Well lifecycle state management UI
- `FieldLifecycleViewModel.cs` - Field lifecycle state management UI
- `ReservoirLifecycleViewModel.cs` - Reservoir lifecycle state management UI

### 3. Process UI Models

**Location**: `Beep.OilandGas.LifeCycle/Models/ProcessUI/`

#### 3.1 ProcessStepViewModel
```csharp
public class ProcessStepViewModel
{
    public string StepId { get; set; }
    public string StepName { get; set; }
    public int SequenceNumber { get; set; }
    public StepStatus Status { get; set; }
    public bool IsCurrentStep { get; set; }
    public bool CanExecute { get; set; }
    public bool RequiresApproval { get; set; }
    public List<ApprovalViewModel> Approvals { get; set; }
    public Dictionary<string, object> StepData { get; set; }
    public List<ValidationMessage> ValidationMessages { get; set; }
}
```

#### 3.2 ProcessStatusViewModel
```csharp
public class ProcessStatusViewModel
{
    public string InstanceId { get; set; }
    public string ProcessName { get; set; }
    public ProcessStatus Status { get; set; }
    public string CurrentState { get; set; }
    public string CurrentStepName { get; set; }
    public int CompletedStepsCount { get; set; }
    public int TotalStepsCount { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public List<ProcessStepViewModel> Steps { get; set; }
}
```

#### 3.3 ProcessActionViewModel
```csharp
public class ProcessActionViewModel
{
    public string ActionId { get; set; }
    public string ActionName { get; set; }
    public string ActionType { get; set; } // EXECUTE, COMPLETE, SKIP, ROLLBACK, APPROVE
    public bool IsEnabled { get; set; }
    public string? DisabledReason { get; set; }
    public bool RequiresConfirmation { get; set; }
    public string? ConfirmationMessage { get; set; }
}
```

## Data Flow

### Process Execution Flow

```
View (Blazor) 
  → ViewModel (Command)
    → ProcessService (Business Logic)
      → Phase Service (CRUD Operations)
        → PPDM Repository (Data Access)
          → Database
```

### State Management Flow

```
ProcessService (State Change)
  → ViewModel (Property Changed)
    → View (UI Update)
```

## ViewModel Responsibilities

### 1. Process Loading
- Load process instance
- Load process definition
- Load step instances
- Calculate progress

### 2. Step Execution
- Validate step data
- Execute step via service
- Handle step completion
- Update UI state

### 3. State Management
- Track current step
- Track completed steps
- Track pending steps
- Manage UI state

### 4. Validation
- Client-side validation
- Display validation messages
- Enable/disable actions

### 5. Error Handling
- Display error messages
- Handle service exceptions
- Provide retry mechanisms

## Implementation Pattern

### Example: WellStartupViewModel

```csharp
public class WellStartupViewModel : ProcessViewModelBase
{
    private readonly ProductionProcessService _productionProcessService;
    
    // Step-specific properties
    public WellTestRequest? TestData { get; set; }
    public ProductionStartRequest? StartData { get; set; }
    
    public WellStartupViewModel(
        IProcessService processService,
        ProductionProcessService productionProcessService,
        ILogger logger)
        : base(processService, logger)
    {
        _productionProcessService = productionProcessService;
    }
    
    public override async Task LoadProcessAsync(string instanceId)
    {
        IsLoading = true;
        try
        {
            CurrentInstance = await _processService.GetProcessInstanceAsync(instanceId);
            ProcessDefinition = await _processService.GetProcessDefinitionAsync(CurrentInstance.ProcessId);
            // Load step instances, calculate progress, etc.
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    public override async Task ExecuteCurrentStepAsync()
    {
        if (CurrentStep == null) return;
        
        var validationResult = await ValidateCurrentStepAsync();
        if (!validationResult.IsValid)
        {
            ErrorMessage = validationResult.ErrorMessage;
            return;
        }
        
        try
        {
            await _productionProcessService.ExecuteStepAsync(
                CurrentInstance.InstanceId,
                CurrentStep.StepId,
                GetStepData(),
                GetCurrentUserId());
            
            await LoadProcessAsync(CurrentInstance.InstanceId);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
```

## Integration Points

### Service Integration
- ViewModels call ProcessService methods
- ProcessService calls Phase Service methods
- Phase Services perform CRUD operations

### UI Integration (Future Blazor)
- Views bind to ViewModel properties
- Views handle user interactions via Commands
- Views display process status and steps

## Implementation Steps

1. **Create ViewModel Base Classes**
   - ProcessViewModelBase
   - LifecycleViewModelBase

2. **Create Process ViewModels**
   - Exploration process ViewModels
   - Development process ViewModels
   - Production process ViewModels
   - Decommissioning process ViewModels

3. **Create Lifecycle ViewModels**
   - WellLifecycleViewModel
   - FieldLifecycleViewModel
   - ReservoirLifecycleViewModel

4. **Create Process UI Models**
   - ProcessStepViewModel
   - ProcessStatusViewModel
   - ProcessActionViewModel

5. **Create View Components** (Future Blazor)
   - ProcessStepComponent.razor
   - ProcessStatusComponent.razor
   - ProcessActionComponent.razor

## Testing Strategy

1. Unit tests for ViewModels
2. Integration tests for ViewModel-Service interaction
3. UI tests for View-ViewModel binding (Future)

## Next Steps

After process foundation is implemented:
1. Create ViewModel base classes
2. Create process-specific ViewModels
3. Create lifecycle ViewModels
4. Create Blazor view components (in Blazor project)

