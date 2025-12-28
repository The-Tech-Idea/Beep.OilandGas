# Production Process - Implementation Plan

## Overview

This document defines the process workflows for the Production phase, including Well production startup, Production operations, Production optimization, and Decline management processes.

## Current State

### What Exists
- ✅ Basic CRUD for production data (PDEN_VOL_SUMMARY)
- ✅ Basic CRUD for reserves (RESERVE_ENTITY)
- ✅ DCA integration for decline analysis
- ✅ ProductionForecasting integration
- ✅ ChokeAnalysis integration
- ✅ SuckerRodPumping integration
- ❌ **Missing**: Well startup workflow, Production operations workflow, Decline management workflow, Workover decision workflow

### What Needs to Be Built
1. Well production startup process (Completion → Testing → Approval → Production Start)
2. Production operations process (Daily Production → Monitoring → Analysis → Optimization)
3. Production decline management process (Decline Detection → DCA Analysis → Forecast → Workover Decision)
4. Workover process (Workover Planning → Execution → Testing → Production Restart)

## Production Process Workflows

### Process 1: Well Production Startup

#### Workflow Definition
```
Well Completion → Production Testing → Production Approval → Production Start → Producing
```

#### Detailed Steps

**Step 1: Well Completion**
- **Action**: Well completion received from Development
- **Required Data**: Completion data, completion equipment
- **Validation**: Well must be in COMPLETED status
- **Output**: Well status updated to READY_FOR_PRODUCTION

**Step 2: Production Testing**
- **Action**: Perform production test
- **Required Data**: Test procedures, test duration, test results
- **Validation**: Well must be ready for production
- **Integration**: Uses WellTestAnalysis for pressure transient analysis
- **Output**: Production test results, well status updated to TESTING

**Step 3: Production Approval**
- **Action**: Approve well for production
- **Required Data**: Test results, production rates, economic justification
- **Validation**: Production test must be completed
- **Approval**: Required from Production Manager
- **Output**: Production approval granted, well status updated to APPROVED

**Step 4: Production Start**
- **Action**: Start production operations
- **Required Data**: Initial production rates, choke settings, operating parameters
- **Validation**: Approval must be granted
- **Integration**: May use ChokeAnalysis for choke sizing
- **Output**: Production started, well status updated to PRODUCING

**Step 5: Producing**
- **Action**: Well in active production
- **Required Data**: Ongoing production data
- **Validation**: Production must be started
- **Output**: Well status confirmed as PRODUCING

#### State Machine
```
COMPLETED → READY_FOR_PRODUCTION → TESTING → APPROVED → PRODUCING
    ↓                ↓                ↓
REJECTED        REJECTED        REJECTED (terminal)
```

### Process 2: Production Operations

#### Workflow Definition
```
Daily Production → Production Monitoring → Performance Analysis → Optimization Decision → Optimization Execution
```

#### Detailed Steps

**Step 1: Daily Production**
- **Action**: Record daily production data
- **Required Data**: Oil production, gas production, water production, wellhead pressure, flowing pressure
- **Validation**: Production data must be valid
- **Output**: Production data recorded in PDEN_VOL_SUMMARY

**Step 2: Production Monitoring**
- **Action**: Monitor well performance
- **Required Data**: Production trends, pressure trends, equipment status
- **Validation**: Production data must exist
- **Output**: Monitoring alerts, performance indicators

**Step 3: Performance Analysis**
- **Action**: Analyze well performance
- **Required Data**: Historical production, current performance, reservoir data
- **Validation**: Sufficient production history required
- **Integration**: Uses NodalAnalysis for performance optimization
- **Output**: Performance analysis results, recommendations

**Step 4: Optimization Decision**
- **Action**: Decide on optimization actions
- **Required Data**: Analysis results, optimization options, economic impact
- **Validation**: Performance analysis must be completed
- **Approval**: May require approval for major optimizations
- **Output**: Optimization decision (CHOKE_ADJUSTMENT, GAS_LIFT_OPTIMIZATION, WORKOVER, etc.)

**Step 5: Optimization Execution**
- **Action**: Execute optimization
- **Required Data**: Optimization parameters, execution plan
- **Validation**: Optimization decision must be made
- **Output**: Optimization executed, production parameters updated

#### State Machine
```
PRODUCING → MONITORING → ANALYZING → OPTIMIZING → PRODUCING
    ↓            ↓            ↓
ALERT        ALERT        ALERT (returns to monitoring)
```

### Process 3: Production Decline Management

#### Workflow Definition
```
Decline Detection → DCA Analysis → Production Forecast → Economic Analysis → Workover Decision
```

#### Detailed Steps

