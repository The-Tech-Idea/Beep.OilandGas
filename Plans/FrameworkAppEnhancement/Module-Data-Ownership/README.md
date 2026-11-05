# Module-owned data restoration

Date: 2026-11-04. Status: all 502 original files moved/consolidated; source project retired; all 39 source-mode analyzer-enabled framework builds pass; integrated host and broader regression verification remain open.

Scanned 502 production C# files in TheTechIdea.Data, 1213 distinct declared names, 65 affected existing projects and 5 forwarding files. See [inventory CSV](inventory.csv) and [machine-readable inventory](inventory.json).

Initial owner assignments below are proposals; actual destinations and reasons are recorded in restoration-progress.json and inventory actual_* fields. Historical same-filename paths are evidence to investigate, not proof of original type ownership. Lexical callers can contain identifier collisions and omit reflection/dynamic use. Final shared placements use the existing Models contract layer where lower-level consumers or native partial groups require it; recorded reasons do not establish a general centralized ownership rule.

## Sequence

1. Confirm the move manifest and correct ownership instructions; preserve the staged soft-reset changes.
2. Review native entities, shared base types and contract dependencies first. Module owners retain entities/rules; existing dependency-safe contract projects own genuinely shared interfaces/DTOs. Record any cycle-blocked placement.
3. Migrate one owner group and every immediate caller in the same slice, including naming and source-linked test fixtures.
4. Validate affected build/serialization/security/persistence paths. Remove forwarding and obsolete registrations after consumers move.
5. Remove TheTechIdea.Data after its inventory and remaining-reference gates reach zero; update package/solution/docs/scripts and run the selected integration gates.

[Per-type naming and caller checklist](naming-and-caller-changes.csv) lists proposed namespaces/files and current callers; destination names require confirmation against each project's existing conventions.

## Current verification and behavior

All 39 direct framework builds pass with the real analyzer and default BeepDM source configuration. The broad focused caller profile compiles against the restored owners. Current selected tests: 41 caller, 88 ownership, 6 native financial contract and 127 financial behavior cases pass; counts describe separate scopes, not a full regression run. Compiled owner checks pass serialization, schema admission, native owner/key and all 12 permit metadata contributions. The fresh production declaration audit checks all 1,200 restored identities across 7,343 production C# files, with zero missing or cross-project duplicates.

Missing receiver/policy evidence remains unavailable. Present malformed or inconsistent authoritative proof now fails before the financial review factory returns, allowing the owning API failure handler to report it instead of hiding corruption or failing during response serialization. Stale invoice/stock/receivable comparisons return false using their canonical comparisons; malformed authoritative proof still fails. Diagnostic decimal arithmetic uses exact coefficients/scales and returns unavailable when a result cannot be represented exactly. Permit document changes are reported through the real failure reporter before their review state becomes CHANGED; without a reporter the exception propagates.

The full regression run is not accepted. Historical generic FACILITY expectations have now been reconciled against active Plan 352; 58 relationship cases pass, including FIELD/grant/filter/revocation and generic mutation refusal. The controlled native-projection fixture advertises implemented full-row capability, preserves already-projected dictionaries, and retains explicit missing-projection cases. The expanded native run passed 398/408 before final repairs; all ten failures pass in the final 71-case targeted gate. A fresh full 408-case run is not claimed. Four affected maintenance cases now pass after correcting that fixture; production authorization and projection requirements are unchanged. The final selected financial behavior rerun passed 127 cases, including all five reviewer rendering cases. The selected 127-case run excluded the long-running 201-write history case; that separate isolated test now passes 1/1 in 29.8714 minutes. Canonical Beep.Web lacks Diagnostics Log/failure-log source definitions, so integrated API/Web, OIDC, browser and live-provider acceptance is not claimed. Generated-only retired-directory cleanup remains blocked by automatic approval review.

Current caller completion: 22/25 top-level test projects compile; the remaining three depend on unavailable canonical Diagnostics outputs. Nodal and Pipeline callers pass 13 and 9 focused tests. The nested company-environment suite passes 65/65 against actual owners. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final targeted 27/27 recheck, without claiming a fresh full 594-case pass. The expanded internal graph includes 73 projects and 290 literal edges with zero cycles.

Reproduce the compiled runtime gate with `dotnet run --project Plans/FrameworkAppEnhancement/Module-Data-Ownership/evidence/final-restoration/OwnerChecks.Tests.csproj`. See [current observed evidence](evidence/canonical-dependency-recovery/observed-evidence.json) for build/test scope and logs; per-project current sections supersede initial proposals.

## Naming policy

Use established project namespaces and feature folders; remove generic TheTechIdea.Data namespaces. Use PascalCase for ordinary CLR types and feature-specific request/response names when ambiguous. Keep PPDM/native table/entity/key names governed by existing schema mappings. A physical move alone does not require a namespace rename; schedule renames with all callers and wire/persisted metadata checks. No parallel DTOs, duplicate stores or new global data owner.

