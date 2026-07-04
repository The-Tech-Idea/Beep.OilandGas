# Phase 2 — Workflow Engine Enhancement

> **Status:** Not Started | **Depends on:** Phase 1 (PERSONA_ROLE mapping, field scoping)
> **Est. Effort:** 3–4 weeks | **Module:** `Beep.OilandGas.LifeCycle` (extend existing)

---

## Objectives

1. Implement Delegation of Authority (DoA) with financial thresholds — the single most important governance control for oil & gas
2. Add dynamic step routing based on entity attributes (e.g., route AFE > $500K to Executive instead of Manager)
3. Move from SLA tracking (already exists) to SLA enforcement with escalation actions
4. Support quorum-based parallel approval (N of M must approve)
5. Enable sub-process spawning with proper parent-child lifecycle management
6. Add workflow versioning with in-flight instance migration
7. Add conditional branching to step transitions (if/then/else logic)

---

## Current State vs Target State

| Aspect | Current | Target |
|--------|---------|--------|
| Approval routing | Static (same approvers for all AFEs) | Dynamic (AFE amount determines approval level) |
| SLA tracking | Tracks breach, no action | Auto-escalate to backup on breach |
| Parallel approval | All must approve | N of M quorum configurable |
| Sub-processes | PARENT_PROCESS_INSTANCE_ID column exists, unused | Parent orchestrates child lifecycle |
| Versioning | Not supported | New version created, in-flight instances migrate |
| Branching | Sequential steps only | Conditional transitions (if amount > X → step A, else → step B) |

---

## Module Architecture

All new entities go into `Beep.OilandGas.LifeCycle\Data\Tables\` following the existing pattern:

```
Beep.OilandGas.LifeCycle\
├── Data\
│   └── Tables\
│       ├── PROCESS_DEFINITION.cs          (existing)
│       ├── PROCESS_INSTANCE.cs            (existing)
│       ├── PROCESS_STEP_INSTANCE.cs       (existing)
│       ├── PROCESS_HISTORY.cs             (existing)
│       ├── PROCESS_APPROVAL.cs            (existing)
│       ├── DELEGATION_OF_AUTHORITY.cs     (NEW - Phase 2.1)
│       ├── WORKFLOW_VERSION.cs            (NEW - Phase 2.6)
│       └── WORKFLOW_DEPENDENCY.cs         (NEW - Phase 2.7)
├── Services\
│   └── Processes\
│       ├── ApprovalWorkflowEngine.cs      (existing - MODIFY for DoA + quorum)
│       ├── ProcessStateMachine.cs         (existing - MODIFY for conditional branching)
│       ├── SlaTrackingService.cs          (existing - MODIFY for escalation actions)
│       ├── PPDMProcessService.cs          (existing - MODIFY for dynamic routing)
│       ├── ProcessServiceBase.cs          (existing - MODIFY for sub-process spawning)
│       ├── DoAEvaluationService.cs        (NEW)
│       ├── DynamicRoutingService.cs       (NEW)
│       ├── EscalationActionService.cs     (NEW)
│       └── WorkflowVersioningService.cs   (NEW)
└── Modules\
    └── LifeCycleModule.cs                 (existing - MODIFY to register new entities)
