# Implementation 49 — Company FIELD data entitlement

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/11/12/20. Status: implemented with focused verification. Legal title, partnership/operator rights, remaining domain boundaries and release acceptance remain open.

## Problem and resulting behavior

FIELD has no company-owner column. A stored app FIELD grant and an active FIELD record previously permitted access without current company PPDM data-entitlement evidence. Generic FIELD_COMPONENT links and financial interest percentages cannot establish the meaning or level of that access.

The existing [row policy](../../Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.cs), [grant admission verifier](../../Beep.OilandGas.ApiService/Services/PPDMAssetGrantTargetVerifier.cs), [access facade](../../Beep.OilandGas.LifeCycle/Services/AccessControl/UserAssetAccessService.cs) and [diagnostics](../../Beep.OilandGas.ApiService/Services/CompanyRowLevelSecurityService.cs) now use a mandatory company FIELD prerequisite. [Shared configuration and interface](../../TheTechIdea.Data/OilGas/PPDMFieldEligibility.cs) belong to TheTechIdea.Data. The [API prerequisite implementation](../../Beep.OilandGas.ApiService/Services/PPDMCompanyFieldEligibility.cs) reads the canonical PPDM ENT_COMPONENT, ENTITLEMENT and reference models through the existing Beep repository. It does not create another grant, interest or ownership store.

For FIELD and FIELD-inherited child paths, effective authority is the lower of the current app grant and current company PPDM entitlement level. No current evidence removes that path from readable predicates; mutation validation refuses its write/delete use. Admission and renewal refuse requested levels above the entitlement. Stored grants, revisions and retirement snapshots remain intact. This supports explicit entitlement downgrades without rewriting historical app authority.

Independent direct WELL/POOL/qualified FACILITY paths keep their existing company relationship and grant checks. A noninheriting FIELD grant adds no prerequisite to an independent child query. Unknown tables still have no reviewed generic ownership policy. IdentityServer remains authentication-only; current app/company roles and endpoint checks remain independent.

## Existing PPDM relationship owners

The checked-out [PPDM 3.9 metadata](../../Beep.OilandGas.PPDM39.DataManagement/Core/Metadata/PPDM39Metadata.json) and canonical models define this first data-entitlement path:

| Evidence | Required relationship |
|---|---|
| ENT_COMPONENT | Exact FIELD_ID and BUSINESS_ASSOCIATE_ID for the selected company; configured ENT_COMP_TYPE and ACCESS_COND_CODE; ACTIVE_IND=Y. Complete unique key is ENTITLEMENT_ID, COMPONENT_ID, COMPONENT_OBS_NO. |
| ENTITLEMENT | Exact component ENTITLEMENT_ID; configured ENTITLEMENT_TYPE; ACTIVE_IND=Y. Header ACCESS_COND_CODE maps to the configured application access level. |
| R_ENT_TYPE | Exact configured ENTITLEMENT_TYPE, active and current. |
| R_ENT_COMPONENT_TYPE | Exact configured ENT_COMP_TYPE, active and current. |
| R_ACCESS_CONDITION | Exact component ACCESS_COND_CODE references ACCESS_CONDITION_CODE, active and current. |
| R_ENT_ACCESS_TYPE | Exact header ACCESS_COND_CODE references ACCESS_TYPE, active and current. |

The component and header access-code columns have different reference owners. They are checked separately; display names and presumed READ/WRITE/DELETE database code values never supply authority. Metadata checks compare canonical model names, primary-key sets and exact referenced-column/table/key triplets before provider access. Multiple legitimate BA-related foreign keys on ENT_COMPONENT are preserved.

This implements data access over the existing entitlement owners. Financial partnership interests, land rights, legal title and operatorship need their own reviewed domain commands and evidence. FIELD_COMPONENT.BUSINESS_ASSOCIATE_ID, WELL.OPERATOR and an interest percentage are not converted into FIELD entitlement. PPDM's published [3.9 subject-area relationships](https://dl.ppdm.org/dl/1881) provide context for the separate entitlement and interest subjects; the checked-out 3.9 models/metadata are the implementation contract.

## Explicit company vocabulary and date interpretation

[API registration](../../Beep.OilandGas.ApiService/Program.cs) binds `PPDM:FieldDataAccess` to PPDMFieldEligibilityConfiguration. It contains Companies entries, each with:

| Property | Deployment review requirement |
|---|---|
| TenantId, BusinessAssociateId | Exactly one entry matching the selected tenant/company pair. |
| TimeZoneId | An explicitly reviewed zone available to the host. |
| EntitlementType | Exact existing R_ENT_TYPE.ENTITLEMENT_TYPE for application data entitlement. |
| ComponentType | Exact existing R_ENT_COMPONENT_TYPE.ENT_COMP_TYPE for this FIELD interpretation. |
| ComponentAccessConditionCode | Exact existing R_ACCESS_CONDITION.ACCESS_CONDITION_CODE describing the reviewed unconditional component interpretation. |
| AccessTypes | Exactly READ, WRITE and DELETE keys, each mapped to a distinct existing R_ENT_ACCESS_TYPE.ACCESS_TYPE. |

