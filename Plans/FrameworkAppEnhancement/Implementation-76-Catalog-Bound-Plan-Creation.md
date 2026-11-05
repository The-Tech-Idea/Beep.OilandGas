# Implementation 76: catalog-bound corrective-action plan creation

Date: 2026-10-06. Owners: CA-32, FG-40/42. The [root master tracker](../../MASTER-TODO-TRACKER.md) owns overall execution. This binds the [recorded creation review](Implementation-75-Recorded-Plan-Creation-Review.md) to the [existing atomic command](Implementation-73-Atomic-Corrective-Action-Plan-Creation.md) and [public API/SDK](Implementation-74-Reviewed-Corrective-Action-Plan-API-and-SDK.md).

## Delivered behavior

The existing Data-owned `CreateCAPlanRequest` requires an explicit uppercase 64-character `ExpectedRevision`. The API JSON contract requires the field, and the existing SDK rejects missing/malformed revisions before transport. Callers retain the exact creation review; the server never refreshes or substitutes a revision for them.

The canonical command independently rechecks current write/case/state authority and the bounded participant/reference/incident-history catalog. It compares the submitted revision and exact participant observation/role/BA and response-type selections before entering the transaction, again before mutation, and during precommit/postcommit confirmation. Selected and unselected reference changes, participant interpretation changes and inactive response-history additions cannot become an adopted creation intent. Existing prepared-mutation checks still run for each physical write.

The original catalog revision is recorded with the complete participant observation keys in the existing response's structured REMARK provenance. Exact unchanged-effect retry uses the original intent and the existing four-row/audit/full supported graph checks. Only the internal typed-command catalog check excludes its own separately confirmed project response effect when reproducing the original catalog revision; every surrounding response remains fingerprinted and validated. The public creation review still refuses existing project history. Changed payload/revision, substituted project links, missing effects and later child history remain conflicts; there is no historical body or provenance fallback.

No SQL, schema migration, business EF path, duplicate store, grant or alternate mutation owner is introduced. Existing Beep repository/transaction ownership remains authoritative.

## Source owners

- [Existing intent](../../TheTechIdea.Data/OilGas/CreateCAPlanRequest.cs) adds the required revision; [wire submission](../../TheTechIdea.Data/OilGas/CorrectiveActionPlanContracts.cs) continues to reference that intent and the FIELD review.
- [Canonical command](../../Beep.OilandGas.ApiService/Services/PPDMCompanyCorrectiveActionCommands.Plan.cs) and [canonical evidence policy](../../Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.HseCommands.cs).
- [Existing SDK helper](../../Beep.OilandGas.Client/App/Services/Field/HseIncidentClient.cs) freezes the revision with the submitted immutable intent before transport awaits.
- [Command cases](../../Beep.OilandGas.Repository.Tests/CompanyCorrectiveActionPlanCommandTests.cs), [SDK cases](../../Beep.OilandGas.Repository.Tests/InvestigatorClientTests.cs) and [actual HTTP cases](../../Beep.OilandGas.Repository.Tests/CurrentFieldLifecycleGateTests.PlanCreation.cs).

## Verification

The checkpoint reached terminal exit 0: **22 passed**, zero failures/skips, in **6.2171 minutes**. The first affected run reported HTTP creation exceeding the fixture client's default 100 seconds. It was deliberately stopped after that failure and the correction was prepared; the exact session returned terminal exit 1 and no test host remained. It is interrupted failed-source evidence, not a completed gate. The checkpoint covers mandatory revision shape, stale selected/unselected reference/participant/history evidence, unchanged four-row creation/retry and all required JSON members. The final gate also covers drift during insertion, existing rollback/lost-acknowledgment/graph/readback cases and actual SDK/controller/service/facade command integration. Test fixtures retain the original reviewed revision across retries; they do not fetch a new revision during retry.

Logs: `%TEMP%/oilgas-plan-revision-checkpoint.log` and `%TEMP%/oilgas-plan-revision-final.log`. Counts are scoped and overlap historical gates; do not sum them into unique lifetime totals. Native HTTP uses explicit authentication/orchestrator/outer-permission fixtures with real canonical owners, simulated persisting Beep provider and SQLite app authorization. Missing external analyzer projects are not cloned or replaced.

## Remaining gates

