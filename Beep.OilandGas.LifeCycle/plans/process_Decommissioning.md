# Decommissioning Process - Implementation Plan

## Overview

This document defines the process workflows for the Decommissioning phase, including Well abandonment and Facility decommissioning processes.

## Current State

### What Exists
- ✅ Basic CRUD for WELL_ABANDONMENT table
- ✅ Basic CRUD for FACILITY_DECOMMISSIONING table
- ✅ Basic CRUD for ENVIRONMENTAL_RESTORATION table
- ✅ Basic CRUD for DECOMMISSIONING_COST table
- ❌ **Missing**: Well abandonment workflow, Facility decommissioning workflow, Cost estimation workflow

### What Needs to Be Built
1. Well abandonment process (Planning → Regulatory Approval → Plugging → Site Restoration → Completion)
2. Facility decommissioning process (Planning → Equipment Removal → Site Cleanup → Regulatory Closure → Completion)
3. Cost estimation process (Cost Planning → Cost Approval → Cost Tracking)

## Decommissioning Process Workflows

### Process 1: Well Abandonment Process

#### Workflow Definition
```
Abandonment Planning → Regulatory Approval → Well Plugging → Site Restoration → Abandonment Completion
```

#### Detailed Steps

**Step 1: Abandonment Planning**
- **Action**: Plan well abandonment
- **Required Data**: Abandonment reason, abandonment method, cost estimates
- **Validation**: Well must be in appropriate status (Suspended, Non-Producing)
- **Output**: WELL_ABANDONMENT entity created with status PLANNING

**Step 2: Regulatory Approval**
- **Action**: Obtain regulatory approval
- **Required Data**: Abandonment plan, regulatory applications
- **Validation**: Planning must be completed
- **Integration**: Uses PermitManagementService
- **Output**: Regulatory approval obtained, status updated to APPROVED

**Step 3: Well Plugging**
- **Action**: Plug and abandon well
- **Required Data**: Plugging operations, plugging materials, plugging depth
- **Validation**: Regulatory approval must be obtained
- **Output**: Well plugged, status updated to PLUGGED

**Step 4: Site Restoration**
- **Action**: Restore well site
- **Required Data**: Restoration activities, environmental compliance
- **Validation**: Well must be plugged
- **Output**: Site restored, status updated to RESTORED

**Step 5: Abandonment Completion**
- **Action**: Complete abandonment process
- **Required Data**: Final inspection, regulatory closure
- **Validation**: Site restoration must be completed
- **Approval**: Required from Regulatory Affairs
- **Output**: Abandonment completed, well status updated to ABANDONED

#### State Machine
```
PLANNING → APPROVED → PLUGGED → RESTORED → ABANDONED
    ↓         ↓
REJECTED  REJECTED (terminal)
```

### Process 2: Facility Decommissioning Process

#### Workflow Definition
```
Decommissioning Planning → Equipment Removal → Site Cleanup → Regulatory Closure → Decommissioning Complete
```

#### Detailed Steps

**Step 1: Decommissioning Planning**
- **Action**: Plan facility decommissioning
- **Required Data**: Decommissioning scope, cost estimates, schedule
- **Validation**: Facility must be in appropriate status
- **Output**: FACILITY_DECOMMISSIONING entity created with status PLANNING

**Step 2: Equipment Removal**
- **Action**: Remove equipment
- **Required Data**: Equipment inventory, removal procedures, disposal methods
- **Validation**: Planning must be completed
- **Output**: Equipment removed, status updated to EQUIPMENT_REMOVED

**Step 3: Site Cleanup**
- **Action**: Clean up facility site
- **Required Data**: Cleanup activities, environmental testing
- **Validation**: Equipment must be removed
- **Output**: Site cleaned, status updated to CLEANED

**Step 4: Regulatory Closure**
- **Action**: Obtain regulatory closure
- **Required Data**: Closure documentation, regulatory compliance
- **Validation**: Site cleanup must be completed
- **Approval**: Required from Regulatory Affairs
- **Output**: Regulatory closure obtained, status updated to CLOSED

**Step 5: Decommissioning Complete**
- **Action**: Complete decommissioning
- **Required Data**: Final inspection, completion documentation
- **Validation**: Regulatory closure must be obtained
- **Output**: Decommissioning completed, facility status updated to DECOMMISSIONED