## Per-project plans

The initial scan produced 65 project plans. The completion audit added the nested [CompanyEnvironment.Tests consumer](CompanyEnvironment.Tests.md), giving 66 current project plans.

- [Beep.OilandGas.Accounting](Beep.OilandGas.Accounting.md)
- [Beep.OilandGas.ApiService](Beep.OilandGas.ApiService.md)
- [Beep.OilandGas.ApiService.Tests](Beep.OilandGas.ApiService.Tests.md)
- [Beep.OilandGas.Branchs](Beep.OilandGas.Branchs.md)
- [Beep.OilandGas.ChokeAnalysis](Beep.OilandGas.ChokeAnalysis.md)
- [Beep.OilandGas.ChokeAnalysis.Tests](Beep.OilandGas.ChokeAnalysis.Tests.md)
- [Beep.OilandGas.Client](Beep.OilandGas.Client.md)
- [Beep.OilandGas.CompressorAnalysis](Beep.OilandGas.CompressorAnalysis.md)
- [Beep.OilandGas.CompressorAnalysis.Tests](Beep.OilandGas.CompressorAnalysis.Tests.md)
- [Beep.OilandGas.DataManager](Beep.OilandGas.DataManager.md)
- [Beep.OilandGas.Decommissioning](Beep.OilandGas.Decommissioning.md)
- [Beep.OilandGas.DevelopmentPlanning](Beep.OilandGas.DevelopmentPlanning.md)
- [Beep.OilandGas.Diagnostics](Beep.OilandGas.Diagnostics.md)
- [Beep.OilandGas.Drawing](Beep.OilandGas.Drawing.md)
- [Beep.OilandGas.Drawing.Benchmarks](Beep.OilandGas.Drawing.Benchmarks.md)
- [Beep.OilandGas.Drawing.Tests](Beep.OilandGas.Drawing.Tests.md)
- [Beep.OilandGas.DrillingAndConstruction](Beep.OilandGas.DrillingAndConstruction.md)
- [Beep.OilandGas.EconomicAnalysis](Beep.OilandGas.EconomicAnalysis.md)
- [Beep.OilandGas.EconomicAnalysis.Tests](Beep.OilandGas.EconomicAnalysis.Tests.md)
- [Beep.OilandGas.EnhancedRecovery](Beep.OilandGas.EnhancedRecovery.md)
- [Beep.OilandGas.EnhancedRecovery.Tests](Beep.OilandGas.EnhancedRecovery.Tests.md)
- [Beep.OilandGas.FlashCalculations](Beep.OilandGas.FlashCalculations.md)
- [Beep.OilandGas.FlashCalculations.Benchmarks](Beep.OilandGas.FlashCalculations.Benchmarks.md)
- [Beep.OilandGas.FlashCalculations.Tests](Beep.OilandGas.FlashCalculations.Tests.md)
- [Beep.OilandGas.GasLift](Beep.OilandGas.GasLift.md)
- [Beep.OilandGas.GasLift.Tests](Beep.OilandGas.GasLift.Tests.md)
- [Beep.OilandGas.GasProperties](Beep.OilandGas.GasProperties.md)
- [Beep.OilandGas.GasProperties.Tests](Beep.OilandGas.GasProperties.Tests.md)
- [Beep.OilandGas.HSE](Beep.OilandGas.HSE.md)
- [Beep.OilandGas.HeatMap](Beep.OilandGas.HeatMap.md)
- [Beep.OilandGas.HydraulicPumps](Beep.OilandGas.HydraulicPumps.md)
- [Beep.OilandGas.LeaseAcquisition](Beep.OilandGas.LeaseAcquisition.md)
- [Beep.OilandGas.LifeCycle](Beep.OilandGas.LifeCycle.md)
- [Beep.OilandGas.LifeCycle.Tests](Beep.OilandGas.LifeCycle.Tests.md)
- [Beep.OilandGas.Models](Beep.OilandGas.Models.md)
- [Beep.OilandGas.NodalAnalysis](Beep.OilandGas.NodalAnalysis.md)
- [Beep.OilandGas.NodalAnalysis.Tests](Beep.OilandGas.NodalAnalysis.Tests.md)
- [Beep.OilandGas.OilProperties](Beep.OilandGas.OilProperties.md)
- [Beep.OilandGas.PPDM.Models](Beep.OilandGas.PPDM.Models.md)
- [Beep.OilandGas.PPDM39](Beep.OilandGas.PPDM39.md)
- [Beep.OilandGas.PPDM39.DataManagement](Beep.OilandGas.PPDM39.DataManagement.md)
- [Beep.OilandGas.PermitsAndApplications](Beep.OilandGas.PermitsAndApplications.md)
- [Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf](Beep.OilandGas.PermitsAndApplications.Pdf.Wkhtmltopdf.md)
- [Beep.OilandGas.PermitsAndApplications.Tests](Beep.OilandGas.PermitsAndApplications.Tests.md)
- [Beep.OilandGas.PipelineAnalysis](Beep.OilandGas.PipelineAnalysis.md)
- [Beep.OilandGas.PipelineAnalysis.Tests](Beep.OilandGas.PipelineAnalysis.Tests.md)
- [Beep.OilandGas.PlungerLift](Beep.OilandGas.PlungerLift.md)
- [Beep.OilandGas.PlungerLift.Tests](Beep.OilandGas.PlungerLift.Tests.md)
- [Beep.OilandGas.ProductionAccounting](Beep.OilandGas.ProductionAccounting.md)
- [Beep.OilandGas.ProductionForecasting](Beep.OilandGas.ProductionForecasting.md)
- [Beep.OilandGas.ProductionOperations](Beep.OilandGas.ProductionOperations.md)
- [Beep.OilandGas.ProspectIdentification](Beep.OilandGas.ProspectIdentification.md)
- [Beep.OilandGas.ProspectIdentification.Tests](Beep.OilandGas.ProspectIdentification.Tests.md)
- [Beep.OilandGas.PumpPerformance](Beep.OilandGas.PumpPerformance.md)
- [Beep.OilandGas.Repository](Beep.OilandGas.Repository.md)
- [Beep.OilandGas.Repository.Tests](Beep.OilandGas.Repository.Tests.md)
- [Beep.OilandGas.SuckerRodPumping](Beep.OilandGas.SuckerRodPumping.md)
- [Beep.OilandGas.SuckerRodPumping.Tests](Beep.OilandGas.SuckerRodPumping.Tests.md)
- [Beep.OilandGas.UserManagement](Beep.OilandGas.UserManagement.md)
- [Beep.OilandGas.Web](Beep.OilandGas.Web.md)
- [Beep.OilandGas.Web.Auth.Tests](Beep.OilandGas.Web.Auth.Tests.md)
- [Beep.OilandGas.Web.Tests](Beep.OilandGas.Web.Tests.md)
- [Beep.OilandGas.WellTestAnalysis](Beep.OilandGas.WellTestAnalysis.md)
- [Beep.OilandGas.WellTestAnalysis.Tests](Beep.OilandGas.WellTestAnalysis.Tests.md)
- [TheTechIdea.Data](TheTechIdea.Data.md)

