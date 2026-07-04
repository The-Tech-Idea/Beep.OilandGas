# Phase 3 — Cross-Role Orchestration

> **Status:** Not Started | **Depends on:** Phase 2 (enhanced workflow engine)
> **Est. Effort:** 3–4 weeks | **Module:** Extends `Beep.OilandGas.LifeCycle` (new entities + services, registered in `LifeCycleModule`)

---

## Objectives

1. Design and implement 25 cross-role workflow templates that model real O&G handoffs between personas
2. Define formal role-to-role handoff contracts (data requirements, SLAs, approval context)
3. Route tasks to the correct persona's inbox automatically
4. Orchestrate multi-entity workflow chains (AFE → Cost → Journal → Revenue)
5. Define workflow dependency graphs (Workflow B blocked until Workflow A step X completes)
6. Trigger workflows automatically from business events (production posted → start revenue recognition)

---

## Why Extend LifeCycle (Not a New Project)?

All cross-role orchestration is workflow-level concern — it defines process definitions, step sequences, and entity routing rules. `Beep.OilandGas.LifeCycle` already owns:
- The 5 PROCESS_* tables and their entities
- `ProcessServiceBase`, `PPDMProcessService`, `ApprovalWorkflowEngine`
- `ProcessDefinitionInitializer` (80 process definitions across 4 partial files)
- `LifeCycleModule` (Order=50) with `EntityTypes` registration

Cross-role orchestration adds new entity types and services, but they all operate on the same workflow infrastructure. They belong in the same project.

**What gets added to LifeCycle:**

```
Beep.OilandGas.LifeCycle\
├── Data\
│   └── Tables\
│       ├── (existing 5 PROCESS_* entities)
│       ├── DELEGATION_OF_AUTHORITY.cs     (Phase 2)
│       ├── WORKFLOW_VERSION.cs            (Phase 2)
│       ├── WORKFLOW_DEPENDENCY.cs         (Phase 2)
│       ├── ROLE_HANDOFF_CONTRACT.cs      (Phase 3 - NEW)
│       ├── WORKFLOW_DEPENDENCY_GRAPH.cs   (Phase 3 - NEW)
│       ├── BUSINESS_EVENT_TRIGGER.cs      (Phase 3 - NEW)
│       └── CROSS_PERSONA_TASK.cs          (Phase 3 - NEW)
├── Services\
│   └── Processes\
│       ├── (existing services)
│       ├── HandoffValidationService.cs    (Phase 3 - NEW)
│       ├── CrossPersonaTaskRouter.cs      (Phase 3 - NEW)
│       ├── MultiEntityWorkflowOrchestrator.cs (Phase 3 - NEW)
│       ├── WorkflowDependencyGraphService.cs  (Phase 3 - NEW)
│       └── BusinessEventTriggerService.cs (Phase 3 - NEW)
├── Definitions\
│   ├── CrossRoleWorkflowSeed.Finance.cs   (Phase 3 - NEW)
│   ├── CrossRoleWorkflowSeed.Operations.cs (Phase 3 - NEW)
│   ├── CrossRoleWorkflowSeed.HSECompliance.cs (Phase 3 - NEW)
│   └── CrossRoleWorkflowSeed.AssetLifecycle.cs (Phase 3 - NEW)
└── Modules\
    └── LifeCycleModule.cs                 (MODIFY — add EntityTypes + seed calls)
```

**No new .csproj, no new module, no new solution entry.** LifeCycle already references ProductionAccounting, HSE, and all other domain projects it needs.

---

## The 25 Cross-Role Workflow Templates

### Set 1: Finance & Accounting (8 workflows)

