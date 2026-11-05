# Implementation 35: company PPDM row authorization

Date: 2026-10-05. Owners: CA-03/05/06 and FG-11/12/20. Extends Implementation 34's canonical company asset registry. Status: partial foundation implementation with focused evidence; no company isolation, phase or release closure.

## Result

The existing PPDMGenericRepository now accepts the API's shared IPPDMRowAccessPolicy contract, owned by TheTechIdea.Data. Generic CRUD, the concrete CSV import executor and CSV export controller supply the same scoped CompanyPPDMRowAccessPolicy through their existing repository construction. Program registers that policy against the existing IUserAssetAccessStore; no additional asset store, repository implementation or database routing path was added.

The policy resolves current company authority and the immutable reviewed PPDM_CORE binding through CompanyModuleConnection. Grants come from RepositoryAssetAccessStore for the selected account, company and database fingerprint. A company Administrator role alone does not authorize business rows. Grant levels have explicit cumulative semantics: READ permits reads, WRITE permits reads/inserts/updates, DELETE also permits deletions. FIELD inheritance requires the grant's Inherit flag. This is asset-grant authorization; it does not establish legal/operator ownership or physical existence of the granted asset.

The initial reviewed table policies use the actual PPDM model and checked-in metadata:

| Table | Read/write authorization paths | Identity |
|---|---|---|
| FIELD | Direct FIELD grant on FIELD_ID | FIELD_ID |
| WELL | Direct WELL grant on UWI, or inherited FIELD grant on ASSIGNED_FIELD | UWI |
| POOL | Direct POOL grant on POOL_ID, or inherited FIELD grant on FIELD_ID | POOL_ID |
| FACILITY | Inherited FIELD grant on PRIMARY_FIELD_ID | FACILITY_ID + FACILITY_TYPE |

FACILITY's ambiguous direct asset ID is refused until a reviewed composite grant identity exists. POOL has a single key in the canonical metadata; POOL_TYPE does not become a guessed second key. Entity/table names, declared primary keys, string ownership columns and inherited references to FIELD.FIELD_ID must match the reviewed policy. Other tables refuse company generic access until their ownership/reference policy is reviewed. There is no arbitrary-column inference or unscoped reference-table allowance.

## Shared repository checks

Scoped reads validate/copy caller filters, then issue separate SDK AppFilter conjunctions for each authorized predicate. The union is deduplicated by complete metadata primary key. No SQL, SQL-fragment IN predicate or handwritten provider statement is generated. Supported caller comparisons are =, <>, >, >=, < and <= on actual model properties. Empty grants issue no provider read. More than 256 unique predicates refuses the selection; production query scaling/paging remains open.

Company authority, binding and grants refresh before each provider query and after every result, including the combined output. Returned rows must retain current asset access and satisfy every required equality filter; incomplete keys and providers ignoring those filters refuse the result. Caller filters cannot replace the mandatory ownership predicate. Remaining comparison correctness and real provider collation/capability behavior require provider tests.

Single inserts/updates/deletes check the required asset level immediately before provider access. Updates/deletes load the previous row through the same scoped path using every primary key, validate its identity, and check both previous/proposed authorization. Generic reparenting is refused even when both field grants are held. The generic service now checks the route ID against the update body's single primary key; composite updates require a separate reviewed command. Soft delete requires DELETE before its existing common-column/update path. Hard deletion uses the same selected company policy.

Direct UnitOfWork access and existing direct batch mutation methods refuse on a policy-bound repository. Batch commands need explicit scoped implementation rather than bypassing checks. Related repository construction propagates the policy; FK queries use the same guarded method. An unreviewed referenced table therefore refuses validation rather than reading all rows. This limits imports until reference policies are reviewed. CSV row insertion uses the checked single-insert path; each row can fail after earlier rows committed. CSV import remains explicitly partial, not transactional/atomic.

CSV admission/execution requires the existing DataManagement.ImportData permission; export admission requires DataManagement.ExportData. Queued execution independently checks its captured actor/company/database and import permission. These complement asset checks and captured company-role/permission freshness. Complete endpoint/domain permission mappings remain a rollout gate.

## Focused evidence

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false
```

**294 passed, zero failed/skipped**: previous 263 plus 31 row-access cases. The focused target now references the actual PPDM39 and Models projects, removing overlapping Models source links. Actual PPDMGenericRepository, canonical PPDM models and checked-in metadata execute with the source-linked production generic API service, concrete CSV executor and policy. Company authority/grants use real SQLite EF access persistence and actual RepositoryAccessService. Business IDataSource is a recording mock of the real SDK contract, not an invented SDK interface or real relational provider.

Cases cover same-tenant company switching, no-grant/no-administrator bypass, direct/inherited grants and levels, separate SDK predicates/deduplication, real pool/facility identity, corrupt key/reference metadata, filter injection/overrides/copying, ignored provider filters, access/binding revocation during reads, write/delete levels, route/ownership/actor/missing-row refusals, batch/UnitOfWork refusal, provider write failure, and actual CSV permission/per-row refusal with explicit partial writes. The PPDM library builds with zero errors; missing analyzer project warnings remain. No unrelated host dependency stubs were created. `git diff --check` passes.

Program and the export/admission controller wiring are source changes; complete HTTP/DI, browser/facade and hosted worker/controller execution were not run. API controller fixtures were updated for the mandatory policy/permission constructor path. Existing missing host projects remain outside the authorized focused gate.

## Remaining acceptance work

Extend the same policy with reviewed reference/domain table ownership and composite identities, explicit asset creation/reparent commands, grant-assignment ownership/existence evidence and joint-venture/operator rules. Review all other repository construction/default connection paths and shared caches before claiming company-wide row isolation. Other domain repositories still need the company policy and their action permission mappings; their internal framework usage is not proven by this gateway slice.

Provider filtering/collation/pagination/transaction/concurrency, revocation atomicity during a provider write, in-flight connection lifecycle, provider cancellation and import recovery remain open. No transaction is claimed between the EF access registry and business datasource. Review global role-definition administration next: a company Administrator must not be able to change shared role definitions for other tenants. Legacy user/JWT scopes and primary-role writes also remain open.

Business/module schemas remain on Beep Migration. This slice changes no schema, adds no SQL/migration and applies no live database changes. Physical identity/provisioning, finance, engineering, operations, multinational policies and full release evidence remain planned.
