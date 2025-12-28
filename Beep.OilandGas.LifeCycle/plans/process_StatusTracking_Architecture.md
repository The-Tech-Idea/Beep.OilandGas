# Process Status Tracking Architecture

## Overview

This document explains how process status is maintained in the LifeCycle system. It uses a **hybrid approach** combining existing PPDM tables with new application-level tables for process workflow management.

## Status Tracking Strategy

### Two-Level Status Tracking

1. **Entity Status** - Status of the actual entity (Well, Field, Pool, Facility, etc.)
   - Uses PPDM tables where they exist
   - Uses new status tracking tables where needed

2. **Process Status** - Status of workflow processes running on entities
   - Uses new process workflow tables (application-level)

## Status Tracking Architecture

### Level 1: Entity Status (PPDM + New Status Tables)

#### Existing PPDM Tables (Use These)

**WELL_STATUS** - Already exists in PPDM
```sql
-- Existing PPDM table
WELL_STATUS (
    WELL_STATUS_ID,
    WELL_ID,
    STATUS_TYPE, -- Current well status
    EFFECTIVE_DATE,
    ...
)
```

**APPLICATION** - Already exists in PPDM (for permits)
```sql
-- Existing PPDM table
APPLICATION (
    APPLICATION_ID,
    CURRENT_STATUS, -- Draft, Submitted, UnderReview, Approved, etc.
    ...
)
```

#### New Status Tracking Tables (Need to Create)

These tables track detailed status history for entities that don't have comprehensive status tracking in PPDM:

**POOL_STATUS** - Track pool definition process status
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

**FACILITY_STATUS** - Track facility development status
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

**PIPELINE_STATUS** - Track pipeline development status
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

**FIELD_PHASE** - Track field lifecycle phase
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

**RESERVOIR_STATUS** - Track reservoir lifecycle status
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

**ABANDONMENT_STATUS** - Track well abandonment status
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

**DECOMMISSIONING_STATUS** - Track facility decommissioning status
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

### Level 2: Process Workflow Status (New Application Tables)

These tables are **NOT part of PPDM**. They are application-level tables for workflow/process management and should be in a separate application database.

**PROCESS_DEFINITION** - Process templates
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

**PROCESS_INSTANCE** - Active process instances
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

**PROCESS_STEP_INSTANCE** - Individual step execution
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

**PROCESS_HISTORY** - Process execution history
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

**PROCESS_APPROVAL** - Process approvals
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

## Status Tracking Flow

### Example: Well Development Process

```
1. Entity Status (WELL_STATUS table - PPDM)
   - Well status: PLANNED → DRILLING → COMPLETED → PRODUCING

2. Process Status (PROCESS_INSTANCE table - Application)
   - Process: "Well Development Process"
   - Status: IN_PROGRESS
   - Current Step: "Drilling"

3. Step Status (PROCESS_STEP_INSTANCE table - Application)
   - Step: "Drilling"
   - Status: IN_PROGRESS
   - Started: 2024-01-15

4. History (PROCESS_HISTORY table - Application)
   - Action: STEP_STARTED
   - Previous State: PLANNED
   - New State: DRILLING
   - Date: 2024-01-15
```

## Database Architecture

### PPDM Database
- Contains entity data (WELL, FIELD, POOL, FACILITY, etc.)
- Contains entity status where it exists (WELL_STATUS, APPLICATION)
- Contains new entity status tables (POOL_STATUS, FACILITY_STATUS, etc.)

### Application Database (Separate)
- Contains process workflow tables (PROCESS_*)
- These are NOT part of PPDM standard
- Supports multiple PPDM databases

## Implementation Approach

### 1. Use Existing PPDM Where Possible

```csharp
// Use existing WELL_STATUS table
var wellStatus = await GetWellStatusAsync(wellId);

// Use existing APPLICATION table for permits
var permitStatus = await GetPermitStatusAsync(applicationId);
```

### 2. Create New Status Tables for Entities

```csharp
// Create POOL_STATUS entry when pool status changes
await CreatePoolStatusAsync(poolId, "DELINEATED", userId);

// Create FACILITY_STATUS entry when facility status changes
await CreateFacilityStatusAsync(facilityId, "CONSTRUCTING", userId);
```

### 3. Create Process Workflow Tables

```csharp
// Start process instance
var processInstance = await StartProcessAsync("WellDevelopment", wellId, fieldId, userId);

// Track process steps
await ExecuteStepAsync(processInstance.InstanceId, "Drilling", stepData, userId);

// Track process history
await AddHistoryEntryAsync(processInstance.InstanceId, historyEntry);
```

## Status Synchronization

### Entity Status ↔ Process Status

When a process step completes, update both:

1. **Process Status** (PROCESS_STEP_INSTANCE)
   ```csharp
   await CompleteStepAsync(instanceId, stepId, "SUCCESS", userId);
   ```

2. **Entity Status** (Entity status table)
   ```csharp
   await UpdateWellStatusAsync(wellId, "DRILLING", userId);
   ```

### Status Derivation

Entity status can be derived from:
- Current process step (if process is active)
- Latest status in entity status table
- Business rules

## Data Models

### Entity Status Model
```csharp
public class EntityStatus
{
    public string EntityId { get; set; }
    public string EntityType { get; set; } // WELL, FIELD, POOL, etc.
    public string CurrentStatus { get; set; }
    public DateTime StatusDate { get; set; }
    public string ChangedBy { get; set; }
    public List<EntityStatusHistory> History { get; set; }
}
```

### Process Status Model
```csharp
public class ProcessStatus
{
    public string InstanceId { get; set; }
    public string ProcessName { get; set; }
    public string CurrentState { get; set; }
    public string CurrentStep { get; set; }
    public ProcessStatusEnum Status { get; set; } // IN_PROGRESS, COMPLETED, etc.
    public DateTime StartDate { get; set; }
    public List<ProcessStepStatus> Steps { get; set; }
}
```

## Summary

### What to Use from PPDM
- ✅ Existing status tables (WELL_STATUS, APPLICATION)
- ✅ Entity tables (WELL, FIELD, POOL, FACILITY, etc.)

### What to Create New (PPDM Database)
- ✅ Entity status tracking tables (POOL_STATUS, FACILITY_STATUS, PIPELINE_STATUS, FIELD_PHASE, RESERVOIR_STATUS, ABANDONMENT_STATUS, DECOMMISSIONING_STATUS)

### What to Create New (Application Database)
- ✅ Process workflow tables (PROCESS_DEFINITION, PROCESS_INSTANCE, PROCESS_STEP_INSTANCE, PROCESS_HISTORY, PROCESS_APPROVAL)

## Key Principles

1. **Entity Status** = Current state of the entity (Well, Field, etc.)
2. **Process Status** = Current state of workflow process running on entity
3. **Status History** = Track all status changes for audit trail
4. **Separation** = Process workflow tables are application-level, not PPDM standard
5. **Synchronization** = Keep entity status and process status in sync

## Implementation Priority

1. **Phase 1**: Create entity status tables (POOL_STATUS, FACILITY_STATUS, etc.)
2. **Phase 2**: Create process workflow tables (PROCESS_*)
3. **Phase 3**: Implement status synchronization logic
4. **Phase 4**: Implement status derivation and business rules

