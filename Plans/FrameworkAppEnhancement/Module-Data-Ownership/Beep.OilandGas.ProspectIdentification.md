# Beep.OilandGas.ProspectIdentification — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

0 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

This project consumes changed contracts; no original Data definitions are assigned here. Caller checks remain in scope.

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.ProspectIdentification/Beep.OilandGas.ProspectIdentification.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

0 candidate incoming files; 104 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.ProspectIdentification/Core/Interfaces/IFieldExplorationService.cs`
- `Beep.OilandGas.ProspectIdentification/Core/Interfaces/IProspectIdentificationService.cs`
- `Beep.OilandGas.ProspectIdentification/Core/Interfaces/IProspectRiskEconomicAnalysisService.cs`
- `Beep.OilandGas.ProspectIdentification/Core/Interfaces/IProspectTechnicalMaturationService.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/AnalogSearchRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/EconomicEvaluationRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ExploratoryWellRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ExploratoryWellResponse.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/PeerReviewRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/PlayAnalysisRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/PortfolioReportRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ProbabilisticAssessmentRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ProspectAnalysisWorkflowRequests.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ProspectComparisonRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ProspectEvaluationRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ProspectRankingRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ProspectReportRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ProspectRiskAssessmentRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/ResourceEstimateRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/RiskAssessmentRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/RiskAssessmentResponse.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/SeismicLineRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/SeismicLineResponse.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/SeismicSurveyRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/SeismicSurveyResponse.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/SensitivityAnalysisRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Contracts/VolumetricAnalysisRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Core/PROSPECT.Table.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Core/PROSPECT_WORKFLOW_STAGE.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Core/Prospect.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Core/ProspectEvaluation.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Core/ProspectRanking.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Core/ProspectRequest.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Core/ProspectResponse.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/CreateProspect.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/CriteriaScoring.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/EconomicViabilityAnalysis.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/Fault.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/Horizon.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/MigrationPathAnalysis.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/PortfolioOptimizationResult.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/ProspectComparison.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/ProspectRiskAnalysisResult.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/ProspectValidation.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/ResourceEstimationResult.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/RiskCategory.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/SealSourceAssessment.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/SeismicInterpretationAnalysis.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/SeismicSurvey.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/TrapGeometryAnalysis.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Projections/UpdateProspect.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/AnalogProspect.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/EXPLORATION_BUDGET.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/EXPLORATION_COSTS.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/EXPLORATION_PERMIT.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/EXPLORATION_PROGRAM.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/EconomicEvaluation.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/LEAD.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PLAY.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_ANALOG.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_BA.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_DISCOVERY.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_ECONOMIC.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_FIELD.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_HISTORY.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_MIGRATION.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_PLAY.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_PORTFOLIO.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_RANKING.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_RESERVOIR.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_RISK_ASSESSMENT.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_SEIS_SURVEY.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_SOURCE_ROCK.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_TRAP.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_VOLUME_ESTIMATE.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PROSPECT_WELL.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PeerReview.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PlayAnalysis.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PlayStatistics.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PortfolioReport.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/PortfolioSummary.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/ProbabilisticAssessment.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/ProspectReport.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/R_EXPLORATION_REFERENCE_CODE.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/R_LEAD_STATUS.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/R_PLAY_TYPE.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/ResourceEstimate.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/RiskAssessment.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/SensitivityAnalysis.cs`
- `Beep.OilandGas.ProspectIdentification/Data/Tables/VolumetricAnalysis.cs`
- `Beep.OilandGas.ProspectIdentification/ExplorationReferenceCodes.ProcessEngine.cs`
- `Beep.OilandGas.ProspectIdentification/ExplorationReferenceCodes.cs`
- `Beep.OilandGas.ProspectIdentification/LeadExplorationWorkflowOptions.cs`
- `Beep.OilandGas.ProspectIdentification/Modules/ExplorationModule.cs`
- `Beep.OilandGas.ProspectIdentification/Services/ILeadExplorationService.cs`
- `Beep.OilandGas.ProspectIdentification/Services/IProspectEvaluationService.cs`
- `Beep.OilandGas.ProspectIdentification/Services/ISeismicAnalysisService.cs`
- `Beep.OilandGas.ProspectIdentification/Services/PPDMExplorationService.cs`
- `Beep.OilandGas.ProspectIdentification/Services/ProspectEvaluationService.cs`
- `Beep.OilandGas.ProspectIdentification/Services/ProspectIdentificationService.ProspectsAndPortfolio.cs`
- `Beep.OilandGas.ProspectIdentification/Services/ProspectIdentificationService.RiskAndEconomics.cs`
- `Beep.OilandGas.ProspectIdentification/Services/ProspectIdentificationService.TechnicalMaturation.cs`
- `Beep.OilandGas.ProspectIdentification/Services/ProspectIdentificationService.cs`
- `Beep.OilandGas.ProspectIdentification/Services/SeismicAnalysisService.cs`

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
