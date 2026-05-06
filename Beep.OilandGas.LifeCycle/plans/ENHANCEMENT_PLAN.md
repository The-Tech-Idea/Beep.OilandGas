# Beep.OilandGas.LifeCycle - Enhancement Plan

## Executive Summary

The LifeCycle project is the central orchestration layer for the entire Oil & Gas asset lifecycle -- from lead exploration through decommissioning. It already has a solid foundation: a process/workflow engine with state machines, 12 phase-specific process services, 3 entity lifecycle state machines (Well, Field, Reservoir), integration with 25+ calculation/feature projects, and access control.

**This plan addresses critical gaps** per project conventions (CLAUDE.md): missing `ModuleSetupBase`, missing DI extension method, incomplete workflow engine features (approvals, SLA, transitions), missing UserManagement integration, and incomplete feature project integration.

---

## Phase 1: Foundation & Compliance (Week 1)

### 1.1 Create `LifeCycleModule` (ModuleSetupBase)

**Why:** CLAUDE.md mandates that any project introducing extension tables MUST ship a `ModuleSetupBase` with `EntityTypes` and `SeedAsync`.

**Extension tables to register:**

| Table | Entity Type | Purpose |
|---|---|---|
| `FIELD_PHASE` | `FIELD_PHASE` | Field phase tracking |
| `RESERVOIR_STATUS` | `RESERVOIR_STATUS` | Reservoir status tracking |
| `PROCESS_DEFINITION` | `PROCESS_DEFINITION` | Workflow definitions |
| `PROCESS_INSTANCE` | `PROCESS_INSTANCE` | Active process instances |
| `PROCESS_STEP_INSTANCE` | `PROCESS_STEP_INSTANCE` | Step execution records |
| `PROCESS_HISTORY` | `PROCESS_HISTORY` | Process execution audit trail |
| `PROCESS_APPROVAL` | `PROCESS_APPROVAL` | Approval workflow records |
| `ABANDONMENT_STATUS` | `ABANDONMENT_STATUS` | Abandonment status tracking |
| `DECOMMISSIONING_STATUS` | `DECOMMISSIONING_STATUS` | Decommissioning status tracking |
| `ENVIRONMENTAL_RESTORATION` | `ENVIRONMENTAL_RESTORATION` | Environmental restoration records |
| `DECOMMISSIONING_COST` | `DECOMMISSIONING_COST` | Decommissioning cost estimates |
| `ORGANIZATION_HIERARCHY_CONFIG` | `ORGANIZATION_HIERARCHY_CONFIG` | Org hierarchy configuration |

**File:** `Modules/LifeCycleModule.cs`
- ModuleId: `LIFECYCLE`
- Order: `50` (after SECURITY at 40, before domain modules)
- EntityTypes: all 12 extension tables above
- SeedAsync: delegates to `LifeCycleSeedService`

### 1.2 Create `LifeCycleSeedService`

**Why:** Seed reference data, process definitions, and lifecycle reference codes.

**Seeds:**
1. **Process Definitions** -- all 20+ process definitions from `ProcessDefinitionInitializer` (persisted to DB)
2. **Lifecycle Reference Codes** -- well lifecycle states, field phases, reservoir states
3. **Status Transition Rules** -- valid transitions for each entity type
4. **Default Approval Chains** -- standard approval hierarchies per process type
5. **SLA Templates** -- default SLA configurations per process step

**File:** `Services/Seeding/LifeCycleSeedService.cs`
- Interface: `ILifeCycleSeedService` in `Core/Interfaces/`
- Result: `LifeCycleSeedResult` with counts per entity type

### 1.3 Create `DependencyInjection/LifeCycleServiceCollectionExtensions.cs`

**Why:** Centralized DI registration (pattern used by UserManagement, CompressorAnalysis, etc.)

