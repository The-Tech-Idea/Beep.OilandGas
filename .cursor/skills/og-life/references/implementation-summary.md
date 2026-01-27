# Implementation Summary

## Architecture

- Process workflows are stored in application tables and managed by PPDMProcessService.
- Entity lifecycle states are tracked in PPDM tables and managed by lifecycle services.
- Phase process services orchestrate workflow steps and coordinate with lifecycle services.

## Key components

Models:
- Beep.OilandGas.LifeCycle/Models/Processes/ProcessDefinition.cs
- Beep.OilandGas.LifeCycle/Models/Processes/ProcessStepDefinition.cs
- Beep.OilandGas.LifeCycle/Models/Processes/ProcessInstance.cs
- Beep.OilandGas.LifeCycle/Models/Processes/ProcessStepInstance.cs
- Beep.OilandGas.LifeCycle/Models/Processes/ProcessState.cs
- Beep.OilandGas.LifeCycle/Models/Processes/ProcessTransition.cs
- Beep.OilandGas.LifeCycle/Models/Processes/ProcessHistory.cs

Services:
- Beep.OilandGas.LifeCycle/Services/Processes/IProcessService.cs
- Beep.OilandGas.LifeCycle/Services/Processes/ProcessServiceBase.cs
- Beep.OilandGas.LifeCycle/Services/Processes/PPDMProcessService.cs
- Beep.OilandGas.LifeCycle/Services/Processes/ProcessStateMachine.cs
- Beep.OilandGas.LifeCycle/Services/Processes/ProcessValidator.cs
- Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.cs
- Beep.OilandGas.LifeCycle/Services/Processes/ProcessServiceExtensions.cs
- Beep.OilandGas.LifeCycle/Services/Processes/ProcessIntegrationHelper.cs
- Beep.OilandGas.LifeCycle/Services/Exploration/Processes/ExplorationProcessService.cs
- Beep.OilandGas.LifeCycle/Services/Development/Processes/DevelopmentProcessService.cs
- Beep.OilandGas.LifeCycle/Services/Production/Processes/ProductionProcessService.cs
- Beep.OilandGas.LifeCycle/Services/Decommissioning/Processes/DecommissioningProcessService.cs
- Beep.OilandGas.LifeCycle/Services/WellLifecycle/WellLifecycleService.cs
- Beep.OilandGas.LifeCycle/Services/FieldLifecycle/FieldLifecycleService.cs
- Beep.OilandGas.LifeCycle/Services/ReservoirLifecycle/ReservoirLifecycleService.cs

SQL scripts:
- Beep.OilandGas.LifeCycle/Scripts/ProcessWorkflowTables.sql
- Beep.OilandGas.PPDM39/Scripts/Sqlserver/*_STATUS_*.sql and FIELD_PHASE_*.sql

## Usage patterns

- Start a process via the phase process service.
- Execute steps and approvals through that service.
- Transition entity state via lifecycle services.
- Use ProcessIntegrationHelper when a process step must update entity state.
