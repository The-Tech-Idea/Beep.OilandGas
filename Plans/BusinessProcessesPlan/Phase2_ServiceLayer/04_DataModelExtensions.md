# Phase 2 — Data Model Extensions
## C# Class Definitions for Process Engine

> All classes extend existing PPDM entities or are new supporting types.  
> Location: `Beep.OilandGas.Models/Data/Process/`  
> All model classes inherit `ModelEntityBase` which implements `IPPDMEntity` and includes audit columns.

---

## ProcessDefinition

Represents a template/blueprint for a repeatable business process.  
Maps to `PROJECT.PROJECT_TYPE = 'PROCESS_DEFINITION'` + `PROJECT_PLAN` for steps.

```csharp
namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Immutable blueprint for a repeatable business process.
    /// Seeded at startup by ProcessDefinitionInitializer.
    /// Maps to: PROJECT (header) + PROJECT_PLAN (step templates).
    /// </summary>
    public class ProcessDefinition : ModelEntityBase
    {
        /// <summary>
        /// Stable unique identifier. Never change post-deployment.
        /// Example: "GATE-3-FID", "WO-TURNAROUND", "EXP-LEAD-ASSESS".
        /// Stored in PROJECT.PROJECT_OBS_NO.
        /// </summary>
        public string ProcessId { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable display name.
        /// Stored in PROJECT.PROJECT_NAME.
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// State machine type. Links to _transitionRegistry key.
        /// Values: "WORK_ORDER", "GATE_REVIEW", "HSE_INCIDENT",
        ///         "COMPLIANCE_REPORT", "WELL_LIFECYCLE", "FACILITY_LIFECYCLE",
        ///         "RESERVOIR_MGMT", "PIPELINE_INTEGRITY".
        /// Stored in PROJECT.PROJECT_TYPE.
        /// </summary>
        public string ProcessType { get; set; } = string.Empty;

        /// <summary>
        /// Numeric category (1–12) matching BusinessProcessCategoryNode.
        /// Stored in PROJECT.AREA_ID used as category tag.
        /// </summary>
        public string ProcessCategory { get; set; } = string.Empty;

        /// <summary>
        /// Comma-delimited jurisdiction tags: "USA", "CANADA", "INTERNATIONAL".
        /// Stored in PROJECT.REMARKS.
        /// </summary>
        public string JurisdictionTags { get; set; } = string.Empty;

        /// <summary>
        /// Ordered list of step blueprints.
        /// Each maps to a PROJECT_PLAN row.
        /// </summary>
        public List<ProcessStepDefinition> Steps { get; set; } = new();

        /// <summary>
        /// Minimum number of approvers required for gate-type processes.
        /// 0 = not applicable.
        /// Stored in PROJECT.ESTIMATED_COST (repurposed int field).
        /// </summary>
        public int MinApproverCount { get; set; } = 0;

        /// <summary>
        /// Whether this process requires document attachments before it can be submitted.
        /// Drives document-attachment guard condition.
        /// </summary>
        public bool RequiresDocuments { get; set; } = false;

        /// <summary>
        /// List of RM_INFORMATION_ITEM subtypes that must be attached.
        /// Populated for GATE_REVIEW and COMPLIANCE_REPORT process types.
        /// Serialized as JSON in PROJECT.SURFACE_EQUIPMENT_DESC (repurposed text field).
        /// </summary>
        public List<string> RequiredDocumentTypes { get; set; } = new();
    }
}
```

---

## ProcessStepDefinition

Represents a single step template within a process definition.

