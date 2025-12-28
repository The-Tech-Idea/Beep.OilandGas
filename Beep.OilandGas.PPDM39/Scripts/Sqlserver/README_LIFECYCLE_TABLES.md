# Lifecycle Tables - Organization Summary

## PPDM Standard Tables

The following tables extend PPDM 3.9 standard to support lifecycle management:

### Entity Status Tables (PPDM Database)
- **FIELD_PHASE** - Tracks field lifecycle phases (EXPLORATION → DEVELOPMENT → PRODUCTION → DECLINE → DECOMMISSIONING → DECOMMISSIONED)
- **RESERVOIR_STATUS** - Tracks reservoir lifecycle states (DISCOVERED → APPRAISED → DEVELOPED → PRODUCING → DEPLETED → ABANDONED)
- **ABANDONMENT_STATUS** - Tracks well abandonment status (PLANNING → APPROVED → PLUGGED → RESTORED → ABANDONED)
- **DECOMMISSIONING_STATUS** - Tracks facility decommissioning status (PLANNING → EQUIPMENT_REMOVED → CLEANED → CLOSED → DECOMMISSIONED)

### SQL Scripts Location
All PPDM table scripts are in: `Beep.OilandGas.PPDM39/Scripts/Sqlserver/`
- `FIELD_PHASE_TAB.sql`, `FIELD_PHASE_PK.sql`, `FIELD_PHASE_FK.sql`
- `RESERVOIR_STATUS_TAB.sql`, `RESERVOIR_STATUS_PK.sql`, `RESERVOIR_STATUS_FK.sql`
- `ABANDONMENT_STATUS_TAB.sql`, `ABANDONMENT_STATUS_PK.sql`, `ABANDONMENT_STATUS_FK.sql`
- `DECOMMISSIONING_STATUS_TAB.sql`, `DECOMMISSIONING_STATUS_PK.sql`, `DECOMMISSIONING_STATUS_FK.sql`

### Model Classes Location
All model classes are in: `Beep.OilandGas.PPDM.Models/39/`
- `FIELD_PHASE.cs`
- `RESERVOIR_STATUS.cs`
- `ABANDONMENT_STATUS.cs`
- `DECOMMISSIONING_STATUS.cs`

### Metadata Location
Metadata entries are in: `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/`
- `LifecycleTablesMetadata.json` - Contains metadata for all lifecycle tables
- **Note**: These entries should be merged into `PPDM39Metadata.json` manually or via script

## Application Database Tables (NOT PPDM Standard)

### Process Workflow Tables (Application Database)
These tables are NOT part of PPDM standard and should be created in a separate application database:
- **PROCESS_DEFINITION** - Process/workflow definitions
- **PROCESS_INSTANCE** - Active process instances
- **PROCESS_STEP_INSTANCE** - Step execution instances
- **PROCESS_HISTORY** - Process execution history
- **PROCESS_APPROVAL** - Process approval records

### SQL Script Location
- `ProcessWorkflowTables.sql` - Complete script for all process workflow tables
- **Location**: `Beep.OilandGas.PPDM39/Scripts/Sqlserver/ProcessWorkflowTables.sql`

### Model Classes Location
Process workflow models are in: `Beep.OilandGas.LifeCycle/Models/Processes/`
- `ProcessDefinition.cs`
- `ProcessInstance.cs`
- `ProcessStepInstance.cs`
- `ProcessHistory.cs`
- (These are application-level models, not PPDM models)

## Adding Metadata

To add the lifecycle table metadata to the main metadata file:

1. Open `Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/PPDM39Metadata.json`
2. Find the entry for `FIELD_VERSION` (around line 15228)
3. Add the entries from `LifecycleTablesMetadata.json` after `FIELD_VERSION`
4. Save the file

Alternatively, use a script to merge the JSON files.