```

**Key decision:** We extend the existing `LifeCycle` module rather than creating a separate module because:
1. The 5 PROCESS_* tables already live there
2. The new entities are tightly coupled to process execution
3. The `ProcessServiceBase` + `PPDMProcessService` already provide the service layer
4. Module registration happens in `LifeCycleModule.EntityTypes` — we add types, not create a new module

---

## Task Details

### P2-01: Create DELEGATION_OF_AUTHORITY Entity

**File:** `Beep.OilandGas.LifeCycle\Data\Tables\DELEGATION_OF_AUTHORITY.cs` (NEW)

```csharp
public class DELEGATION_OF_AUTHORITY : ModelEntityBase
{
    public string DOA_ID { get; set; }              // PK
    public string DOA_NAME { get; set; }             // "AFE Approval Limits"
    public string ENTITY_TYPE { get; set; }          // "AFE", "COST_TRANSACTION", "REVENUE_TRANSACTION"
    public string FIELD_NAME { get; set; }           // Entity field to evaluate: "ESTIMATED_COST", "AMOUNT"
    public string COMPARISON_OPERATOR { get; set; }  // "GREATER_THAN", "LESS_THAN", "BETWEEN"
    public decimal THRESHOLD_VALUE { get; set; }     // e.g., 50000.00
    public decimal? THRESHOLD_VALUE_MAX { get; set; } // For BETWEEN operator
    public string CURRENCY_CODE { get; set; }        // "USD"
    public string APPROVAL_LEVEL { get; set; }       // "LEVEL_1", "LEVEL_2", "LEVEL_3", "LEVEL_4", "LEVEL_5"
    public string REQUIRED_ROLE { get; set; }        // Role needed at this level
    public int APPROVAL_SEQUENCE { get; set; }       // Order in approval chain
    public bool REQUIRES_UNANIMOUS { get; set; }     // All approvers at this level or just one?
    public string ESCALATION_ROLE { get; set; }      // Backup if primary unavailable
    public int ESCALATION_HOURS { get; set; }        // Auto-escalate after N hours
    public string PROCESS_TYPE { get; set; }         // Which process type this DOA applies to
    public string NOTES { get; set; }
}
```

**Table registration:** Add `typeof(DELEGATION_OF_AUTHORITY)` to `LifeCycleModule.EntityTypes`.

---

### P2-02: Implement DoA Threshold Evaluation Service

**File:** `Beep.OilandGas.LifeCycle\Services\Processes\DoAEvaluationService.cs` (NEW)

```csharp
public interface IDoAEvaluationService
{
    /// <summary>
    /// Given an entity (e.g., an AFE with ESTIMATED_COST = $750,000),
    /// determine which approval levels are required.
    /// Returns ordered list of (level, role, threshold) tuples.
    /// </summary>
    Task<List<DoaApprovalLevel>> EvaluateThresholdsAsync(
        string entityType, 
        Dictionary<string, object> entityFields,
        string processType);

    /// <summary>
    /// Get the escalation path for a given approval level.
    /// </summary>
    Task<DoaEscalationPath> GetEscalationPathAsync(string doaId, string currentLevel);
}

public class DoaApprovalLevel
{
    public string Level { get; set; }          // "LEVEL_1"
    public string RequiredRole { get; set; }    // "Manager"
    public decimal ThresholdValue { get; set; } // 50000
    public bool RequiresUnanimous { get; set; }
    public int ApprovalSequence { get; set; }
}

