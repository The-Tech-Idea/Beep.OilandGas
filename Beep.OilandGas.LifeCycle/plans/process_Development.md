# Development Process - Implementation Plan

## Overview

This document defines the process workflows for the Development phase, including Pool definition, Facility development, Well development, and Pipeline development processes.

## Current State

### What Exists
- ✅ Basic CRUD for POOL table
- ✅ Basic CRUD for FACILITY table
- ✅ Basic CRUD for PIPELINE table
- ✅ Basic CRUD for development wells
- ✅ Gas Lift, Pipeline Analysis, Compressor Analysis integrations
- ❌ **Missing**: Pool definition workflow, Facility development workflow, Well development workflow, Pipeline development workflow

### What Needs to Be Built
1. Pool definition process (Identification → Delineation → Reserve Assignment → Approval)
2. Facility development process (Planning → Design → Permitting → Construction → Commissioning)
3. Well development process (Planning → Drilling Permit → Drilling → Completion → Production Handover)
4. Pipeline development process (Planning → Design → Permitting → Construction → Testing → Commissioning)

## Development Process Workflows

### Process 1: Pool Definition Process

#### Workflow Definition
```
Pool Identification → Pool Delineation → Reserve Assignment → Pool Approval → Pool Active
```

#### Detailed Steps

**Step 1: Pool Identification**
- **Action**: Identify potential pool
- **Required Data**: Pool name, geological formation, location
- **Validation**: Formation must be identified, location must be valid
- **Output**: POOL entity created with status IDENTIFIED

**Step 2: Pool Delineation**
- **Action**: Delineate pool boundaries
- **Required Data**: Seismic data, well data, geological interpretation
- **Validation**: At least one delineation method required
- **Output**: Pool boundaries defined, POOL status updated to DELINEATED

**Step 3: Reserve Assignment**
- **Action**: Assign reserves to pool
- **Required Data**: Reserve estimates, recovery factors
- **Validation**: Delineation must be completed
- **Calculation**: Calculate pool reserves from reservoir data
- **Output**: Reserve data assigned to pool, POOL status updated to RESERVES_ASSIGNED

**Step 4: Pool Approval**
- **Action**: Approve pool for development
- **Required Data**: Reserve data, economic justification
- **Validation**: Reserves must be assigned
- **Approval**: Required from Reservoir Engineer and Development Manager
- **Output**: Pool status updated to APPROVED

**Step 5: Pool Active**
- **Action**: Activate pool for development activities
- **Required Data**: Approval confirmation
- **Validation**: Pool must be approved
- **Output**: Pool status updated to ACTIVE

#### State Machine
```
POOL_IDENTIFIED → POOL_DELINEATED → RESERVES_ASSIGNED → POOL_APPROVED → POOL_ACTIVE
                    ↓                    ↓
                REJECTED            REJECTED (terminal)
```

### Process 2: Facility Development Process

#### Workflow Definition
```
Facility Planning → Facility Design → Permitting → Construction → Testing → Commissioning → Facility Active
```

#### Detailed Steps

**Step 1: Facility Planning**
- **Action**: Plan facility development
- **Required Data**: Facility type, capacity requirements, location
- **Validation**: Facility type must be specified, capacity must be defined
- **Output**: FACILITY entity created with status PLANNING

**Step 2: Facility Design**
- **Action**: Design facility
- **Required Data**: Design specifications, equipment requirements, layout
- **Validation**: Planning must be completed
- **Integration**: May use CompressorAnalysis, PipelineAnalysis for design
- **Output**: Facility design completed, status updated to DESIGNED

**Step 3: Permitting**
- **Action**: Obtain required permits
- **Required Data**: Permit applications, regulatory requirements
- **Validation**: Design must be completed
- **Integration**: Uses PermitManagementService
- **Output**: Permits obtained, status updated to PERMITTED

**Step 4: Construction**
- **Action**: Construct facility
- **Required Data**: Construction schedule, contractor information
- **Validation**: Permits must be obtained
- **Output**: Construction progress tracked, status updated to CONSTRUCTING