| ID | Workflow Name | From Persona | To Persona | Trigger Entity | Description |
|----|--------------|-------------|------------|----------------|-------------|
| CRW-01 | Production → Revenue Recognition | Production Engineer | Accountant | PDEN_VOL_SUMMARY | Monthly volumes posted → auto-create REVENUE_TRANSACTION + trigger revenue recognition workflow |
| CRW-02 | AFE Cost Tracking | Drilling Engineer | Accountant | AFE | AFE actual costs updated → cost review → journal entry posting |
| CRW-03 | Royalty Calculation & Payment | Production Engineer | Accountant | ROYALTY_CALCULATION | Production allocated → royalty calculated → payment processed → owner statement |
| CRW-04 | Joint Interest Billing (JIB) | Accountant | Asset Manager | COST_TRANSACTION | Costs allocated to partners → JIB statement generated → partner approval → payment |
| CRW-05 | Capital vs Expense Classification | Drilling Engineer | Accountant | AFE_LINE_ITEM | Engineer classifies costs → Accountant reviews → Manager approves → GL posted |
| CRW-06 | Production Tax Filing | Accountant | Compliance Officer | TAX_CALCULATION | Revenue → tax calculated → compliance review → regulatory filing |
| CRW-07 | Period Close | Accountant | Executive | JOURNAL_ENTRY | Month-end entries posted → Accountant reconciles → Manager reviews → Executive signs off |
| CRW-08 | Budget vs Actuals Review | All Engineers | Accountant | COST_TRANSACTION | Monthly actuals vs AFE estimate → variance analysis → corrective action |

### Set 2: Operations & Engineering (6 workflows)

| ID | Workflow Name | From Persona | To Persona | Trigger Entity | Description |
|----|--------------|-------------|------------|----------------|-------------|
| CRW-09 | Well Handoff: Drilling → Production | Drilling Engineer | Production Engineer | WELL | Well completed → handoff package (wellbore diagram, completion report, test data) → Production accepts |
| CRW-10 | Workover Proposal → Approval → Execution | Production Engineer | Drilling Engineer | WELL | Production identifies declining well → proposes workover → Drilling evaluates → AFE approved → executes |
| CRW-11 | Facility Modification Request | Production Engineer | Facilities Engineer | FACILITY | Production needs modification → Facilities assesses → AFE → design → construction → handback |
| CRW-12 | Production Optimization Review | Production Engineer | Reservoir Engineer | WELL / POOL | Production data analyzed → Reservoir evaluates → recommends changes → implemented → results tracked |
| CRW-13 | Well Test → Reservoir Model Update | Production Engineer | Reservoir Engineer | WELL_TEST | Well test completed → data validated → reservoir model updated → reserves impact assessed |
| CRW-14 | Pipeline Capacity Review | Production Engineer | Facilities Engineer | PIPELINE | Production forecast → pipeline capacity check → bottleneck identified → debottlenecking plan |

### Set 3: HSE, Compliance & Regulatory (6 workflows)

| ID | Workflow Name | From Persona | To Persona | Trigger Entity | Description |
|----|--------------|-------------|------------|----------------|-------------|
| CRW-15 | Incident → Investigation → Corrective Action | HSE Officer | Field Engineer | HSE_INCIDENT | Incident reported → HSE classifies → investigation team assigned → root cause → corrective action → verified |
| CRW-16 | Near Miss → Risk Assessment | Any Persona | HSE Officer | HSE_INCIDENT (near miss) | Any worker reports near miss → HSE assesses → risk level determined → preventive action |
| CRW-17 | Permit to Work | Field Engineer | HSE Officer | PERMIT | Engineer requests permit → HSE reviews hazards → permit issued → work executed → permit closed |
| CRW-18 | Regulatory Filing Review | Compliance Officer | Executive | REGULATORY_FILING | Filing prepared → Compliance reviews → Legal reviews (if needed) → Executive signs → submitted |
| CRW-19 | Environmental Spill Response | Field Engineer | HSE Officer | HSE_INCIDENT (environmental) | Spill detected → immediate response → HSE notified → regulatory notification (if reportable) → remediation → closure |
| CRW-20 | GHG Emissions Report | Production Engineer | Compliance Officer | EMISSION_RECORD | Emissions data collected → Production validates → Compliance compiles → regulatory submission |

### Set 4: Asset Lifecycle & Planning (5 workflows)

| ID | Workflow Name | From Persona | To Persona | Trigger Entity | Description |
|----|--------------|-------------|------------|----------------|-------------|
| CRW-21 | Discovery → Development Decision | Exploration Geologist | Development Planner | FIELD / POOL | Discovery confirmed → reserves estimated → development concept selected → FDP initiated |
| CRW-22 | FDP → Field Development → First Oil | Development Planner | Drilling Engineer → Production Engineer | FIELD | FDP approved → wells drilled → facilities built → first oil → handover to operations |
| CRW-23 | Reserves Revision | Reservoir Engineer | Executive | RESERVES_ESTIMATE | Annual reserves review → Reservoir calculates → peer review → Executive approval → external auditor review |
| CRW-24 | Decommissioning Planning | Production Engineer | Decommissioning Coordinator | WELL / FACILITY | Asset reaches end of life → decommissioning plan → regulatory approval → execution → environmental restoration |
| CRW-25 | Asset Acquisition / Divestiture | Asset Manager | Executive → Accountant | FIELD / WELL | Acquisition target identified → due diligence → valuation → Executive approval → Accountant books entry |