**Registers:**
- All phase services (Exploration, Development, Production, Decommissioning, etc.)
- All process services (ProcessServiceBase implementations)
- Lifecycle services (WellLifecycleService, FieldLifecycleService, ReservoirLifecycleService)
- Calculation service (PPDMCalculationService)
- Access control services (UserProfileService, UserAssetAccessService, AssetHierarchyService)
- Integration services (DataFlowService, AnalysisResultStorage)
- FieldOrchestrator (scoped, with all dependencies)
- Seed service (ILifeCycleSeedService)

**File:** `DependencyInjection/LifeCycleServiceCollectionExtensions.cs`
```csharp
public static IServiceCollection AddLifeCycleServices(
    this IServiceCollection services,
    IConfiguration configuration)
```

---

## Phase 2: Workflow Engine Enhancements (Week 2)

### 2.1 State Transition Engine

**Current gap:** `ProcessDefinitionInitializer` defines steps but `Transitions` dictionaries are empty.

**Enhancement:**
- Populate state transitions for all 20+ process definitions
- Add transition conditions (guard clauses)
- Add transition actions (side effects)
- Add transition validation (pre/post conditions)

**File:** `Services/Processes/ProcessDefinitionInitializer.Transitions.cs` (new partial)

### 2.2 Approval Workflow Engine

**Current gap:** `PROCESS_APPROVAL` table exists but approval workflow is not fully implemented.

**Enhancement:**
- Multi-level approval chains (sequential, parallel, any-of-N)
- Approval delegation (delegate to another user)
- Approval escalation (auto-escalate after SLA expiry)
- Approval comments and audit trail
- Approval status: PENDING, APPROVED, REJECTED, ESCALATED, DELEGATED

**Files:**
- `Services/Processes/ApprovalWorkflowEngine.cs`
- `Core/Interfaces/IApprovalWorkflowEngine.cs`
- Models: `ApprovalChain`, `ApprovalStep`, `ApprovalRequest`, `ApprovalResponse`

### 2.3 SLA Tracking Engine

**Current gap:** No SLA tracking on process steps.

**Enhancement:**
- SLA configuration per process step type
- SLA timers (start, pause, resume, expire)
- SLA breach notifications
- SLA escalation rules
- SLA reporting (average completion time, breach rate)

**Files:**
- `Services/Processes/SlaTrackingService.cs`
- `Core/Interfaces/ISlaTrackingService.cs`
- Models: `SlaConfiguration`, `SlaTimer`, `SlaBreach`

### 2.4 Workflow Template System

**Enhancement:**
- Reusable workflow templates (create once, instantiate many times)
- Template versioning
- Template inheritance (base template -> specialized variants)
- Template parameterization (customizable steps per instance)

**Files:**
- `Services/Processes/WorkflowTemplateService.cs`
- `Core/Interfaces/IWorkflowTemplateService.cs`
- Models: `WorkflowTemplate`, `WorkflowTemplateVersion`, `WorkflowTemplateParameter`

### 2.5 Business Rule Engine

**Enhancement:**
- Declarative business rules (JSON/YAML-defined)
- Rule types: validation, calculation, state transition guard, notification trigger
- Rule evaluation engine with dependency resolution
- Rule versioning and audit

**Files:**
- `Services/Processes/BusinessRuleEngine.cs`
- `Core/Interfaces/IBusinessRuleEngine.cs`
- Models: `BusinessRule`, `RuleCondition`, `RuleAction`, `RuleEvaluationResult`

---

## Phase 3: UserManagement Integration (Week 3)

### 3.1 Lifecycle-Specific Roles

**Enhancement:** Add lifecycle-specific roles to `DefaultSecuritySeedService` (in UserManagement project):

