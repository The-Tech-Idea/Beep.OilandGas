# Beep.OilandGas.LifeCycle Overview

## Purpose

- Separate lifecycle management from data management.
- Centralize oil field lifecycle logic across exploration, development, production, and decommissioning.

## Core services

- Field orchestration: FieldOrchestrator.
- Phase services: PPDMExplorationService, PPDMDevelopmentService, PPDMProductionService, PPDMDecommissioningService.
- Supporting services: PPDMCalculationService, PPDMAccountingService, PermitManagementService, WellComparisonService.

## Dependencies

- Beep.OilandGas.PPDM39: core interfaces, DTOs, metadata.
- Beep.OilandGas.PPDM39.DataManagement: data access and repositories.
- Calculation libraries: DCA, ProductionForecasting, EconomicAnalysis, NodalAnalysis, WellTestAnalysis, FlashCalculations.
- Operational libraries: GasLift, PipelineAnalysis, CompressorAnalysis, ChokeAnalysis, SuckerRodPumping.
- Accounting and permits: Accounting, PermitsAndApplications.

## Integration pattern

- Retrieve data from PPDM tables.
- Execute calculations or lifecycle operations.
- Store results in PPDM tables.
- Log errors and provide consistent exception messages.

## Migration notes

- Services moved from Beep.OilandGas.PPDM39.DataManagement.Services to Beep.OilandGas.LifeCycle.Services.
- Interfaces remain in Beep.OilandGas.PPDM39.Core.DTOs and Beep.OilandGas.PPDM39.Core.Interfaces.
