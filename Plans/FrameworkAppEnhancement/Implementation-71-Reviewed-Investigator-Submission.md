# Implementation 71 — reviewed investigator submission

HSE investigator tab recovery (2026-10-27): the existing investigator state now retains the exact review and participant in protected tab-session storage before sending, restores it after page disposal, and blocks a second assignment while the result is unconfirmed. A changed roster alone no longer clears that original; the current recorded investigator must match the original participant before clearing. Protected keys bind company, account, database, FIELD and incident; another original cannot replace or clear it on the same store instance. The page loads storage after interactive render. Focused actual protected-storage, screen, state and incident tests pass 35/35 with controlled API and JS storage. This is tab-session recovery input, not durable server-side submission proof; cross-circuit concurrency, real browser, native provider and historical attribution remain open.


HSE investigator assignment screen (2026-10-27): the incident list links by encoded query identity to an authenticated investigator workspace using the existing company/FIELD-bound InvestigatorAssignmentState and typed API. Users review recorded participant role observations, select an exact row, submit only with current HSE.ManageIncidents permission, then review the recorded outcome before another change. The shared state keeps an uncertain outcome for the life of the page; it is not durable across disposal. Focused actual screen, existing state and incident list tests pass 33/33 with controlled API/FIELD data, including encoded identity and Arabic participant, permission refusal, late responses and outcome review. The native command was not exercised by this gate; live provider/browser and durable recovery remain open.


Date: 2026-10-06. Continues [Implementation 70](Implementation-70-Recorded-Investigator-Assignment.md). CA-03/05/06/32; FG-10/11/12/20/40/42. The [root master tracker](../../MASTER-TODO-TRACKER.md) owns the complete plan; no phase closes.

## Problem and resulting behavior

The previous BA-only assignment request could reverse an intervening assignment when an old request was sent again. It also rejected a BA with multiple current recorded role observations because the caller could not select the complete party/role key. The existing assignment route now requires a Data-owned typed body with the exact party observation, role observation, BA identity and expected review revision. There is one assignment owner and no automatic BA-only compatibility path.

The new read-only investigator review uses the existing incident/facade/command and canonical case/participant policy. Its SHA-256 revision binds the actual account/company, physical database, selected FIELD and selection revision, case/read interpretation, recorded incident state/reference, actual FIELD write grant identities/revisions and FIELD entitlement, complete active/inactive participant history and canonical current participant reference/crew evidence. Current recorded choices expose only their complete party/role keys, BA IDs and roles; the current investigator is identified separately. No app-user, contact details, qualification or employment evidence is inferred.

A submission independently checks current app HSE.View/HSE.ManageIncidents, exact case visibility, FIELD write/data entitlement and selected context. It compares the required revision before starting a transaction, then rechecks complete history and participant/reference evidence inside the owned Serializable Beep transaction before writing. Exact key/BA mismatches refuse. Existing compound retirement/insertion, role history allocation, prepared-reference/audit fences and complete roster readback remain the sole mutation path. Explicit full-key selection permits the same BA in more than one recorded party/role; identity alone no longer chooses a role.

Reusing the original submission after a successful assignment returns a conflict because its reviewed roster changed. The old request also refuses after an intervening assignment; it cannot restore the earlier person. A fresh read-only review confirms the recorded current investigator. A freshly reviewed submission selecting that same recorded party performs no new write. After an uncertain commit, the caller must review the actual result before another submission. This is optimistic concurrency and outcome review, not a durable assignment-intent ledger.

## Source/API owners

- `TheTechIdea.Data/OilGas/InvestigatorAssignmentReview.cs`: shared review, minimal complete-key participant choice and assignment body. `JsonRequired` prevents incomplete JSON contracts.
- `TheTechIdea.Data/OilGas/ICompanyIncidentCommands.cs`: extends the existing typed command contract with read-only review and reviewed submission.
- `Beep.OilandGas.ApiService/Services/PPDMCompanyIncidentCommands.cs`: existing command/review authority and shared complete history reader; no competing store or command path.
- `Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.cs`: captures the existing canonical participant/reference roster evidence, with closing case/roster/reference checks.
- `IHSEService`, `IFieldHSEService`, `HSEIncidentService` and `PPDMHSEService`: actual existing owners carry the shared review and body through their independently reviewed FIELD/account boundaries.
- `TheTechIdea.Data/OilGas/InvestigatorAssignmentReviewValidator.cs`: shared context/revision/full-key validation and exact reviewed selection.
- `Beep.OilandGas.Client/App/Services/Field/HseIncidentClient.cs`: typed SDK HTTP operations using the existing company-captured transport. It validates returned account/company/incident/FIELD/keys/revision/current-role evidence, freezes choices and request bodies, refuses unreviewed selections and discards late acknowledgments after switching away and back. It never fetches a new revision or retries a mutation automatically.
- Existing `IFieldService`/`FieldService`: delegates the new typed operations to that same helper via the existing remote transport; no local datasource or parallel command path. The thin helper is compiled/tested below; the complete facade/client assembly and DI/browser wiring remain source-only.
- `HSEController`: authenticated `GET /api/field/current/hse/incidents/{incidentId}/investigator-review` and the existing `PUT /api/field/current/hse/incidents/{incidentId}/investigator` now requiring the reviewed JSON body. Actor and connection remain server-owned. The former `?baId=...` request without a body is not accepted. No consumer of the former BA-only SDK/Web investigator request was found. The existing Field SDK facade now provides typed investigator review/submission through its new internal HTTP helper; UI/process integration remains required.

