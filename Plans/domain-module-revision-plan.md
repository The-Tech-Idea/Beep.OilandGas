# Domain Module Revision Plan

> **Status:** Planning | **Created:** 2026-07-03 | **Based on:** 26 domain modules surveyed
> **Standards:** [standards.md](standards.md) | **Completed:** [accounting-revision-master-plan.md](accounting-revision-master-plan.md)

---

## Module Survey Results

| Module | Services | Has Module | Interfaces | CS Errors | Priority |
|--------|----------|-----------|------------|-----------|----------|
| `ChokeAnalysis` | 3 | ✅ | 0 | Pre-existing | P2 |
| `CompressorAnalysis` | 3 | ✅ | 1 | Pre-existing | P2 |
| `Decommissioning` | 2 | ✅ | 1 | Pre-existing | P2 |
| `DevelopmentPlanning` | 3 | ✅ | 2 | Pre-existing | P2 |
| `DrillingAndConstruction` | 2 | ✅ | 0 | Pre-existing | P2 |
| `EconomicAnalysis` | 2 | ✅ | 2 | Pre-existing | P2 |
| `EnhancedRecovery` | 4 | ✅ | 1 | Pre-existing | P2 |
| `FlashCalculations` | 3 | ✅ | 0 | Pre-existing | P3 |
| `GasLift` | 3 | ✅ | 0 | Pre-existing | P3 |
| `GasProperties` | 2 | ❌ | 0 | Pre-existing | P3 |
| `HSE` | 0 | ✅ | 0 | Pre-existing | P2 |
| `HeatMap` | 1 | ❌ | 0 | Pre-existing | P3 |
| `HydraulicPumps` | 4 | ❌ | 1 | Pre-existing | P3 |
| `LeaseAcquisition` | 11 | ✅ | 4 | Pre-existing | P1 |
| `NodalAnalysis` | 4 | ✅ | 0 | Pre-existing | P2 |
| `OilProperties` | 3 | ❌ | 0 | Pre-existing | P3 |
| `PermitsAndApplications` | 8 | ✅ | 6 | Pre-existing | P1 |
| `PipelineAnalysis` | 12 | ❌ | 1 | Pre-existing | P2 |
| `PlungerLift` | 4 | ❌ | 1 | Pre-existing | P3 |
| `ProductionAccounting` | 42 | ✅ | 42 | Pre-existing | P1 |
| `ProductionForecasting` | 9 | ✅ | 1 | Pre-existing | P2 |
| `ProspectIdentification` | 10 | ✅ | 8 | Pre-existing | P1 |
| `PumpPerformance` | 2 | ❌ | 1 | Pre-existing | P3 |
| `SuckerRodPumping` | 1 | ❌ | 0 | Pre-existing | P3 |
| `WellTestAnalysis` | 2 | ❌ | 0 | Pre-existing | P3 |
| `ProductionOperations` | 10 | ✅ | 4 | Pre-existing | P1 |

---

## Phase Overview

| Phase | Scope | Modules | Tasks | Est. Effort |
|-------|-------|---------|-------|-------------|
| **1** | Interface Extraction | LeaseAcquisition, PermitsAndApplications, ProspectIdentification, ProductionOperations | 18 | 1-2 weeks |
| **2** | Standardization | Decommissioning, DevelopmentPlanning, DrillingAndConstruction, EconomicAnalysis, EnhancedRecovery, ChokeAnalysis, CompressorAnalysis, HSE, NodalAnalysis, PipelineAnalysis, ProductionForecasting | 11 | 1 week |
| **3** | Module Setup Completion | GasProperties, HeatMap, OilProperties, FlashCalculations, GasLift, HydraulicPumps, PlungerLift, PumpPerformance, SuckerRodPumping, WellTestAnalysis | 10 | 1 week |
| **4** | ProductionAccounting Audit | ProductionAccounting (largest module — 42 services) | 6 | 1 week |
| **5** | Pre-Existing Error Fixes | All modules with build errors | 15 | 1-2 weeks |

**Total: 60 tasks, 5-7 weeks**

---

## Phase 1 — Interface Extraction (P1 Projects)

These modules have significant service counts but incomplete interfaces:

