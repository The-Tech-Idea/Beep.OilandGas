# Implementation 58 — company retirement request boundary

Date: 2026-10-05. Execution owners: CA-03/05/06/32/33, FG-10/11/12/20/40. [Master tracker](../../MASTER-TODO-TRACKER.md) owns execution status. This continues [Implementation 57](Implementation-57-Current-FIELD-Lifecycle-Guard.md) through the existing registered `PPDMDecommissioningService`, its profile/repository owners and its two constructor callers. The wider plan remains active.

## Problem and resulting behavior

The actual retirement service accepted a default/global PPDM39 connection and built repositories without the app's mandatory row policy. FIELD and caller actor parameters could reach these paths independently of the reviewed persistent preference. Missing metadata was sometimes reported as an empty portfolio or replaced by a separate status/activity reader that fabricated record IDs. These behaviors could not establish company-owned retirement evidence.

The existing service now requires `ICompanyFieldSelectionService` and `IPPDMRowAccessPolicy`. All 13 public operations require an exact FIELD ID and the profile's current available selection. All six mutations require the exact reviewed application actor. The phase captures one account/company/database/FIELD/revision review and refuses to adopt changed evidence in the same request. Its connection is resolved from that review before every repository construction; all 18 constructions pass the required row policy. Success returns revalidate the same profile evidence. This capture is ephemeral request state, not another selection store.

The existing API process registration and orchestrator phase factory supply these mandatory dependencies; they no longer pass a global connection to this retirement owner. Those complete Program/orchestrator paths remain source-only. The actual complete retirement class and existing mapping service compile in the focused target using their actual sources and actual module contracts. No fake diagnostics project, provider implementation, interface or production identity helper is added.

Missing WELL_ABANDONMENT, FACILITY_DECOMMISSIONING, ENVIRONMENTAL_RESTORATION or DECOMMISSIONING_COST metadata is an explicit unavailable/refusal state. An unresolved cost entity also refuses. The alternate WELL_ACTIVITY/FACILITY_STATUS portfolio readers are removed, including their generated fallback IDs. FIELD association checks use ordinal identity comparison. Existing engineering unit and cost formulas are not certified by this boundary change.

## Actual schema and domain limitations

The checked-in canonical PPDM metadata does not contain the four retirement extension-table definitions, and the current company row policy has no reviewed ownership path for those extension tables or retirement status/reference writes. An existing CLR entity alone does not prove a schema, complete key, relationship, company/resource authority or provider support. The focused tests preserve this actual absence; they do not invent metadata to declare the feature ready.

Consequently, extension-table operations remain unavailable in that configuration. Supplying missing metadata alone does not bypass the mandatory row policy. Complete the existing module setup/metadata and Beep MigrationManager owners with reviewed table/complete-key/FK/resource definitions and real provider acceptance. Do not restore a legacy reader, assume table existence or add handwritten SQL to make the checks pass.

The class still contains domain work needing repair: status/history transition semantics, complete composite identities and mapping, row relationships/child table policy, method parameters and date/unit evidence, multi-record write ownership/transactions/readback and durable command outcome handling. Cost estimation still uses hardcoded rates, USD and factor heuristics. This slice does not approve those estimates, numerical accuracy, legal/regulatory decisions or retirement mutation readiness. Audit the existing module retirement service and process wrappers as part of owner consolidation; do not add another retirement model/store/service path.

## Focused evidence

The final affected gate passed **194 tests, 0 failed, 0 skipped, in 1 minute 13 seconds**. It combines the 28 new actual retirement cases with the 166 current FIELD/profile/selector/UI/MVC/PPDM policy cases from Implementation 57. No new all-inventory run is claimed. The gate compiles the complete actual `PPDMDecommissioningService` and `PPDMMappingService`; the recorded Beep provider is an explicit test boundary. SQLite backs the real existing registry/profile and no SQL/schema/live migration is introduced.

The 28 retirement cases cover all 13 operations with a FIELD different from the reviewed selection, all six mutations with a foreign actor, all four actual missing extension metadata cases, first selection, actual WELL prerequisite reads in the selected database with current operator/asset predicates, a provider returning a foreign-operator WELL, request revision changes and FIELD entitlement loss. They verify refusals before unintended phase work, required company predicates, no retirement mutations and no alternate status/activity portfolio reads. The foreign-provider case overrides only WELL reads and verifies that the actual WELL repository was reached; a profile refusal is not substituted for that proof.

The initial run passed 27 and failed one assertion that expected no profile FIELD evidence after entitlement loss. The assertion was corrected to verify that no retirement WELL read occurs, preserving the required authoritative profile revalidation. This failed checkpoint is not acceptance evidence. The final affected gate is recorded by the root tracker and `%TEMP%/oilgas-retirement-final-affected.log`. Actual-source build evidence: `%TEMP%/oilgas-retirement-source-build.log`; initial retirement test evidence: `%TEMP%/oilgas-retirement-boundary-focused.log`. The obsolete unused repository namespace import was removed to compile the actual source. Four known missing-analyzer warnings remain; zero compile errors.

The same retirement cases and existing shared fixture are linked into the normal API test target. That target and the full registered host remain unverified because external dependencies are absent. Focused source compilation and recorded provider behavior are not full API, OIDC, claims bridge, DI, browser, installed provider, transaction or concurrency certification.

Reproduce the affected gate:

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --filter 'FullyQualifiedName~CompanyRetirementBoundaryTests|FullyQualifiedName~CurrentFieldLifecycleGateTests|FullyQualifiedName~CompanyPPDMRowAccessTests|FullyQualifiedName~CompanyFieldSelectionTests|FullyQualifiedName~FieldSelectionTests|FullyQualifiedName~FieldSelectionEndpointTests|FullyQualifiedName~RepositoryAccountProfileTests'
```

## Next acceptance owners

- Complete retirement extension metadata/setup through the existing Beep MigrationManager paths, authoritative complete identities/FKs, company/resource/child/reference row policies and actual provider verification. Existing method aliases, mappings, currency/unit/rate inputs and domain transitions require their own verified repair before real reads/mutations are promoted.
- Bind the existing development, production, HSE, exploration and process phase/helper owners and direct registrations to mandatory reviewed connection/resource/row authority. Exercise actual implementations and the concrete orchestrator with the real external dependencies.
- Complete registered API/Web/auth/role/company middleware and browser/circuit acceptance, retained-preference retirement, durable lease status/full-interpretation/history revisions, creation reconciliation/fencing and real driver/area/FK/multi-process gates. Continue the wider accounting, engineering, governance, financial and release plan.

Business/module schema changes remain Beep MigrationManager-owned; the existing approved EF identity/profile registry remains its current owner. This slice applies no schema migration or live provider write and adds no compatibility path. No retirement feature, phase, full plan or production readiness is marked complete.

Follow-up: [Implementation 59](Implementation-59-Company-Production-Request-and-Failure-Boundary.md) compiles and verifies the actual production request/repository/failure owner. Supported company FIELD/POOL reads work before FIELD selection; caught equipment/storage errors propagate. Remaining phase/child/reference, typed storage/complete identity/transaction, numerical/provider and host gates remain open; retirement acceptance above is unchanged.