public class DoaEscalationPath
{
    public string PrimaryRole { get; set; }
    public string EscalationRole { get; set; }
    public int EscalationHours { get; set; }
}
```

**Logic:**
1. Load all DOA rules for the given entity_type + process_type, ordered by THRESHOLD_VALUE ascending
2. For each rule, evaluate: does the entity field value trigger this threshold?
3. Return the triggered levels in approval sequence order

**Example:** AFE with `ESTIMATED_COST = $750,000`:
- LEVEL_1: $0–$50,000 → Manager → triggered ✓
- LEVEL_2: $50,001–$500,000 → Senior Manager → triggered ✓
- LEVEL_3: $500,001–$5,000,000 → Executive → triggered ✓
- LEVEL_4: > $5,000,000 → Board → NOT triggered
- Result: 3-level approval chain [Manager → Senior Manager → Executive]

---

### P2-03: Integrate DoA into ApprovalWorkflowEngine

**File:** `ApprovalWorkflowEngine.cs` (MODIFY)

Current `CreateApprovalChainAsync` creates approvals from a static `ApprovalChainConfig`. 

**New overload:**
```csharp
public async Task<ApprovalChainResult> CreateApprovalChainWithDoAAsync(
    string processInstanceId,
    string stepInstanceId,
    string entityType,
    Dictionary<string, object> entityFields,
    string userId)
{
    // 1. Call DoAEvaluationService.EvaluateThresholdsAsync
    // 2. Build ApprovalChainConfig dynamically from DOA results
    // 3. Call existing CreateApprovalChainAsync with dynamic config
    // 4. Record which DOA rules were applied in PROCESS_APPROVAL.APPROVAL_NOTES
}
```

**Integration point in PPDMProcessService.CompleteStepAsync:**
When completing a step that transitions to an approval step, check if DOA rules exist for this entity type. If yes → use `CreateApprovalChainWithDoAAsync`. If no → use existing static config.

---

### P2-04: Seed Default DoA Thresholds

**Add to `LifeCycleSeedService` or create `DoASeedService`:**

```csharp
public async Task SeedDefaultDoAThresholdsAsync(string userId)
{
    var thresholds = new[]
    {
        // AFE Approval Limits
        new DELEGATION_OF_AUTHORITY 
        { 
            DOA_NAME = "AFE Standard Limits", ENTITY_TYPE = "AFE", 
            FIELD_NAME = "ESTIMATED_COST", COMPARISON_OPERATOR = "GREATER_THAN",
            THRESHOLD_VALUE = 0, APPROVAL_LEVEL = "LEVEL_1", 
            REQUIRED_ROLE = "Manager", APPROVAL_SEQUENCE = 1, PROCESS_TYPE = "AFE_APPROVAL"
        },
        new DELEGATION_OF_AUTHORITY 
        { 
            DOA_NAME = "AFE Standard Limits", ENTITY_TYPE = "AFE",
            FIELD_NAME = "ESTIMATED_COST", THRESHOLD_VALUE = 50000,
            APPROVAL_LEVEL = "LEVEL_2", REQUIRED_ROLE = "SeniorManager", 
            APPROVAL_SEQUENCE = 2, PROCESS_TYPE = "AFE_APPROVAL"
        },
        // ... 3 more levels for AFE
        // Cost Transaction Approval Limits
        // Revenue Transaction Approval Limits
    };
    // Upsert each via PPDMGenericRepository
}
```

**5-Level Default Scale:**
| Level | Threshold | Role | Example |
|-------|-----------|------|---------|
| LEVEL_1 | > $0 | Team Lead / Supervisor | Routine expenses |
| LEVEL_2 | > $50,000 | Manager | Minor AFEs, workovers |
| LEVEL_3 | > $500,000 | Senior Manager / Asset Manager | Major workovers, facilities |
| LEVEL_4 | > $5,000,000 | Executive / VP | New wells, large projects |
| LEVEL_5 | > $50,000,000 | Board / Partners | FDP, platform development |

---

### P2-05: Implement Dynamic Routing Service

**File:** `Beep.OilandGas.LifeCycle\Services\Processes\DynamicRoutingService.cs` (NEW)

```csharp
public interface IDynamicRoutingService
{
    /// <summary>
    /// Given a completed step and the current entity state,
    /// determine which step comes next (may differ from static definition).
    /// </summary>
    Task<ProcessStepDefinition> ResolveNextStepAsync(
        ProcessInstance instance,
        ProcessStepInstance completedStep,
        Dictionary<string, object> entityContext);

    /// <summary>
    /// Evaluate all conditional branches from a step and return the first matching one.
    /// </summary>
    Task<string> EvaluateBranchConditionAsync(
        string conditionExpression,
        Dictionary<string, object> context);
}
```

**Condition expression syntax** (stored in `StepConfiguration` / `ConditionalNextSteps`):
```json
{
  "conditionalNextSteps": [
    {
      "condition": "ESTIMATED_COST > 500000",
      "nextStepId": "EXECUTIVE_REVIEW"
    },
    {
      "condition": "ESTIMATED_COST <= 500000",
      "nextStepId": "MANAGER_APPROVAL"
    }
  ]
}
```

Supported operators: `>`, `<`, `>=`, `<=`, `==`, `!=`, `IN`, `CONTAINS`, `IS_NULL`, `IS_NOT_NULL`

---

### P2-06: Update ProcessServiceBase for Dynamic Step Resolution

**File:** `ProcessServiceBase.cs` (MODIFY — `CompleteStepAsync` method)

Current logic: always uses `stepDefinition.NextStepId` or `ConditionalNextSteps[0]`.

New logic:
```csharp
protected async Task<string> ResolveNextStepAsync(
    ProcessInstance instance, 
    ProcessStepInstance completedStep,
    ProcessDefinition definition)
{
    // 1. Load entity data for the instance (from INSTANCE_DATA_JSON + database)
    var entityContext = await LoadEntityContextAsync(instance);
    
    // 2. Check for dynamic routing rules
    var nextStep = await _dynamicRoutingService.ResolveNextStepAsync(
        instance, completedStep, entityContext);
    
    if (nextStep != null) return nextStep.StepId;
    
    // 3. Fall back to static definition
    return completedStep.StepId; // default: no branching
}
```

---

### P2-07: Implement Escalation Action Service

**File:** `Beep.OilandGas.LifeCycle\Services\Processes\EscalationActionService.cs` (NEW)

```csharp
public interface IEscalationActionService
{
    /// <summary>
    /// Execute escalation for a step that has breached its SLA.
    /// </summary>
    Task<EscalationResult> EscalateAsync(
        string processInstanceId, 
        string stepInstanceId,
        SlaBreachInfo breach);