| Role | Permissions |
|---|---|
| `ExplorationManager` | Exploration.*, ProspectIdentification.*, LeaseAcquisition.*, Seismic.* |
| `DevelopmentEngineer` | DevelopmentPlanning.*, Drilling.*, WellManagement.*, Facilities.* |
| `ProductionEngineer` | Production.*, ProductionForecasting.*, NodalAnalysis.*, WellTestAnalysis.* |
| `ReservoirEngineer` | Reservoir.*, ProductionForecasting.*, EnhancedRecovery.*, FlashCalculations.* |
| `DecommissioningManager` | Decommissioning.*, Environmental.*, Permits.*, HSE.* |
| `HSEManager` | HSE.*, Environmental.*, Regulatory.*, Permits.*, Inspection.* |
| `FacilityManager` | Facilities.*, Pipeline.*, Maintenance.*, Inspection.*, WorkOrder.* |
| `AssetManager` | All lifecycle phases (read), EconomicAnalysis.*, Reporting.*, Dashboard.* |
| `WorkflowApprover` | All `*.Approve` permissions across domains |
| `ProcessAdministrator` | Process definition management, workflow template management, SLA configuration |

### 3.2 Lifecycle Personas

**Enhancement:** Add lifecycle-specific personas to `DefaultSecuritySeedService`:

| Persona Code | Name | Category | Default Route |
|---|---|---|---|
| `EXPLORATION_GEOLOGIST` | Exploration Geologist | Exploration | `/lifecycle/exploration/dashboard` |
| `DEVELOPMENT_PLANNER` | Development Planner | Development | `/lifecycle/development/dashboard` |
| `PRODUCTION_ENGINEER` | Production Engineer | Production | `/lifecycle/production/dashboard` |
| `RESERVOIR_ENGINEER` | Reservoir Engineer | Reservoir | `/lifecycle/reservoir/dashboard` |
| `DECOMMISSIONING_COORDINATOR` | Decommissioning Coordinator | Decommissioning | `/lifecycle/decommissioning/dashboard` |
| `HSE_COORDINATOR` | HSE Coordinator | HSE | `/lifecycle/hse/dashboard` |
| `FACILITY_OPERATOR` | Facility Operator | Facilities | `/lifecycle/facilities/dashboard` |
| `ASSET_MANAGER` | Asset Manager | Management | `/lifecycle/asset/dashboard` |
| `WORKFLOW_ADMINISTRATOR` | Workflow Administrator | Administration | `/lifecycle/workflow/admin` |

### 3.3 Role-Based Workflow Access

**Enhancement:**
- Process definitions linked to required roles (who can start, execute, approve each step)
- Step-level role requirements (only users with specific roles can execute certain steps)
- Dynamic role assignment during workflow execution (temporary role grants)
- Role-based dashboard views (different views per role)

**Files:**
- `Services/Processes/RoleBasedAccessService.cs`
- `Core/Interfaces/IRoleBasedAccessService.cs`

---

## Phase 4: Feature Project Integration (Week 4)

### 4.1 Exploration Phase Integration

**Current gap:** ProspectIdentification, LeaseAcquisition not fully wired.

**Enhancement:**
- Wire `LeadExplorationService` -> `ProspectIdentification` project
- Wire `ProspectIdentification` -> `LeaseAcquisition` project
- Create `ExplorationWorkflowService` that orchestrates: Lead -> Prospect -> Lease -> Exploration Well
- Integrate `SeismicAnalysis` (if exists) or `Drawing` for seismic interpretation

**Workflow:**
```
Lead Identified -> Lead Qualified -> Prospect Defined -> Prospect Screened -> 
Lease Acquired -> Exploration Well Planned -> Exploration Well Drilled -> 
Discovery Appraised -> Discovery Evaluated -> Go/No-Go Decision
```

### 4.2 Development Phase Integration

**Current gap:** DevelopmentPlanning, DrillingAndConstruction not fully integrated.

**Enhancement:**
- Wire `PPDMDevelopmentService` -> `DevelopmentPlanning` project
- Wire `PPDMDevelopmentService` -> `DrillingAndConstruction` project
- Create `DevelopmentWorkflowService` that orchestrates: FDP -> Well Program -> Facility Design -> Pipeline Design -> Construction
- Integrate economic analysis for project sanction