Frozen Web creation/picker/pending-intent state, explicit uncertain-outcome reconciliation and circuit/restart recovery; required UI guidance remains unresolved. Durable lifetime intent/ledger/outbox, process/history consistency and qualification/disclosure/jurisdiction/approval/closure remain open. Complete SDK facade/client assembly, actual host/DI/OIDC/browser and physical provider/concurrency/performance acceptance remain open. This slice binds recorded catalog evidence, not a lifetime intent receipt or legal qualification. No phase or feature closes; the full multinational, finance, engineering, operations and release plan remains active.


Executed checkpoint:

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~Catalog_revision|FullyQualifiedName~SDK_plan_creation_requires_the_canonical|FullyQualifiedName~Creation_records_one_atomic|FullyQualifiedName~Actual_HSE_plan_HTTP_creation_requires_every_explicit_JSON_member' --logger 'console;verbosity=normal'
```

Initial affected gate (failed HTTP case, then interrupted; not passing evidence):

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~CompanyCorrectiveActionPlanCommandTests|FullyQualifiedName~InvestigatorClientTests|FullyQualifiedName~Actual_HSE_plan_HTTP|FullyQualifiedName~Actual_HSE_plan_creation_review' --logger 'console;verbosity=normal'
```

The checkpoint preceded two test-only native HTTP stale-catalog cases; production source stayed unchanged. The final assembly was rebuilt only after the checkpoint's terminal exit. Do not rebuild the focused assembly while that test process is live.


## Timeout correction in progress

The initial affected run exposed redundant nested write-authority captures inside each command catalog capture. The command already checks current write authority before/inside transaction, on each prepared mutation and throughout closing confirmation. Its internal catalog path now uses that captured authority solely to reproduce the reviewed revision; the public review still performs its own independent opening/closing write checks. Participant/reference/history catalog capture and closing rereads remain intact. No timeout was raised or predicate weakened. Four new revocation cases target grant and ManageIncidents removal during preparation and precommit catalog confirmation. The old process has reached terminal exit 1 and no test host remains. The corrected source was rebuilt for a six-case authority/unchanged-effect/native-HTTP checkpoint: **6 passed**, zero failures/skips, **5.5667 minutes**, terminal exit 0. All four preparation/closing permission/grant revocation cases passed; unchanged four-row retry and the complete actual HTTP/SDK create/retry/read/action journey passed with the unchanged client timeout. The final affected gate subsequently reached terminal failure as recorded below; it is not certified. The 22-test checkpoint proves the earlier source, not this optimization.


The old affected-run log is retained as `%TEMP%/oilgas-plan-revision-pre-correction.log`. Corrected-source checkpoint log: `%TEMP%/oilgas-plan-revision-authority-checkpoint.log`. The six-case checkpoint targets both grant and permission revocation during preparation/closing, unchanged four-row retry, and the full native HTTP/SDK create/retry/read/action journey. Original transport timeout settings remain unchanged.