There are no production defaults, vocabulary seeds or inferred aliases. Missing, duplicated, incomplete or invalid configuration is an actionable conflict when FIELD authority is requested. Review existing company records and configure their actual reference vocabulary and date zone before FIELD use. This turn neither edits deployment configuration nor fabricates records. Configuration is deployment-owned, captured before provider awaits, and not editable through an unreviewed client endpoint.

Headers and components require EFFECTIVE_DATE. An absent EXPIRY_DATE means no expiry is recorded. Nullable reference boundaries remain open. Effective time is inclusive; expiry is exclusive. UTC values are instants; Unspecified values use the configured company zone. Machine-local, ambiguous DST, invalid DST and inverted/empty ranges are conflicts. Header/component/reference windows are checked again when the lookup returns. With multiple valid paths, only paths still current at return contribute to the maximum level.

Nonblank ENT_COMPONENT.ENT_RULE/EXPIRY_ACTION or ENTITLEMENT.ACCESS_CONDITION/USE_CONDITION/EXPIRY_ACTION require a reviewed interpreter and cause an explicit conflict. Conditional semantics are not executed or guessed by this first unconditional path. Other entitlement vocabularies do not silently map to an app level.

## Datasource and authority checks

Prerequisite reads are server-only and intentionally avoid a circular user-grant lookup. Every component query includes exact field, company, configured type/condition and active predicates. Header/reference reads have exact canonical code/key and active predicates. Beep rows must honor all predicates and canonical cardinality; duplicate keys and more than 256 returned evidence rows are conflicts. This is a checked result limit, not proof of provider-side pagination or bounded network transfer.

The existing MigrationConnectionTarget check validates the cached datasource against its configured target before opening, before and after reads, and before return. Failed open, failed Beep ErrorObject, conversion errors and thrown provider failures remain errors. Current account/company membership, actor and reviewed database binding are re-resolved across await boundaries. The row policy also intersects its initial app-grant snapshot with fresh unchanged grants after entitlement lookups, so revocation or a changed level/revision during lookup cannot retain authority.

The existing [PPDMGenericRepository conversion](../../Beep.OilandGas.PPDM39/Core/PPDMGenericRepository.cs) supports required read columns for these prerequisite keys. Missing or null raw COMPONENT_OBS_NO is refused before constructing its nonnullable decimal property, preventing an absent key from becoming default zero. Explicit zero remains distinct and is preserved. Existing dictionary, DataRow and canonical typed conversion remain one shared path.

The facade obtains effective row levels from the policy instead of promoting them from raw stored grants. Existing SDK responses therefore carry the cap. FIELD scope diagnostics apply that policy too; stored-grant review/retirement remains an administrative inventory and intentionally performs no PPDM eligibility lookup. Hierarchy and CRUD/CSV consumers continue using the same mandatory row policy.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
```

**1122 passed, 0 failed, 0 skipped**, adding 91 cases to the previous 1031; final duration 1 minute 22 seconds. New coverage is 88 [eligibility cases](../../Beep.OilandGas.Repository.Tests/CompanyFieldEligibilityTests.cs) and three [actual SDK/facade cases](../../Beep.OilandGas.Repository.Tests/AccessControlClientTests.cs). They cover complete company entitlement chains and codes, unsupported vocabulary/terms, foreign/inactive/missing evidence, metadata drift, complete/duplicate/numeric keys, provider and cached-target failure, current membership/binding changes, explicit zones and DST refusal, expiry during the last read, multiple entitlement paths, app-grant caps, generic mutation refusal, independent direct versus inherited children, admission refusal, grant/entitlement revocation during reads, diagnostics and raw dictionary/DataRow numeric-key conversion. Existing admission/editor/hierarchy fixtures now include explicit test entitlement records; no production domain defaults are seeded.

The focused gate compiles and executes actual linked policy/admission/store/facade/SDK/diagnostic/PPDM sources, with SQLite app-authority persistence and a recording Beep datasource. SDK round trips use an in-process dispatcher and shared JSON, not a live API host. No Razor source changed; the three-component compile result from Implementation 48 remains historical evidence.

No schema migration, handwritten SQL, live datasource write or deployment is performed. Business/module schema work remains Beep MigrationManager; this slice adds no EF migration or new persistence owner. Missing shared projects are not manufactured and broad host builds are not substituted for this focused gate.

## Outstanding acceptance and next work

Company configuration and entitlement records need provider-specific review and real-data acceptance. Atomic provider snapshots, evidence revocation across tables, long-running operations and simultaneous company/grant/configuration changes are not transactionally proven by this gate. Many FIELD grants can require repeated bounded lookups; provider query cost and selection limits remain acceptance work. Full DI/options binding, HTTP/OIDC/browser, provider upgrades/concurrency, other table/reference owners, remaining local-mode/domain wrappers, shared connection/cache boundaries and release gates remain open.

Next, review canonical INT_SET/partner and land-right owners for explicit legal/operator/JV resource eligibility and commands. Keep financial interests separate from data entitlement. Continue unowned/old-binding app-grant provenance and ordinary grant/revoke history, then authoritative provisioning/physical identity and remaining foundation workflows before CA-04 period close. The root tracker retains these dependencies and no CA/FG phase is closed by this slice.
