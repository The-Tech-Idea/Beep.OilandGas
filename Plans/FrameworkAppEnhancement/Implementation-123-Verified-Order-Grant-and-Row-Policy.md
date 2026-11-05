# Implementation 123: Verified order grant and row policy

## Company-admin to maintenance access boundary

Implementation 122 made direct order grants available in the existing Administrator screen and SDK, while server admission and row-policy behavior still lacked execution evidence. This slice compiles and exercises the actual current CompanyWorkOrderFieldReview, PPDMAssetGrantTargetVerifier, RepositoryAssetAccessStore and CompanyPPDMRowAccessPolicy through the existing focused company test target. It supports the administrator -> maintenance order access handoff; it does not certify inspection execution or return to service.

The first focused compilation exposed a missing ICommonColumnHandler import in CompanyWorkOrderFieldReview; the actual existing interface namespace is now imported. Malformed recorded component scope now produces a structured Conflict refusal instead of an unclassified InvalidOperationException. The existing grant store also explicitly refuses inherited WORK_ORDER requests before target lookup or saving, aligning the server with the current UI/SDK direct-order contract.

## Authoritative dependency selection in the focused gate

Skipping unrelated project builds initially selected older transitive Data output, causing missing current contracts. The focused repository target now accepts an explicit FocusedDataAssemblyPath to an actual current Data build, checks that it exists, and removes the older transitive Data project result before reference resolution. The normal project path remains unchanged. The explicit reference uses its full path so assembly resolution cannot prefer an older same-name output. Current WellTestResponse and IFieldProductionService sources are linked in this focused mode to take precedence over stale DTO copies in previously compiled model dependencies. No missing production project, domain contract or provider is replaced with a fake implementation.

## Focused execution evidence

The final affected gate passed **238 tests**, zero failed/skipped, terminal exit 0, reported duration 20 seconds. It selects CompanyAssetGrantTargetTests, CompanyAssetGrantUiTests, AccessControlClientTests and CompanyPPDMRowAccessTests. Log: `%TEMP%/oilgas-order-native-123.log`. Command:

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:BuildProjectReferences=false "-p:FocusedDataAssemblyPath=<actual current TheTechIdea.Data.dll>" --no-restore --filter 'FullyQualifiedName~CompanyAssetGrantTargetTests|FullyQualifiedName~CompanyAssetGrantUiTests|FullyQualifiedName~AccessControlClientTests|FullyQualifiedName~CompanyPPDMRowAccessTests' --verbosity quiet
```

Eleven new order cases execute the actual admission/store/policy owners: a direct WRITE grant requires both recorded FIELD parents; READ-only or revoked second-parent entitlement prevents saving; missing, duplicate and unscoped components refuse; reparenting during entitlement review refuses; effective order access is capped by every current parent; a direct grant downgrade during awaited parent review cannot retain WRITE; and inherited order grant requests cannot save. Existing tests cover affected company/identity/access/UI/SDK behavior. The intermediate grant-only gates passed 25 existing cases and then 35 cases after the first ten order additions. These overlapping totals are not summed with the earlier 316 state/service/SDK cases.

The app-owned RBAC/grant repository executes against SQLite test storage. Beep datasource calls use the existing controlled PPDM fixture with actual metadata and explicit recorded test entitlement rows. Tests mutate those fixture rows and app grants during awaited reads. This proves the selected source paths with those fixtures, not SQL Server/native driver locks, real PPDM data, hosted DI/controller/OIDC, browser roles or production company membership. Existing nullable, source-precedence and enum analyzer warnings remain; no full clean host build is claimed. Whitespace check passed.

## Remaining delivery

Live provider/date-window and concurrent transaction acceptance, actual HTTP grant/check/list commands and browser controls remain open. The order hierarchy still needs truthful multiple-parent presentation, and maintenance duties need approved app-role mapping and standard role guards. Inspection Beep migration/transactions, durable receipts/corrections, independent acceptance, return-to-service/accounting and all other persona journeys remain open. No handwritten SQL, new business EF lane, live grants, deployment or migration execution. No persona, phase or overall goal closure.
