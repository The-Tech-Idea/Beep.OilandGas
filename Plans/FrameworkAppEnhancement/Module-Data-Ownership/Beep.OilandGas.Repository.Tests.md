# Beep.OilandGas.Repository.Tests — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

0 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

This project consumes changed contracts; no original Data definitions are assigned here. Caller checks remain in scope.

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

0 candidate incoming files; 121 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.Repository.Tests/AccessControlClientTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyAssetAccessTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyAssetFacadeTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyAssetGrantTargetTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyAssetGrantUiTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyAssetHierarchyTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyAssetRelationshipTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyClientScopeTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyCorrectiveActionAuthorityTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyCorrectiveActionCommandTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyCorrectiveActionPlanCommandTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyDevelopmentBoundaryTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyExecutivePortfolioHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyFieldEligibilityTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyFieldGhgProductionReaderTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyFieldInterestAdministrationTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyFieldInterestReviewTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyFieldObligationReaderTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyFieldSelectionTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyFocusedClaimsBoundary.cs`
- `Beep.OilandGas.Repository.Tests/CompanyGhgArithmeticServiceTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyHazopAuthorityTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyHierarchyConfigurationTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyHierarchyUiTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyHseBoundaryTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyHseRowAccessTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyHseWorkflowHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyHseWorkflowServiceTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyHubScopeTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyIncidentCommandTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyLeaseDataTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceAcceptanceHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceAssetEvidenceHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceAssetReturnHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceClosureHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceCompleteApprovalHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceExecutionHandoffHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceHandoffHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceLicenseConditionTermsTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceQualifiedAssetHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceQualifiedAssetReturnTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceQualifiedAssetReturnWriteTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceQualifiedAssetTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceQualifiedLicensingTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceQualifiedOperatingTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceQualifiedPolicyTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceQualifiedReturnParticipantTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceQualifiedWorkScopeTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceRecordedWorkHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceReturnPolicyHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceReturnPolicyTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyMaintenanceStatusMutationBoundaryTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyModuleConnectionTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyPPDMRowAccessTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyPPDMTestFixture.cs`
- `Beep.OilandGas.Repository.Tests/CompanyPermitApplicationDataAccessTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyPersonnelIdentityHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyProductionBoundaryTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyProductionInterventionSourceTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyProgressTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyRecordedApprovalHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyRetirementBoundaryTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyRowSecurityTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyWorkOrderBudgetHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyWorkOrderCompletionDecisionHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyWorkOrderCompletionHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyWorkOrderReturnAcceptanceHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompanyWorkSubmissionRecoveryHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/CompositeFacilityGrantTests.cs`
- `Beep.OilandGas.Repository.Tests/CurrentCompanyHttpAcceptanceCollection.cs`
- `Beep.OilandGas.Repository.Tests/CurrentFieldLifecycleGateTests.ComplianceEvidence.cs`
- `Beep.OilandGas.Repository.Tests/CurrentFieldLifecycleGateTests.PlanCreation.cs`
- `Beep.OilandGas.Repository.Tests/CurrentFieldLifecycleGateTests.cs`
- `Beep.OilandGas.Repository.Tests/DevelopmentFacilityNativeHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/EquipmentAssetAccessTests.cs`
- `Beep.OilandGas.Repository.Tests/FacilityCreationSourceTests.cs`
- `Beep.OilandGas.Repository.Tests/FacilityEditNativeWriteTests.cs`
- `Beep.OilandGas.Repository.Tests/FacilityEditPreflightTests.cs`
- `Beep.OilandGas.Repository.Tests/FacilityQualifiedIdentityTests.cs`
- `Beep.OilandGas.Repository.Tests/FieldSelectionEndpointTests.cs`
- `Beep.OilandGas.Repository.Tests/FocusedFinancialContracts.cs`
- `Beep.OilandGas.Repository.Tests/HseAssignedActionsHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/HseClosureConcurrencyQualification.cs`
- `Beep.OilandGas.Repository.Tests/HseClosurePhaseClientTests.cs`
- `Beep.OilandGas.Repository.Tests/HseClosureProviderQualificationTests.cs`
- `Beep.OilandGas.Repository.Tests/InvestigatorAssignmentStateTests.cs`
- `Beep.OilandGas.Repository.Tests/InvestigatorClientTests.cs`
- `Beep.OilandGas.Repository.Tests/LeaseClientTests.cs`
- `Beep.OilandGas.Repository.Tests/LeaseCreationUiTests.cs`
- `Beep.OilandGas.Repository.Tests/LeaseEndpointBindingTests.cs`
- `Beep.OilandGas.Repository.Tests/LeasePortfolioUiTests.cs`
- `Beep.OilandGas.Repository.Tests/LocalDbInstallationTests.cs`
- `Beep.OilandGas.Repository.Tests/ModuleDatabaseTargetTests.cs`
- `Beep.OilandGas.Repository.Tests/PermitApplicationWorkflowStateTests.cs`
- `Beep.OilandGas.Repository.Tests/PermitDocumentProviderHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/PermitDocumentStagingTests.cs`
- `Beep.OilandGas.Repository.Tests/PermitProviderHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/PermitProviderQualificationTests.cs`
- `Beep.OilandGas.Repository.Tests/PermitRepositoryMutationAdmissionTests.cs`
- `Beep.OilandGas.Repository.Tests/PermitReviewedCommandApiTests.cs`
- `Beep.OilandGas.Repository.Tests/PpdmDataClientTests.cs`
- `Beep.OilandGas.Repository.Tests/ProductionHttpExternalBoundary.cs`
- `Beep.OilandGas.Repository.Tests/ProductionInterventionHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/ProductionInterventionNativeDecisionTests.cs`
- `Beep.OilandGas.Repository.Tests/RecordingFailureReporter.cs`
- `Beep.OilandGas.Repository.Tests/RegistrationProfileTests.cs`
- `Beep.OilandGas.Repository.Tests/RepositoryAccountProfileTests.cs`
- `Beep.OilandGas.Repository.Tests/RepositoryAdministrationTests.cs`
- `Beep.OilandGas.Repository.Tests/RepositoryBootstrapTests.cs`
- `Beep.OilandGas.Repository.Tests/RepositoryCompanyAccessTests.cs`
- `Beep.OilandGas.Repository.Tests/RepositoryMigrationTests.cs`
- `Beep.OilandGas.Repository.Tests/RepositoryPersonaTests.cs`
- `Beep.OilandGas.Repository.Tests/RepositoryReadinessFailureTests.cs`
- `Beep.OilandGas.Repository.Tests/RepositoryRegistrationTests.cs`
- `Beep.OilandGas.Repository.Tests/RepositoryTenantBindingTests.cs`
- `Beep.OilandGas.Repository.Tests/RoleWorkspaceTests.cs`
- `Beep.OilandGas.Repository.Tests/StoredAssetGrantReviewTests.cs`
- `Beep.OilandGas.Repository.Tests/StoredAssetGrantReviewUiTests.cs`
- `Beep.OilandGas.Repository.Tests/StoredFieldSelectionAuthorityTests.cs`
- `Beep.OilandGas.Repository.Tests/WorkOrderMaterialSourceHttpTests.cs`
- `Beep.OilandGas.Repository.Tests/WorkOrderPrerequisiteHttpTests.cs`

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