**Workflow:**
```
Go Decision -> Field Development Plan -> Well Program Design -> 
Facility Design -> Pipeline Design -> Economic Sanction -> 
Construction -> Commissioning -> First Oil
```

### 4.3 Production Phase Integration

**Current gap:** ProductionOperations, ProductionForecasting, ProductionAccounting partial.

**Enhancement:**
- Wire `PPDMProductionService` -> `ProductionOperations` project
- Wire `PPDMProductionService` -> `ProductionForecasting` project
- Wire `PPDMProductionService` -> `ProductionAccounting` project
- Create `ProductionWorkflowService` that orchestrates: Well Startup -> Production Optimization -> Decline Management -> Workover
- Integrate all artificial lift services (GasLift, SuckerRodPumping, PlungerLift, HydraulicPumps, PumpPerformance)
- Integrate analysis services (NodalAnalysis, WellTestAnalysis, ChokeAnalysis)

**Workflow:**
```
Well Startup -> Production Allocation -> Performance Monitoring -> 
Optimization Study -> Artificial Lift Design -> Implementation -> 
Decline Analysis -> Workover Planning -> Workover Execution
```

### 4.4 Decommissioning Phase Integration

**Current gap:** Decommissioning project integration is weak.

**Enhancement:**
- Wire `PPDMDecommissioningService` -> `Decommissioning` project
- Create `DecommissioningWorkflowService` that orchestrates: Planning -> Permitting -> Execution -> Restoration -> Certification
- Integrate cost estimation and environmental compliance

**Workflow:**
```
Decommissioning Plan -> Regulatory Approval -> Well Plugging -> 
Facility Removal -> Site Restoration -> Environmental Certification -> 
Final Reporting -> Asset Closure
```

### 4.5 Cross-Cutting Integration

**Enhancement:**
- **Permits:** Wire `PermitManagementService` -> `PermitsAndApplications` project
- **HSE:** Wire `PPDMHSEService` -> full HSE compliance workflow
- **Inspection:** Create inspection templates tied to equipment lifecycle
- **Maintenance:** Auto-create work orders from inspection findings
- **Work Orders:** Trigger from inspection, maintenance, production anomalies

---

## Phase 5: Data Classes & Models (Week 5)

### 5.1 PPDM Table Usage (First Priority)

All existing PPDM tables are already used:
- `FIELD`, `WELL`, `FACILITY`, `POOL`, `PIPELINE`
- `WELL_STATUS`, `FACILITY_STATUS`, `WELL_XREF`
- `WELL_TEST`, `WELL_TUBULAR`, `WELL_ACTIVITY`, `WELL_EQUIPMENT`
- `WELL_ABANDONMENT`, `FACILITY_DECOMMISSIONING`
- `PDEN`, `PDEN_VOL_SUMMARY`, `RESERVE_ENTITY`, `PRODUCTION_FORECAST`
- `APPLICATION`, `APPLIC_AREA`

### 5.2 Extension Table Classes (Create if Missing)

For each extension table in `LifeCycleModule.EntityTypes`, create the table class if it doesn't exist:

**Location:** `Beep.OilandGas.LifeCycle/Data/Tables/`

