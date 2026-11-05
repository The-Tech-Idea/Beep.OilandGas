# Implementation 39 — Asset grant target verification

Date: 2026-10-05. Owners: CA-03/05/06; FG-11/12/20. Status: implemented with focused persistence/SDK evidence; company isolation and release remain partial.

## Problem and resulting behavior

The canonical asset grant store checked company authority, membership and database scope, but could admit an arbitrary asset identifier without checking the selected PPDM datasource. New grants and renewals now require an exact active company BUSINESS_ASSOCIATE record and an exact active FIELD, WELL or POOL record in the current reviewed PPDM binding before changing the existing grant registry.

An absent/inactive record, inconsistent metadata, ambiguous/provider-mismatched result, unavailable datasource or provider failure refuses admission. A failed renewal preserves the existing level and audit/concurrency values. Revoke remains available for cleanup after a target disappears. No new grant table, domain model, migration or general data access path is introduced.

This proves existence and active status only. It does not prove legal ownership, operator authority, joint-venture participation or company entitlement to assign that asset. Those remain required company-isolation gates.

## Implementation

- [PPDMAssetGrantTargetVerifier](../../Beep.OilandGas.ApiService/Services/PPDMAssetGrantTargetVerifier.cs) is the mandatory API admission dependency. It resolves the current CompanyModuleConnection, checks the reviewed fingerprint and current company Administrator, and validates canonical model/key metadata before opening the configured datasource.
- Two bounded reads use the existing PPDMGenericRepository and Beep AppFilter predicates: selected BUSINESS_ASSOCIATE_ID plus ACTIVE_IND="Y", then the canonical target key plus ACTIVE_IND="Y". Existing repository conversion supports typed, dictionary and DataRow results. Exact type/key/active-status checks reject provider results that widen or ignore predicates; duplicate results are refused. Datasource open state and reported failures are checked.
- The admission lookup is server-only and privileged under current company administration. It does not apply user asset grants to these two reads, because admission of an initial grant cannot depend on that grant already existing. It exposes no arbitrary-table/query endpoint.
- [RepositoryAssetAccessStore](../../Beep.OilandGas.ApiService/Services/RepositoryAssetAccessStore.cs) verifies before adding or updating the existing deterministic grant record. It rechecks database binding, administrator identity and subject membership after verification. Company/binding or authority changes observed during SDK reads cannot persist a grant.
- [Program](../../Beep.OilandGas.ApiService/Program.cs) registers the verifier with the canonical store. The production dependency is mandatory; there is no permissive fallback implementation. Complete host DI execution remains unverified.

## Canonical identity and ownership inventory

The existing PPDM model/metadata owners remain authoritative; no parallel ownership registry or guessed relationship-code policy is added.

| Existing model | Canonical identity | Existing relationship evidence | Current admission behavior |
|---|---|---|---|
| [BUSINESS_ASSOCIATE](../../Beep.OilandGas.PPDM.Models/39/BUSINESS_ASSOCIATE.cs) | BUSINESS_ASSOCIATE_ID | Selected company's existing PPDM identity | Requires one exact active record |
| [FIELD](../../Beep.OilandGas.PPDM.Models/39/FIELD.cs) | FIELD_ID | GROUP_FIELD_ID points to FIELD; no direct company/operator column | Requires exact active field; company ownership remains open |
| [WELL](../../Beep.OilandGas.PPDM.Models/39/WELL.cs) | UWI | OPERATOR references BUSINESS_ASSOCIATE; ASSIGNED_FIELD references FIELD | Requires exact active well; operator/legal eligibility remains open |
| [POOL](../../Beep.OilandGas.PPDM.Models/39/POOL.cs) | POOL_ID | BUSINESS_ASSOCIATE_ID and FIELD_ID relationships | Requires exact active pool; relationship entitlement remains open |
| [FACILITY](../../Beep.OilandGas.PPDM.Models/39/FACILITY.cs) | FACILITY_ID + FACILITY_TYPE | CURRENT_OPERATOR references BUSINESS_ASSOCIATE; PRIMARY_FIELD_ID references FIELD | Direct grant admission refused: the current single-string asset contract cannot identify the complete key |

Existing FIELD_COMPONENT and BA_COMPONENT relationships include company/field/component and effective/expiry information. Their relationship semantics have not established legal/operator eligibility. A company operating one well does not establish authority over its entire field. Existing ENT_SECURITY_BA/ENT_COMPONENT/ENT_GROUP records do not replace application-owned RBAC or revive the removed PPDM scope authority.

Field-inherited facility row access remains governed by the existing reviewed row policy. Historical direct facility grants can still be revoked; this slice neither invents a composite-key encoding nor promotes incomplete facility identities.

## Focused verification

Command:

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
```

Result: **438 passed, 0 failed, 0 skipped**, including **25 new cases** in [CompanyAssetGrantTargetTests](../../Beep.OilandGas.Repository.Tests/CompanyAssetGrantTargetTests.cs). Real company/Identity/access persistence and the actual verifier, grant store, PPDM metadata and repository execute against a recording Beep datasource. Cases cover:

- FIELD/WELL/POOL grants with exact active predicates, selected connection and persisted audit; no SDK writes.
- Missing/inactive company or asset, wrong-case identifier/wrong-company/inactive results despite filters, duplicates, failed open, provider exception and failure flag.
- Inconsistent canonical key metadata, incomplete direct facility identity and wrong database fingerprint.
- Binding changes, removed Administrator, expired actor/subject membership during reads, failed renewal preserving state and subsequent revoke cleanup.
- Dictionary and DataRow results through the existing repository converter.

Earlier focused store/row-policy fixtures explicitly mock this new admission dependency to retain their isolated authority/policy scope. Their prior 413 checks do not independently prove target verification. The new 25 cases execute the real mandatory verifier. Wider API/LocalDB constructor fixtures were updated but were not run as an integrated host suite.

Actual Models, PPDM and shared Data projects build in this focused invocation. Existing missing-analyzer warnings remain. Full host HTTP/auth-owner/DI startup, OIDC/browser flows, supported live providers and atomic consistency with concurrent PPDM mutations are unverified. No full regression or deployment claim is made.

## Remaining work

Implement legal/operator/joint-venture grant eligibility from reviewed existing PPDM relationships, and define complete facility identities in the canonical shared contract before direct admission. Grant listing still reads the registry; this slice does not retroactively validate or backfill historical grants. Existing row reads still use the reviewed row policy and actual datasource records. Historical-grant eligibility review is required before company isolation can be claimed.

Remaining table/reference/domain policies, shared connection/cache scope, historical library authority retirement, physical database identity/provisioning and complete provider/HTTP/client evidence stay open. Admission checks and registry writes are not a cross-datasource atomic transaction; observed authority changes are refused, while races after the final check still require provider/concurrency evidence.

Business/module schemas remain on Beep Migration; the existing Identity/access repository uses EF. This slice adds no schema migration, handwritten SQL or live database mutation. All broader finance, engineering, operations, regional and release phases retain their existing owners and gates.
