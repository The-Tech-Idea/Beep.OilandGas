# Full project-by-project table ownership audit

Date: 2026-04-24
Scope: ModuleSetupBase ownership and EntityTypes coverage by project.

## Rules used for this audit
- A project should register EntityTypes only for project-owned persisted table classes.
- Persisted table classes are identified by project-owned model classes intended for schema creation (commonly SCREAMING_SNAKE_CASE table classes).
- Projections/contracts are excluded even when they inherit ModelEntityBase.
- Feature modules must not register PPDM39 shared table classes from `Beep.OilandGas.PPDM39.Models`.

## Context map

### Files to modify
| File | Purpose | Changes needed |
|------|---------|----------------|
| Plans/module-setup-phase-plan/00_Master_Phased_Plan.md | Master phase status | Mark completed phases and add audit summary |
| Plans/module-setup-phase-plan/09_Plan_Index_and_Todo_Tracker.md | Tracker/index | Reflect completed module work and known blockers |
| Plans/module-setup-phase-plan/10_Project_By_Project_Table_Audit.md | Full audit report | Add project-by-project ownership matrix |

### Dependencies (may need updates)
| File | Relationship |
|------|--------------|
| Beep.OilandGas.ApiService/Program.cs | Registers module implementations as IModuleSetup |
| Beep.OilandGas.* /Modules/*.cs | Source of current module ownership + EntityTypes |

### Test/build evidence
| Item | Coverage |
|------|----------|
| dotnet build for Decommissioning/Permits/LeaseAcquisition | Completed in session (no reported errors) |
| dotnet build ApiService | Currently failing due to LifeCycle `PPDM39.Models.PROSPECT` reference |

### Reference patterns
| File | Pattern |
|------|---------|
| Beep.OilandGas.ProspectIdentification/Modules/ExplorationModule.cs | Module with populated local EntityTypes |
| Beep.OilandGas.ProductionAccounting/Modules/ProductionModule.cs | Module with large local table ownership list |
| Beep.OilandGas.EconomicAnalysis/Modules/EconomicsModule.cs | Intentional empty EntityTypes (projections-only project) |

### Risk assessment
- [ ] Breaking changes to public API
- [x] Database migrations needed (by design, from module-driven schema creation)
- [ ] Configuration changes required

## Project-by-project findings

| Project | ModuleSetupBase status | Local persisted tables found | Current plan decision |
|--------|-------------------------|------------------------------|-----------------------|
| Beep.OilandGas.UserManagement | SecurityModule implemented | Yes (security/user scope/profile tables used by module) | Keep module; ownership is valid |
| Beep.OilandGas.LeaseAcquisition | LeaseAcquisitionModule implemented | 4 (`LEASE_ACQUISITION`, `FEE_MINERAL_LEASE`, `GOVERNMENT_LEASE`, `NET_PROFIT_LEASE`) | Complete for current scope |
| Beep.OilandGas.ProspectIdentification | ExplorationModule implemented | 26 core/local exploration table classes | Complete for current scope |
| Beep.OilandGas.DrillingAndConstruction | DevelopmentModule (legacy execution naming) implemented | None currently in project | Keep `DRILLING_EXECUTION` metadata and empty EntityTypes until local tables exist |
| Beep.OilandGas.DevelopmentPlanning | DevelopmentPlanningModule implemented | 1 (`DEVELOPMENT_COSTS`) | Complete for current scope |
| Beep.OilandGas.ProductionAccounting | ProductionModule implemented | Large owned table set in `Data/ProductionAccounting` (module registers 106) | Complete for current scope |
| Beep.OilandGas.HSE | HseModule implemented | None currently in project | Keep empty EntityTypes with explicit rationale |
| Beep.OilandGas.EconomicAnalysis | EconomicsModule implemented | None currently in project (`Data/Projections` only) | Keep empty EntityTypes with explicit rationale |
| Beep.OilandGas.Decommissioning | DecommissioningModule implemented | 2 (`DECOMMISSIONING_STATUS`, `ABANDONMENT_STATUS`) | Complete for current scope |
| Beep.OilandGas.PermitsAndApplications | PermitsModule implemented | 17 permit table classes under `Data/PermitsAndApplications/Tables` | Complete for current scope |
| Beep.OilandGas.ProductionForecasting | No module | None found | Do not add module yet; re-evaluate when local tables are introduced |
| Beep.OilandGas.ProductionOperations | No module | None found | Do not add module yet; re-evaluate when local tables are introduced |
| Beep.OilandGas.LifeCycle | No module | None found | Do not add module yet; orchestrator/service layer only at present |
| Beep.OilandGas.EnhancedRecovery | No module | None found | Do not add module yet; re-evaluate when local tables are introduced |

## Ownership decisions captured
- Development planning ownership is in `Beep.OilandGas.DevelopmentPlanning`.
- Drilling/construction project remains execution ownership and should not claim planning setup.
- Empty EntityTypes is valid only when a project has no local persisted table classes.
- ProductionAccounting table ownership is valid even though classes are not under a literal `Data/Tables` folder.

## Open work
- Resolve current ApiService/LifeCycle build blocker: `Beep.OilandGas.PPDM39.Models.PROSPECT` unresolved usage in `Beep.OilandGas.LifeCycle/Services/FieldOrchestrator.cs`.
- Run setup orchestration tests after compile is green.
- Add CI scan to reject feature module usage of `Beep.OilandGas.PPDM39.Models` table types.