---

## Task Details

### P3-01: Design the 25 Cross-Role Workflow Templates

**Deliverable:** Detailed specification for each of the 25 workflows including:
- Step sequence with role assignments per step
- Data requirements per step (what entity fields must be populated)
- Approval types (sequential, parallel, quorum, DOA-driven)
- SLA targets per step
- Escalation paths
- Handoff contract (data + approval context passed between roles)

**Format:** Each workflow as a JSON document in `Beep.OilandGas.WorkflowManagement\Definitions\` that can be loaded by `ProcessDefinitionInitializer`.

---

### P3-02 to P3-05: Create Process Definitions (4 task groups)

Each workflow becomes a `ProcessDefinition` seeded via LifeCycle's existing seeding pipeline:

```csharp
// In LifeCycleModule.SeedAsync (or LifeCycleSeedService):
await SeedCrossRoleWorkflowsAsync(context, userId);
```

The 25 definitions are split across 4 seed files for maintainability:
- `CrossRoleWorkflowSeed.Finance.cs` — CRW-01 through CRW-08
- `CrossRoleWorkflowSeed.Operations.cs` — CRW-09 through CRW-14
- `CrossRoleWorkflowSeed.HSECompliance.cs` — CRW-15 through CRW-20
- `CrossRoleWorkflowSeed.AssetLifecycle.cs` — CRW-21 through CRW-25

Each follows the existing `ProcessDefinitionInitializer` partial class pattern.

---

### P3-06: Define RoleHandoffContract Model

**File:** `Beep.OilandGas.WorkflowManagement\Data\Tables\ROLE_HANDOFF_CONTRACT.cs` (NEW)

```csharp
public class ROLE_HANDOFF_CONTRACT : ModelEntityBase
{
    public string HANDOFF_CONTRACT_ID { get; set; }     // PK
    public string PROCESS_DEFINITION_ID { get; set; }    // FK → which workflow uses this
    public string FROM_STEP_ID { get; set; }             // Handoff occurs after this step
    public string FROM_ROLE { get; set; }                // Role handing off
    public string TO_STEP_ID { get; set; }               // Handoff goes to this step
    public string TO_ROLE { get; set; }                  // Role receiving
    public string REQUIRED_DATA_FIELDS_JSON { get; set; } // ["FIELD_ID", "ESTIMATED_COST", "APPROVAL_DATE"]
    public string REQUIRED_DOCUMENTS_JSON { get; set; }  // ["Completion Report", "Well Test Data"]
    public string SLA_CONTEXT_JSON { get; set; }         // { "maxResponseHours": 48, "priority": "HIGH" }
    public string APPROVAL_CONTEXT_JSON { get; set; }    // { "delegatedFrom": "...", "scopeLimitedTo": "..." }
    public string VALIDATION_RULES_JSON { get; set; }    // Rules engine expressions for handoff validity
}
```

**Handoff contract validation** (what `HandoffValidationService` checks):
1. Are all `REQUIRED_DATA_FIELDS` populated on the entity?
2. Are all `REQUIRED_DOCUMENTS` attached/uploaded?
3. Does the receiving role have sufficient permissions?
4. Is the receiving person assigned to the correct field/asset?
5. Has the SLA context been acknowledged by the receiver?

---

### P3-07: HandoffValidationService

**File:** `Beep.OilandGas.WorkflowManagement\Services\HandoffValidationService.cs` (NEW)

```csharp
public interface IHandoffValidationService
{
    Task<HandoffValidationResult> ValidateHandoffAsync(
        string handoffContractId, 
        string processInstanceId,
        string entityType, 
        string entityId);

    Task<List<string>> GetMissingRequirementsAsync(
        string handoffContractId, 
        string entityType, 
        string entityId);
}