    /// <summary>
    /// Register available escalation actions.
    /// </summary>
    void RegisterAction(string actionType, Func<EscalationContext, Task> handler);
}

public class EscalationResult
{
    public bool Success { get; set; }
    public string ActionTaken { get; set; }      // "REASSIGNED", "NOTIFIED", "AUTO_APPROVED"
    public string NewAssigneeId { get; set; }
    public string NotificationSentTo { get; set; }
    public DateTime ActionTimestamp { get; set; }
}
```

**Default escalation actions:**
| Action | Description | Configurable |
|--------|-------------|-------------|
| `REASSIGN_TO_BACKUP` | Move task to escalation role | Backup role, delay hours |
| `NOTIFY_MANAGER` | Email/notify the assignee's manager | Manager hierarchy level |
| `NOTIFY_REQUESTER` | Notify the original requester | — |
| `AUTO_ESCALATE_LEVEL` | Move approval up one DOA level | Max auto-escalate levels |
| `SUSPEND_PROCESS` | Halt the process, flag for admin | — |
| `AUTO_APPROVE` | Auto-approve (only for low-risk steps) | Max auto-approve threshold |

---

### P2-08: Update SlaTrackingService with Escalation Triggers

**File:** `SlaTrackingService.cs` (MODIFY)

Current: `SlaTrackingService` tracks time but only reports breach — no action.

**New background monitor** (via `IHostedService` or timer):
```csharp
public class SlaMonitorService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Query all PROCESS_STEP_INSTANCE where STATUS = 'IN_PROGRESS' 
            //    and SLA_HOURS is set and STARTED_DATE + SLA_HOURS < now
            var breached = await FindBreachedStepsAsync();
            
            foreach (var step in breached)
            {
                // 2. Check if escalation already triggered for this breach
                if (await WasAlreadyEscalatedAsync(step)) continue;
                
                // 3. Execute escalation action
                await _escalationService.EscalateAsync(
                    step.PROCESS_INSTANCE_ID, 
                    step.PROCESS_STEP_INSTANCE_ID, 
                    new SlaBreachInfo { ... });
                
                // 4. Log escalation to PROCESS_HISTORY
                await _processService.AddHistoryEntryAsync(...);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

---

### P2-09 & P2-10: Quorum-Based Parallel Approval

**File:** `ApprovalWorkflowEngine.cs` (MODIFY)

Add a new approval type: `QUORUM` (N of M must approve).

```csharp
public enum ApprovalType 
{ 
    SEQUENTIAL,   // One after another (existing)
    PARALLEL,     // All must approve (existing)
    ANY,          // Any one can approve (existing)
    QUORUM        // N of M must approve (NEW)
}
```

**Quorum configuration in StepConfiguration:**
```json
{
  "approvalType": "QUORUM",
  "quorumRequired": 3,
  "quorumTotal": 5,
  "approvers": [
    { "userId": "...", "role": "Manager" },
    { "userId": "...", "role": "Engineer" },
    { "userId": "...", "role": "Accountant" },
    { "userId": "...", "role": "HSE_Officer" },
    { "userId": "...", "role": "Executive" }
  ]
}
```

**`IsApprovalChainCompleteAsync` update:**
```csharp
if (approvalType == "QUORUM")
{
    var approvedCount = approvals.Count(a => a.APPROVAL_STATUS == "APPROVED");
    var rejectedCount = approvals.Count(a => a.APPROVAL_STATUS == "REJECTED");
    var totalCount = approvals.Count;
    
    if (approvedCount >= quorumRequired) return (true, "APPROVED");
    if (rejectedCount > (totalCount - quorumRequired)) return (true, "REJECTED");
    return (false, null);
}
```

---

### P2-11: Sub-Process Spawning

**File:** `ProcessServiceBase.cs` (MODIFY — add `SpawnSubProcessAsync`)

```csharp
public async Task<ProcessInstance> SpawnSubProcessAsync(
    string parentProcessInstanceId,
    string subProcessDefinitionId,
    string entityType,
    string entityId,
    string userId,
    Dictionary<string, object>? contextData = null)
{
    // 1. Validate parent process exists and is active
    var parent = await GetProcessInstanceAsync(parentProcessInstanceId);
    if (parent == null) throw new ArgumentException("Parent process not found");
    
    // 2. Start sub-process with PARENT_PROCESS_INSTANCE_ID set
    var subProcess = await StartProcessAsync(
        subProcessDefinitionId, entityType, entityId, userId, contextData);
    
    subProcess.PARENT_PROCESS_INSTANCE_ID = parentProcessInstanceId;
    await UpdateProcessInstanceAsync(subProcess);
    
    // 3. Add parent history entry: "Sub-process {id} spawned"
    await AddHistoryEntryAsync(parentProcessInstanceId, null, 
        "SUB_PROCESS_SPAWNED", userId, 
        $"Sub-process {subProcess.InstanceId} started for {entityType}/{entityId}");
    
    // 4. Parent step WAITS until sub-process completes (or continues in parallel, configurable)
    return subProcess;
}
```

**Parent-child lifecycle rules:**
- Cancelling parent → cancels all children (cascading)
- Child completion → parent step can auto-advance (if configured as blocking)
- Child failure → parent can be notified or continue (configurable)

---

### P2-12: Workflow Versioning Service

**File:** `Beep.OilandGas.LifeCycle\Services\Processes\WorkflowVersioningService.cs` (NEW)

```csharp
public interface IWorkflowVersioningService
{
    Task<ProcessDefinition> CreateNewVersionAsync(
        string processId, string changeDescription, string userId);
    
    Task<List<ProcessDefinition>> GetVersionHistoryAsync(string processId);
    
    Task<MigrationResult> MigrateInFlightInstanceAsync(
        string instanceId, string targetVersionId, string userId);
}

public class MigrationResult
{
    public bool Success { get; set; }
    public string FromVersion { get; set; }
    public string ToVersion { get; set; }
    public List<string> RemappedStepIds { get; set; }
    public List<string> Warnings { get; set; }
}
```

**WORKFLOW_VERSION entity:**
```csharp
public class WORKFLOW_VERSION : ModelEntityBase
{
    public string VERSION_ID { get; set; }          // PK
    public string PROCESS_DEFINITION_ID { get; set; } // FK
    public string VERSION_NUMBER { get; set; }       // "1.0", "1.1", "2.0"
    public string CHANGE_DESCRIPTION { get; set; }
    public string PREVIOUS_VERSION_ID { get; set; }  // FK to previous WORKFLOW_VERSION
    public string PROCESS_CONFIG_SNAPSHOT { get; set; } // Full JSON snapshot
    public DateTime EFFECTIVE_DATE { get; set; }
    public string DEPRECATED_STEP_IDS { get; set; }  // JSON array of removed step IDs
    public string STEP_REMAPPING_JSON { get; set; }  // old_step_id → new_step_id mapping
}
```

---

### P2-13: In-Flight Instance Migration

When a process definition is updated, running instances can be migrated:
1. Compare instance's current step to new version's steps
2. If step still exists → no change, instance continues
3. If step was removed → map to replacement step via `STEP_REMAPPING_JSON`
4. If step was split → instance goes to first new step
5. Add `PROCESS_HISTORY` entry: "Instance migrated from v1.0 to v1.1"

**This is optional per instance** — process owners can choose to let in-flight instances complete on old version while new instances use new version.

---

### P2-14: Conditional Branching in ProcessStateMachine

**File:** `ProcessStateMachine.cs` (MODIFY)

Add `ConditionalTransition` type:
```csharp
public class ConditionalTransition : ProcessTransition
{
    public string ConditionExpression { get; set; }        // "ESTIMATED_COST > 500000"
    public string TrueTargetStateId { get; set; }
    public string FalseTargetStateId { get; set; }
    public Dictionary<string, object> TrueActions { get; set; }
    public Dictionary<string, object> FalseActions { get; set; }
}
```

**`ExecuteTransitionAsync` update:**
```csharp
if (transition is ConditionalTransition ct)
{
    var conditionResult = await _dynamicRoutingService.EvaluateBranchConditionAsync(
        ct.ConditionExpression, contextData);
    
    var targetState = conditionResult ? ct.TrueTargetStateId : ct.FalseTargetStateId;
    var actions = conditionResult ? ct.TrueActions : ct.FalseActions;
    
    // Execute actions for the chosen branch
    await ExecuteActionsAsync(actions, contextData, userId);
    return targetState;
}
```

---

### P2-15: Update ProcessDefinitionInitializer

Add new process definitions that use the enhanced engine features:

```csharp
// AFE Approval with DoA (replaces static GATE_AFE_APPROVAL)
private async Task InitializeAfeDoAApprovalAsync(string userId)
{
    var definition = new ProcessDefinition
    {
        ProcessId = "AFE_DOA_APPROVAL",
        ProcessName = "AFE Approval with Delegation of Authority",
        ProcessType = "FINANCIAL",
        EntityType = "AFE",
        Steps = new List<ProcessStepDefinition>
        {
            new() { StepId = "AFE_PREPARE", StepName = "Prepare AFE", SequenceNumber = 1,
                RequiredRoles = new(){"PetroleumEngineer", "DrillingEngineer"} },
            new() { StepId = "DOA_EVALUATION", StepName = "DOA Threshold Evaluation", 
                SequenceNumber = 2, StepType = "SYSTEM" },
            new() { StepId = "DOA_APPROVAL", StepName = "Multi-Level Approval (DoA)", 
                SequenceNumber = 3, RequiresApproval = true, 
                StepConfiguration = new() { ["approvalType"] = "DYNAMIC_DOA" } },
            new() { StepId = "AFE_ACTIVE", StepName = "AFE Active", SequenceNumber = 4 }
        }
    };
    await CreateProcessDefinitionIfNotExistsAsync(definition, userId);
}
```

---

## Phase 2 Completion Checklist

- [ ] DELEGATION_OF_AUTHORITY table created and seeded with 5-level defaults
- [ ] DoA evaluation integrated into ApprovalWorkflowEngine
- [ ] Dynamic routing resolves correct next step based on entity attributes
- [ ] SLA breach triggers escalation action (not just logging)
- [ ] Quorum approval works: 3 of 5 approve → chain completes
- [ ] Sub-process spawns with correct parent link and lifecycle management
- [ ] WORKFLOW_VERSION table created, versioning service works
- [ ] In-flight instances can be migrated to new versions
- [ ] Conditional branching routes to correct step based on expressions
- [ ] Enhanced process definitions seeded in ProcessDefinitionInitializer
- [ ] All new entities registered in LifeCycleModule.EntityTypes
- [ ] Backward compatible — existing 80 process definitions still work

## Phase 2 Deliverables

| # | File | Action |
|---|------|--------|
| 1 | `LifeCycle\Data\Tables\DELEGATION_OF_AUTHORITY.cs` | CREATE |
| 2 | `LifeCycle\Data\Tables\WORKFLOW_VERSION.cs` | CREATE |
| 3 | `LifeCycle\Data\Tables\WORKFLOW_DEPENDENCY.cs` | CREATE |
| 4 | `LifeCycle\Services\Processes\DoAEvaluationService.cs` | CREATE |
| 5 | `LifeCycle\Services\Processes\DynamicRoutingService.cs` | CREATE |
| 6 | `LifeCycle\Services\Processes\EscalationActionService.cs` | CREATE |
| 7 | `LifeCycle\Services\Processes\WorkflowVersioningService.cs` | CREATE |
| 8 | `LifeCycle\Services\Processes\SlaMonitorService.cs` | CREATE |
| 9 | `LifeCycle\Services\Processes\ApprovalWorkflowEngine.cs` | MODIFY |
| 10 | `LifeCycle\Services\Processes\ProcessStateMachine.cs` | MODIFY |
| 11 | `LifeCycle\Services\Processes\SlaTrackingService.cs` | MODIFY |
| 12 | `LifeCycle\Services\Processes\PPDMProcessService.cs` | MODIFY |
| 13 | `LifeCycle\Services\Processes\ProcessServiceBase.cs` | MODIFY |
| 14 | `LifeCycle\Services\Processes\ProcessDefinitionInitializer.cs` | MODIFY |
| 15 | `LifeCycle\Modules\LifeCycleModule.cs` | MODIFY |
| 16 | `PPDM39\Scripts\Sqlserver\WorkflowEnhancementTables.sql` | CREATE |

---

*Previous: [Phase 1 — Foundation: RBAC Hardening](phase-1-foundation-rbac.md)*
*Next: [Phase 3 — Cross-Role Orchestration](phase-3-cross-role-orchestration.md)*