**Step 5: Testing**
- **Action**: Test facility systems
- **Required Data**: Test procedures, test results
- **Validation**: Construction must be completed
- **Output**: Test results recorded, status updated to TESTING

**Step 6: Commissioning**
- **Action**: Commission facility
- **Required Data**: Commissioning procedures, acceptance criteria
- **Validation**: Testing must be completed and passed
- **Approval**: Required from Operations Manager
- **Output**: Facility commissioned, status updated to COMMISSIONED

**Step 7: Facility Active**
- **Action**: Activate facility for operations
- **Required Data**: Commissioning confirmation
- **Validation**: Facility must be commissioned
- **Output**: Facility status updated to ACTIVE

#### State Machine
```
FACILITY_PLANNING → FACILITY_DESIGNED → PERMITTED → CONSTRUCTING → TESTING → COMMISSIONED → FACILITY_ACTIVE
        ↓                ↓                ↓
    REJECTED        REJECTED        REJECTED (terminal)
```

### Process 3: Well Development Process

#### Workflow Definition
```
Well Planning → Drilling Permit → Drilling → Completion → Production Testing → Production Handover
```

#### Detailed Steps

**Step 1: Well Planning**
- **Action**: Plan development well
- **Required Data**: Well location, target formation, well design
- **Validation**: Location must be valid, target formation must be specified
- **Output**: WELL entity created with status PLANNED

**Step 2: Drilling Permit**
- **Action**: Obtain drilling permit
- **Required Data**: Permit application data
- **Validation**: Well planning must be completed
- **Integration**: Uses PermitManagementService
- **Output**: Drilling permit obtained, status updated to PERMITTED

**Step 3: Drilling**
- **Action**: Drill well
- **Required Data**: Drilling progress, drilling reports
- **Validation**: Permit must be obtained
- **Output**: Drilling progress tracked, status updated to DRILLING

**Step 4: Completion**
- **Action**: Complete well
- **Required Data**: Completion design, completion operations
- **Validation**: Drilling must be completed
- **Integration**: May use Gas Lift, Choke Analysis for completion design
- **Output**: Well completed, status updated to COMPLETED

**Step 5: Production Testing**
- **Action**: Test well production
- **Required Data**: Test procedures, test results
- **Validation**: Completion must be done
- **Integration**: Uses WellTestAnalysis
- **Output**: Test results recorded, status updated to TESTING

**Step 6: Production Handover**
- **Action**: Hand over well to production
- **Required Data**: Handover documentation, acceptance
- **Validation**: Production testing must be completed
- **Approval**: Required from Production Manager
- **Output**: Well status updated to PRODUCING, handover to Production service

#### State Machine
```
WELL_PLANNED → PERMITTED → DRILLING → COMPLETED → TESTING → PRODUCING
    ↓            ↓            ↓
REJECTED    REJECTED    REJECTED (terminal)
```

### Process 4: Pipeline Development Process

#### Workflow Definition
```
Pipeline Planning → Pipeline Design → Permitting → Construction → Testing → Commissioning → Pipeline Active
```

#### Detailed Steps

**Step 1: Pipeline Planning**
- **Action**: Plan pipeline development
- **Required Data**: Pipeline route, capacity requirements, endpoints
- **Validation**: Route must be defined, capacity must be specified
- **Output**: PIPELINE entity created with status PLANNING

**Step 2: Pipeline Design**
- **Action**: Design pipeline
- **Required Data**: Pipeline specifications, diameter, material, route details
- **Validation**: Planning must be completed
- **Integration**: Uses PipelineAnalysis for capacity and flow design
- **Output**: Pipeline design completed, status updated to DESIGNED

**Step 3: Permitting**
- **Action**: Obtain required permits
- **Required Data**: Permit applications, right-of-way, environmental permits
- **Validation**: Design must be completed
- **Integration**: Uses PermitManagementService
- **Output**: Permits obtained, status updated to PERMITTED

**Step 4: Construction**
- **Action**: Construct pipeline
- **Required Data**: Construction schedule, contractor information, construction progress
- **Validation**: Permits must be obtained
- **Output**: Construction progress tracked, status updated to CONSTRUCTING

