# Implementation 59 — company production request and failure boundary

Date: 2026-10-05. Execution owners: CA-03/05/06/30, FG-10/11/12/20/30/40. [Master tracker](../../MASTER-TODO-TRACKER.md) owns execution status. This continues [Implementation 58](Implementation-58-Company-Retirement-Request-Boundary.md) through the existing registered `PPDMProductionService`. The wider plan remains active.

## Problem and resulting behavior

The production owner used a global/default connection and constructed repositories without required company row policy. Field operations and WELL calculations could receive resource parameters independently of the current profile selection. Four calculation paths replaced a missing actor with SYSTEM. Equipment-read and analysis-storage failures were reported as degraded but suppressed; missing storage metadata also returned without saving. Callers could receive an analysis after a failed save or after the recorded rod string could not be read.

The existing owner now requires the profile selection service and row policy. All 27 public asynchronous operations capture and check the existing reviewed account/company/database/FIELD/revision. Company-wide section reads require reviewed company evidence without requiring a selected FIELD; the supported FIELD/POOL reads have focused positive evidence. They use current company grants and predicates and never create a preference. FIELD-specific operations require exact current selection. Supplied actor assertions must match the actual profile account; required mutation actors cannot be missing. Four optional calculation actors use the reviewed server account, with no SYSTEM substitute. WELL operations check exact current FIELD membership before reading calculation inputs, including the intervention method without a FIELD parameter.

All 38 repository constructions resolve the connection from the same profile review and pass mandatory row policy. Public success returns recheck that review. A changed preference/revision, account, company, database or FIELD cannot be adopted within the request. This is a request fence through the existing profile owner, not another persisted selection or authorization model.

The API production registration and orchestrator phase factory supply the required dependencies. Those complete Program/orchestrator/host paths remain source-only. The focused target compiles the complete actual production class and actual mapping/calculator sources; it does not replace the production implementation with a test class.

## Failure behavior and reporting ownership

The six handled-failure catches now log and rethrow the original failure. Failed equipment reads do not substitute a uniform rod string in those error paths; malformed stored choke evidence does not continue to another/default record after that caught error. The four storage helpers explicitly refuse missing WELL_EQUIPMENT metadata instead of returning an unsaved success. The public calculation methods already await those helpers and propagate errors.

Because this owner no longer consumes handled-failure reporting, its direct `IFailureReporter` dependency is removed. Unhandled errors remain owned by the existing API error/reporting path; no replacement failure abstraction, diagnostics project or production helper is invented. Full registered-host reporting execution is not established by the focused tests. The genuine no-record/engineering-default paths still require the separate numerical/domain repair below.

## Focused source and runtime evidence

The complete production service compiles with its real mapping service, existing shared models, the real GasChokeCalculator/Choke validation/constants/exceptions, ZFactorCalculator and constants, and the real SuckerRod load/power/API11L calculator sources. The focused project links those actual files; no synthetic phase interfaces or calculator stand-ins are supplied. The source editor used Roslyn to preserve conditional return control flow and exclude lambda/local-function returns when adding the success fence. It is a temporary development tool, not an app runtime path. Four known missing-analyzer warnings remain; zero compile errors.

The 55 production cases use the actual profile, registry/account/company services, row policy, mapping and complete production implementation. Beep rows and metadata faults are an explicit recording-provider boundary. The operation matrix exercises every FIELD entry point and supplied/required actor boundary before unintended phase work. Positive company FIELD/POOL reads work without a current selection, retain real recorded rows, use only the reviewed database and enforce current company/asset predicates. Actual provider callbacks changing the preference revision refuse success; an authorized WELL in another FIELD cannot supply calculation inputs.

Four missing-storage and four storage-failure cases invoke the actual private helpers with explicit result objects; they establish exception behavior, not numerical calculation or successful persistence. An actual rod-string helper failure preserves its original exception instead of returning an invented string. The initial gate passed 53 cases and failed two because the sizing fixture omitted its actual target-flow parameter. That fixture invocation was corrected; the failed checkpoint is not acceptance evidence.

The final affected gate passed **249 tests, 0 failed, 0 skipped, in 1 minute 53 seconds**. It combines these 55 cases with the 194 retirement/current-FIELD/profile/selector/UI/MVC/PPDM policy cases. The root tracker and `%TEMP%/oilgas-production-final-affected.log` own final counts. Build log: `%TEMP%/oilgas-production-source-build.log`; initial test log: `%TEMP%/oilgas-production-boundary-focused.log`. Cases also link into the normal API test target, whose full external-dependency/host build remains unverified. No new all-inventory checkpoint, production OIDC/claims bridge, live provider, browser or numerical certification is implied.

Reproduce the affected gate:

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --filter 'FullyQualifiedName~CompanyProductionBoundaryTests|FullyQualifiedName~CompanyRetirementBoundaryTests|FullyQualifiedName~CurrentFieldLifecycleGateTests|FullyQualifiedName~CompanyPPDMRowAccessTests|FullyQualifiedName~CompanyFieldSelectionTests|FullyQualifiedName~FieldSelectionTests|FullyQualifiedName~FieldSelectionEndpointTests|FullyQualifiedName~RepositoryAccountProfileTests'
```

## Domain and release work remains required

- Complete reviewed company/resource ownership paths for production, reserve, WELL_ACTIVITY, WELL_TEST, WELL_EQUIPMENT, forecast, facility and recovery child/reference tables. Mandatory policy currently refuses tables without a reviewed path; this is not production feature completion.
- Repair the existing dictionary-based WELL_EQUIPMENT storage paths to canonical typed entities, complete actual keys/FKs and approved reference codes, transactions/readback/durable outcome handling. A failed or unavailable save now propagates; successful analysis persistence is not proven.
- Remove remaining fabricated no-data rates/status/history/engineering defaults and adopt approved parameter, date, currency/unit and provenance owners. Validate numerical correlations and input ranges through existing calculation owners. Actual-source compilation is not a numerical acceptance result.
- Bind the existing development, HSE, exploration and process owners/direct registrations to reviewed connection/resource/row authority. Complete retirement extension metadata/setup/resource/mapping/domain/transaction acceptance through Beep MigrationManager and existing owners. Compile/exercise the actual orchestrator and host with real dependencies.
- Complete API/Web auth/standard-role/company middleware, typed transport, production HTTP/browser/circuit/provider and multi-process/concurrent revocation acceptance; continue durable lease status/full-interpretation/history, creation fencing/reconciliation and wider accounting, governance and release work.

No SQL, schema migration, live mutation or deployment is introduced. Business/module schema work remains Beep MigrationManager-owned; the existing approved EF identity/profile registry remains its owner. No production feature, phase, full plan, numerical accuracy or production readiness is marked complete.

Follow-up: [Implementation 60](Implementation-60-Company-Development-Request-and-Failure-Boundary.md) continues through the actual development owner, with complete source/calculator/contract compilation and 90 new boundary/failure cases in the final 339-case affected gate. It does not close production, development or wider domain/provider/release work above.
