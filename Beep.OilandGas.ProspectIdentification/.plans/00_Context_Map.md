# Prospect Identification Context Map

## Exploration Business Foundation

Prospect identification should start from the real exploration business process, not from the current class inventory.

### Well-known exploration practices

- Exploration is a funnel, not a flat CRUD surface: many leads, fewer prospects, and very few drill candidates.
- Prospect maturation is evidence-based: seismic interpretation, nearby well calibration, analog review, risk assessment, and volumetric estimation should support each stage advance.
- Decisions are stage-gated: screening, ranking, budget or AFE approval, drill or no-drill, and post-well outcome capture.
- Portfolio discipline matters: prospects are ranked by chance of success, recoverable volume, economics, and strategic fit, not by a single status field.
- Outcomes must close the loop: a drilled prospect becomes a discovery or appraisal candidate, a dry hole, or a monitored opportunity with lessons captured.

### Canonical exploration process flow

1. Lead identified and screened.
2. Prospect created and matured.
3. Evidence assembled from seismic, wells, and analogs.
4. Risk and volume estimated.
5. Portfolio or gate review ranks the opportunity.
6. Well program and AFE prepared for drill-candidate prospects.
7. Management approval decides drill, defer, monitor, or abandon.
8. Well result is captured and handed off to discovery/appraisal or archived as a non-commercial outcome.

### Data-model families we actually need

- Core business objects:
	- `PROSPECT`
	- `LEAD` when the lead funnel is part of the active workflow
	- `PLAY` only if play inventory is truly part of active exploration screening
- Evidence and technical support:
	- seismic survey and line data
	- well and well-status evidence
	- analog, risk, and volume support objects only where they back a real workflow step
- Workflow and decision state:
	- `PROSPECT_WORKFLOW_STAGE`
	- ranking, comparison, report, and approval models as transient process-owned shapes
- Handoff and outcome capture:
	- discovery linkage
	- prospect-to-well linkage
	- result capture for drill, defer, monitor, or abandon decisions

### Planning implication

- The plan should keep a very small core data set.
- Everything else should be justified by a named exploration workflow or decision gate.
- Geological or detail-heavy classes without an active workflow or endpoint should stay deferred.

## Files to Modify

| File | Purpose | Change Needed |
|------|---------|---------------|
| `Beep.OilandGas.ProspectIdentification/Modules/ExplorationModule.cs` | Project-owned exploration module registration | Keep only prospect-identification core PPDM tables and shared PPDM evidence tables in the module entity list. |
| `Beep.OilandGas.ProspectIdentification/Services/ProspectIdentificationService.cs` | Live API-facing prospect CRUD/evaluation service | Keep repository access PPDM-first and map prospect field/status/resource columns consistently. |
| `Beep.OilandGas.ProspectIdentification/.plans/*.md` | Prospect-identification planning bundle | Document the module, data, service, and workflow boundaries with a tracker. |

## Current Inventory Snapshot

- `Beep.OilandGas.ProspectIdentification` currently contains 95 C# files.
- `Data/ProspectIdentification` currently contains 45 C# files, including the `Domain` subfolder.
- `Data/Evaluation` currently contains 30 C# files.
- `Services` currently contains 6 C# files.
- Canonical `PROSPECT` table ownership belongs to the ProspectIdentification project:
	- canonical project shape: `Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT`
	- duplicate shared package shape still exists in `Beep.OilandGas.PPDM.Models` and should be retired after metadata-impact confirmation

## Dependencies

| File | Relationship |
|------|--------------|
| `Beep.OilandGas.ApiService/Program.cs` | Registers the live `Beep.OilandGas.Models.Core.Interfaces.IProspectIdentificationService` and the project-owned `ExplorationModule`. |
| `Beep.OilandGas.ApiService/Controllers/Operations/ProspectIdentificationController.cs` | Current API consumer of the live prospect service surface. |
| `Beep.OilandGas.LifeCycle/Services/Exploration/PPDMExplorationService.cs` | Owning field-scoped exploration service pattern to follow. |
| `Beep.OilandGas.LifeCycle/Services/Exploration/Processes/ExplorationProcessService.cs` | Orchestrates lead-to-prospect and prospect-to-discovery processes. |
| `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.cs` | Seeds the canonical exploration process definitions. |
| `Beep.OilandGas.LifeCycle/Services/Processes/ProcessDefinitionInitializer.WorkOrders.cs` | Seeds the exploration gate review workflow. |

## Test Files

| Test | Coverage |
|------|----------|
| None found | Prospect-identification service and controller flows currently have no focused automated tests in the repo. |

## Reference Patterns

| File | Pattern |
|------|---------|
| `Beep.OilandGas.LifeCycle/Services/HSE/PPDMHSEService.cs` | Field-scoped PPDM wrapper over a domain service family. |
| `Beep.OilandGas.ProductionAccounting/Modules/ProductionModule.cs` | Project-owned `IModuleSetup` with a small, explicit entity list. |
| `Beep.OilandGas.HSE/.plans/00_HSE_PPDM_Overview.md` | Planning-sheet structure for a PPDM-first domain refactor. |

## Risk Assessment

- [x] Breaking changes to live API contract avoided by keeping `Beep.OilandGas.Models.Core.Interfaces.IProspectIdentificationService` unchanged.
- [x] Database migration scope narrowed to exploration-domain tables only; `POOL*` remains in development/reservoir ownership.
- [ ] Local PPDM-style model volume is high, and `PROSPECT` is now a confirmed duplicate ownership case between the project and the shared PPDM package.
- [ ] Seismic and evaluation services still need conversion off direct `UnitOfWork` + `FIELD`/`SEIS_SET` assumptions.
- [ ] Prospect-identification test coverage still needs to be added.