public class HandoffValidationResult
{
    public bool IsValid { get; set; }
    public List<string> PassedChecks { get; set; }
    public List<string> FailedChecks { get; set; }
    public List<string> Warnings { get; set; }
    public bool CanProceed { get; set; }
    public string BlockingReason { get; set; }
}
```

---

### P3-08: CrossPersonaTaskRouter

**File:** `Beep.OilandGas.WorkflowManagement\Services\CrossPersonaTaskRouter.cs` (NEW)

This is the bridge between workflow state and UI. When a step is assigned to a role, this service determines WHICH persona(s) should see it in their task inbox.

```csharp
public interface ICrossPersonaTaskRouter
{
    /// <summary>
    /// When a step becomes active and is assigned to a role,
    /// determine which personas should see it in their task list.
    /// </summary>
    Task<List<PersonaTaskRoute>> RouteTaskAsync(
        string processInstanceId,
        string stepInstanceId,
        string assignedRole,
        string entityType,
        string entityId);

    /// <summary>
    /// Get all pending tasks for a given persona.
    /// </summary>
    Task<List<CrossPersonaTask>> GetTasksForPersonaAsync(
        string personaCode, string userId);

    /// <summary>
    /// Get task counts per persona (for dashboard badges).
    /// </summary>
    Task<Dictionary<string, int>> GetTaskCountsByPersonaAsync(string userId);
}

public class PersonaTaskRoute
{
    public string PersonaCode { get; set; }          // "ACCOUNTANT"
    public string TaskType { get; set; }             // "APPROVAL", "REVIEW", "DATA_ENTRY"
    public string Route { get; set; }                // "/accounting/dashboard"
    public int Priority { get; set; }                // 1=critical, 2=high, 3=normal, 4=low
    public DateTime? DueDate { get; set; }
}
```

**Resolution logic:**
1. Given `assignedRole`, query `PERSONA_ROLE` to find all personas with that role
2. Filter by field scope (only personas with access to this entity's field)
3. Rank by priority (PRIMARY role match > secondary)
4. Return ordered list of persona routes

---

### P3-09: Update NavigationPolicyService

**File:** `NavigationPolicyService.cs` (MODIFY)

Add `CoreRouteToWorkflow` entries for the cross-role task inbox:
```csharp
["tasks/inbox"] = "processes",
["tasks/approvals"] = "processes",
["tasks/reviews"] = "processes",
```

---

### P3-10: MultiEntityWorkflowOrchestrator

**File:** `Beep.OilandGas.WorkflowManagement\Services\MultiEntityWorkflowOrchestrator.cs` (NEW)

This orchestrates chains like AFE → Cost → Journal → Revenue where each step operates on a different entity type.

```csharp
public interface IMultiEntityWorkflowOrchestrator
{
    /// <summary>
    /// Start a multi-entity workflow chain.
    /// Each step in the chain may create/update a different entity type.
    /// </summary>
    Task<ChainExecutionResult> ExecuteChainAsync(
        string chainDefinitionId,
        string initiatingEntityType,
        string initiatingEntityId,
        Dictionary<string, object> chainContext,
        string userId);

    /// <summary>
    /// Get the current state of all entities in a running chain.
    /// </summary>
    Task<ChainState> GetChainStateAsync(string chainInstanceId);
}

public class ChainExecutionResult
{
    public string ChainInstanceId { get; set; }
    public List<ChainStepResult> StepResults { get; set; }
    public bool AllStepsCompleted { get; set; }
    public string CurrentStepId { get; set; }
}