```csharp
namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Blueprint for one step within a ProcessDefinition.
    /// Maps to PROJECT_PLAN table.
    /// </summary>
    public class ProcessStepDefinition : ModelEntityBase
    {
        /// <summary>
        /// Stable unique step identifier within a process.
        /// Example: "EXP-LA-01", "DEV-AFE-03".
        /// Stored in PROJECT_PLAN.PLAN_OBS_NO.
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Display name for this step.
        /// Stored in PROJECT_PLAN.PLAN_DESC.
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// 1-based sequence number within the process.
        /// Stored in PROJECT_PLAN.SEQ_NO.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// PPDM table this step primarily operates on.
        /// Example: "HSE_INCIDENT", "PROJECT_STEP_BA".
        /// Stored in PROJECT_PLAN.PLAN_TYPE.
        /// </summary>
        public string PrimaryPPDMTable { get; set; } = string.Empty;

        /// <summary>
        /// Transition trigger name that advances to this step.
        /// Must match a key in _transitionRegistry.
        /// Stored in PROJECT_PLAN.PLAN_NAME.
        /// </summary>
        public string InboundTrigger { get; set; } = string.Empty;

        /// <summary>
        /// Optional guard condition key for this step's completion.
        /// Example: "ALL_CONDITIONS_SATISFIED", "MIN_APPROVERS_MET".
        /// Stored in PROJECT_PLAN.PLAN_SUBTYPE.
        /// </summary>
        public string? CompletionGuardKey { get; set; }

        /// <summary>
        /// Jurisdiction restriction. Null = applies to all jurisdictions.
        /// Values: "USA", "CANADA", "UK", "NORWAY", or null.
        /// Stored in PROJECT_PLAN.AREA_ID (repurposed).
        /// </summary>
        public string? JurisdictionRestriction { get; set; }
    }
}
```

---

## ProcessInstance

Represents a running execution of a process definition.

```csharp
namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// A specific runtime execution of a ProcessDefinition.
    /// Maps to PROJECT table (PROJECT_TYPE matches ProcessDefinition.ProcessType).
    /// One ProcessInstance = one PROJECT row at runtime.
    /// </summary>
    public class ProcessInstance : ModelEntityBase
    {
        /// <summary>
        /// Runtime-generated unique identifier for this process execution.
        /// Stored in PROJECT.PROJECT_ID.
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Links to the template. Stored in PROJECT.PROJECT_OBS_NO.
        /// </summary>
        public string ProcessId { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable label for this specific execution.
        /// Example: "Well 42-H P&A — 2024-Q3".
        /// Stored in PROJECT.PROJECT_NAME.
        /// </summary>
        public string InstanceName { get; set; } = string.Empty;

        /// <summary>
        /// Current state in the state machine.
        /// Example: "IN_PROGRESS", "UNDER_REVIEW", "APPROVED".
        /// Stored in PROJECT.PROJECT_STATUS.
        /// </summary>
        public string CurrentState { get; set; } = string.Empty;

        /// <summary>
        /// ID of the entity this process is operating on.
        /// Example: WELL_ID, FACILITY_ID, HSE_INCIDENT_ID.
        /// Stored in PROJECT.WELL_ID or PROJECT.SOURCE (for non-well entities).
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// PPDM table that owns the entity.
        /// Used by guards to construct dynamic AppFilter queries.
        /// Stored in PROJECT.PROJECT_TYPE_SUBTYPE.
        /// </summary>
        public string EntityTableName { get; set; } = string.Empty;

        /// <summary>
        /// Active field context. Links to FIELD.FIELD_ID.
        /// Stored in PROJECT.FIELD_ID.
        /// </summary>
        public string FieldId { get; set; } = string.Empty;

        /// <summary>
        /// Current active step ID.
        /// Stored in PROJECT.CURRENT_STEP_ID (extension column).
        /// </summary>
        public string CurrentStepId { get; set; } = string.Empty;

        /// <summary>
        /// DateTime when the process was started.
        /// Stored in PROJECT.ACTUAL_START_DATE.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// DateTime when the process reached a terminal state.
        /// Stored in PROJECT.ACTUAL_END_DATE.
        /// </summary>
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        /// User who initiated this process instance.
        /// Stored in PROJECT.OPERATOR_BA_ID.
        /// </summary>
        public string InitiatingUserId { get; set; } = string.Empty;

        /// <summary>
        /// Jurisdiction of this specific execution.
        /// Example: "USA", "CANADA", "UK_NORTH_SEA".
        /// Stored in PROJECT.COUNTRY_CODE.
        /// </summary>
        public string Jurisdiction { get; set; } = string.Empty;

        /// <summary>
        /// Free-text context notes (last transition reason, hold reason, etc.)
        /// Stored in PROJECT.REMARKS.
        /// </summary>
        public string? Notes { get; set; }
    }
}
```

