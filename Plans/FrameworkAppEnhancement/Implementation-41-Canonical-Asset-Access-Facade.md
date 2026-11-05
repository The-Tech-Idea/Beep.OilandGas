# Implementation 41 — Canonical asset access facade

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/11/12/20. Status: implemented with focused evidence; complete company/domain/client/release gates remain open.

## Problem and resulting behavior

The active UserAssetAccessService read FIELD children using PPDM repositories without the canonical row policy. Its access checks could also accept a direct registry grant without confirming a current stored record. The new company constraints in Implementation 40 therefore did not cover this facade. Inherited FACILITY results supplied only FACILITY_ID, losing FACILITY_TYPE from the composite key.

The facade now derives accessible assets from active stored PPDM rows through the existing scoped repository/policy. Direct and inherited results require the subject's current grants, current company relationships and the reviewed database binding. Missing/inactive/foreign rows cannot turn a registry record into an access decision. Unknown ownership paths refuse access. Facility results include FacilityType, and facility checks require both key parts. Ambiguous historical direct facility grants cannot authorize a facility type.

## Existing owners and implementation

- [UserAssetAccessService](../../Beep.OilandGas.LifeCycle/Services/AccessControl/UserAssetAccessService.cs) uses a mandatory subject-policy factory and the existing PPDMGenericRepository. It queries only reviewed FIELD/WELL/POOL/FACILITY models with ACTIVE_IND="Y" and the same direct/inherited grant union and company constraints as generic API reads. The unused PPDMMappingService dependency and unscoped child-query branches are removed.
- [CompanyPPDMRowAccessPolicy](../../Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.cs) accepts an optional trusted subject selector supplied by API orchestration. Its normal generic-data instance still uses the actor. Subject grants resolve through the same canonical asset store, which rechecks self/current company Administrator and subject membership. Another user's grants are never replaced with the actor's grants. The selector does not change company/database identity or authorize a caller by itself.
- [Program](../../Beep.OilandGas.ApiService/Program.cs) constructs subject policies from the current CompanyModuleConnection, existing registry and metadata. This registration is source evidence; integrated host DI/startup is not verified.
- Before and after datasource reads, the facade preserves the existing MigrationConnectionTarget validation of configured and cached source identity. It rechecks module scope and subject grants before projecting results. It derives the strongest currently applicable READ/WRITE/DELETE level without allowing inheritance when includeInherited=false.
- Permission-qualified checks require both the application permission and current stored asset access. A positive permission resolution triggers a fresh asset read, so an earlier grant snapshot cannot retain revoked access or a former access level across that await boundary. Provider failures propagate explicitly; they are not reported as access denial or success.
- [PPDMGenericRepository](../../Beep.OilandGas.PPDM39/Core/PPDMGenericRepository.cs) now refuses a reported Beep provider failure immediately after a scoped read, before checking or returning rows. Valid-looking or empty results do not override Errors.Failed.
- [AccessControlController](../../Beep.OilandGas.ApiService/Controllers/AccessControl/AccessControlController.cs) forwards the canonical facility key qualifier. Existing self/Administrator UI/API guards and independent fresh store/policy authority remain in force.

## Shared contracts and facility boundary

The active contracts move into TheTechIdea.Data, retaining their existing namespaces and the single ModelEntityBase owner:

- [IAccessControlService](../../TheTechIdea.Data/OilGas/IAccessControlService.cs).
- [AssetAccess](../../TheTechIdea.Data/OilGas/AssetAccess.cs), [AccessCheckRequest](../../TheTechIdea.Data/OilGas/AccessCheckRequest.cs), [AccessCheckResponse](../../TheTechIdea.Data/OilGas/AccessCheckResponse.cs) and [grant/revoke requests](../../TheTechIdea.Data/OilGas/AccessControlRequests.cs).

The former Models declarations are removed, not copied or forwarded. FacilityType is an additive request/result property and an optional service check argument. For FACILITY, AssetId identifies FACILITY_ID and FacilityType identifies FACILITY_TYPE. Returned facilities with the same ID but different types remain distinct. A single-ID facility check refuses before reading; a qualifier on another asset type also refuses.

This completes facility identity in the facade's inherited read/check result, not direct facility grant persistence or all hierarchy/client consumers. Existing incomplete direct facility grant records remain readable as registry history elsewhere and revocable; this facade excludes them from access decisions. The registry remains the grant-history owner, while accessible-asset results describe current stored rows.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
```

Result: **538 passed, 0 failed, 0 skipped**, including **43 new cases** in [CompanyAssetFacadeTests](../../Beep.OilandGas.Repository.Tests/CompanyAssetFacadeTests.cs). Actual company/Identity/access persistence, canonical PPDM metadata and the real facade, API policy/store/admission and PPDM repository execute with a recording Beep datasource. Coverage includes:

- Active company-owned WELL/POOL/FACILITY inheritance; missing, inactive and foreign direct records cannot prove access.
- Direct-only selection, strongest current levels, complete composite facility results/checks and refusal of ambiguous direct facility history.
- Providers ignoring company filters, stale/ambiguous/missing cached targets and target changes during a read.
- Binding/membership/grant revocation during reads, fresh levels after reads, self versus authorized subject inspection and refusal of ordinary cross-subject inspection.
- Required current application permissions across all access levels, and grant revocation/downgrade/binding changes during permission resolution.
- Actual facade grant/revoke through the real selected-scope admission verifier/registry; unresolved scopes and reported provider failures remain explicit failures.
- Unknown paths, misapplied facility qualifiers and changed scope after reading.

Two obsolete API-only fixtures, AssetStorageBoundaryTests and AssetInheritanceAuthorizationTests, are retired. They asserted raw registry/unscoped anonymous-child behavior and denial on datasource failures. Their relevant scope, inheritance, permission and cached-target gates are replaced by the actual focused cases above. Existing API/Web service doubles and Moq calls are updated for the canonical check contract; those broader suites were not rerun. Isolated grant seeding in the new fixture explicitly mocks admission except the actual facade grant/revoke cases, which use the real verifier.

Shared Data, Models and PPDM projects build in the focused invocation. Existing missing-analyzer warnings remain. No full host HTTP/auth-owner/DI/OIDC/browser run, live provider proof, transactional snapshot or broad regression is claimed. Runtime races after final checks remain a provider/concurrency gate.

## Findings and next work

The active SDK access client still issues GET for check-access where the controller requires POST/shared request/AccessCheckResponse. Its asset-list route uses ppdm39access instead of AccessControl, and its permission route differs from the canonical controller. Its profile URL matches UserProfileController, but the response is untyped instead of using the existing shared UserProfile contract. AccessMode local service branches remain in that client. Align those methods through the existing captured-company transport and remove local access fallback with focused HTTP contract evidence. Full HTTP/controller integration remains separate.

Remaining hierarchy/domain wrappers still contain PPDM reads and single-ID facility nodes/checks; those need canonical row-policy and complete-identity integration before promoted end-to-end hierarchy access. Complete direct facility grant identity/persistence, FIELD entitlement, legal/operator/JV interests and historical-grant review remain open. No guessed relationship codes, ownership registry or cross-company defaults are introduced.

Physical identity/provisioning, remaining domain/reference/cache paths, historical authority retirement, authoritative period close and all finance/engineering/operations/regional/release tasks retain their existing owners and gates. Business/module schemas remain Beep Migration; the existing Identity/access registry remains EF. No schema change, handwritten SQL or live database migration was performed.