public class ChainState
{
    public Dictionary<string, string> EntityStates { get; set; }
    // e.g., { "AFE": "APPROVED", "COST_TRANSACTION": "POSTED", "JOURNAL_ENTRY": "DRAFT" }
    public string OverallStatus { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

---

### P3-11, P3-12, P3-13: Three Key Multi-Entity Chains

#### P3-11: AFE → Cost → Journal → Revenue Chain
```
[Engineer creates AFE] 
  → [DoA approval chain (Phase 2)] 
  → [AFE ACTIVE] 
  → [Costs recorded against AFE (COST_TRANSACTION)] 
  → [Accountant reviews costs] 
  → [JOURNAL_ENTRY posted] 
  → [Production revenue recognized (REVENUE_TRANSACTION)] 
  → [Royalties calculated (ROYALTY_CALCULATION)]
```

#### P3-12: Production → Revenue → Royalty Chain
```
[Production Engineer posts PDEN_VOL_SUMMARY] 
  → [Data validated (quality rules)] 
  → [Accountant creates REVENUE_TRANSACTION] 
  → [ROYALTY_CALCULATION triggered] 
  → [Owner payment processed] 
  → [JOURNAL_ENTRY posted] 
  → [Period-end reconciliation]
```

#### P3-13: Incident → Investigation → Corrective Action Chain
```
[HSE_INCIDENT reported] 
  → [HSE Officer classifies severity] 
  → [Investigation team assigned] 
  → [Root cause analysis] 
  → [Corrective action plan] 
  → [Action assigned to Field Engineer] 
  → [Action completed] 
  → [HSE Officer verifies] 
  → [Incident closed]
```

Each chain is a `ProcessDefinition` where individual steps may spawn sub-processes (Phase 2.11) for the per-entity workflows.

---

### P3-14 & P3-15: Workflow Dependency Graph

**File:** `Beep.OilandGas.WorkflowManagement\Services\WorkflowDependencyGraphService.cs` (NEW)

```csharp
public interface IWorkflowDependencyGraphService
{
    /// <summary>
    /// Define that Workflow B Step Y cannot start until Workflow A Step X completes.
    /// </summary>
    Task AddDependencyAsync(
        string dependentProcessId, string dependentStepId,
        string prerequisiteProcessId, string prerequisiteStepId,
        string dependencyType); // "BLOCKING", "ADVISORY", "CONDITIONAL"

    /// <summary>
    /// Check if all prerequisites for a step are satisfied.
    /// </summary>
    Task<DependencyCheckResult> CheckPrerequisitesAsync(
        string processInstanceId, string stepId);

    /// <summary>
    /// Get the full dependency graph for visualization.
    /// </summary>
    Task<DependencyGraph> GetDependencyGraphAsync(string processDefinitionId);
}
```

**Entity:** `WORKFLOW_DEPENDENCY_GRAPH`
```csharp
public class WORKFLOW_DEPENDENCY_GRAPH : ModelEntityBase
{
    public string DEPENDENCY_ID { get; set; }
    public string DEPENDENT_PROCESS_DEF_ID { get; set; }
    public string DEPENDENT_STEP_ID { get; set; }
    public string PREREQUISITE_PROCESS_DEF_ID { get; set; }
    public string PREREQUISITE_STEP_ID { get; set; }
    public string DEPENDENCY_TYPE { get; set; }  // BLOCKING, ADVISORY, CONDITIONAL
    public string CONDITION_EXPRESSION { get; set; } // For CONDITIONAL type
}
```

---

### P3-16 & P3-17: Business Event Triggers

**File:** `Beep.OilandGas.WorkflowManagement\Services\BusinessEventTriggerService.cs` (NEW)

```csharp
public interface IBusinessEventTriggerService
{
    /// <summary>
    /// Register a trigger: when entity X is created/updated with status Y, start workflow Z.
    /// </summary>
    Task RegisterTriggerAsync(BusinessEventTrigger trigger, string userId);

    /// <summary>
    /// Called by domain services when a business event occurs.
    /// </summary>
    Task OnBusinessEventAsync(BusinessEvent eventData);

    /// <summary>
    /// Evaluate all triggers against an event and start matching workflows.
    /// </summary>
    Task<List<string>> EvaluateAndFireAsync(BusinessEvent eventData);
}

public class BusinessEvent
{
    public string EventType { get; set; }        // "ENTITY_CREATED", "ENTITY_UPDATED", "STATUS_CHANGED"
    public string EntityType { get; set; }       // "PDEN_VOL_SUMMARY", "AFE", "HSE_INCIDENT"
    public string EntityId { get; set; }
    public string FieldId { get; set; }
    public Dictionary<string, object> ChangedFields { get; set; }
    public string PreviousStatus { get; set; }
    public string NewStatus { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
}
```

**Entity:** `BUSINESS_EVENT_TRIGGER`
```csharp
public class BUSINESS_EVENT_TRIGGER : ModelEntityBase
{
    public string TRIGGER_ID { get; set; }
    public string TRIGGER_NAME { get; set; }
    public string EVENT_TYPE { get; set; }          // "ENTITY_CREATED", "STATUS_CHANGED"
    public string ENTITY_TYPE { get; set; }         // "PDEN_VOL_SUMMARY"
    public string CONDITION_EXPRESSION { get; set; } // "NewStatus == 'POSTED'"
    public string TARGET_PROCESS_DEF_ID { get; set; } // Which workflow to start
    public bool IS_ACTIVE { get; set; }
    public int PRIORITY { get; set; }
}
```

**Integration points (Phase 3.17):**
- `ProductionAggregationService` → after posting volumes, fires `BusinessEvent` with `EntityType=PDEN_VOL_SUMMARY`
- `HseIncidentService` → after incident created, fires `BusinessEvent` with `EntityType=HSE_INCIDENT`
- `AccountingService` → after AFE status changes, fires `BusinessEvent`

---

### P3-18: Seed All Cross-Role Definitions + Event Triggers

**In `LifeCycleSeedService` (or `LifeCycleModule.SeedAsync`):**
1. Seed 25 cross-role process definitions (adds to existing 80)
2. Seed default handoff contracts for each cross-role step
3. Seed business event triggers (e.g., PDEN_VOL_SUMMARY posted → CRW-01)
4. Seed workflow dependencies (e.g., CRW-02 AFE Cost Tracking depends on CRW-01 Revenue Recognition)

---

## Phase 3 Completion Checklist

- [ ] New `Beep.OilandGas.WorkflowManagement` project created with proper references
- [ ] `WorkflowManagementModule` extending `ModuleSetupBase`, Order=55, auto-discovered
- [ ] All 4 new entities registered in `EntityTypes`
- [ ] 25 cross-role process definitions seeded and verified
- [ ] Handoff contracts validate required data before cross-role transitions
- [ ] CrossPersonaTaskRouter correctly routes tasks to personas
- [ ] AFE→Cost→Journal→Revenue chain executes end-to-end
- [ ] Production→Revenue→Royalty chain executes end-to-end
- [ ] Incident→Investigation→CorrectiveAction chain executes end-to-end
- [ ] Workflow dependencies block step execution until prerequisites met
- [ ] Business events auto-trigger workflows
- [ ] All new services follow existing DI factory pattern

## Phase 3 Deliverables

| # | File | Action |
|---|------|--------|
| 1 | `LifeCycle\Data\Tables\ROLE_HANDOFF_CONTRACT.cs` | CREATE |
| 2 | `LifeCycle\Data\Tables\WORKFLOW_DEPENDENCY_GRAPH.cs` | CREATE |
| 3 | `LifeCycle\Data\Tables\BUSINESS_EVENT_TRIGGER.cs` | CREATE |
| 4 | `LifeCycle\Data\Tables\CROSS_PERSONA_TASK.cs` | CREATE |
| 5 | `LifeCycle\Services\Processes\HandoffValidationService.cs` | CREATE |
| 6 | `LifeCycle\Services\Processes\CrossPersonaTaskRouter.cs` | CREATE |
| 7 | `LifeCycle\Services\Processes\MultiEntityWorkflowOrchestrator.cs` | CREATE |
| 8 | `LifeCycle\Services\Processes\WorkflowDependencyGraphService.cs` | CREATE |
| 9 | `LifeCycle\Services\Processes\BusinessEventTriggerService.cs` | CREATE |
| 10 | `LifeCycle\Definitions\CrossRoleWorkflowSeed.Finance.cs` | CREATE |
| 11 | `LifeCycle\Definitions\CrossRoleWorkflowSeed.Operations.cs` | CREATE |
| 12 | `LifeCycle\Definitions\CrossRoleWorkflowSeed.HSECompliance.cs` | CREATE |
| 13 | `LifeCycle\Definitions\CrossRoleWorkflowSeed.AssetLifecycle.cs` | CREATE |
| 14 | `LifeCycle\Modules\LifeCycleModule.cs` | MODIFY (register entities + seed calls) |
| 15 | `LifeCycle\Services\Processes\ProcessDefinitionInitializer.cs` | MODIFY (wire cross-role seeds) |
| 16 | `Web\Services\NavigationPolicyService.cs` | MODIFY (cross-role task routes) |

---

*Previous: [Phase 2 — Workflow Engine Enhancement](phase-2-workflow-engine.md)*
*Next: [Phase 4 — Governance & Compliance](phase-4-governance-compliance.md)*