| Table Class | Columns |
|---|---|
| `FIELD_PHASE.Table.cs` | FIELD_PHASE_ID, FIELD_ID, PHASE, PHASE_START_DATE, PHASE_END_DATE, STATUS, DESCRIPTION |
| `RESERVOIR_STATUS.Table.cs` | RESERVOIR_STATUS_ID, RESERVOIR_ID, STATUS, STATUS_DATE, STATUS_REASON, NOTES |
| `PROCESS_DEFINITION.Table.cs` | PROCESS_DEFINITION_ID, PROCESS_NAME, PROCESS_TYPE, VERSION, DESCRIPTION, IS_ACTIVE |
| `PROCESS_INSTANCE.Table.cs` | PROCESS_INSTANCE_ID, PROCESS_DEFINITION_ID, ENTITY_TYPE, ENTITY_ID, STATUS, STARTED_DATE, COMPLETED_DATE |
| `PROCESS_STEP_INSTANCE.Table.cs` | STEP_INSTANCE_ID, PROCESS_INSTANCE_ID, STEP_ID, STEP_NAME, STATUS, ASSIGNED_TO, STARTED_DATE, COMPLETED_DATE |
| `PROCESS_HISTORY.Table.cs` | PROCESS_HISTORY_ID, PROCESS_INSTANCE_ID, EVENT_TYPE, EVENT_DATE, USER_ID, DETAILS |
| `PROCESS_APPROVAL.Table.cs` | APPROVAL_ID, PROCESS_INSTANCE_ID, STEP_INSTANCE_ID, APPROVER_ID, STATUS, APPROVAL_DATE, COMMENTS |
| `ABANDONMENT_STATUS.Table.cs` | ABANDONMENT_STATUS_ID, WELL_ID, STATUS, STATUS_DATE, PLUGGING_DATE, CERTIFICATION_DATE |
| `DECOMMISSIONING_STATUS.Table.cs` | DECOMMISSIONING_STATUS_ID, FACILITY_ID, STATUS, STATUS_DATE, REMOVAL_DATE, RESTORATION_DATE |
| `ENVIRONMENTAL_RESTORATION.Table.cs` | RESTORATION_ID, SITE_ID, RESTORATION_TYPE, START_DATE, COMPLETION_DATE, STATUS, CERTIFICATION |
| `DECOMMISSIONING_COST.Table.cs` | COST_ID, ENTITY_TYPE, ENTITY_ID, COST_TYPE, ESTIMATED_COST, ACTUAL_COST, CURRENCY, ESTIMATE_DATE |
| `ORGANIZATION_HIERARCHY_CONFIG.Table.cs` | CONFIG_ID, ORGANIZATION_ID, PARENT_ORG_ID, HIERARCHY_LEVEL, HIERARCHY_PATH |

### 5.3 Workflow Models (Projections/DTOs)

**Location:** `Beep.OilandGas.LifeCycle/Data/`

| Model | Purpose |
|---|---|
| `WorkflowDefinition` | Workflow template with steps, transitions, roles |
| `WorkflowInstance` | Running workflow instance with current state |
| `WorkflowStep` | Individual step definition with conditions, SLA, roles |
| `WorkflowTransition` | State transition with guards and actions |
| `ApprovalChain` | Multi-level approval configuration |
| `SlaConfiguration` | SLA rules per step type |
| `BusinessRule` | Declarative business rule |
| `WorkflowEvent` | Event in workflow execution (audit) |

---

## Phase 6: Seed Data (Week 6)

### 6.1 Process Definition Seeds

Seed all 20+ process definitions from `ProcessDefinitionInitializer` into `PROCESS_DEFINITION` table:

| Process | Steps |
|---|---|
| Lead-to-Prospect | Lead Capture -> Geological Review -> Geophysical Analysis -> Prospect Definition -> Screening -> Approval |
| Prospect-to-Discovery | Prospect Ranking -> Well Planning -> Drilling -> Evaluation -> Discovery Declaration |
| Discovery-to-Development | Appraisal Planning -> Appraisal Drilling -> Reservoir Characterization -> FDP -> Sanction |
| Well Development | Well Design -> Drilling Program -> Completion Design -> Execution -> Commissioning |
| Facility Development | FEED -> Detailed Design -> Procurement -> Construction -> Commissioning |
| Pipeline Development | Route Selection -> Design -> Permitting -> Construction -> Commissioning |
| Well Startup | Pre-Startup Check -> Well Testing -> Production Allocation -> Monitoring |
| Production Operations | Daily Monitoring -> Optimization -> Workover Planning -> Execution |
| Decline Management | Decline Analysis -> Forecast Update -> Optimization Study -> Implementation |
| Well Abandonment | Planning -> Regulatory Approval -> Plugging -> Verification -> Certification |
| Facility Decommissioning | Planning -> Permitting -> Removal -> Site Restoration -> Certification |
| HSE Incident Management | Incident Report -> Investigation -> RCA -> Corrective Action -> Closure |
| Work Order Management | Request -> Approval -> Scheduling -> Execution -> Verification -> Closure |
| Maintenance Planning | Inspection -> Condition Assessment -> Work Order Creation -> Execution -> Verification |
| Inspection Management | Schedule -> Execute -> Report -> Findings -> Recommendations -> Follow-up |
| Permit Management | Application -> Review -> Approval -> Issuance -> Compliance -> Closure |

