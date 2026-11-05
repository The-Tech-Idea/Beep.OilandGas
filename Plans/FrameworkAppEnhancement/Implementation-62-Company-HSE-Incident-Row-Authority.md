# Implementation 62 — company HSE incident row authority

Date: 2026-10-06. Execution owners: CA-03/05/06/32, FG-10/11/12/20/40/42. [Master tracker](../../MASTER-TODO-TRACKER.md) owns execution status. This continues [Implementation 61](Implementation-61-Company-HSE-Request-and-Evidence-Boundary.md) through the existing company policy, Beep evidence reader, canonical PPDM repository and actual incident/RCA owners. The full plan remains active.

## Problem and resulting behavior

The previous request guard refused every incident repository read because canonical HSE tables had no reviewed company row path. The existing policy now authorizes bounded reads of HSE_INCIDENT, HSE_INCIDENT_COMPONENT, HSE_INCIDENT_DETAIL, HSE_INCIDENT_RESPONSE, HSE_INCIDENT_CAUSE and HSE_INCIDENT_BA through recorded primary FIELD evidence. An eligible inherited FIELD app grant is required. It does not establish legal, operator or JV ownership, unrestricted case visibility, or permission to change incident state.

The actual incident service and RCA service can read recorded incident and cause data through the existing profile, access registry, entitlement interpreter, policy and Beep repository. Incident mapping verifies its primary FIELD before reading detail/investigator information. Alternate component/detail selection is removed: exactly one canonical primary FIELD component and one PRIMARY detail are required. Incident identity comparison uses ordinal case. Existing public contracts remain unchanged.

## Canonical owners and evidence

- `CompanyPPDMRowAccessPolicy` owns this extension; `CompanyPPDMEvidenceReader` remains the shared bounded evidence reader. The active repository, authorization store and canonical PPDM models are reused.
- Complete canonical table keys and exact parent FKs are checked against the existing PPDM metadata. The component must reference HSE_INCIDENT and FIELD, with exact component role/type reference relationships. Child table keys retain all observation/party parts.
- The existing app grant must be active, explicitly inherited and capped by current company FIELD data entitlement. Direct FIELD grants without inheritance provide no incident authority. The target FIELD must exist uniquely and be active in the reviewed datasource.
- The existing primary role `PRIMARY_FIELD_CONTEXT` and type `INCIDENT_LOCATION` are shared through `HseReferenceCodes.FieldContext`. Exactly one active recorded reference row for each code is required. Production data is never seeded to satisfy authority.
- Candidate components are read with explicit FIELD/primary-role/active equality predicates. Every active primary component for each candidate incident is then read by exact incident ID across FIELDs. Two primary rows, including another FIELD or malformed primary type, refuse authority rather than selecting the first row.
- The incident requires a unique active header. Missing/inactive headers, missing FIELDs, expired/future primary evidence and inactive grants produce no authorized payload. Missing/ambiguous vocabulary, keys or relationship evidence refuses interpretation. Dated authority evidence must carry an explicit UTC boundary; local/unspecified dates refuse rather than infer an operating timezone. Undated active evidence is allowed; this is an active association policy, not certification of historical legal validity.
- Every result group contains exact INCIDENT_ID; component groups also require the reviewed FIELD_ID. ACTIVE_IND=Y is mandatory for every family table. The existing repository preserves requested equality filters, rereads policy before each provider call, verifies returned predicates/keys and validates rows against fresh evidence after the call and before return.
- Grants and entitlement are reread after association evidence. A revoked grant, reparented primary component, changed selected database or failed provider cannot retain payload authority. Evidence lookups and incident predicate groups retain the existing 256-row/group bounds. This deliberately favors current authority; real-provider query cost/large-inventory performance remains an acceptance gate.
- All generic incident insert/update/delete operations explicitly refuse. Effective incident access reports READ even under a WRITE/DELETE FIELD grant; reviewed command/reference/transaction support must precede write availability.

Required editor/column/default dependencies are supplied by existing Program factories and subject-bound access/hierarchy factories. Their focused real-source consumers compile; integrated Program/host DI remains source-only.

## Focused verification

**919 final affected tests pass, 0 failed/skipped, in 4 minutes 41 seconds**, including 37 new incident-authority cases, the previous 522-case lifecycle/HSE/selector gate and 360 affected access/hierarchy/configuration/facility/eligibility cases. Final rebuild has zero errors, four known missing external analyzer warnings and four existing HSE nullable warnings. Final log: `%TEMP%/oilgas-hse-policy-final.log`.

