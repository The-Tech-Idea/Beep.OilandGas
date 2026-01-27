# Lifecycle Process Management Guide

## Overview

Provide workflow orchestration for all lifecycle phases with step execution, approvals, and history tracking.

## Architecture

Two-level status tracking:
- Entity status in PPDM tables (Well, Field, Reservoir, etc.).
- Process status in application tables (process definitions, instances, steps, history).

## Quick start

- Initialize default process definitions with ProcessDefinitionInitializer.
- Start a phase process (ExplorationProcessService, DevelopmentProcessService, ProductionProcessService, DecommissioningProcessService).
- Execute steps and approvals through phase process services.
- Transition entity states via WellLifecycleService, FieldLifecycleService, ReservoirLifecycleService.

## Available processes

Exploration:
- Lead to Prospect
- Prospect to Discovery
- Discovery to Development

Development:
- Pool Definition
- Facility Development
- Well Development
- Pipeline Development

Production:
- Well Production Startup
- Production Operations
- Decline Management
- Workover

Decommissioning:
- Well Abandonment
- Facility Decommissioning

## Entity lifecycle states

- Well: PLANNED -> DRILLING -> COMPLETED -> PRODUCING -> WORKOVER -> SUSPENDED -> ABANDONED
- Field: EXPLORATION -> DEVELOPMENT -> PRODUCTION -> DECLINE -> DECOMMISSIONING -> DECOMMISSIONED
- Reservoir: DISCOVERED -> APPRAISED -> DEVELOPED -> PRODUCING -> DEPLETED -> ABANDONED

## Database setup

- PPDM scripts: Beep.OilandGas.PPDM39/Scripts/Sqlserver/ (FIELD_PHASE, RESERVOIR_STATUS, ABANDONMENT_STATUS, DECOMMISSIONING_STATUS).
- Process workflow tables: Beep.OilandGas.LifeCycle/Scripts/ProcessWorkflowTables.sql.

## Service registration

Register in DI:
- IProcessService, PPDMProcessService, phase process services.
- WellLifecycleService, FieldLifecycleService, ReservoirLifecycleService.

## Helper classes

- ProcessServiceExtensions: DTO conversion and execution helpers.
- ProcessIntegrationHelper: coordinate process execution with entity state transitions.

## Status tracking

- Keep process instance state and entity status synchronized.
- Update history when steps or transitions complete.