**Step 5: Testing**
- **Action**: Test pipeline
- **Required Data**: Hydrostatic testing, pressure testing, leak testing
- **Validation**: Construction must be completed
- **Output**: Test results recorded, status updated to TESTING

**Step 6: Commissioning**
- **Action**: Commission pipeline
- **Required Data**: Commissioning procedures, acceptance criteria
- **Validation**: Testing must be completed and passed
- **Approval**: Required from Operations Manager
- **Output**: Pipeline commissioned, status updated to COMMISSIONED

**Step 7: Pipeline Active**
- **Action**: Activate pipeline for operations
- **Required Data**: Commissioning confirmation
- **Validation**: Pipeline must be commissioned
- **Output**: Pipeline status updated to ACTIVE

#### State Machine
```
PIPELINE_PLANNING → PIPELINE_DESIGNED → PERMITTED → CONSTRUCTING → TESTING → COMMISSIONED → PIPELINE_ACTIVE
        ↓                ↓                ↓
    REJECTED        REJECTED        REJECTED (terminal)
```

## Service Implementation

### DevelopmentProcessService
**Location**: `Beep.OilandGas.LifeCycle/Services/Development/Processes/DevelopmentProcessService.cs`

#### Methods

```csharp
// Pool Definition Process
Task<ProcessInstance> StartPoolDefinitionProcessAsync(string poolId, string fieldId, string userId);
Task<bool> DelineatePoolAsync(string instanceId, PoolDelineationRequest data, string userId);
Task<bool> AssignReservesAsync(string instanceId, ReserveAssignmentRequest data, string userId);
Task<bool> ApprovePoolAsync(string instanceId, string userId);
Task<bool> ActivatePoolAsync(string instanceId, string userId);

// Facility Development Process
Task<ProcessInstance> StartFacilityDevelopmentProcessAsync(string facilityId, string fieldId, string userId);
Task<bool> DesignFacilityAsync(string instanceId, FacilityDesignRequest data, string userId);
Task<bool> ObtainFacilityPermitsAsync(string instanceId, string userId);
Task<bool> StartConstructionAsync(string instanceId, ConstructionRequest data, string userId);
Task<bool> TestFacilityAsync(string instanceId, FacilityTestRequest data, string userId);
Task<bool> CommissionFacilityAsync(string instanceId, CommissioningRequest data, string userId);
Task<bool> ActivateFacilityAsync(string instanceId, string userId);

// Well Development Process
Task<ProcessInstance> StartWellDevelopmentProcessAsync(string wellId, string fieldId, string userId);
Task<bool> ObtainDrillingPermitAsync(string instanceId, string userId);
Task<bool> StartDrillingAsync(string instanceId, DrillingRequest data, string userId);
Task<bool> CompleteWellAsync(string instanceId, CompletionRequest data, string userId);
Task<bool> TestWellProductionAsync(string instanceId, WellTestRequest data, string userId);
Task<bool> HandoverToProductionAsync(string instanceId, string userId);

// Pipeline Development Process
Task<ProcessInstance> StartPipelineDevelopmentProcessAsync(string pipelineId, string fieldId, string userId);
Task<bool> DesignPipelineAsync(string instanceId, PipelineDesignRequest data, string userId);
Task<bool> ObtainPipelinePermitsAsync(string instanceId, string userId);
Task<bool> StartPipelineConstructionAsync(string instanceId, ConstructionRequest data, string userId);
Task<bool> TestPipelineAsync(string instanceId, PipelineTestRequest data, string userId);
Task<bool> CommissionPipelineAsync(string instanceId, CommissioningRequest data, string userId);
Task<bool> ActivatePipelineAsync(string instanceId, string userId);
```

### PoolDefinitionProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Development/Processes/PoolDefinitionProcess.cs`

Specific implementation for Pool definition workflow.

### FacilityDevelopmentProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Development/Processes/FacilityDevelopmentProcess.cs`

Specific implementation for Facility development workflow.

### WellDevelopmentProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Development/Processes/WellDevelopmentProcess.cs`

Specific implementation for Well development workflow.

### PipelineDevelopmentProcess
**Location**: `Beep.OilandGas.LifeCycle/Services/Development/Processes/PipelineDevelopmentProcess.cs`