Example body uses values from the actual review, without manufacturing a revision:

```json
{
  "expectedRevision": "<revision from the current investigator review>",
  "partyObservation": 17,
  "roleObservation": 4,
  "businessAssociateId": "<recorded BA for that exact role>"
}
```

The body requirement changes the previous request contract intentionally; deployment/version coordination remains open before release. No SQL, new business EF path, business schema change, live migration, reference/grant seeding or deployment. Beep MigrationManager remains the business schema owner; existing identity/access registry EF ownership is unchanged.

## Verification

The initial two-case checkpoint compiled the actual new shared contract and existing incident/facade/policy/command sources and passed both cases, zero failed/skipped, terminal exit 0 (38s). It verifies read-only complete-key review and explicit selection of a BA with multiple recorded roles. Log: `%TEMP%/oilgas-investigator-review-checkpoint.log`.

The first affected run found a test-helper request-context ordering failure: it constructed the service before awaiting the new review, and resetting HttpContextAccessor inside the helper cleared the caller context. The run was intentionally interrupted (terminal exit 1), and no full-gate pass is claimed. The helper now completes review before constructing the assignment owner, preserving the real authorization check. The corrected affected gate finished with **423 passed, one failed, zero skipped, 424 total**, terminal exit 1 (16m39s). All 52 command, 117 HSE row, 192 HSE boundary and 49 controller/account cases passed, together with 13 actual review/assignment/body HTTP cases. Its only failure was the new BA-only HTTP fixture expecting 400: ASP.NET correctly returned 415 because that request has no JSON body/content type, and no write occurred. The fixture now expects 415 for that request, while null/incomplete JSON bodies still require 400. Log: `%TEMP%/oilgas-investigator-review-affected-corrected.log`.

No production API/incident/facade/command/row-policy source changed after this broad gate. Subsequently added sources are the shared SDK review validator and Field SDK HTTP helper/delegation. The final SDK/body/transport gate compiled these actual shared-validator/helper sources and passed **69 cases**, zero failed/skipped, terminal exit 0 (9s): all 19 SDK cases, all three corrected body-refusal HTTP cases and 47 existing company/PPDM transport cases. Log: `%TEMP%/oilgas-investigator-review-sdk-final.log`. This verifies escaped routes, required wire members including explicit null current investigator, context/key/revision/role rejection, one frozen write without retry under lost acknowledgment, stale company-generation response refusal and the corrected 415/400 body refusals. The broad gate's fixture failure is superseded for that behavior by this exact rebuilt final gate. Counts overlap; no unique summed total or wholly green broad rerun is claimed. Earlier positive checkpoint and broad API/HSE results retain their exact source scope.

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~CompanyIncidentCommandTests|FullyQualifiedName~Actual_HSE_investigator|FullyQualifiedName~CompanyHseRowAccessTests|FullyQualifiedName~CompanyHseBoundaryTests|FullyQualifiedName~HSEControllerTests' --logger 'console;verbosity=minimal'
```

Final SDK/body/transport verification:

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~InvestigatorClientTests|FullyQualifiedName~Actual_HSE_investigator_HTTP_requires_a_complete_reviewed_body|FullyQualifiedName~CompanyClientScopeTests|FullyQualifiedName~PpdmDataClientTests' --logger 'console;verbosity=minimal'
```

Focused compilation has zero errors; known analyzer, HSE nullable and refusal-handler warnings remain. Missing external projects were not replaced.

Recording Beep provider, real identity/access SQLite registry and loopback fixture authentication/orchestrator restoration are named focused boundaries. They do not prove installed-provider isolation/concurrency or actual host/DI/OIDC/browser acceptance. Previous Implementation 70 counts retain their historical source scope.

Documentation validation: nine scoped documents decode as UTF-8; all 294 local links resolve; no pending Implementation 71 verification placeholders remain. `git -c core.safecrlf=false diff --check` passes. All test sessions are terminal; no repository testhost remains live.

## Remaining complete-plan requirements

Stable assignment intent, durable intent ledger/status/reconciliation/outbox across sessions and recorded supersession semantics remain open. This review does not promote retries of the old intent to success or close durable workflow requirements. The SDK helper has focused evidence for review/submission and stale company responses. Full client-facade assembly/DI, Web/process selection, explicit retained outcome-review state and real host/browser flows remain open. New participant party creation, app-user/person mapping, medical disclosure, qualification/employment and country-specific privacy, attachment/blob ownership, configuration administration/revision, canonical new corrective-action plan creation and full process handoffs remain required.

Scientific exposure, units/calendar/country/provenance inputs, installed-provider transaction/isolation/fault recovery, host/auth/browser/client/circuit behavior, large-roster performance and all other multinational/finance/engineering/operations/release owners remain part of the full plan. Other generic incident mutations keep their existing fences. No complete HSE journey, feature or phase is ready on this checkpoint alone.

## Subsequent Web outcome state

[Implementation 72](Implementation-72-Investigator-Outcome-Review-State.md) extends the existing HSE Web client with actual Field SDK delegation and introduces generation-bound selection, frozen submission and explicit retained outcome-review state. Historical gate results above retain their source scope. The complete wrapper/facade/host, actual Razor/browser consumption, durable cross-session intent and all full-plan gates remain open.
