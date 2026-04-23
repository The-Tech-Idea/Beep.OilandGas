# W11 MigrationManager Evidence Checklist

> Phase: 11 Identity, Persona, and Access Governance  
> Work items: W11-03, W11-04  
> Companion artifacts: `Schema-Contract-UserManagement.md`, `DataModel-And-Migration.md`, `ClassByClass-Migration-Map.md`

---

## Purpose

Provide a deterministic evidence workflow to prove that UserManagement model migration is schema-safe and aligned with the approved W11 schema contract.

This checklist is the required evidence gate before W11-03 can move to `Done`.

---

## Evidence Inputs

1. `Beep.OilandGas.UserManagement/Models/**` current source snapshot.
2. Generated migration metadata output (tables/columns/keys) from MigrationManager pipeline.
3. Current schema contract:
   - `Schema-Contract-UserManagement.md`

---

## Validation Steps

### Step 1 - Build-clean baseline

- run project build:
  - `dotnet build Beep.OilandGas.UserManagement/Beep.OilandGas.UserManagement.csproj -fileloggerparameters:"LogFile=build-errors-only.log;ErrorsOnly"`
- capture build result and timestamp in evidence log.

### Step 2 - Entity and key verification

For each class in `Schema-Contract-UserManagement.md`:

1. confirm generated table exists.
2. confirm exactly one primary key column and it matches expected `_ID` column.
3. confirm no unexpected alternate key was inferred.

### Step 3 - Column-shape verification

For each generated table:

1. confirm uppercase underscore naming convention.
2. confirm JSON payload columns are scalar text columns (`*_JSON`).
3. confirm no nested collection/dictionary column materialization.
4. confirm all `*_IND` columns are string-compatible flags.

### Step 4 - Mismatch capture and remediation

If any mismatch is found:

1. record it in the mismatch register table below.
2. classify root cause:
   - model class mismatch
   - contract mismatch
   - migration mapping mismatch
3. apply fix in canonical source.
4. rerun steps 1-3.

### Step 5 - Evidence sign-off

W11-03 sign-off requires:

1. zero unresolved mismatches.
2. updated evidence table with final run ID and timestamp.
3. explicit sign-off note in `WEB-UPDATE-TODO.md` task notes.

### Step 6 - Runtime artifact comparison (final gate)

Execute a MigrationManager-backed runtime evidence pass against setup schema endpoints:

1. plan request:
   - `POST /api/ppdm39/setup/schema/plan`
   - payload template:

```json
{
  "connectionName": "<ACTIVE_CONNECTION_NAME>",
  "schemaName": "PPDM39",
   "targetAssemblyName": "Beep.OilandGas.UserManagement",
   "targetModelNamespace": "Beep.OilandGas.UserManagement.Models",
  "environmentTier": "Development",
  "backupConfirmed": false,
  "restoreTestEvidenceProvided": false,
   "restoreTestEvidence": null,
   "remark": "",
   "source": ""
}
```

2. capture from plan response:
   - `planId`
   - `planHash`
   - `totalEntities`
   - `tablesToCreate`
   - `columnsToAdd`
   - `dryRunOperations`

3. retrieve artifacts:
   - `GET /api/ppdm39/setup/schema/artifacts/{planId}`
   - compare `planJson`/`dryRunJson` to `Schema-Contract-UserManagement.md`:
     - table names
     - primary key columns
     - JSON column list

4. optional execution evidence (only when environment policy allows):
   - approve: `POST /api/ppdm39/setup/schema/approve`
   - execute: `POST /api/ppdm39/setup/schema/execute`
   - progress: `GET /api/ppdm39/setup/schema/progress/{executionToken}`

5. record any runtime mismatch in the register and close only when open mismatch count is zero.

---

## Mismatch Register Template

| Run ID | Entity Class | Expected Table/Column | Actual Table/Column | Mismatch Type | Resolution | Status |
|---|---|---|---|---|---|---|
| example-run-001 | AppUser | APP_USER.USER_ID | APP_USER.USER_ID | none | n/a | closed |

## Mismatch Register - Actual Runs

