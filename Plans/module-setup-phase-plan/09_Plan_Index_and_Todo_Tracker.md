# Process Projects Plan Index and Tracker

## Project documents
- [00_Master_Phased_Plan.md](00_Master_Phased_Plan.md)
- [01_ProspectIdentification_Plan.md](01_ProspectIdentification_Plan.md)
- [02_DrillingAndConstruction_Plan.md](02_DrillingAndConstruction_Plan.md)
- [03_DevelopmentPlanning_Plan.md](03_DevelopmentPlanning_Plan.md)
- [04_ProductionAccounting_Plan.md](04_ProductionAccounting_Plan.md)
- [05_EconomicAnalysis_Plan.md](05_EconomicAnalysis_Plan.md)
- [06_HSE_Plan.md](06_HSE_Plan.md)
- [07_Decommissioning_Plan.md](07_Decommissioning_Plan.md)
- [08_PermitsAndApplications_Plan.md](08_PermitsAndApplications_Plan.md)
- [10_Project_By_Project_Table_Audit.md](10_Project_By_Project_Table_Audit.md)

## Global todo tracker
- [x] Remove WELL and WELL_STATUS from feature modules.
- [x] Remove feature-module PPDM39.Models table registrations.
- [x] Approve module ownership split for DevelopmentPlanning vs DrillingExecution.
- [x] Mark DrillingAndConstruction DevelopmentModule metadata as legacy execution ownership.
- [x] Implement DevelopmentPlanning ModuleSetupBase module.
- [x] Implement Decommissioning ModuleSetupBase module.
- [x] Implement Permits ModuleSetupBase module.
- [x] Implement LeaseAcquisition ModuleSetupBase module.
- [x] Populate local EntityTypes in each feature module from project-owned table classes (project-specific storage paths allowed, not only `Data/Tables`).
- [x] Validate table-class inventory project-by-project and document ownership matrix.
- [x] Refactor seeds to local ownership only (or explicit skip reason when no seed exists).
- [x] Run process-project build verification matrix end-to-end — **DONE 2025-04-25**: `dotnet build Beep.OilandGas.sln -v q` → **0 Error(s)  0 Warning(s)**. Fixed RunTicketController (BSW_VOLUME/BSW_PERCENTAGE), ProductionOperationsController (ROW_ID), Program.cs (ProductionAccountingModuleSetup class name), created Drawing/WellLogLayer.cs and DrawingSampleGallery scene.
- [x] Run setup orchestration tests and capture evidence — **DONE 2025-04-25**: `ModuleSetupOrchestratorTests.cs` added to `Beep.OilandGas.ApiService.Tests`; **16/16 tests passed** (0 failures). Coverage: execution order ASC, ModuleId tie-break, deduplication, fault isolation, abort signal, cancellation, idempotency, `RunSeedForModulesAsync` filtering/empty/no-match, aggregate counters.
- [x] Add CI guard for forbidden PPDM39.Models usage in feature modules — **DONE 2025-04-25**: `Scripts/ci-guard-module-ownership.ps1` created; scanned 71 module files — **[PASS] 0 violations**. GitHub Actions workflow added at `.github/workflows/ci.yml` (build → tests → guard jobs).

## Notes
- Full project-by-project audit now documented in [10_Project_By_Project_Table_Audit.md](10_Project_By_Project_Table_Audit.md).
- Next-wave projects (`ProductionForecasting`, `ProductionOperations`, `LifeCycle`, `EnhancedRecovery`) currently have no local persisted table classes and therefore no ModuleSetupBase changes are planned until table ownership appears.