#### State Machine
```
PLANNING → EQUIPMENT_REMOVED → CLEANED → CLOSED → DECOMMISSIONED
    ↓
REJECTED (terminal)
```

## Service Implementation

### DecommissioningProcessService
**Location**: `Beep.OilandGas.LifeCycle/Services/Decommissioning/Processes/DecommissioningProcessService.cs`

#### Methods

```csharp
// Well Abandonment Process
Task<ProcessInstance> StartWellAbandonmentProcessAsync(string wellId, string fieldId, string userId);
Task<bool> PlanAbandonmentAsync(string instanceId, AbandonmentPlanRequest data, string userId);
Task<bool> ObtainRegulatoryApprovalAsync(string instanceId, string userId);
Task<bool> PlugWellAsync(string instanceId, WellPluggingRequest data, string userId);
Task<bool> RestoreSiteAsync(string instanceId, SiteRestorationRequest data, string userId);
Task<bool> CompleteAbandonmentAsync(string instanceId, string userId);

// Facility Decommissioning Process
Task<ProcessInstance> StartFacilityDecommissioningProcessAsync(string facilityId, string fieldId, string userId);
Task<bool> PlanDecommissioningAsync(string instanceId, DecommissioningPlanRequest data, string userId);
Task<bool> RemoveEquipmentAsync(string instanceId, EquipmentRemovalRequest data, string userId);
Task<bool> CleanupSiteAsync(string instanceId, SiteCleanupRequest data, string userId);
Task<bool> ObtainRegulatoryClosureAsync(string instanceId, string userId);
Task<bool> CompleteDecommissioningAsync(string instanceId, string userId);

// Cost Estimation
Task<DecommissioningCostEstimateResponse> EstimateCostsAsync(string fieldId, CostEstimateRequest data);
Task<bool> ApproveCostEstimateAsync(string estimateId, string userId);
Task<bool> TrackCostsAsync(string instanceId, CostTrackingRequest data, string userId);
```

## DTOs Required

```csharp
public class AbandonmentPlanRequest { }
public class WellPluggingRequest { }
public class SiteRestorationRequest { }
public class DecommissioningPlanRequest { }
public class EquipmentRemovalRequest { }
public class SiteCleanupRequest { }
public class CostEstimateRequest { }
public class CostTrackingRequest { }
```

## PPDM Tables

### Existing Tables (Use These)
- `WELL_ABANDONMENT`
- `FACILITY_DECOMMISSIONING`
- `ENVIRONMENTAL_RESTORATION`
- `DECOMMISSIONING_COST`

### New Tables Needed

#### ABANDONMENT_STATUS Table
```sql
CREATE TABLE ABANDONMENT_STATUS (
    ABANDONMENT_STATUS_ID VARCHAR(50) PRIMARY KEY,
    WELL_ABANDONMENT_ID VARCHAR(50),
    STATUS VARCHAR(50), -- PLANNING, APPROVED, PLUGGED, RESTORED, ABANDONED, REJECTED
    STATUS_DATE DATETIME,
    STATUS_CHANGED_BY VARCHAR(50),
    NOTES TEXT,
    ROW_CREATED_DATE DATETIME
);
```

#### DECOMMISSIONING_STATUS Table
```sql
CREATE TABLE DECOMMISSIONING_STATUS (
    DECOMMISSIONING_STATUS_ID VARCHAR(50) PRIMARY KEY,
    FACILITY_DECOMMISSIONING_ID VARCHAR(50),
    STATUS VARCHAR(50), -- PLANNING, EQUIPMENT_REMOVED, CLEANED, CLOSED, DECOMMISSIONED, REJECTED
    STATUS_DATE DATETIME,
    STATUS_CHANGED_BY VARCHAR(50),
    NOTES TEXT,
    ROW_CREATED_DATE DATETIME
);
```

## Integration Points

- PPDMDecommissioningService for CRUD operations
- PermitManagementService for regulatory approvals
- PPDMAccountingService for cost tracking
- Environmental services for compliance

## MVVM Structure

### ViewModels (Future)
- `WellAbandonmentViewModel.cs`
- `FacilityDecommissioningViewModel.cs`
- `CostEstimationViewModel.cs`

## Implementation Steps

1. Create process definitions
2. Implement DecommissioningProcessService
3. Implement specific process classes
4. Create PPDM tables
5. Create DTOs
6. Add methods to PPDMDecommissioningService