Specific implementation for Pipeline development workflow.

## DTOs Required

### Pool Process DTOs
```csharp
public class PoolDelineationRequest { }
public class PoolDelineationResponse { }
public class ReserveAssignmentRequest { }
public class ReserveAssignmentResponse { }
public class PoolApprovalRequest { }
```

### Facility Process DTOs
```csharp
public class FacilityDesignRequest { }
public class FacilityDesignResponse { }
public class ConstructionRequest { }
public class ConstructionResponse { }
public class FacilityTestRequest { }
public class FacilityTestResponse { }
public class CommissioningRequest { }
public class CommissioningResponse { }
```

### Well Process DTOs
```csharp
public class DrillingRequest { }
public class DrillingResponse { }
public class CompletionRequest { }
public class CompletionResponse { }
public class WellTestRequest { }
public class WellTestResponse { }
public class ProductionHandoverRequest { }
```

### Pipeline Process DTOs
```csharp
public class PipelineDesignRequest { }
public class PipelineDesignResponse { }
public class PipelineTestRequest { }
public class PipelineTestResponse { }
```

## PPDM Tables

### Existing Tables (Use These)
- `POOL` - Pool data
- `FACILITY` - Facility data
- `PIPELINE` - Pipeline data
- `WELL` - Well data
- `WELL_EQUIPMENT` - Well equipment (completion, gas lift, etc.)
- `FACILITY_EQUIPMENT` - Facility equipment (compressors, etc.)
- `APPLICATION` - Permits

### New Tables Needed

#### POOL_STATUS Table
```sql
CREATE TABLE POOL_STATUS (
    POOL_STATUS_ID VARCHAR(50) PRIMARY KEY,
    POOL_ID VARCHAR(50),
    STATUS VARCHAR(50), -- IDENTIFIED, DELINEATED, RESERVES_ASSIGNED, APPROVED, ACTIVE, REJECTED
    STATUS_DATE DATETIME,
    STATUS_CHANGED_BY VARCHAR(50),
    NOTES TEXT,
    ROW_CREATED_DATE DATETIME
);
```

#### FACILITY_STATUS Table
```sql
CREATE TABLE FACILITY_STATUS (
    FACILITY_STATUS_ID VARCHAR(50) PRIMARY KEY,
    FACILITY_ID VARCHAR(50),
    STATUS VARCHAR(50), -- PLANNING, DESIGNED, PERMITTED, CONSTRUCTING, TESTING, COMMISSIONED, ACTIVE, REJECTED
    STATUS_DATE DATETIME,
    STATUS_CHANGED_BY VARCHAR(50),
    NOTES TEXT,
    ROW_CREATED_DATE DATETIME
);
```

#### WELL_DEVELOPMENT Table
```sql
CREATE TABLE WELL_DEVELOPMENT (
    WELL_DEVELOPMENT_ID VARCHAR(50) PRIMARY KEY,
    WELL_ID VARCHAR(50),
    DEVELOPMENT_PHASE VARCHAR(50), -- PLANNED, PERMITTED, DRILLING, COMPLETED, TESTING, PRODUCING
    PHASE_START_DATE DATETIME,
    PHASE_END_DATE DATETIME,
    PHASE_STATUS VARCHAR(50), -- IN_PROGRESS, COMPLETED, FAILED
    PHASE_DATA_JSON TEXT,
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50)
);
```

#### PIPELINE_STATUS Table
```sql
CREATE TABLE PIPELINE_STATUS (
    PIPELINE_STATUS_ID VARCHAR(50) PRIMARY KEY,
    PIPELINE_ID VARCHAR(50),
    STATUS VARCHAR(50), -- PLANNING, DESIGNED, PERMITTED, CONSTRUCTING, TESTING, COMMISSIONED, ACTIVE, REJECTED
    STATUS_DATE DATETIME,
    STATUS_CHANGED_BY VARCHAR(50),
    NOTES TEXT,
    ROW_CREATED_DATE DATETIME
);
```