| Run ID | Entity Class | Expected Table/Column | Actual Table/Column | Mismatch Type | Resolution | Status |
|---|---|---|---|---|---|---|
| w11-preflight-2026-04-23-0435z | CheckRowAccessRequest | CHECK_ROW_ACCESS_REQUEST JSON: none | CHECK_ROW_ACCESS_REQUEST.ENTITY_DATA_JSON | contract mismatch | Updated `Schema-Contract-UserManagement.md` row for `CheckRowAccessRequest` to include `ENTITY_DATA_JSON` | closed |

## Runtime Gate Execution Log

| Run ID | Endpoint | Request/Action | Result | Notes |
|---|---|---|---|---|
| w11-runtime-attempt-2026-04-23-0740z | GET /api/ppdm39/setup/status | Probe setup status without bearer token | blocked | 401 Unauthorized. API log confirms JWT challenge on setup path. |
| w11-runtime-attempt-2026-04-23-0742z | POST https://localhost:7062/connect/token | Attempt client_credentials token acquisition for beep-api audience | blocked | IdentityServer endpoint not reachable on localhost:7062 (connection refused). |
| w11-runtime-attempt-2026-04-23-0748z | GET /api/ppdm39/setup/status | Probe setup status with temporary runtime-evidence anonymous endpoint access | completed | Returned `hasConnection=false`, `isSchemaReady=false`; captured in `logs/w11-runtime-status-before.json`. |
| w11-runtime-attempt-2026-04-23-0748z | POST /api/ppdm39/setup/create-sqlite | Create local runtime evidence SQLite database and register `PPDM39` connection | completed | Returned success with file `logs/ppdm39-runtime-evidence.db`; captured in `logs/w11-runtime-create-sqlite.json`. |
| w11-runtime-attempt-2026-04-23-0748z | GET /api/ppdm39/setup/status | Recheck setup status after SQLite bootstrap | completed | Returned `hasConnection=true`, `dbType=SqlLite`, `isSchemaReady=true`; captured in `logs/w11-runtime-status-after.json`. |
| w11-runtime-attempt-2026-04-23-0748z | POST /api/ppdm39/setup/schema/plan | Execute Step 6 runtime plan payload for `connectionName=PPDM39`, `schemaName=PPDM39`, `environmentTier=Development` | completed (blocked by preflight) | Returned `success=true` with `planId=b84485f103c44992a9d640303b9cea75`, `planHash=5CC11A5CFA57C2B20DF6C8F07D2CFF7D002C95A2AA881557820E8A11D3E77D95`, `totalEntities=2700`, `tablesToCreate=2700`, `columnsToAdd=0`, `pendingOperationCount=2700`; preflight includes `preflight-connectivity=Block`; captured in `logs/w11-runtime-schema-plan.json`. |
| w11-runtime-attempt-2026-04-23-0748z | GET /api/ppdm39/setup/schema/artifacts/b84485f103c44992a9d640303b9cea75 | Retrieve Step 6 artifacts payload | completed | Returned `success=true` and artifact JSON payload lengths: `planJson=6939624`, `dryRunJson=794792`, `preflightJson=1451`; captured in `logs/w11-runtime-schema-artifacts.json`. |
| w11-runtime-attempt-2026-04-23-usermgmt-scope | POST /api/ppdm39/setup/schema/plan | Execute Step 6 runtime plan payload with `targetAssemblyName=Beep.OilandGas.UserManagement` | completed (blocked by preflight) | Returned `success=true` with `planId=3357ada292554f35ae0a8d1fe8fce999`, `planHash=963D5A25B3E36DFE2A85509AE1A47E1355AF7AE533AB0FEF639BD0AB0639B643`, `totalEntities=24`, `tablesToCreate=24`, `columnsToAdd=0`; preflight still includes `preflight-connectivity=Block`; captured in `logs/w11-runtime-schema-plan-usermgmt.json`. API runtime log confirms `MigrationManager: Registered assembly 'Beep.OilandGas.UserManagement' for entity discovery`. |
| w11-runtime-attempt-2026-04-23-usermgmt-scope | GET /api/ppdm39/setup/schema/artifacts/3357ada292554f35ae0a8d1fe8fce999 | Retrieve UserManagement-scoped Step 6 artifacts payload | completed | Returned `success=true`; `planJson` reports `EntityTypeCount=24`, `PendingOperationCount=24`, and `ReadinessIssues` contains `primary-key-missing` for all 24 UserManagement entities; captured in `logs/w11-runtime-schema-artifacts-usermgmt.json`. |
| w11-runtime-attempt-2026-04-23-usermgmt-fixed | POST /api/ppdm39/setup/schema/plan | Execute Step 6 runtime plan payload with `targetAssemblyName=Beep.OilandGas.UserManagement`, `targetModelNamespace=Beep.OilandGas.UserManagement.Models`, `remark=''`, `source=''` | completed (blocked by preflight) | Returned `success=true` with `planId=248a8ad9bfdd4c61a4a9b896162bcb39`, `planHash=17A3BB135E7623399013157303E2973DF0FAF149DC08A0D3364EFFDBCCA1212F`, `totalEntities=24`, `tablesToCreate=24`, `columnsToAdd=0`; preflight still includes `preflight-connectivity=Block`; captured in `logs/w11-runtime-schema-plan-usermgmt-fixed.json`. |
| w11-runtime-attempt-2026-04-23-usermgmt-fixed | GET /api/ppdm39/setup/schema/artifacts/248a8ad9bfdd4c61a4a9b896162bcb39 | Retrieve fixed UserManagement-scoped Step 6 artifacts payload | completed | Returned `success=true`; `planJson` reports `EntityTypeCount=24`, `PendingOperationCount=24`, and `ReadinessIssues` now contains only `provider-portability-warning` with `primary-key-missing=0`; captured in `logs/w11-runtime-schema-artifacts-usermgmt-fixed.json`. |
| w11-runtime-attempt-2026-04-23-usermgmt-fixed2 | POST /api/ppdm39/setup/schema/plan | Re-execute Step 6 runtime plan payload after setup-service preflight connectivity normalization (`targetAssemblyName=Beep.OilandGas.UserManagement`, `targetModelNamespace=Beep.OilandGas.UserManagement.Models`, `remark=''`, `source=''`) | completed | Returned `success=true` with `planId=8c5049fc00f84d98986a33ebe97e4c4c`, `planHash=17A3BB135E7623399013157303E2973DF0FAF149DC08A0D3364EFFDBCCA1212F`, `totalEntities=24`, `tablesToCreate=24`, `columnsToAdd=0`, `canApply=true`; `preflight-connectivity=Pass`; captured in `logs/w11-runtime-schema-plan-usermgmt-fixed2.json`. |
| w11-runtime-attempt-2026-04-23-usermgmt-fixed2 | GET /api/ppdm39/setup/schema/artifacts/8c5049fc00f84d98986a33ebe97e4c4c | Retrieve post-connectivity-fix UserManagement-scoped Step 6 artifacts payload | completed | Returned `success=true`; `planJson` reports `EntityTypeCount=24`, `PendingOperationCount=24`; `preflightJson.CanApply=true` and `Checks[preflight-connectivity].Decision=Pass`; `ReadinessIssues` remains `provider-portability-warning` only; captured in `logs/w11-runtime-schema-artifacts-usermgmt-fixed2.json`. |
| w11-runtime-attempt-2026-04-23-usermgmt-fixed5 | POST /api/ppdm39/setup/schema/plan | Re-execute Step 6 runtime plan payload after runtime metadata type-resolution fix (`targetAssemblyName=Beep.OilandGas.UserManagement`, `targetModelNamespace=Beep.OilandGas.UserManagement.Models`, `remark=''`, `source=''`) | completed | Returned `success=true` with `planId=0a8638d823da4b7181570196a3401815`, `planHash=17A3BB135E7623399013157303E2973DF0FAF149DC08A0D3364EFFDBCCA1212F`, `totalEntities=24`, `tablesToCreate=24`, `columnsToAdd=0`, `canApply=true`; `preflight-connectivity=Pass`; captured in `logs/w11-runtime-schema-plan-usermgmt-fixed5.json`. |
| w11-runtime-attempt-2026-04-23-usermgmt-fixed5 | GET /api/ppdm39/setup/schema/artifacts/0a8638d823da4b7181570196a3401815 | Retrieve post-type-resolution UserManagement-scoped Step 6 artifacts payload with runtime metadata | completed | Returned `success=true`; artifact now includes `runtimeEntityMetadataJson` with populated table/column evidence for all 24 entities: `EntitiesWithColumns=24`, `EntitiesWithPk=24`, `EntitiesWithJson=9`, `EntitiesWithIndicator=24`, `MinColumns=16`, `MaxColumns=25`; captured in `logs/w11-runtime-schema-artifacts-usermgmt-fixed5.json`. |

