# Implementation 66 — corrective-action participant assignment

Subsequent action addition: [Implementation 67](Implementation-67-Atomic-Corrective-Action-Addition.md) extends this same command owner with atomic step/OPEN-status/responsible-participant creation and explicit caller intent. Its own focused gates own addition acceptance; the historical counts below remain scoped to assignment. The full plan remains active.

Date: 2026-10-06. Execution owners: CA-03/05/06/32, FG-10/11/12/20/40/42. The [root master tracker](../../MASTER-TODO-TRACKER.md) owns execution status. This continues [Implementation 65](Implementation-65-Corrective-Action-Due-Date-and-Completion-Commands.md); the full plan remains active.

## Problem and resulting behavior

The actual assignment method changed BUSINESS_ASSOCIATE_ID in place even though it belongs to the six-column PROJECT_STEP_BA primary key. A new assignment assumed ROLE_SEQ_NO = 1 instead of using its recorded PROJECT_BA_ROLE. Generic HSE mutations were fenced, and there was no owned compound transaction, complete historical readback or retry reconciliation.

The existing Data-owned [typed command contract](../../TheTechIdea.Data/OilGas/ICompanyCorrectiveActionCommands.cs) now owns assignment as well as due-date/completion. The existing [ApiService command implementation](../../Beep.OilandGas.ApiService/Services/PPDMCompanyCorrectiveActionCommands.cs) independently requires current app HSE.ManageIncidents/HSE.View, inherited entitled FIELD WRITE, exact current actor/company/database/FIELD/revision and the canonical incident/project/action/status relationship. The requested BA must already be an actual unique active project participant with one current approved RESPONSIBLE role and recorded role sequence. Missing, ambiguous, foreign, expired or fractional role evidence refuses; the command never creates a participant or invents a role sequence.

Assignment requires recorded OPEN status. It can repair a missing responsible assignment using the explicit requested participant, but cannot approve/close an incident or reassign a completed action. Missing due dates are not fabricated during assignment; recorded step/status facts remain unchanged.

## Complete keys, transactions and confirmation

- The existing [table registry](../../Beep.OilandGas.PPDM39/Core/PPDM39TableRegistry.cs), metadata, company row policy, bounded evidence reader, canonical repository and common-column handler retain ownership. No duplicate model/store/reference vocabulary or direct client database path is added.
- PROJECT_STEP_BA uses PROJECT_ID, BUSINESS_ASSOCIATE_ID, ROLE, ROLE_SEQ_NO, STEP_ID and STEP_BA_OBS_NO. Reassignment clones the old row and retires it with ACTIVE_IND = N without changing any key; a new complete-key row records the requested participant and its actual role sequence. The old created audit and payload are preserved, with actual changed audit from the canonical handler.
- Observation allocation includes active and inactive responsible-assignment history for the exact project/action. The bounded inventory must have complete unique keys and explicit Y/N activity. Allocation beyond the supported integer range refuses. It does not reuse an inactive identity or assume observation one.
- Retirement plus insertion use the existing [owned Beep transaction lane](../../Beep.OilandGas.PPDM39/Core/PPDMDataSourceTransaction.cs) and require an actual held Serializable transaction/physical connection. Private one-use mutation permits bind exact type/table/operation, complete original/prepared scalar/audit snapshots, current authority and target evidence. Generic HSE/PROJECT writes stay fenced. No SQL or parallel transaction owner is introduced.
- The retirement permit validates the exact original active assignment through the existing row policy; only its command-private prepared inactive clone can retire it. The new assignment is independently validated through canonical action/BA/project-participant/approved-role/window relationships. Neither a generic inactive-row write nor a FIELD grant can authorize reassignment.
- Fresh context checks preserve actual step/status snapshots and the target BA/member/role snapshots before mutation and confirmation. Before commit and after acknowledgment, the command checks the full saved historical inventory, the exact active assignment and actual audit columns. Provider OK, wrong payload, extra/missing rows, changed role or lost history cannot establish success.
- The same current participant/role is a no-write retry under fresh authority, owned transaction and complete readback. Lost commit acknowledgment reports an unconfirmed outcome; a retry reconciles the actual recorded effect without another history row. This remains recorded-effect reconciliation for the existing operation; general durable intent/outbox and cross-process provider acceptance stay open.

