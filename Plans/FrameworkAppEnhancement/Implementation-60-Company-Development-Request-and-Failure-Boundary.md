# Implementation 60 — company development request and failure boundary

Date: 2026-10-05. Execution owners: CA-03/05/06/20/21/22/23, FG-10/11/12/20/30/40/42. [Master tracker](../../MASTER-TODO-TRACKER.md) owns execution status. This continues [Implementation 59](Implementation-59-Company-Production-Request-and-Failure-Boundary.md) through the existing registered `PPDMDevelopmentService`. The wider plan remains active.

## Problem and resulting behavior

The development owner used a global/default connection and repositories without required company policy. A supplied FIELD, WELL, facility or actor could differ from the current profile. Rig assignment fetched a WELL by ID without checking its FIELD. Compressor analysis used supplied/default inputs without checking an actual facility. Seven analysis save failures were suppressed; missing equipment definitions, plan storage and read metadata could also return apparent success. Missing parent wellbore links fell back to every wellbore in the FIELD.

The existing owner now requires the authoritative company FIELD selection service and PPDM row policy. All 28 public asynchronous operations require exact current FIELD/account/company/database/revision; supplied actor assertions must match the reviewed account and required actors cannot be missing. Seven optional calculation storage calls use the actual reviewed account, with no SYSTEM substitute. All 28 repository constructions use the reviewed connection and mandatory row policy. Public success returns recheck that review, so a changed request cannot adopt a later preference or binding. This fence reuses the existing profile and policy owners; there is no second persisted selection, authorization model or datasource store.

WELL operations use canonical UWI/ASSIGNED_FIELD/ACTIVE_IND predicates and check a unique actual row before calculation or drilling/rig work. The fetched rig target and gas-lift input row are checked again before use. Public POOL/WELL/FACILITY/PIPELINE lists reject rows outside the requested FIELD; requested POOL and FACILITY lookups reject substituted or ambiguous identities. Facility calculations require a uniquely recorded active facility; the current scalar facility interface does not choose one of multiple FACILITY_TYPE keys. Pipeline calculation entry points require the existing ownership path, which currently refuses unsupported PIPELINE policy before a calculation. Parent wellbore absence no longer broadens the lookup to all FIELD wellbores; the remaining WELL_XREF relationship interpretation still needs domain repair.

The actual API registration and orchestrator factory supply mandatory profile/policy dependencies. Their complete Program/orchestrator/host paths remain source-only; the focused target does not compile or exercise the entire registered host.

## Failure and persistence boundary

The seven caught storage failures log and rethrow the original failure. Five missing WELL_EQUIPMENT/FACILITY_EQUIPMENT definitions explicitly refuse analysis storage. Seven missing read-definition branches refuse unavailable metadata instead of producing an empty collection or false mutation result. FIELD_VERSION and WELL_ACTIVITY metadata absence refuses plan/design commands before a returned unpersisted plan.

Pipeline storage still submits invalid dictionaries to a typed PIPELINE repository. The existing repository rejects those entities before writes; that ArgumentException now reaches the caller instead of becoming unsaved success. This is verified failure propagation, not repaired storage or successful analysis persistence. Canonical typed storage/schema ownership and resource policy remain required.

The service no longer consumes handled-failure reporting, so its direct IFailureReporter dependency is removed. Existing API error/reporting remains the unhandled-error owner; no substitute diagnostics abstraction or fake missing project is introduced. Registered-host reporting remains unverified.

## Focused source and runtime evidence

The focused target compiles the complete actual development class and mapper, the actual gas-lift potential/valve design/spacing, pipeline capacity/flow and compressor power/pressure calculator sources, plus their actual module contracts and transitive permit enums/table contract. Those are existing files, not duplicate production models or synthetic calculators. Four known missing-analyzer warnings remain; the final test build owns current compile evidence. No external missing project is manufactured. The temporary Roslyn development editor preserves conditional returns and excludes lambda/local-function returns; it is not an app runtime path.

