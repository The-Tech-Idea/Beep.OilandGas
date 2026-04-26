# Phase 04 - Wizard Orchestration And UX Unification

## Objective

Collapse the old and new setup entry points into one canonical setup workflow that orchestrates connection readiness, migration planning, execution, seed, and optional demo data steps over typed service calls.

## Current Gaps

- there are multiple wizard shapes in the web layer with overlapping setup responsibilities
- progress handling is split across create, copy, seed, and legacy setup flows
- compatibility pages still represent historical setup paths rather than intentional user journeys
- the current setup pages are better aligned than before, but they still need one product-level workflow definition

## Best-Practice Drivers

- `mudblazor` skill: stepper-based admin flows should stay typed, state-driven, and component-safe
- `beepserviceregistration` skill: wizard orchestration should not compensate for startup misconfiguration
- `migration` skill: the wizard should review and execute plan artifacts, not hide them

## Target UX Flow

1. Environment and driver readiness
2. Connection definition and validation
3. Schema privilege and provider capability checks
4. Migration plan review
5. Execution with progress and checkpointing
6. Reference data and LOV seeding
7. Optional dummy or demo data generation
8. Setup completion, current connection activation, and next-step navigation

## Implementation Scope

### 1. Choose the canonical wizard

Adopt one primary route and one primary component tree for setup, with any older wizard kept as a compatibility redirect or reduced shell.

### 2. Replace implicit execution with reviewed actions

The wizard should surface:

- connection validation state
- provider capabilities
- migration summary and operation counts
- warnings and blocked policy decisions
- progress and checkpoint state
- post-run artifacts and summary

### 3. Standardize progress and restart behavior

All long-running setup flows should share:

- one operation ID pattern
- progress tracking and reconnect behavior
- restart and resume messaging
- deterministic success and failure summaries

### 4. Keep the page boundary thin

The wizard pages should orchestrate `IDataManagementService` and typed setup contracts, not build anonymous payloads or own route logic.

## Deliverables

- canonical setup wizard route and component map
- progress and resume interaction model
- compatibility plan for older setup entry points
- UX copy and state model for blocked, warning, running, resumable, and completed stages

## Validation And Exit Criteria

- one setup wizard is product-primary
- long-running operations support reconnect and resume semantics
- users can review migration impact before execution
- no Razor page owns setup route strings or setup-specific anonymous payloads

## Dependencies

- `IDataManagementService`
- progress tracking client and API support
- migration orchestrator from Phase 03

## Target Files

- `Beep.OilandGas.Web/Pages/PPDM39/CreateDatabaseWizard.razor`
- `Beep.OilandGas.Web/Pages/PPDM39/CreateDatabaseWizard.razor.cs` (if present)
- `Beep.OilandGas.Web/Pages/PPDM39/DatabaseManagement.razor`
- `Beep.OilandGas.Web/Services/DataManagementService.cs`

## Execution Checklist

- [x] Declare one canonical setup wizard route and component tree.
- [x] Map all legacy setup pages/routes to compatibility redirects.
- [x] Ensure wizard consumes typed contracts only (no anonymous payload builders).
- [x] Surface migration plan summary, policy blocks, and approval state before execute.
- [x] Standardize operation ID, progress polling, reconnect behavior, and resume actions.
- [x] Publish compatibility route retirement timeline in this phase deliverable.

## UX State Contract

- [x] Required states: ready, warning, blocked, running, resumable, completed, failed.
- [x] Each state must map to explicit backend status codes and typed payload fields.
- [x] Failure state must include next-safe-action guidance (retry/resume/rollback review).

## Acceptance Criteria

- One product-primary wizard path is active.
- Users can review migration impact and policy status before execution.
- Long-running setup operations survive browser reconnect and can resume.
- Compatibility routes do not bypass canonical orchestration services.