The current-field HSE facade now exposes AssignCorrectiveActionResponsibleAsync, and the actual HSEController adds `PUT /api/field/current/hse/incidents/{incidentId}/cas/{stepSeq}/responsible`. The [Data-owned request](../../TheTechIdea.Data/OilGas/AssignCorrectiveActionResponsibleRequest.cs) accepts only BusinessAssociateId. All fifteen actor-bearing HSE controller operations now require the existing [RepositoryRequestAccount](../../Beep.OilandGas.ApiService/Services/RepositoryRequestAccount.cs) active-account gateway, matching the current-FIELD guard; no caller actor or external subject is accepted. The canonical shared claims helper source was unavailable in the sibling checkout, and no replica/fallback was added. The facade/route is additive; existing method contracts remain. HSEController constructor wiring now requires that existing account gateway; direct controller tests use its actual fixture request context. Program already owns its registration. Full-host DI remains unverified.

The actual CorrectiveActionService now delegates assignment to this required command owner. Assignment-only direct repository/sequence helpers were removed. The previously registered command/facade/orchestrator use the same owner; no extra DI path is added. Full host and concrete orchestrator registration remain source-only until their planned gates pass.

## Verification

**Core command/relationship gate: 493 passed, 0 failed/skipped, in 20 minutes 35 seconds**, including all 53 command cases (26 new assignment cases and the previous 27 due-date/completion/transaction cases). The process exited 0; the focused rebuild had zero errors and eight known warnings. Log: `%TEMP%/oilgas-ca-assignment-final.log`.

**Final facade/HTTP gate: 240 passed, 0 failed/skipped, in 3 minutes 42 seconds.** This compiles the complete actual HSEController/facade/typed-command/refusal-handler sources and includes all eight new assignment HTTP cases, all 187 actual HSE boundary cases, 38 existing current-FIELD/lifecycle cases and seven controller cases. The process exited 0 with zero compile errors and nine known warnings: four absent external analyzer references, four existing HSE nullable assignments and the existing refusal-handler unnamed-enum warning now exposed by source linkage. Log: `%TEMP%/oilgas-ca-assignment-http-final.log`.

**Final active-account gate: 49 passed, 0 failed/skipped, in 33 seconds.** It covers 45 refusal cases across all fifteen HSE write actions (inactive, unresolved and anonymous accounts), plus the four controller regression cases. The process exited 0; production source is unchanged from the facade/HTTP gate. Log: `%TEMP%/oilgas-hse-account-actions-final.log`. The focused rebuild has zero errors and the same nine known warnings.

The initial four-case checkpoint passed, zero failed/skipped, in 2 minutes 39 seconds: actual reassignment/history/audit/no-write retry, first assignment from inactive history, and both compound write failures. The rebuild reported twelve known warnings (four absent external analyzer references, four existing HSE nullable assignments and four existing generated Models member-hiding warnings), zero errors. The later target-freshness recheck and three additional cases are verified by the 493-case core gate above; this four-case checkpoint alone is not acceptance. Log: `%TEMP%/oilgas-ca-assignment-checkpoint.log`.

CompanyCorrectiveActionCommandTests uses the actual HSE/CA service, current app profile/RBAC/eligibility/policy, actual canonical repository/common-column handler and shared owned transaction helper. Its explicit persisting test provider keeps full primary keys and an undo ledger; it is not a production shim or driver certification. New cases also refuse changes to captured participant-role payload during policy validation, bounded-history overflow and observation-allocation exhaustion. New cases cover reassignment with a recorded role sequence other than one, first assignment/history allocation, complete-key history/audit, no-write retry, both write failures, target membership/reference/window ambiguity, role/permission/status/history drift, provider OK without exact persisted data and lost-acknowledgment reconciliation. Due-date/completion and current authority/facade/relationship checks remain in the final affected gate.

