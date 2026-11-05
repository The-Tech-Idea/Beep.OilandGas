# Implementation 81: recorded process source detail

Updated 2026-10-06. Extends [assigned-work landing](Implementation-80-Company-Assigned-Work-Landing.md) and the [accountant source journey](Persona-01-Accountant-and-Controller.md). Partial: actual detail view state verified; page/host/native API integration remains source-only.

## Source problem and behavior

The existing process detail page defaulted status to IN_PROGRESS and start date to today before receiving a record. Failed reloads could retain previous entity/step/history values; history was fetched independently even when no source record was confirmed. Current transitions were never loaded, so the empty transition panel did not deliver an approval path.

The same route now displays confirmed company/FIELD, process/entity identity, recorded status/state/current step, actual start actor/time, step status/assignment/role and recorded history. Missing company/FIELD and load failures have explicit messages and reload controls. The existing FIELD selector remains the authority for selection; a queue task does not silently change FIELD or grant access. Assigned-work navigation returns to the existing shared inbox. MudBlazor and shared CSS/theme conventions are preserved.

## Active owner changes

[ProcessInstanceDetailState](../../Beep.OilandGas.Web/Services/ProcessInstanceDetailState.cs) reads through delegates to the existing BusinessProcessServiceClient and existing ProcessInstance/ProcessHistoryEntry types. It adds no persistence or duplicate domain contract. Company changes invalidate state; source and history must validate together before publication. The source must match requested process identity and selected FIELD and contain process/entity identities. History must match the source and have unique recorded IDs. Null/mismatched/failed loads publish no confirmed process or history. Late company/field invalidation prevents publication.

[Existing detail page](../../Beep.OilandGas.Web/Pages/PPDM39/BusinessProcess/ProcessInstanceDetail.razor) uses this state, standard authenticated route guard, CompanyClientContext, existing FIELD data owner and typed API client. It queues busy reloads, clears on FIELD events, rejects changes while resolving selected FIELD and unsubscribes on disposal. It shows actual recorded steps instead of an invented definition/timeline. No transitions or approval controls are guessed from role/persona labels. Implementing actual independently reviewed approval/posting remains required; this read slice does not replace that objective.

## Focused evidence

[9 state cases](../../Beep.OilandGas.Web.Auth.Tests/ProcessInstanceDetailStateTests.cs) pass, zero failed/skipped, terminal exit 0, 1.1056 seconds, through [Test-ProcessInstanceDetailState.ps1](../../Scripts/Test-ProcessInstanceDetailState.ps1). Compiles actual production state and company sources against actual existing shared Data/Models/PPDM/DataManagementModels assemblies, with explicit read/history delegates. No production dependency shim or missing-project clone. Log `%TEMP%/oilgas-process-detail-state.log`.

Covers recorded status/history publication, foreign identity/FIELD/history and duplicate history refusal, late company response, FIELD invalidation during history, failed refresh clearing old values and missing FIELD no request. Tests do not certify HTTP, current business process implementation, actual provider consistency, source resource authority or rendered UI. Existing Process types remain their current owners; migration of legacy shared process contracts to Data remains a broader ownership task.

## Next delivery requirements

- Independently verify and harden actual process read/history API resource entitlement and current-company/current-FIELD/binding closure. Existing field equality alone does not certify deep-link source authorization.
- Render and exercise queue -> FIELD selection -> source detail/reload across companies, changed process IDs, unavailable/restricted source, loading/error/retry and sign-out; keyboard/RTL/local time display remains open.
- Implement current reviewed permitted actions, independent approver/self-approval denial, source transaction posting, retained uncertain intent and authoritative readback/KPI drill-down through existing process/accounting owners. No full finance/workflow journey is newly certified.

No SQL, schema, business EF lane, synthetic dates/status/metrics or grants. Full goal and all unfinished persona/CA/FG gates remain open.