---

## ProcessGuardException

Thrown by guard functions when a transition cannot proceed.

```csharp
namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Thrown by a process state machine guard when a transition is blocked.
    /// Contains structured fields so the API can return a machine-readable error response.
    /// </summary>
    public class ProcessGuardException : InvalidOperationException
    {
        /// <summary>
        /// The name of the blocked transition (trigger name).
        /// Example: "start", "complete", "approve".
        /// </summary>
        public string TransitionName { get; }

        /// <summary>
        /// Human-readable description of the missing requirement.
        /// Example: "Contractor assignment (PROJECT_STEP_BA) is required".
        /// </summary>
        public string RequiredField { get; }

        /// <summary>
        /// Regulatory / standard reference justifying the guard.
        /// Example: "IOGP S-501 §3.2", "BSEE SEMS §250.1920".
        /// </summary>
        public string RegulatoryReference { get; }

        public ProcessGuardException(
            string transitionName,
            string requiredField,
            string regulatoryRef)
            : base($"Transition '{transitionName}' blocked: {requiredField} [{regulatoryRef}]")
        {
            TransitionName     = transitionName;
            RequiredField      = requiredField;
            RegulatoryReference = regulatoryRef;
        }
    }
}
```

---

## ProcessTransitionRecord

Audit trail entry for each state machine transition.

```csharp
namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Persisted audit record for every state machine transition.
    /// Stored in PPDM_AUDIT_HISTORY with TABLE_NAME = 'PROCESS_INSTANCE'.
    /// </summary>
    public class ProcessTransitionRecord : ModelEntityBase
    {
        /// <summary>InstanceId of the ProcessInstance. Stored in ROW_ID.</summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>State before this transition. Stored in OLD_VALUE.</summary>
        public string FromState { get; set; } = string.Empty;

        /// <summary>Trigger name that caused this transition. Stored in CHANGE_TYPE.</summary>
        public string Trigger { get; set; } = string.Empty;

        /// <summary>State after this transition. Stored in NEW_VALUE.</summary>
        public string ToState { get; set; } = string.Empty;

        /// <summary>User who triggered the transition. Stored in CHANGE_USER.</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>When the transition occurred. Stored in CHANGE_DATE.</summary>
        public DateTime TransitionDate { get; set; }

        /// <summary>Optional free-text reason or comment. Stored in REMARK.</summary>
        public string? Reason { get; set; }
    }
}
```

---

## ProcessSummaryDto

Read-only DTO returned by `GetProcessSummariesForFieldAsync`.  
Lives in `Beep.OilandGas.Models/Data/Process/` (no separate DTO namespace per CLAUDE.md).

```csharp
namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Lightweight view model for displaying process instance lists.
    /// Not persisted — assembled by IProcessService from PROJECT + POOL + WELL reads.
    /// </summary>
    public class ProcessSummaryDto
    {
        public string InstanceId    { get; set; } = string.Empty;
        public string ProcessId     { get; set; } = string.Empty;
        public string ProcessName   { get; set; } = string.Empty;
        public string ProcessType   { get; set; } = string.Empty;
        public string EntityId      { get; set; } = string.Empty;
        public string EntityName    { get; set; } = string.Empty;
        public string CurrentState  { get; set; } = string.Empty;
        public string Jurisdiction  { get; set; } = string.Empty;
        public DateTime? StartDate  { get; set; }
        public string InitiatedBy   { get; set; } = string.Empty;

        /// <summary>Available trigger names from current state. Populated by ProcessStateMachine.</summary>
        public List<string> AvailableTransitions { get; set; } = new();
    }
}
```
