# Implementation 40 — Company asset relationship constraints

Date: 2026-10-05. Owners: CA-03/05/06; FG-11/12/20. Status: implemented with focused evidence; company isolation and broader phases remain partial.

## Problem and resulting behavior

Asset grants and field inheritance could authorize a WELL, POOL or FACILITY record whose existing company relationship pointed to another company in the same reviewed database. The target existence check alone did not prevent that mismatch, and changing the operator after assignment could leave a historical grant readable.

The current company's identifier now constrains WELL.OPERATOR, POOL.BUSINESS_ASSOCIATE_ID and FACILITY.CURRENT_OPERATOR. Existing asset grants remain required. Every grant-based SDK read ANDs this company condition with caller filters and its individual direct/inherited asset predicate; returned rows are checked again. WELL/POOL grant admission and renewal use the same reviewed relationship mapping. Mutations require the selected company on both stored and proposed rows.

These conditions establish the exact recorded company relationship, not legal title or complete operator/JV entitlement. An operator relationship does not independently grant access, and the implementation does not assume cross-company entitlement from field inheritance or a joint-venture label.

## Canonical implementation

| Existing owner | Change |
|---|---|
| [CompanyPPDMRowAccessPolicy](../../Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.cs) | Owns the reviewed company-column mapping shared with admission. Requires one exact canonical foreign key to BUSINESS_ASSOCIATE.BUSINESS_ASSOCIATE_ID; missing, redirected or duplicate metadata refuses execution. Adds fresh company constraints to row postchecks and mutation checks. |
| [IPPDMRowAccessPolicy](../../TheTechIdea.Data/OilGas/IPPDMRowAccessPolicy.cs) | Adds mandatory GetReadConstraintsAsync: all returned conditions are conjunctive, and none independently grants access. No default permissive implementation. |
| [PPDMGenericRepository](../../Beep.OilandGas.PPDM39/Core/PPDMGenericRepository.cs) | Appends fresh copied company constraints to every bounded grant query, preserving caller filters, grant union semantics, exact equality checks and key deduplication. |
| [PPDMAssetGrantTargetVerifier](../../Beep.OilandGas.ApiService/Services/PPDMAssetGrantTargetVerifier.cs) | Uses the same canonical relationship mapping for WELL/POOL admission predicates and exact result postchecks, alongside active company/target verification. |
| [CompanyRowLevelSecurityService](../../Beep.OilandGas.ApiService/Services/CompanyRowLevelSecurityService.cs) and [shared results](../../TheTechIdea.Data/OilGas/IRowLevelSecurityService.cs) | Diagnostics expose CompanyConstraints separately from OR ReadPredicates and caller Filters. Consumers must AND both condition lists with each grant predicate. Empty grant predicates still deny all. The service rechecks request/company scope before returning the plan. |

Business data continues through the existing Beep repository and API gateway. Application RBAC, membership, database target and access registry owners are preserved. No domain entity, alternate grant store, ownership registry, SQL query or migration is added.

## Boundaries and historical records

- WELL requires OPERATOR equal to the selected BUSINESS_ASSOCIATE_ID. POOL requires its BUSINESS_ASSOCIATE_ID equal to that company. Field-inherited FACILITY requires CURRENT_OPERATOR equal to that company. Missing or differently cased identifiers cannot widen an exact match.
- Caller filters asking for another company remain conjunctive with the mandatory selected-company condition; they cannot replace it. SDK providers that ignore the condition cannot return data past policy/equality postchecks.
- Historical direct/inherited grants cannot bypass these constraints. An observed operator/company change blocks reads and renewal. The registry retains its existing history until explicit revoke; this is not an automatic data migration or grant backfill.
- Generic mutations cannot change the relationship to another company, remove it, or operate on a stored row belonging to another company. Existing action-level and field reparenting guards remain required. CSV uses that same repository mutation policy; partial import evidence is retained and no atomic-import claim is made.
- FIELD has no reviewed direct company column. Its admission still proves active company/field existence and its row path still uses current explicit grants. FIELD entitlement and historical FIELD-grant review remain unresolved. Operating one well does not establish authority over a whole field.
- Direct FACILITY grant admission remains refused until the canonical shared contract supplies its complete composite identity. Field-inherited facility reads additionally enforce CURRENT_OPERATOR; this does not complete facility entitlement.
- Non-operator/JV access is not inferred from generic FIELD_COMPONENT/BA_COMPONENT links or guessed type codes. Explicit interests, their effective dates and approved scoped access remain future work using the existing owners.

## Focused verification

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger 'console;verbosity=minimal'
```

Result: **495 passed, 0 failed, 0 skipped**, including **57 new cases** in [CompanyAssetRelationshipTests](../../Beep.OilandGas.Repository.Tests/CompanyAssetRelationshipTests.cs). The real SQLite company/Identity/access registry, PPDM metadata JSON, API admission/store/policy/generic service and PPDM repository execute against a recording Beep datasource. Coverage includes:

- Foreign, missing and case-mismatched WELL/POOL relationships refusing admission; provider results ignoring company filters refusing persistence.
- Missing, redirected, wrong-key and duplicate relationship metadata for WELL/POOL/FACILITY refusing provider execution.
- Company constraints on every direct/inherited SDK conjunction, deduplication and unchanged caller filters.
- Company switches with identical field/asset IDs in one datasource returning only the selected relationship; historical/inherited grants and caller filters cannot expose another company.
- Provider filter bypass and company relationship changes during a read refusing returned data.
- Actual generic WELL/POOL insert/update refusal; mutation-policy checks on existing/proposed WELL/POOL/FACILITY rows, including deletes and missing company values.
- Existing grant reads and renewal after relationship changes; preserved registry history and revoke cleanup.
- Actual CSV executor allowing the first company-owned row and refusing the following foreign-operator row; the partial write is explicit.
- Diagnostic company conditions, preserved caller assertions and deny-all without an asset grant.

Existing positive row fixtures now explicitly supply their company relationship, and their query assertions account for the mandatory condition. Prior authority-only grant helpers still mock admission to seed historical registry records; new admission cases execute the real verifier. These boundaries are intentional and do not claim a live provider or integrated host run.

The identical-ID switch cases deliberately give the recording datasource rows with repeated canonical keys to prove predicate/postcheck behavior. They do not prove that a physical PPDM schema can store both records: its existing primary-key constraints remain authoritative. Real database topology and independent-company identical-ID storage still require the planned provider/provisioning evidence.

Actual shared Data, Models and PPDM projects build in the focused invocation. Existing missing-analyzer warnings remain. Complete HTTP/auth-owner/DI/OIDC/browser/client execution, real provider behavior, legal/JV semantics, cross-datasource atomicity and concurrent changes after the final check remain unverified. No full regression, deployment or phase closure is claimed.

## Next work

Review FIELD/company relationships and legal/operator/JV interests, including historical grant eligibility; implement complete facility identities through canonical contracts; then continue remaining table/reference/domain policies, connection/cache scoping and historical library authority retirement. Physical identity/provisioning, authoritative period close, finance/engineering/operations/regional requirements and release gates retain their existing task owners.

Business/module schemas remain Beep Migration; the existing Identity/access repository remains EF. No handwritten SQL or live database migration was performed.
