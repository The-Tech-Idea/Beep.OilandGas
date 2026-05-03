# Phase 2: Lifecycle Orchestration

## Goal
Keep field-scope enforcement centralized in lifecycle/orchestrator services.

## Target Files
- `Beep.OilandGas.LifeCycle/Services/HSE/PPDMHSEService.cs`
- `Beep.OilandGas.LifeCycle/Services/FieldOrchestrator.cs`
- `Beep.OilandGas.LifeCycle/Core/Interfaces/IFieldOrchestrator.cs`

## TODO Checklist
- [x] Confirm `PPDMHSEService` enforces active field ownership on mutating operations.
- [x] Confirm `FieldOrchestrator` resets `_hseService` when active field changes.
- [x] Confirm shared dependency and connection policy.
- [x] Confirm API does not bypass orchestrator for field-scoped HSE calls.

## Verification
- `dotnet build Beep.OilandGas.LifeCycle/Beep.OilandGas.LifeCycle.csproj`
