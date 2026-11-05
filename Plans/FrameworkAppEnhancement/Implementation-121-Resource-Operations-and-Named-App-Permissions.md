# Implementation 121: Resource operations and named app permissions

## Workflow finding

Checking the maintenance inspection controls against the current company-role boundary found two distinct facts. Planner, technician and permit-issuer responsibilities in Persona 08 are business duties, not approved seeded ASP.NET role names. Do not invent those role names or treat a maintenance persona preference as authority. Separately, the shared asset check compared a READ/WRITE/DELETE resource operation to the named application permission catalog as well as the resource grant. RepositoryApplicationAuthorizationReader resolves exact app permission identifiers from current selected-company authority; the current catalog uses identifiers such as HSE.View and Facilities.Edit. A valid resource WRITE check should not require an invented app permission named WRITE.

## Existing owner correction

TheTechIdea.Data's existing AssetAccessLevelChecks now identifies requests that require named application permission resolution. UserAssetAccessService uses that distinction: resource operations must meet the actual current resource level; named app permission requests additionally invoke the existing selected-company application permission reader. Unknown names, including a trailing-space WRITE name, remain explicit app permission requests and do not become operation aliases. Missing or unrecognized resource grants still refuse.

The existing closing asset read remains in place for every nonblank request. The service cannot retain a revoked resource level across permission review. This changes neither HasPermissionAsync nor role resolution, and does not grant roles, infer domain approval rights or remove API-owned domain checks. Controllers still need explicit approved role/permission guards for duties such as independent acceptance; direct order access alone does not prove those responsibilities. IdentityServer remains authentication-only.

## Focused evidence

Actual isolated TheTechIdea.Data build: zero warnings/errors, 1.63 seconds. Scripts/Test-WorkOrderListState.ps1: **306 passed**, zero failed/skipped, terminal exit 0, 1.2103 seconds. Log: `%TEMP%/oilgas-resource-operation-121.log`. Ten new actual helper cases distinguish resource operations from named app permissions. Four additional cases execute the actual UserAssetAccessService with strict boundary fixtures: missing grant, inherited FIELD grant, inactive order grant and a direct order grant without independent row authority all refuse. The fixtures verify exact subject/database scope and canonical order policy selection; unexpected datasource and app-permission calls throw. The earlier 292 cases remain green.

An initial focused compilation failed because the new test omitted the existing ICommonColumnHandler namespace; adding the actual interface import corrected it. The final gate compiles current actual access/work-order owners. These service refusal cases do not exercise a successful datasource read, named permission success, native API policy/provider, live grant revocation, transport, authentication, Razor events or browser behavior. They are not proof that inspection commands or a persona journey are delivered. Existing gate nullable/analyzer warnings remain; the isolated Data build is clean.

## Remaining delivery

Complete the approved company app-role mapping and standard role guards for the maintenance duties, plus truthful multiple-parent order access administration. Verify the actual grant -> landing/task -> recorded detail -> inspection requirement/result -> independent acceptance -> return-to-service chain. Implementation 120's API resource wiring remains source-only in this gate. Native Beep migration/provider transactions, durable outcomes/corrections, regional policy and the remaining company persona journeys remain open. No SQL, new business EF lane, live grants, migration execution or phase/persona/goal closure.