Reproduce the final affected gate:

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger "console;verbosity=normal" --filter "FullyQualifiedName~CompanyCorrectiveActionCommandTests|FullyQualifiedName~CompanyCorrectiveActionAuthorityTests|FullyQualifiedName~CompanyHseBoundaryTests|FullyQualifiedName~CompanyHazopAuthorityTests|FullyQualifiedName~CompanyHseRowAccessTests|FullyQualifiedName~CurrentFieldLifecycleGateTests|FullyQualifiedName~CompanyPPDMRowAccessTests"
```

The shared lease transaction helper/fixture was not changed in this slice, so the prior lease checkpoint remains historical evidence and is excluded from this affected gate. No new hierarchy/selector/all-inventory test claim is made.

The first runnable HTTP/facade checkpoint passed 235 of 237 cases, including all eight new assignment HTTP cases; two direct controller fixtures had no active HttpContextAccessor in their calling execution context. Their synchronous setup now binds the actual request to the existing gateway. The 240-case final facade/HTTP gate verifies that correction and supersedes this failed checkpoint. Log: `%TEMP%/oilgas-ca-assignment-http-checkpoint.log`.

The HTTP verification uses actual MVC routing/model binding, the current-FIELD filter, HSEController, facade, company profile/RBAC/row policy/command/repository and refusal handler, over loopback Kestrel. Authentication and concrete orchestrator restoration are explicit fixture boundaries; it does not establish production OIDC/claims transformation or full-host DI. The existing lifecycle HTTP fixture now selects the actual controller surface; it does not add another host helper. Legacy HSE public Models contracts retain the planned Data consolidation work.

Reproduce the facade/HTTP and final active-account gates:

```powershell
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger "console;verbosity=normal" --filter "FullyQualifiedName~CurrentFieldLifecycleGateTests|FullyQualifiedName~CompanyHseBoundaryTests|FullyQualifiedName~HSEControllerTests"
dotnet test Beep.OilandGas.Repository.Tests -p:FocusedCompanyTests=true -p:UseBeepDMSource=false -p:GeneratePackageOnBuild=false --no-restore --logger "console;verbosity=normal" --filter "FullyQualifiedName~HSEControllerTests"
```

The facade/HTTP gate preceded the expansion from three to 45 account-refusal cases. The final account gate checks the expanded current test source; no production-source edits followed the facade/HTTP build. These scopes overlap and must not be summed as unique tests or presented as an all-inventory/full-host checkpoint.

Documentation validation: nine updated documents decode as UTF-8, all 268 local links resolve and no pending verification placeholders remain. `git diff --check` passes.

## Next acceptance and remaining plan

Complete canonical corrective-action project/incident-response creation and action addition with explicit approved response/step semantics, complete-key allocation, owned Beep transaction, intent/reconciliation and exact readback. Do not derive project ownership from app-looking IDs, invent response types or silently create project participants. Continue restricted incident/case/attachment visibility and incident/RCA/barrier/HAZOP approval/closure prerequisites.

Real Beep driver isolation/ownership/rollback/readback/date/audit/concurrent cross-process evidence, current permission changes across stores, full host/API/auth/standard-role/Web/browser gates, inventory/performance and canonical Data contract consolidation remain open. Other scientific/unit/calendar/jurisdiction, exploration/process, production/development/retirement, lease/history/circuit, finance/accounting and regional hosting/localization recommendations retain their existing master-plan owners. No feature/CA-32/phase/full-plan closure.

No handwritten SQL, new business EF/schema migration, live mutation or deployment. Business/module schemas use Beep MigrationManager; the approved EF identity/access/profile registry retains its current owner.