#### CONSTRUCTION Table
```sql
CREATE TABLE CONSTRUCTION (
    CONSTRUCTION_ID VARCHAR(50) PRIMARY KEY,
    ENTITY_ID VARCHAR(50), -- Facility, Pipeline, or Well ID
    ENTITY_TYPE VARCHAR(50), -- FACILITY, PIPELINE, WELL
    CONSTRUCTION_TYPE VARCHAR(50), -- NEW, EXPANSION, MODIFICATION
    START_DATE DATETIME,
    PLANNED_END_DATE DATETIME,
    ACTUAL_END_DATE DATETIME,
    CONSTRUCTION_STATUS VARCHAR(50), -- PLANNED, IN_PROGRESS, COMPLETED, SUSPENDED
    CONTRACTOR_NAME VARCHAR(200),
    CONSTRUCTION_COST DECIMAL(18,2),
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50)
);
```

#### COMMISSIONING Table
```sql
CREATE TABLE COMMISSIONING (
    COMMISSIONING_ID VARCHAR(50) PRIMARY KEY,
    ENTITY_ID VARCHAR(50), -- Facility, Pipeline, or Well ID
    ENTITY_TYPE VARCHAR(50), -- FACILITY, PIPELINE, WELL
    COMMISSIONING_DATE DATETIME,
    COMMISSIONING_STATUS VARCHAR(50), -- PENDING, IN_PROGRESS, COMPLETED, FAILED
    ACCEPTANCE_CRITERIA TEXT,
    ACCEPTANCE_RESULTS TEXT,
    COMMISSIONED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50)
);
```

## Integration with Existing Services

### Integration with PPDMDevelopmentService
- DevelopmentProcessService will call PPDMDevelopmentService for CRUD operations
- Process steps will update POOL, FACILITY, PIPELINE, WELL tables
- Process history will be tracked in PROCESS_HISTORY table

### Integration with Calculation Services
- PipelineAnalysis for pipeline design
- CompressorAnalysis for facility design
- GasLift for well completion design

### Integration with PermitManagementService
- Drilling permit applications
- Facility permit applications
- Pipeline permit applications

### Integration with PPDMProductionService
- Well handover to production
- Facility handover to production

## MVVM Structure

### ViewModels (Future Blazor Integration)
**Location**: `Beep.OilandGas.LifeCycle/ViewModels/Development/`

- `PoolDefinitionViewModel.cs` - Pool definition process UI
- `FacilityDevelopmentViewModel.cs` - Facility development process UI
- `WellDevelopmentViewModel.cs` - Well development process UI
- `PipelineDevelopmentViewModel.cs` - Pipeline development process UI

### Process UI Models
- `PoolDelineationUI.cs` - Pool delineation form data
- `FacilityDesignUI.cs` - Facility design form data
- `WellCompletionUI.cs` - Well completion form data
- `PipelineDesignUI.cs` - Pipeline design form data
- `ConstructionUI.cs` - Construction progress form data
- `CommissioningUI.cs` - Commissioning form data

## Implementation Steps

1. **Create Process Definitions**
   - Define Pool definition process
   - Define Facility development process
   - Define Well development process
   - Define Pipeline development process

2. **Implement DevelopmentProcessService**
   - Implement process orchestration methods
   - Integrate with PPDMDevelopmentService
   - Integrate with calculation services
   - Integrate with PermitManagementService

3. **Implement Specific Process Classes**
   - PoolDefinitionProcess
   - FacilityDevelopmentProcess
   - WellDevelopmentProcess
   - PipelineDevelopmentProcess

4. **Create PPDM Tables**
   - Create status tracking tables
   - Create construction and commissioning tables

5. **Create DTOs**
   - Pool process DTOs
   - Facility process DTOs
   - Well process DTOs
   - Pipeline process DTOs

6. **Add Methods to PPDMDevelopmentService**
   - Status management methods
   - Construction tracking methods
   - Commissioning methods

7. **Create ViewModels** (Future)
   - Process ViewModels for Blazor UI

## Testing Strategy

1. Unit tests for each process step
2. Integration tests for complete workflows
3. Validation tests for business rules
4. Approval workflow tests
5. State transition tests
6. Integration tests with calculation services