### ProductionAccounting (42 services, all have interfaces ✅)
No interface work needed — ProductionAccounting already has 42 services with 42 interfaces. Focus on:
- ✅ Interfaces complete — skip
- Audit service-to-interface mapping for any missed
- Check DI registration coverage

### LeaseAcquisition (11 services, 4 interfaces)
| ID | Task | Status |
|----|------|--------|
| DM-01 | Extract `ILeaseAcquisitionService` | [ ] |
| DM-02 | Extract `ILeaseDataService` | [ ] |
| DM-03 | Audit remaining 7 services for interface needs | [ ] |

### PermitsAndApplications (8 services, 6 interfaces)
| ID | Task | Status |
|----|------|--------|
| DM-04 | Extract `IPermitApplicationWorkflowService` | [ ] |
| DM-05 | Extract `IPermitComplianceCheckService` | [ ] |

### ProspectIdentification (10 services, 8 interfaces)
| ID | Task | Status |
|----|------|--------|
| DM-06 | Extract `IProspectEvaluationService` | [ ] |
| DM-07 | Extract `ISeismicAnalysisService` | [ ] |

### ProductionOperations (10 services, 4 interfaces)
| ID | Task | Status |
|----|------|--------|
| DM-08 | Extract `IProductionManagementService` | [ ] |
| DM-09 | Extract `IFacilityManagementService` | [ ] |
| DM-10 | Audit remaining 4 services for interface needs | [ ] |

---

## Phase 2 — Standardization (P2 Projects)

These modules have 0-2 interfaces and need naming/parameter standardization:

### All P2 Modules — Common Tasks

| ID | Task | Status |
|----|------|--------|
| DM-11 | Standardize `cn` → `connectionName` in all P2 services (if any remain) | [ ] |
| DM-12 | Add `CancellationToken` to all async methods | [ ] |
| DM-13 | Add `ILogger<T>?` injection to all services missing it | [ ] |
| DM-14 | Standardize constructor pattern per [standards.md](standards.md) | [ ] |
| DM-15 | Add XML doc comments to all public methods | [ ] |
| DM-16 | Audit all Module.cs files for proper ModuleSetupBase implementation | [ ] |

### Missing Interfaces (0-1 interface modules)

| ID | Service | Module | Status |
|----|---------|--------|--------|
| DM-17 | `ChokeAnalysisService` → `IChokeAnalysisService` | ChokeAnalysis | [ ] |
| DM-18 | `CompressorAnalysisService` → `ICompressorAnalysisService` | CompressorAnalysis | [ ] |
| DM-19 | `DrillingOperationService` → `IDrillingOperationService` | DrillingAndConstruction | [ ] |
| DM-20 | `NodalAnalysisService` → `INodalAnalysisService` | NodalAnalysis | [ ] |
| DM-21 | `EnhancedRecoveryService` → `IEnhancedRecoveryService` | EnhancedRecovery | [ ] |

---

## Phase 3 — Module Setup Completion (P3 Projects)

These modules lack `IModuleSetup` implementations:

| ID | Task | Status |
|----|------|--------|
| DM-22 | Create `GasPropertiesModule : ModuleSetupBase` | [ ] |
| DM-23 | Create `HeatMapModule : ModuleSetupBase` | [ ] |
| DM-24 | Create `OilPropertiesModule : ModuleSetupBase` | [ ] |
| DM-25 | Create `HydraulicPumpsModule : ModuleSetupBase` | [ ] |
| DM-26 | Create `PlungerLiftModule : ModuleSetupBase` | [ ] |
| DM-27 | Create `PumpPerformanceModule : ModuleSetupBase` | [ ] |
| DM-28 | Create `SuckerRodPumpingModule : ModuleSetupBase` | [ ] |
| DM-29 | Create `WellTestAnalysisModule : ModuleSetupBase` | [ ] |
| DM-30 | Create `PipelineAnalysisModule : ModuleSetupBase` | [ ] |
| DM-31 | Register entities in each new module's `EntityTypes` | [ ] |

---

## Phase 4 — ProductionAccounting Audit

ProductionAccounting is the largest and most complete module. Focus on quality:

| ID | Task | Status |
|----|------|--------|
| DM-32 | Audit all 42 services for missing DI registrations | [ ] |
| DM-33 | Verify all 42 interfaces are in correct namespace (Models.Core.Interfaces) | [ ] |
| DM-34 | Check for duplicate services (InventoryLcmService exists in BOTH Accounting + ProductionAccounting) | [ ] |
| DM-35 | Audit Partial class files for ProductionAccountingService (3 partials) | [ ] |
| DM-36 | Verify ModuleSetup seeding covers all entity types | [ ] |
| DM-37 | Cross-check ProductionAccounting interfaces vs Accounting interfaces for overlap | [ ] |

---

## Phase 5 — Pre-Existing Error Fixes

Fix the CS errors that exist across domain modules:

| ID | Task | Module | Status |
|----|------|--------|--------|
| DM-38 | Fix `PlungerLiftAnalysisRequest` nullable conversion errors | PlungerLift | [ ] |
| DM-39 | Fix `PipelineAnalysisService.DataManagement.cs` type resolution | PipelineAnalysis | [ ] |
| DM-40 | Add missing `using Beep.OilandGas.Decommissioning.Constants` | Decommissioning | [x] |
| DM-41 | Add missing `using Beep.OilandGas.EconomicAnalysis.Constants` | EconomicAnalysis | [x] |
| DM-42 | Fix `DevelopmentPlanning` duplicate class `DevelopmentPlanningDefaults` | DevelopmentPlanning | [x] |
| DM-43 | Fix `viscosityAtSC` undeclared variable | GasProperties | [x] |
| DM-44 | Fix `ModuleSetupOrchestrator` namespace in `PPDM39SetupService` | PPDM39.DataManagement | [x] |
| DM-45 | Add missing `using Beep.OilandGas.PPDM39.Core` to all services | All modules | [~] |
| DM-46 | Add `ProjectReference` to PPDM39 where missing | GasProperties, HeatMap, etc. | [~] |
| DM-47 | Fix CS0136/Cs0841 name collision errors from sed | All affected | [~] |
| DM-48 | Remove duplicate `using` directives found in multiple files | All affected | [ ] |
| DM-49 | Add `NoWarn` for pre-existing CS warnings where appropriate | All modules | [ ] |
| DM-50 | Verify all 26 modules compile with 0 CS errors | All modules | [ ] |

---

## Phase 6 — Reference Value Standardization

Apply the same `AccountingReferenceCodes` pattern to all domain modules:

| ID | Task | Status |
|----|------|--------|
| DM-51 | Create `WellReferenceCodes` (well status, well type, wellbore type) | [ ] |
| DM-52 | Create `ProductionReferenceCodes` (production status, fluid type, measurement method) | [ ] |
| DM-53 | Create `ReservoirReferenceCodes` (reservoir type, drive mechanism, EOR method) | [ ] |
| DM-54 | Create `DrillingReferenceCodes` (rig type, drilling method, completion type) | [ ] |
| DM-55 | Create `HseReferenceCodes` (incident type, severity, permit type) | [ ] |
| DM-56 | Create `FacilityReferenceCodes` (facility type, equipment type) | [ ] |
| DM-57 | Create `PipelineReferenceCodes` (pipeline type, material, inspection method) | [ ] |
| DM-58 | Seed all reference codes via respective ModuleSetup classes | [ ] |
| DM-59 | Replace magic strings in all domain services with typed constants | [ ] |
| DM-60 | Document all reference codes in `docs/DOMAIN_REFERENCE_CODES.md` | [ ] |

---

## Priority Order

```
Phase 5 (Error Fixes) → Phase 1 (Interfaces) → Phase 2 (Standardization) 
→ Phase 3 (Module Setup) → Phase 4 (ProductionAccounting Audit) 
→ Phase 6 (Reference Values)
```

---

## Related Documents

- [Coding Standards](standards.md)
- [Accounting Revision Plan](accounting-revision-master-plan.md)
- [Accounting Standards Enhancement Plan](accounting-standards-enhancement-plan.md)
- [BA Integration Guide](../docs/BA_INTEGRATION_ACCOUNTING.md)

---

*Last updated: 2026-07-03*