The development cases use the actual profile/account/company/registry/row-policy/mapping/service sources and the shared recording Beep provider fixture. The operation matrix covers every FIELD entry point, supplied actors and required missing actors. Positive POOL/WELL/FACILITY reads preserve selected FIELD/company rows and database. Other cases cover missing selection, stale revision, an actual WELL-read callback changing the profile, foreign WELL input/mutation targets, ambiguous/missing facilities, missing read/storage/plan definitions, original storage errors, invalid typed pipeline storage and unsupported pipeline authority. A recording provider ignoring FIELD/POOL predicates cannot substitute another authorized record. A WELL changing FIELD between the prerequisite and actual rig lookup is refused before write.

Private storage tests use explicit result objects to establish failure behavior; they do not establish numerical correctness, successful persistence or production engineering data. Standard auth/HTTP/browser and real-provider workflow acceptance remain distinct. Cases also link into the normal API target, whose complete external-dependency build remains unverified.

The first checkpoint passed 79 and failed 6 of 85 in 2 minutes 18 seconds. Four missing-read branches still returned empty success and were repaired; two pipeline fixtures expected policy refusal although the actual earlier typed-entity rejection is ArgumentException. Those cases now assert the actual error and separate public pipeline cases verify policy refusal. That failed checkpoint is not acceptance evidence. The final gate adds stronger provider/resource checks and owns final counts.

The final affected gate passed **339 tests, 0 failed, 0 skipped, in 3 minutes 9 seconds**: 90 new development cases plus the 249 previously affected production/retirement/current-FIELD/selector/UI/MVC/profile/PPDM policy cases. The final build compiled the complete actual development service and required real calculator/contracts with zero errors and four known missing-analyzer warnings. This is an affected gate; no new all-inventory run is claimed. Logs: `%TEMP%/oilgas-development-source-build.log`, `%TEMP%/oilgas-development-boundary-focused.log`, `%TEMP%/oilgas-development-final-affected.log`.

Reproduce the affected gate:

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --filter 'FullyQualifiedName~CompanyDevelopmentBoundaryTests|FullyQualifiedName~CompanyProductionBoundaryTests|FullyQualifiedName~CompanyRetirementBoundaryTests|FullyQualifiedName~CurrentFieldLifecycleGateTests|FullyQualifiedName~CompanyPPDMRowAccessTests|FullyQualifiedName~CompanyFieldSelectionTests|FullyQualifiedName~FieldSelectionTests|FullyQualifiedName~FieldSelectionEndpointTests|FullyQualifiedName~RepositoryAccountProfileTests'
```

## Domain and release work remains required

- Complete existing development/production/retirement child/reference and PIPELINE ownership paths, canonical metadata, complete keys/FKs and reference values through the existing module setup and Beep MigrationManager. Unsupported tables currently refuse authority; this is not phase completion.
- Replace FIELD_VERSION plan and fabricated WELL_ACTIVITY design surrogates through actual DevelopmentPlanning owners; establish plan/asset relationships, approved AFE, drilling completion and construction/handover semantics. ConstructFacility and drilling result-ID/state paths still need canonical owner repair.
- Complete actual typed gas-lift/compressor/pipeline persistence, facility composite contracts, transactions, readback and durable outcome/reconciliation. Required row policy alone does not prove a command is safe or usable.
- Remove unsupported numerical defaults, rounded tubing inputs, guessed pressure/temperature/roughness/efficiency and zero no-data reservoir dashboard metrics through approved input/unit/date/provenance owners. Validate existing correlations independently before promotion. Compilation is not numerical certification.
- Continue existing HSE, exploration and process request/repository owners, then actual concrete orchestrator/host/API/Web/company/auth/standard-role/HTTP/browser/provider/concurrent revocation gates. Development, production and retirement domain repairs remain open.
- Continue durable lease/history/creation/circuit/provider acceptance, financial/accounting, governance, localization, jurisdiction, release and all other recommendation owners in the master plan.

No SQL, business EF migration, schema execution, live mutation or deployment is introduced. Business/module schema work remains Beep MigrationManager-owned; the existing approved identity/access/profile EF registry retains its owner. No feature, phase, numerical accuracy, provider outcome or full-plan readiness is marked complete.

Follow-up: [Implementation 61](Implementation-61-Company-HSE-Request-and-Evidence-Boundary.md) continues through the actual HSE facade and six direct owners, with complete source compilation and 183 new boundary/evidence cases in the 522-case final affected gate. HSE/PROJECT policy and workflow/scientific/provider acceptance, development/production/retirement domain repairs and wider plan work remain required.
