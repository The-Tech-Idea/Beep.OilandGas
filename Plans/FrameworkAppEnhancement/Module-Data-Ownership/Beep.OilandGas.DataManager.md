# Beep.OilandGas.DataManager — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

0 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

This project consumes changed contracts; no original Data definitions are assigned here. Caller checks remain in scope.

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.DataManager/Beep.OilandGas.DataManager.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

0 candidate incoming files; 64 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.DataManager/Core/Exceptions/DataManagerException.cs`
- `Beep.OilandGas.DataManager/Core/Exceptions/ScriptExecutionException.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/FileSystemModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/AccountingModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/AgreementModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/AnalyticsModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/CalculationsModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/ChokeAnalysisModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/CommonModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/CompressorAnalysisModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/DCAModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/DataManagementModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/DecommissioningModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/DevelopmentPlanningModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/EconomicAnalysisModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/ExportModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/FlashCalculationsModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/GasLiftModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/GasPropertiesModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/HeatMapModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/HydraulicPumpsModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/ImbalanceModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/LeaseModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/LifeCycleModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/MeasurementModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/NodalAnalysisModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/OilPropertiesModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/OwnershipModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/PPDM39ModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/PermitsAndApplicationsModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/PipelineAnalysisModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/PlungerLiftModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/PricingModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/ProductionAccountingModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/ProductionForecastingModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/ProductionOperationsModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/ProspectIdentificationModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/PumpPerformanceModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/RenderingModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/RoyaltyModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/SecurityModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/StorageModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/SuckerRodPumpingModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/TradingModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/UnitizationModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/ValidationModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Implementations/Modules/WellTestAnalysisModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Interfaces/IDataManager.cs`
- `Beep.OilandGas.DataManager/Core/Interfaces/IModuleData.cs`
- `Beep.OilandGas.DataManager/Core/Models/ErrorCheckResult.cs`
- `Beep.OilandGas.DataManager/Core/Models/ExecutionState.cs`
- `Beep.OilandGas.DataManager/Core/Models/ModuleExecutionResult.cs`
- `Beep.OilandGas.DataManager/Core/Models/ModuleScriptInfo.cs`
- `Beep.OilandGas.DataManager/Core/Models/ScriptExecutionOptions.cs`
- `Beep.OilandGas.DataManager/Core/Models/ValidationResult.cs`
- `Beep.OilandGas.DataManager/Core/Registry/ModuleDataRegistry.cs`
- `Beep.OilandGas.DataManager/Core/State/DatabaseExecutionStateStore.cs`
- `Beep.OilandGas.DataManager/Core/State/FileExecutionStateStore.cs`
- `Beep.OilandGas.DataManager/Core/State/IExecutionStateStore.cs`
- `Beep.OilandGas.DataManager/Core/Utilities/DatabaseTypeNormalizer.cs`
- `Beep.OilandGas.DataManager/DependencyInjection/DataManagerServiceCollectionExtensions.cs`
- `Beep.OilandGas.DataManager/Logging/DataManagerLogger.cs`
- `Beep.OilandGas.DataManager/Services/DataManager.cs`
- `Beep.OilandGas.DataManager/Services/ScriptValidator.cs`

## Migration tasks

- [x] Record the confirmed existing owner, destination, namespace and dependency decisions in inventory.json and restoration-progress.json; current destination hashes are in observed evidence.
- [x] Keep the internal project graph acyclic and record shared-contract placements without another global data project. The current static superset audit covers 69 existing projects and 284 literal project-reference edges, with zero cycles; external symbolic references are listed separately in evidence/canonical-dependency-recovery/project-reference-audit.json.
- [ ] Move definitions once, update all listed callers and references, then remove forwarding stubs/obsolete definitions. Preserve source changes from the soft-reset commit.
- [ ] Fix CLR namespaces and ambiguous/duplicate DTO names; retain native table/key names and public JSON member names unless all consumers migrate together. Update explicit JsonPropertyName, EF/Beep mappings, entity registration, migration discovery, serializers and stored assembly-qualified type names where applicable.
- [ ] Update typed HTTP client signatures, controller request/response types, service interfaces, DI registration, Razor bindings, setup/module manifests, tests, source-linked fixtures and build scripts that name the moved source/assembly.
- [ ] Reuse existing log, audit and notification paths; check that changed exception/type names do not hide errors, lose correlation or change UI failure feedback. IdentityServer stays authentication-only and app RBAC remains authoritative.
- [ ] Update relevant docs and project/solution/package references. Remove stale centralized-ownership instructions as part of the authorized implementation.

## Verification and dependencies

Run only affected checks selected against the active master tracker. Compile the target owner and immediate consumers, then verify wire serialization, native entity/key registration, migration discovery and retained app-role/company/field boundaries. Test projects must replace source links and assembly HintPaths to moved definitions, not create test-only substitute models. Record full-host/provider/build failures separately from focused harness results. Current executed checks and unavailable integration gates are recorded in the Current implementation section and evidence/canonical-dependency-recovery/observed-evidence.json.

Depends on the common ownership/naming decision and inventory review; caller migration follows each owner group. Finish with one-definition and zero-obsolete-reference checks. Keep a move manifest so an individual slice can be reversed without resetting unrelated staged work.