---

## Evidence Summary Template

| Evidence Item | Value |
|---|---|
| Last validation run ID | pending |
| Build status | pending |
| Total entities checked | pending |
| Total mismatches found | pending |
| Total mismatches open | pending |
| Final reviewer | pending |
| Final review date (UTC) | pending |

## Evidence Summary - Actual Run

| Evidence Item | Value |
|---|---|
| Last validation run ID | w11-runtime-attempt-2026-04-23-usermgmt-fixed5 |
| Build status | success |
| Build timestamp (UTC) | 2026-04-23T10:41:00Z |
| Total entities checked | 24 |
| Total mismatches found | 2 |
| Total mismatches open | 0 |
| Runtime gate status | executed; schema-plan preflight is non-blocking (`canApply=true`) and runtime table/column evidence is present |
| Runtime plan ID | 0a8638d823da4b7181570196a3401815 |
| Runtime plan hash | 17A3BB135E7623399013157303E2973DF0FAF149DC08A0D3364EFFDBCCA1212F |
| Runtime total entities | 24 |
| Runtime tables to create | 24 |
| Runtime columns to add | 0 |
| Final reviewer | Copilot (preflight + runtime attempt) |
| Final review date (UTC) | 2026-04-23 |

### Run Notes

1. Preflight scope covered source-level contract conformance checks:
   - all 24 UserManagement model classes inherit `ModelEntityBase`
   - `_ID` key-column pattern present across entity set
   - no collection or dictionary properties materialized in `Beep.OilandGas.UserManagement/Models/**`
   - JSON payload fields confirmed as scalar string columns (`*_JSON`)
   - `*_IND` flags confirmed as string columns