**Step 1: Decline Detection**
- **Action**: Detect production decline
- **Required Data**: Production history, decline indicators
- **Validation**: Sufficient production history required
- **Calculation**: Detect decline rate, decline type
- **Output**: Decline detected, decline parameters calculated

**Step 2: DCA Analysis**
- **Action**: Perform decline curve analysis
- **Required Data**: Production history, decline type
- **Validation**: Decline must be detected
- **Integration**: Uses DCA library for analysis
- **Output**: DCA results, decline parameters, EUR (Estimated Ultimate Recovery)

**Step 3: Production Forecast**
- **Action**: Forecast future production
- **Required Data**: DCA results, reservoir data
- **Validation**: DCA analysis must be completed
- **Integration**: Uses ProductionForecasting library
- **Output**: Production forecast, reserve estimates

**Step 4: Economic Analysis**
- **Action**: Analyze economics of continued production vs. workover
- **Required Data**: Production forecast, workover costs, operating costs
- **Validation**: Production forecast must exist
- **Integration**: Uses EconomicAnalysis library
- **Output**: Economic analysis results (NPV, IRR, payback)

**Step 5: Workover Decision**
- **Action**: Decide on workover or continue production
- **Required Data**: Economic analysis, workover options, strategic considerations
- **Validation**: Economic analysis must be completed
- **Approval**: Required from Production Manager and Engineering
- **Output**: Workover decision (PROCEED_WITH_WORKOVER, CONTINUE_PRODUCTION, ABANDON)

#### State Machine
```
PRODUCING → DECLINE_DETECTED → DCA_ANALYZED → FORECASTED → ECONOMIC_ANALYZED → WORKOVER_DECIDED
    ↓                ↓                ↓
CONTINUE        CONTINUE        CONTINUE (returns to producing)
```

### Process 4: Workover Process

#### Workflow Definition
```
Workover Planning → Workover Approval → Workover Execution → Post-Workover Testing → Production Restart
```

#### Detailed Steps

**Step 1: Workover Planning**
- **Action**: Plan workover operations
- **Required Data**: Workover type, workover scope, cost estimates, schedule
- **Validation**: Workover decision must be made
- **Output**: Workover plan created, well status updated to WORKOVER_PLANNED

**Step 2: Workover Approval**
- **Action**: Approve workover
- **Required Data**: Workover plan, cost estimates, expected results
- **Validation**: Workover plan must be completed
- **Approval**: Required from Production Manager and Engineering Manager
- **Output**: Workover approved, well status updated to WORKOVER_APPROVED

**Step 3: Workover Execution**
- **Action**: Execute workover
- **Required Data**: Workover progress, operations performed
- **Validation**: Workover must be approved
- **Output**: Workover executed, well status updated to WORKOVER_COMPLETED

**Step 4: Post-Workover Testing**
- **Action**: Test well after workover
- **Required Data**: Test procedures, test results
- **Validation**: Workover must be completed
- **Integration**: Uses WellTestAnalysis
- **Output**: Test results, well status updated to TESTING

**Step 5: Production Restart**
- **Action**: Restart production
- **Required Data**: Production rates, operating parameters
- **Validation**: Post-workover testing must be completed and successful
- **Output**: Production restarted, well status updated to PRODUCING

#### State Machine
```
PRODUCING → WORKOVER_PLANNED → WORKOVER_APPROVED → WORKOVER_COMPLETED → TESTING → PRODUCING
    ↓                ↓                ↓
CANCELLED        CANCELLED        FAILED (may require rework)
```

## Service Implementation

### ProductionProcessService
**Location**: `Beep.OilandGas.LifeCycle/Services/Production/Processes/ProductionProcessService.cs`

#### Methods