Corrected final affected gate (terminal failure; not certified):

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-build --no-restore --filter 'FullyQualifiedName~CompanyCorrectiveActionPlanCommandTests|FullyQualifiedName~InvestigatorClientTests|FullyQualifiedName~Actual_HSE_plan_HTTP|FullyQualifiedName~Actual_HSE_plan_creation_review' --logger 'console;verbosity=normal'
```

The original 22-test checkpoint, interrupted pre-correction run, corrected six-case checkpoint and final affected gate retain separate source scopes and overlap. The six-case checkpoint proves the corrected behavior under fixtures; it is not a physical-provider latency/concurrency or full-host performance acceptance claim.


## Controlled native HTTP fixture scheduling

The corrected six-case checkpoint passed with the original 100-second HttpClient deadline. The broader combined run again exceeded that deadline in the native creation case while provider-heavy command fixtures ran concurrently. That run was deliberately interrupted after the reported failure; its exact session returned terminal exit 1, and no test host remained before rebuilding. Its log is retained as `%TEMP%/oilgas-plan-revision-pre-collection-isolation.log`; it is not passing evidence.

The [native HTTP acceptance collection](../../Beep.OilandGas.Repository.Tests/CurrentCompanyHttpAcceptanceCollection.cs) now runs independently of other xUnit collections. This controls competing fixture workloads while retaining every test case, the original deadline, all assertions and concurrency explicitly exercised within a test. The [actual lifecycle HTTP test class](../../Beep.OilandGas.Repository.Tests/CurrentFieldLifecycleGateTests.cs) uses this collection, and the [focused test project](../../Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj) includes its definition. No domain production source changed after the corrected six-case checkpoint. Production/provider latency and concurrent-user acceptance remain open; the isolated simulated-provider fixture gate cannot establish those.

The final affected gate reached terminal exit 1 with this collection definition: **147 passed, one failed, 148 total, 34.6093 minutes**. All 54 command and 58 SDK cases passed; 35 of 36 native HTTP cases passed. The complete create/retry/read/action journey still exceeded the unchanged 100-second client deadline during initial creation. Collection isolation did not resolve this failure. Request-path profiling and correction remain in progress; no production/provider performance acceptance is claimed. Both interrupted runs, the original 22-case checkpoint and the corrected six-case checkpoint retain their separate source and scheduling scopes. No count from an interrupted run is promoted to a passing gate.


## Prepared-row request profiling and correction

A targeted diagnostic run retained the original HttpClient deadline and instrumented only elapsed time for command authority, selected-reference and catalog captures. It also reached terminal exit 1 during initial HTTP creation: one failed test, 2.6661 minutes. Log: `%TEMP%/oilgas-plan-creation-profile.log`. The trace is simulated-provider fixture evidence, not a production benchmark. Temporary console/timing instrumentation was removed before rebuilding the corrected source.

The per-row `Mutation` already invokes `ContextAsync`, whose selected-reference policy independently captures and closes participant/reference fingerprints. Its evidence callback previously repeated that entire context. Prepared plan rows now close those reference reads with another fresh canonical incident write-authority capture, comparing the original write-authority fingerprint. This retains current membership/database/FIELD/case/state/grant/permission checks after reference reads, the independently closing selected-reference checks, one-use exact payload/actor checks and the owned Serializable transaction. Opening, transaction, catalog and compound-effect closing checks remain unchanged. Other mutation commands keep their existing callbacks.

Two targeted tests revoke the FIELD write grant or ManageIncidents permission during selected-reference reads after the first inserted row. They require refusal before the second row and rollback of the entire saved effect. A nine-case corrected checkpoint covers those two cases, the four catalog preparation/closing revocation cases, the two during-insertion catalog changes and the actual native HTTP/SDK creation/retry/read/action journey. It reached terminal exit 0: **9 passed, zero failures/skips, 11.4708 minutes**. Both mid-reference revocation cases, all four catalog revocation cases and both insertion-drift cases passed. The complete native HTTP/SDK journey passed in 4 minutes 34 seconds with each original 100-second client deadline unchanged. Log: `%TEMP%/oilgas-plan-mutation-authority-checkpoint.log`. The prior 147/148 terminal gate remains failed evidence for the earlier source.


Executed prepared-row correction checkpoint:

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~Prepared_plan_mutation_closes|FullyQualifiedName~Catalog_capture_cannot_keep|FullyQualifiedName~Catalog_change_during_compound|FullyQualifiedName~Actual_HSE_plan_HTTP_and_SDK_create_retry_read_and_later_action' --logger 'console;verbosity=normal'
```


## Corrected affected gate

After the nine-case checkpoint returned terminal exit 0 and no test host remained, the affected gate was started with `--no-build --no-restore` against that same assembly. It targets all **56 canonical plan command**, **58 SDK client** and **36 native plan/review HTTP** cases: **150 passed, zero failures/skips, terminal exit 0, 37.0719 minutes**. The exact running session completed before any affected assembly was rebuilt. Log: `%TEMP%/oilgas-plan-catalog-corrected-final.log`. The earlier terminal 147/148 failure and targeted profiling failure remain evidence for their earlier source scopes. Checkpoint counts overlap this gate and are not unique lifetime totals.

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-build --no-restore --filter 'FullyQualifiedName~CompanyCorrectiveActionPlanCommandTests|FullyQualifiedName~InvestigatorClientTests|FullyQualifiedName~Actual_HSE_plan_HTTP|FullyQualifiedName~Actual_HSE_plan_creation_review' --logger 'console;verbosity=normal'
```

The correction checkpoint proves fixture behavior only. Production/provider latency, concurrent users, full host/DI/OIDC and rendered Web acceptance remain open; neither this checkpoint nor a future focused gate closes a phase or the full implementation goal.
