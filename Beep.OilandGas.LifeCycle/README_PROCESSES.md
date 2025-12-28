# Lifecycle Process Management - User Guide

## Overview

The Lifecycle Process Management system provides comprehensive workflow orchestration for all phases of the oil and gas lifecycle. It manages multi-step processes with state transitions, approvals, and history tracking.

## Architecture

### Two-Level Status Tracking

1. **Entity Status** (PPDM Database)
   - Current state of entities (Well, Field, Reservoir, etc.)
   - Stored in PPDM standard tables

2. **Process Status** (Application Database)
   - Workflow process execution
   - Process definitions, instances, steps, history
   - Application-level tables (NOT PPDM standard)

## Quick Start

### 1. Initialize Process Definitions

```csharp
var processService = new PPDMProcessService(editor, commonColumnHandler, defaults, metadata);
var initializer = new ProcessDefinitionInitializer(processService, logger);

// Initialize all default process definitions
await initializer.InitializeDefaultProcessDefinitionsAsync(userId);
```

### 2. Start a Process

```csharp
var explorationProcessService = new ExplorationProcessService(processService, explorationService, logger);

// Start Lead to Prospect process
var instance = await explorationProcessService.StartLeadToProspectProcessAsync(leadId, fieldId, userId);
```

### 3. Execute Process Steps

```csharp
// Evaluate lead
await explorationProcessService.EvaluateLeadAsync(instance.InstanceId, evaluationData, userId);

// Approve lead
await explorationProcessService.ApproveLeadAsync(instance.InstanceId, userId);

// Promote to prospect
await explorationProcessService.PromoteLeadToProspectAsync(instance.InstanceId, prospectData, userId);
```

### 4. Manage Entity Lifecycle

```csharp
var wellLifecycleService = new WellLifecycleService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);

// Transition well state
await wellLifecycleService.TransitionWellStateAsync(wellId, "DRILLING", userId);

// Get available transitions
var transitions = await wellLifecycleService.GetAvailableTransitionsAsync(wellId);

// Get state history
var history = await wellLifecycleService.GetWellStateHistoryAsync(wellId);
```

## Available Processes

### Exploration Processes

1. **Lead to Prospect**
   - Lead Creation → Lead Evaluation → Lead Approval → Prospect Creation → Prospect Assessment

2. **Prospect to Discovery**
   - Prospect Creation → Risk Assessment → Volume Estimation → Economic Evaluation → Drilling Decision → Discovery Recording

3. **Discovery to Development**
   - Discovery Recording → Appraisal → Reserve Estimation → Economic Analysis → Development Approval

### Development Processes

1. **Pool Definition**
   - Pool Identification → Pool Delineation → Reserve Assignment → Pool Approval → Pool Active

2. **Facility Development**
   - Facility Planning → Design → Permitting → Construction → Testing → Commissioning → Facility Active

3. **Well Development**
   - Well Planning → Drilling Permit → Drilling → Completion → Production Testing → Production Handover

4. **Pipeline Development**
   - Pipeline Planning → Design → Permitting → Construction → Testing → Commissioning → Pipeline Active

### Production Processes

1. **Well Production Startup**
   - Well Completion → Production Testing → Production Approval → Production Start → Producing

2. **Production Operations**
   - Daily Production → Production Monitoring → Performance Analysis → Optimization Decision → Optimization Execution

3. **Decline Management**
   - Decline Detection → DCA Analysis → Production Forecast → Economic Analysis → Workover Decision

4. **Workover**
   - Workover Planning → Workover Approval → Workover Execution → Post-Workover Testing → Production Restart

### Decommissioning Processes

1. **Well Abandonment**
   - Abandonment Planning → Regulatory Approval → Well Plugging → Site Restoration → Abandonment Completion

2. **Facility Decommissioning**
   - Decommissioning Planning → Equipment Removal → Site Cleanup → Regulatory Closure → Decommissioning Complete

## Entity Lifecycle States

### Well States
- PLANNED → DRILLING → COMPLETED → PRODUCING → WORKOVER → SUSPENDED → ABANDONED

### Field Phases
- EXPLORATION → DEVELOPMENT → PRODUCTION → DECLINE → DECOMMISSIONING → DECOMMISSIONED

### Reservoir States
- DISCOVERED → APPRAISED → DEVELOPED → PRODUCING → DEPLETED → ABANDONED

## Database Setup

### 1. PPDM Database
Run SQL scripts in `Beep.OilandGas.PPDM39/Scripts/Sqlserver/`:
- FIELD_PHASE_*.sql
- RESERVOIR_STATUS_*.sql
- ABANDONMENT_STATUS_*.sql
- DECOMMISSIONING_STATUS_*.sql

### 2. Application Database
Run SQL script in `Beep.OilandGas.LifeCycle/Scripts/`:
- ProcessWorkflowTables.sql

## Service Registration

Register services in your DI container:

```csharp
services.AddScoped<IProcessService, PPDMProcessService>();
services.AddScoped<ExplorationProcessService>();
services.AddScoped<DevelopmentProcessService>();
services.AddScoped<ProductionProcessService>();
services.AddScoped<DecommissioningProcessService>();
services.AddScoped<WellLifecycleService>();
services.AddScoped<FieldLifecycleService>();
services.AddScoped<ReservoirLifecycleService>();
```

## Integration with Existing Services

Process services integrate with existing phase services:
- ExplorationProcessService uses PPDMExplorationService
- DevelopmentProcessService uses PPDMDevelopmentService
- ProductionProcessService uses PPDMProductionService
- DecommissioningProcessService uses PPDMDecommissioningService

## Helper Classes

### ProcessServiceExtensions
Extension methods for working with DTOs:
```csharp
// Get process status as DTO
var status = await processService.GetProcessStatusDtoAsync(instanceId);

// Get process instance as DTO
var instanceDto = await processService.GetProcessInstanceDtoAsync(instanceId);

// Start process from DTO
var response = await processService.StartProcessFromDtoAsync(request, userId);
```

### ProcessIntegrationHelper
Coordinates between process services and lifecycle services:
```csharp
var helper = new ProcessIntegrationHelper(
    processService,
    explorationProcessService,
    developmentProcessService,
    productionProcessService,
    decommissioningProcessService,
    wellLifecycleService,
    fieldLifecycleService,
    reservoirLifecycleService,
    logger);

// Start well development with automatic state transition
var instance = await helper.StartWellDevelopmentWithStateTransitionAsync(wellId, fieldId, userId);

// Get comprehensive entity status
var status = await helper.GetEntityProcessStatusAsync(entityId, entityType);
```

## Process Status Tracking

Process status is tracked in two places:
1. **Process Instance** (Application DB) - Current process execution state
2. **Entity Status** (PPDM DB) - Current entity state

Both are kept in sync automatically when processes execute.

## Error Handling

All process services include comprehensive error handling and logging. Errors are logged and exceptions are thrown with descriptive messages.

## Next Steps

1. Initialize process definitions on application startup
2. Integrate process services into your API controllers
3. Create UI components for process management (Blazor)
4. Add process notifications
5. Create process reporting and analytics