```csharp
// Well Production Startup Process
Task<ProcessInstance> StartWellProductionStartupProcessAsync(string wellId, string fieldId, string userId);
Task<bool> PerformProductionTestAsync(string instanceId, WellTestRequest data, string userId);
Task<bool> ApproveProductionAsync(string instanceId, string userId);
Task<bool> StartProductionAsync(string instanceId, ProductionStartRequest data, string userId);
Task<bool> ConfirmProducingAsync(string instanceId, string userId);

// Production Operations Process
Task<ProcessInstance> StartProductionOperationsProcessAsync(string wellId, string fieldId, string userId);
Task<bool> RecordDailyProductionAsync(string instanceId, DailyProductionRequest data, string userId);
Task<bool> MonitorProductionAsync(string instanceId, string userId);
Task<bool> AnalyzePerformanceAsync(string instanceId, PerformanceAnalysisRequest data, string userId);
Task<bool> MakeOptimizationDecisionAsync(string instanceId, OptimizationDecisionRequest data, string userId);
Task<bool> ExecuteOptimizationAsync(string instanceId, OptimizationExecutionRequest data, string userId);

// Production Decline Management Process
Task<ProcessInstance> StartDeclineManagementProcessAsync(string wellId, string fieldId, string userId);
Task<bool> DetectDeclineAsync(string instanceId, string userId);
Task<bool> PerformDCAAnalysisAsync(string instanceId, DCARequest data, string userId);
Task<bool> ForecastProductionAsync(string instanceId, ProductionForecastRequest data, string userId);
Task<bool> AnalyzeEconomicsAsync(string instanceId, EconomicAnalysisRequest data, string userId);
Task<bool> MakeWorkoverDecisionAsync(string instanceId, WorkoverDecisionRequest data, string userId);

// Workover Process
Task<ProcessInstance> StartWorkoverProcessAsync(string wellId, string fieldId, string userId);
Task<bool> PlanWorkoverAsync(string instanceId, WorkoverPlanRequest data, string userId);
Task<bool> ApproveWorkoverAsync(string instanceId, string userId);
Task<bool> ExecuteWorkoverAsync(string instanceId, WorkoverExecutionRequest data, string userId);
Task<bool> TestPostWorkoverAsync(string instanceId, WellTestRequest data, string userId);
Task<bool> RestartProductionAsync(string instanceId, ProductionStartRequest data, string userId);
```

### WellStartupProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Production/Processes/WellStartupProcess.cs`

Specific implementation for Well production startup workflow.

### ProductionOperationsProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Production/Processes/ProductionOperationsProcess.cs`

Specific implementation for Production operations workflow.

### DeclineManagementProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Production/Processes/DeclineManagementProcess.cs`

Specific implementation for Decline management workflow.

### WorkoverProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Production/Processes/WorkoverProcess.cs`

Specific implementation for Workover workflow.

## DTOs Required

### Well Startup DTOs
```csharp
public class ProductionStartRequest { }
public class ProductionStartResponse { }
public class ProductionApprovalRequest { }
```

### Production Operations DTOs
```csharp
public class DailyProductionRequest { }
public class DailyProductionResponse { }
public class PerformanceAnalysisRequest { }
public class PerformanceAnalysisResponse { }
public class OptimizationDecisionRequest { }
public class OptimizationExecutionRequest { }
```

### Decline Management DTOs
```csharp
public class DeclineDetectionRequest { }
public class DeclineDetectionResponse { }
public class ProductionForecastRequest { }
public class ProductionForecastResponse { }
public class WorkoverDecisionRequest { }
public class WorkoverDecisionResponse { }
```

### Workover DTOs
```csharp
public class WorkoverPlanRequest { }
public class WorkoverPlanResponse { }
public class WorkoverExecutionRequest { }
public class WorkoverExecutionResponse { }
```

## PPDM Tables

### Existing Tables (Use These)
- `PDEN_VOL_SUMMARY` - Production data
- `RESERVE_ENTITY` - Reserve data
- `WELL` - Well data
- `WELL_EQUIPMENT` - Well equipment
- `WELL_STATUS` - Well status tracking

### New Tables Needed

