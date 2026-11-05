# Implementation 73: atomic corrective-action plan command

Date: 2026-10-06. Task owners: CA-32, FG-40/42 and the existing HSE/corrective-action owners. The [root master tracker](../../MASTER-TODO-TRACKER.md) remains authoritative. This slice extends the existing command, policy, evidence reader, PPDM repository and owned Beep transaction; it adds no schema, SQL, business EF lane, grants, scheduler or domain store.

## Result and boundary

`ICompanyCorrectiveActionCommands.CreatePlanAsync` now provides actual compound creation rather than inferring a plan ID or relying on a preexisting project. The shared `TheTechIdea.Data` request requires a stable caller plan GUID, explicit bounded plan name/number, an actual response-type reference and the complete recorded incident party/role/BA identity. App-user identity is never substituted for a PPDM business associate.

The existing incident write-authority check is moved into the canonical policy and reused by investigator and new-plan commands: fresh actor/company/database/FIELD review; actual app HSE.View/HSE.ManageIncidents; inherited FIELD WRITE and data entitlement; restricted-case authority; unique current permitted UTC incident state and actual state vocabulary. Participant/crew/person/reference evidence uses the existing participant policy. Project type, response type and responsible role must have current actual reference records and exact canonical metadata.

Inside one owned physical Serializable transaction, the command inserts PROJECT, PROJECT_BA, PROJECT_BA_ROLE and HSE_INCIDENT_RESPONSE in dependency order. It uses one server UTC effective instant and the existing audit handler. A new project's responsible role starts at sequence 1; the original incident party/role keys are retained as explicit command provenance on the response link, without copying private incident remarks. Response sequence allocation includes active and inactive incident history. The supported project/action graph and global project links are bounded, compared before transaction allocation and read back together with surrounding response history. Exact prepared mutations reuse the existing private one-use command policy; generic HSE writes remain fenced.

A stable-ID retry requires the unchanged exact four-row effect, original actor/source/GUID/audit, explicit contents and current authority/reference evidence. Lost commit acknowledgment can be reconciled this way without duplicate writes. Changed intent, missing rows, foreign links, later child history and a different plan ID refuse rather than adopting an existing effect. This is conservative unchanged-effect reconciliation, not a durable lifetime intent receipt. A current project link to any other project also requires reconciliation before creation; support for multiple independent project links must be designed explicitly.

## Verification

The initial command checkpoint passed all **37 tests**, zero failed/skipped, duration **9m01s**, terminal exit 0. It covered atomic dependency order and actual plan resolution, exact creation retry/conflicting intent, all four partial-write rollbacks, lost commit acknowledgment, active/inactive sequence allocation, reference/participant/permission/state/restriction refusals, graph collisions, provider substitution, evidence drift and transaction capability gates. That checkpoint predates the closing inventory comparison, exact inventory metadata checks and removal of the incident command's now-unused asset-store constructor parameter.

Three additional tests exercise malformed inventory metadata, provider OK without a participant effect, and a one-shot stored plan change after its first read. The final-source affected gate passed **186 tests**, zero failed/skipped, duration **28m24s**, terminal exit 0. It includes all **40 plan-creation cases**, existing investigator commands, existing corrective-action commands and actual investigator HTTP commands. All three additional cases passed; the closing one-shot effect change was detected before commit and rolled back. Command and policy production source remained unchanged after that gate. No broader or repeated test run is needed for this slice. 

Fixtures use real command/policy/metadata/repository/transaction code with an explicitly simulated persisting Beep datasource and existing SQLite app authorization store. They do not certify a production driver, physical locking/concurrency, constraints or complete application host.

Executed checkpoint:

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter FullyQualifiedName~CompanyCorrectiveActionPlanCommandTests --logger 'console;verbosity=minimal'
```

Executed final gate:

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~CompanyCorrectiveActionPlanCommandTests|FullyQualifiedName~CompanyIncidentCommandTests|FullyQualifiedName~CompanyCorrectiveActionCommandTests|FullyQualifiedName~Actual_HSE_investigator_HTTP_command' --logger 'console;verbosity=minimal'
```

Initial source compilation found a fixture property-name error; the added provider-OK fixture subsequently found two namespace qualification errors. All were corrected in tests. Missing external analyzer project warnings were left as recorded; no external projects were cloned or replaced. The earlier checkpoint and final gate overlap; never sum them as unique tests. Logs are `%TEMP%/oilgas-plan-command-checkpoint.log` and `%TEMP%/oilgas-plan-command-final.log`; their executed sessions reached terminal completion before another build. Known missing-analyzer-project, nullability and refusal-switch warnings remain; the final focused source compilation had no errors.

Final documentation check: nine updated UTF-8 documents, **317 local file links with none missing**, and `git -c core.safecrlf=false diff --check` passed. No schema or live-data operation was performed. The full goal remains active; this command slice does not close CA-32 or any phase.

## Source owners

- [Shared creation request](../../TheTechIdea.Data/OilGas/CreateCAPlanRequest.cs) and [existing command contract](../../TheTechIdea.Data/OilGas/ICompanyCorrectiveActionCommands.cs).
- [Compound command](../../Beep.OilandGas.ApiService/Services/PPDMCompanyCorrectiveActionCommands.Plan.cs) extends the [existing command/mutation owner](../../Beep.OilandGas.ApiService/Services/PPDMCompanyCorrectiveActionCommands.cs).
- [Shared canonical write/reference preparation](../../Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.HseCommands.cs) extends the [existing row policy](../../Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.cs), with the [investigator command](../../Beep.OilandGas.ApiService/Services/PPDMCompanyIncidentCommands.cs) delegating the moved gate.
- [Focused compound command tests](../../Beep.OilandGas.Repository.Tests/CompanyCorrectiveActionPlanCommandTests.cs) and existing command/HTTP fixtures linked by the existing focused test project.

## Remaining work

- Delivered subsequently in [Implementation 74](Implementation-74-Reviewed-Corrective-Action-Plan-API-and-SDK.md): actual corrective-action service/Field facade/HTTP creation with required reviewed submission, explicit read-only resolution, typed SDK and removal of the misleading process creation call. This document retains the earlier command-only verification scope.
- Add typed SDK/client review, frozen creation intent and read-only outcome reconciliation. Rendered Web integration still needs the missing required UI skill paths or the pending explicit substitute-guidance decision described in [Implementation 72](Implementation-72-Investigator-Outcome-Review-State.md).
- Durable lifetime intent/reconciliation/ledger/outbox, workflow/history consistency, participant qualification/disclosure, new-party creation, configuration administration, attachments and approval/closure remain open.
- Certify actual provider transaction isolation, concurrency, FK behavior and capability differences. Audit unsupported project child owners before expanding creation/replay graph coverage. Full API host/DI/OIDC/browser/scientific/multinational/finance/engineering/operations/release acceptance remains open; no phase closes.

## Public integration follow-up

[Implementation 74](Implementation-74-Reviewed-Corrective-Action-Plan-API-and-SDK.md) now connects the public creation service/facade/POST to this existing command, separates explicit read-only resolution and adds mandatory caller FIELD review plus current-linkage response and typed SDK operations. The command/policy production source and this document's 186-case historical gate remain unchanged. Rendered/client-host/provider/lifetime intent and full workflow gates remain open; the root tracker owns current proof.