New `CompanyHseRowAccessTests` contains 37 cases using the actual profile, SQLite-backed approved identity/access/profile registry, company FIELD eligibility, canonical metadata, row policy, PPDM repository and complete actual incident/RCA sources. The Beep datasource records explicit test evidence; real-driver acceptance remains open. Tests cover successful reads across six canonical tables, actual recorded incident/RCA mapping, no-inheritance refusal, foreign incident isolation, ambiguous/invalid/inactive/date-bound primary evidence, active FIELD and company entitlement, schema drift, reported provider failures, ignored predicates, concurrent reparent/revoke, primary detail ambiguity and the mutation fence. Existing HSE factory/refusal assertions now reflect supported bounded reads without inheritance, instead of obsolete unsupported-table expectations.

The final affected gate additionally exercises existing development/production/retirement/request guards, FIELD/profile/endpoint/selector paths and changed subject-bound facade/hierarchy/configuration/facility factories and FIELD eligibility. Complete actual HSE sources compile; no production stand-ins or excluded failing production methods are introduced. Initial 214 HSE cases passed before the final additional FIELD/schema/provider checks; final-source evidence above supersedes that checkpoint.

Reproduce:

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --filter 'FullyQualifiedName~CompanyHseRowAccessTests|FullyQualifiedName~CompanyHseBoundaryTests|FullyQualifiedName~CompanyDevelopmentBoundaryTests|FullyQualifiedName~CompanyProductionBoundaryTests|FullyQualifiedName~CompanyRetirementBoundaryTests|FullyQualifiedName~CurrentFieldLifecycleGateTests|FullyQualifiedName~CompanyPPDMRowAccessTests|FullyQualifiedName~CompanyFieldSelectionTests|FullyQualifiedName~FieldSelectionTests|FullyQualifiedName~FieldSelectionEndpointTests|FullyQualifiedName~RepositoryAccountProfileTests|FullyQualifiedName~CompanyAssetFacadeTests|FullyQualifiedName~CompanyAssetHierarchyTests|FullyQualifiedName~CompanyHierarchyUiTests|FullyQualifiedName~CompanyHierarchyConfigurationTests|FullyQualifiedName~CompositeFacilityGrantTests|FullyQualifiedName~CompanyFieldEligibilityTests'
```

## Subsequent execution

[Implementation 63](Implementation-63-Company-HSE-Permission-and-HAZOP-Project-Authority.md) additionally requires actual current-company HSE.View permission for incident and HAZOP rows and supplies canonical HAZOP PROJECT read authority, real study/node/deviation summaries and strict recorded graph interpretation. The PROJECT-unavailable statements below describe this implementation's earlier checkpoint. Generic commands, corrective-action/PROJECT_STEP_BA relationships, restricted case visibility and full release acceptance remain required.

## Remaining acceptance and next owners

Read authority does not close CA-32 or HSE readiness. Restricted case visibility, sensitive participant/attachment access, classification/status/participant/equipment reference interpretation and shared Data contract consolidation remain required. HSE components outside the primary incident FIELD are not visible through this policy; their domain meaning and explicit cross-asset case decisions need review. Active child reads are not historical snapshot certification. No query result certifies causation, legal entitlement, workflow completion or calculation accuracy.

Continue the existing HAZOP/corrective-action PROJECT/COMPONENT/STEP/CONDITION/STATUS/BA policies and full-key relationship commands. Complete actual incident reporting/assignment/classification/status/history, RCA/barrier/action approval and closure with participant authority, stable identities/allocation, prerequisites, owned Beep transactions, readback and reconciliation. Generic writes stay fenced. Tick-modulo and max-plus-one allocation, barrier interpretation/defaults, partial workflow JSON, remaining incident state/detail reference semantics and exposure/KPI scientific inputs remain open.

Then continue exploration/process direct registrations and resource ownership, concrete orchestrator/host authorization/HTTP/Web/browser/real-provider/revocation/performance gates. Development, production, retirement, durable leases, accounting, localization/jurisdiction and every other master-plan recommendation retain their owners and acceptance gates.

No SQL, new business EF migration, schema/live execution, live datasource mutation, deployment, feature/phase closure or full-plan completion. Business/module schemas use Beep MigrationManager. Existing approved EF identity/access/profile persistence retains its current owner.
