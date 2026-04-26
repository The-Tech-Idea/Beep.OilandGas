# Phase 2: Lifecycle And Process Integration

## Goal

Bring HSE into the field lifecycle boundary so API and web layers use the same active-field orchestration model as the other phases.

## Work Items

- Add a field-scoped HSE lifecycle interface and implementation.
- Expose the HSE service through `IFieldOrchestrator` and `FieldOrchestrator`.
- Reset the HSE lifecycle service when the active field changes.
- Normalize all incident creation and KPI queries to the active field in lifecycle code.
- Preserve and use the existing default HSE process definitions seeded by `ProcessDefinitionInitializer`.
- Keep process-instance identity aligned with lifecycle/domain identity by binding HSE workflow instances to the real PPDM incident id and resolving process instances by entity id, not by field id or synthetic incident GUIDs.

## Process Coverage

The existing process initializer already seeds these workflows and they should remain the source of process truth:

- `HSE_INCIDENT_REPORTING`
- `HSE_ENVIRONMENTAL_INCIDENT`
- `HSE_HAZOP`
- compliance workflows initialized in `InitializeComplianceWorkflowsAsync`

## Required Outcome

- Controllers should no longer need to trust a caller-provided field id for HSE field-scoped operations.
- Lifecycle code should become the single composition point for PPDM-backed HSE data plus process flow.
- Process-controller actions that represent real HSE domain events should flow through `IFieldHSEService` before or alongside workflow history updates so PPDM state and process state do not drift.

## Verification

- Build `Beep.OilandGas.LifeCycle`.
- Build `Beep.OilandGas.ApiService` after controller rewiring.