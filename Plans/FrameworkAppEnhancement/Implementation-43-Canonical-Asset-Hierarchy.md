# Implementation 43 — Canonical asset hierarchy

Date: 2026-10-05. Owners: CA-03/05/06; FG-01/11/12/20. Status: implemented with focused hierarchy, controller, persistence and Beep read evidence; company phases and integrated release gates remain open.

## Problem and resulting behavior

The active hierarchy read PPDM rows without a row policy before filtering them for a user. Caller-supplied roots and missing assets could become invented nodes, facility paths used only FACILITY_ID, and path validation checked individual access without verifying their relationships. Configuration used an untyped PPDM repository for repository-owned security data that is excluded from module provisioning.

The existing [AssetHierarchyService](../../Beep.OilandGas.LifeCycle/Services/AccessControl/AssetHierarchyService.cs) now requires a subject-bound canonical PPDM policy, current company authority and a reviewed database binding. Reads use PPDMGenericRepository with mandatory active-row filters, existing grant predicates and WELL/POOL/FACILITY company constraints. Missing or inaccessible roots produce null; paths and children contain only visible stored assets. Provider failures and inconsistent provider results remain explicit failures/refusals.

The supported graph uses FIELD relationships to WELL.ASSIGNED_FIELD, POOL.FIELD_ID and FACILITY.PRIMARY_FIELD_ID. It does not invent POOL-to-FACILITY-to-WELL edges. FACILITY_ID and FACILITY_TYPE are preserved together. A granted child can remain visible without conferring access to its parent; the returned path includes the visible stored portion. The ORGANIZATION container identifies the selected company for presentation and is never an asset grant or a fabricated PPDM business record.

## Ownership and API boundaries

- [IAssetHierarchyService](../../TheTechIdea.Data/OilGas/IAssetHierarchyService.cs), AssetHierarchyNode, HierarchyConfig, GetAssetHierarchyRequest and ValidateAccessRequest move from Models to TheTechIdea.Data, preserving namespaces and removing their old definitions.
- The node gains FacilityType; root selectors gain rootFacilityType, and child/path selectors gain facilityType. The existing [controller](../../Beep.OilandGas.ApiService/Controllers/AccessControl/AssetHierarchyController.cs) and [binding wrapper](../../Beep.OilandGas.ApiService/Services/BoundAssetHierarchyService.cs) forward these complete keys.
- [CompanyModuleConnection.ResolveHierarchyCompanyAsync](../../Beep.OilandGas.ApiService/Services/CompanyModuleConnection.cs) uses fresh app-owned company authority. Organization, children, path and config operations require a current selected-company Administrator. Subject operations require self or a current company Administrator and active subject membership. Controller checks and service factories use the same gate; incoming role claims cannot substitute for it. Standard ASP.NET role attributes remain on protected routes.
- organizationId is an assertion of the current selected company. It cannot select a different company or a global hierarchy. Current binding, configured/cached datasource target and authority are checked around reads and before returning results. Accumulated rows are validated again so an earlier table cannot retain a grant revoked during a later read.
- Path validation reads actual subject-visible rows and verifies adjacent stored relationships. Individually accessible but unrelated, reversed, duplicated or invented chains cannot return true. Optional supplied parent identities are assertions, not authoritative relationships. Facility qualifiers remain mandatory.

## Configuration limitation

GetHierarchyConfig and UpdateHierarchyConfig now explicitly return a conflict after company authorization and binding checks, without PPDM configuration reads or writes. The existing repository documentation classifies ORGANIZATION_HIERARCHY_CONFIG as repository-owned and excluded it from module provisioning. The typed repository configuration implementation, provisioning and atomic audited commands remain open. This slice removes the active untyped path rather than claiming that configurable hierarchy management is implemented. The existing admin configuration UI will receive that explicit refusal until the canonical store is delivered.

Business/module schemas remain owned by Beep MigrationManager; the existing Identity/access registry retains its approved EF ownership. No schema changes, handwritten SQL, live migration or provisioning operation occurred.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
```

Final result: **704 passed, 0 failed, 0 skipped**. Log: `%TEMP%/oilgas-hierarchy-final-tests.log`.

The [new hierarchy cases](../../Beep.OilandGas.Repository.Tests/CompanyAssetHierarchyTests.cs) exercise actual services, controller methods, company gate, SQLite Identity/access registry, canonical PPDM metadata, row policy and recording Beep SDK. They cover company/active/relationship filtering, missing/inactive/foreign roots, complete facilities with equal IDs and different types, direct-child access without parent promotion, actual path relationships, malformed/invented paths, subject policy separation, organization assertions, unavailable configuration, provider/filter failures, grant/membership/Administrator/binding/cached-target changes, late revocation of an earlier table and complete-key forwarding. Existing binding-wrapper cases are included. Together these add 90 cases to the previous 614 checks.

This is source-linked production-service/controller execution, not a full ASP.NET HTTP/OIDC/DI host, browser or live PPDM provider test. Equal facility IDs with different types use recording rows and prove selector/key behavior, not physical provider provisioning or uniqueness guarantees. Shared Data, Models and PPDM projects build in the focused invocation; missing-analyzer warnings remain. Full client/host dependency failures were not chased.

## Remaining work

Integrate the existing Web asset tree and config experience with selected-company assertions, complete facility selections and stale-response protection; no SDK hierarchy service exists in the current client surface. Implement the canonical repository-owned hierarchy config store with its provisioning and audited atomic commands. Continue the remaining domain/local-mode/API-gateway and shared connection/reference/cache audit.

Direct facility grant persistence, FIELD entitlement, explicit legal/operator/JV interests, historical-grant review, remaining table/domain ownership policies and physical identity/provisioning remain open. Current operator/business-associate relationships do not establish legal ownership. Cross-store transactional snapshots, provider concurrency, full HTTP/auth-owner/DI/browser, finance/engineering/operations/regional workflows and release evidence retain their existing plan owners. No phase is marked complete.