#### PRODUCTION_STARTUP Table
```sql
CREATE TABLE PRODUCTION_STARTUP (
    PRODUCTION_STARTUP_ID VARCHAR(50) PRIMARY KEY,
    WELL_ID VARCHAR(50),
    STARTUP_DATE DATETIME,
    STARTUP_STATUS VARCHAR(50), -- READY_FOR_PRODUCTION, TESTING, APPROVED, PRODUCING
    INITIAL_OIL_RATE DECIMAL(18,2), -- bbl/day
    INITIAL_GAS_RATE DECIMAL(18,2), -- Mscf/day
    INITIAL_WATER_RATE DECIMAL(18,2), -- bbl/day
    INITIAL_WELLHEAD_PRESSURE DECIMAL(10,2), -- psia
    CHOKE_SIZE DECIMAL(5,2), -- inches
    STARTED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

#### PRODUCTION_OPERATIONS Table
```sql
CREATE TABLE PRODUCTION_OPERATIONS (
    PRODUCTION_OPERATIONS_ID VARCHAR(50) PRIMARY KEY,
    WELL_ID VARCHAR(50),
    OPERATION_DATE DATETIME,
    OPERATION_TYPE VARCHAR(50), -- DAILY_PRODUCTION, MONITORING, ANALYSIS, OPTIMIZATION
    OPERATION_STATUS VARCHAR(50), -- COMPLETED, IN_PROGRESS, FAILED
    OPERATION_DATA_JSON TEXT,
    PERFORMED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

#### DECLINE_MANAGEMENT Table
```sql
CREATE TABLE DECLINE_MANAGEMENT (
    DECLINE_MANAGEMENT_ID VARCHAR(50) PRIMARY KEY,
    WELL_ID VARCHAR(50),
    DECLINE_DETECTED_DATE DATETIME,
    DECLINE_TYPE VARCHAR(50), -- EXPONENTIAL, HYPERBOLIC, HARMONIC
    DECLINE_RATE DECIMAL(10,4), -- per year
    DCA_ANALYSIS_DATE DATETIME,
    EUR_OIL DECIMAL(18,2), -- Estimated Ultimate Recovery, bbl
    EUR_GAS DECIMAL(18,2), -- Bcf
    FORECAST_DATE DATETIME,
    ECONOMIC_ANALYSIS_DATE DATETIME,
    WORKOVER_DECISION VARCHAR(50), -- PROCEED_WITH_WORKOVER, CONTINUE_PRODUCTION, ABANDON
    DECISION_DATE DATETIME,
    DECIDED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

#### WORKOVER Table
```sql
CREATE TABLE WORKOVER (
    WORKOVER_ID VARCHAR(50) PRIMARY KEY,
    WELL_ID VARCHAR(50),
    WORKOVER_TYPE VARCHAR(50), -- STIMULATION, REPAIR, RECOMPLETION, ARTIFICIAL_LIFT_INSTALLATION
    WORKOVER_STATUS VARCHAR(50), -- PLANNED, APPROVED, IN_PROGRESS, COMPLETED, FAILED
    PLANNED_START_DATE DATETIME,
    ACTUAL_START_DATE DATETIME,
    ACTUAL_END_DATE DATETIME,
    WORKOVER_COST DECIMAL(18,2),
    WORKOVER_DESCRIPTION TEXT,
    CONTRACTOR_NAME VARCHAR(200),
    POST_WORKOVER_TEST_DATE DATETIME,
    POST_WORKOVER_OIL_RATE DECIMAL(18,2),
    POST_WORKOVER_GAS_RATE DECIMAL(18,2),
    PRODUCTION_RESTART_DATE DATETIME,
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50)
);
```

## Integration with Existing Services

### Integration with PPDMProductionService
- ProductionProcessService will call PPDMProductionService for CRUD operations
- Process steps will update production data, well status
- Process history will be tracked in PROCESS_HISTORY table

### Integration with PPDMCalculationService
- DCA analysis for decline management
- ProductionForecasting for production forecasts
- EconomicAnalysis for economic evaluation
- NodalAnalysis for performance optimization
- WellTestAnalysis for production testing

### Integration with PPDMDevelopmentService
- Well handover from development
- Equipment installation (gas lift, sucker rod, etc.)

### Integration with Operational Libraries
- ChokeAnalysis for choke optimization
- SuckerRodPumping for artificial lift optimization
- GasLift for gas lift optimization

## MVVM Structure

### ViewModels (Future Blazor Integration)
**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Production/`

- `WellStartupViewModel.cs` - Well production startup process UI
- `ProductionOperationsViewModel.cs` - Production operations process UI
- `DeclineManagementViewModel.cs` - Decline management process UI
- `WorkoverViewModel.cs` - Workover process UI

### Process UI Models
- `ProductionStartupUI.cs` - Production startup form data
- `DailyProductionUI.cs` - Daily production form data
- `PerformanceAnalysisUI.cs` - Performance analysis form data
- `DeclineAnalysisUI.cs` - Decline analysis form data
- `WorkoverPlanUI.cs` - Workover planning form data

## Implementation Steps

1. **Create Process Definitions**
   - Define Well production startup process
   - Define Production operations process
   - Define Decline management process
   - Define Workover process

2. **Implement ProductionProcessService**
   - Implement process orchestration methods
   - Integrate with PPDMProductionService
   - Integrate with PPDMCalculationService
   - Integrate with operational libraries

3. **Implement Specific Process Classes**
   - WellStartupProcess
   - ProductionOperationsProcess
   - DeclineManagementProcess
   - WorkoverProcess

4. **Create PPDM Tables**
   - Create production startup table
   - Create production operations table
   - Create decline management table
   - Create workover table

5. **Create DTOs**
   - Well startup DTOs
   - Production operations DTOs
   - Decline management DTOs
   - Workover DTOs

6. **Add Methods to PPDMProductionService**
   - Production startup methods
   - Production operations methods
   - Decline management methods
   - Workover methods

7. **Create ViewModels** (Future)
   - Process ViewModels for Blazor UI

## Testing Strategy

1. Unit tests for each process step
2. Integration tests for complete workflows
3. Validation tests for business rules
4. Approval workflow tests
5. State transition tests
6. Integration tests with calculation services
7. Performance tests for production operations