## Acceptance gates

- Exactly one production definition per migrated type; confirmed owner and destination for every inventory entry.
- No circular project references, obsolete forwarders, old namespace imports, broken type/assembly strings or stale source-linked fixtures.
- Preserved API wire contracts, native entity identifiers, migration registration, app RBAC and company/field access.
- Affected builds and selected tests recorded with actual outcomes; provider and full-host gaps explicitly open.
- Repository owner accountable; implementing engineer responsible; module maintainers review. Documentation completion does not mark migration complete.

## Implementation checkpoint - 2026-11-04

[Exact move/dependency report](restoration-progress.json): 19 files returned to Models, one to DevelopmentPlanning, one to UserManagement. AGENTS.md corrected. The 365 self-target proposals are now unresolved. 481 files remain; 26 cycle/destination deferrals need resolution. No full-plan closure.

## Latest checkpoint - 2026-11-04

63 source files restored; 439 remain. 42 files now use their owning module Models namespaces. API, UI, migrations and nested test callers received namespace/import updates; focused source links updated. Accounting and Repository builds passed. Latest seven direct owner/Data builds pass after fixing permit namespace shadowing; these use existing dependency outputs and do not prove a clean solution or runtime validation. See restoration-progress.json. Next: recompute remaining dependency groups, migrate group closures and their interfaces, then integrated consumer and selected behavior validation.

## Native/accounting checkpoint - 2026-11-04

86 files restored, 416 remain. Lower-layer type references removed while preserving schema policies/native metadata. 22 accounting DTO namespaces updated; native AFE_LINE_ITEM restored without forwarding. Five direct owner builds pass. [Compiled-assembly evidence](evidence/compiled-owner-check/observed-evidence.json) proves selected guard/native/wire behavior only. Remaining migration, consumer integration, test and provider gates stay open.

## Latest dependency-group checkpoint - 2026-11-04

166 source files migrated/consolidated; 336 remain. [Compiled owner evidence](evidence/round2-compiled-owner-check/observed-evidence.json) covers current direct owner builds and selected schema/native/wire behavior. Integrated API/Web gates remain open on missing external outputs and diagnostic source types. Exact moves, namespace changes, caller/script updates and deferred groups remain in restoration-progress.json. Next: use syntax-tree evidence to resolve the remaining groups and then retire Data after zero-reference checks.

## Latest compiler-bound checkpoint - 2026-11-04

207 original files migrated/consolidated, 295 remain. [Current evidence](evidence/semantic-owner-batch/observed-evidence.json) records the zero-error semantic input, direct owner builds and selected compiled runtime checks. Permit metadata coupling, restricted validation assembly access cleanup, API/Web external dependencies and full migration/fixture/provider gates remain open.