### 6.2 Lifecycle Reference Code Seeds

| Reference Set | Codes |
|---|---|
| `WELL_LIFECYCLE_STATE` | PLANNED, DRILLING, COMPLETING, PRODUCING, WORKOVER, SUSPENDED, ABANDONED |
| `FIELD_LIFECYCLE_PHASE` | EXPLORATION, APPRAISAL, DEVELOPMENT, PRODUCTION, DECLINE, DECOMMISSIONING, DECOMMISSIONED |
| `RESERVOIR_LIFECYCLE_STATE` | DISCOVERED, APPRAISED, DEVELOPED, PRODUCING, DEPLETED, ABANDONED |
| `PROCESS_STATUS` | DRAFT, PENDING, IN_PROGRESS, ON_HOLD, COMPLETED, CANCELLED, REJECTED |
| `STEP_STATUS` | NOT_STARTED, IN_PROGRESS, COMPLETED, SKIPPED, FAILED, PENDING_APPROVAL |
| `APPROVAL_STATUS` | PENDING, APPROVED, REJECTED, ESCALATED, DELEGATED |
| `TRANSITION_CONDITION` | ALWAYS, CONDITIONAL, APPROVAL_REQUIRED, TIME_BASED, EVENT_BASED |

### 6.3 SLA Template Seeds

| Step Type | Default SLA | Escalation |
|---|---|---|---|
| Geological Review | 14 days | 7 days |
| Prospect Screening | 7 days | 3 days |
| Well Planning | 21 days | 14 days |
| Regulatory Approval | 30 days | 15 days |
| Construction | 180 days | 90 days |
| Incident Investigation | 7 days | 3 days |
| Work Order Execution | 14 days | 7 days |

---

## Implementation Order

1. **LifeCycleModule** (ModuleSetupBase) -- foundation for schema creation
2. **Extension table classes** -- define all 12 extension tables
3. **LifeCycleSeedService** -- seed reference data and process definitions
4. **LifeCycleServiceCollectionExtensions** -- centralized DI
5. **Workflow engine enhancements** -- transitions, approvals, SLA, templates, rules
6. **UserManagement integration** -- roles, personas, role-based workflow access
7. **Feature project integration** -- Exploration, Development, Production, Decommissioning
8. **Fix compilation errors** -- double->decimal conversions
9. **Build and verify**

---

## Key Design Principles

1. **PPDM First** -- Always use existing PPDM tables before creating extension tables
2. **ModuleSetupBase** -- All extension tables registered via `EntityTypes`, schema created by Beep tooling (no hand-written SQL)
3. **SeedAsync** -- All reference data seeded idempotently (skip-if-exists)
4. **Factory DI** -- All services registered with factory pattern
5. **FieldOrchestrator** -- Central hub for field-scoped operations
6. **ProcessServiceBase** -- All process services inherit from base
7. **State Machine** -- All lifecycle state transitions go through `ProcessStateMachine`
8. **UserManagement Integration** -- Roles, personas, and permissions from UserManagement project
9. **Audit Trail** -- All workflow events logged to `PROCESS_HISTORY`
10. **SLA Tracking** -- All steps have configurable SLA with escalation