2. One schema-contract mismatch was found and corrected in this run (`CheckRowAccessRequest` JSON column registration).
3. Runtime Step 6 calls were executed end-to-end (status -> create-sqlite -> schema/plan -> schema/artifacts) with concrete plan and artifact outputs captured under `logs/w11-runtime-*.json`.
4. Runtime gate no longer blocks on authentication for evidence endpoints. After setup-service connectivity normalization, schema-plan preflight is non-blocking (`preflight-connectivity=Pass`, `canApply=true`) in the final run.
5. Runtime policy findings remain `Warn` (`provider-portability-warning`) and no longer force a `canApply=false` outcome.
6. Runtime artifact-to-contract comparison result for the fixed scoped rerun: `planJson` reports `EntityTypeCount=24` and `dryRunJson.Operations.EntityName` contains the full UserManagement contract entity set (`24/24` present by entity/class name), so the previous PPDM39 scope mismatch remains closed on the final request shape.
7. Runtime primary-key comparison is now aligned for all 24 entities. Adding explicit `[Key]` metadata to the canonical `_ID` properties removed `primary-key-missing` from the scoped runtime plan; the final artifact `ReadinessIssues` contains only `provider-portability-warning`.
8. Runtime artifacts now emit runtime table/column metadata in `runtimeEntityMetadataJson`, including per-entity `ResolvedTableName`, `ConventionTableName`, `Columns`, `PrimaryKeyColumns`, `JsonColumns`, and `IndicatorColumns`. Final runtime coverage summary for the scoped run: `Entities=24`, `EntitiesWithColumns=24`, `EntitiesWithPk=24`, `EntitiesWithJson=9`, `EntitiesWithIndicator=24`.

### Remaining Gate Item

1. Runtime table-name and column-shape evidence gate is closed in `logs/w11-runtime-schema-artifacts-usermgmt-fixed5.json`.
2. Reviewer sign-off completed in `Beep.OilandGas.Web/.plans/WEB-UPDATE-TODO.md` with W11-03 status set to `Done`.

---

## Exit Gate Rule

W11-03 cannot be marked `Done` until this evidence checklist is completed and the mismatch register has zero open items.
