# Implementation 72 — investigator outcome review state

Date: 2026-10-06. Continues [Implementation 71](Implementation-71-Reviewed-Investigator-Submission.md). CA-03/05/06/32; FG-10/11/12/20/40/42. The [root master tracker](../../MASTER-TODO-TRACKER.md) owns the complete plan. No feature or phase closes.

## Problem and resulting behavior

Reviewed assignment has an API/SDK concurrency contract, but the existing Web HSE client and incident page had no reviewed investigator selection/outcome state. A UI must not automatically adopt a fresh revision and resend an uncertain assignment, display stale choices after company/FIELD/incident changes or attribute another writer's current roster to its own original submission.

The existing IHSEServiceClient/HSEServiceClient now delegates the two typed investigator operations to the existing Field SDK facade. HSEServiceClient requires that actual SDK dependency; no optional fallback or competing Web investigator HTTP implementation is added. Web Program registers the actual app.Field facade and the new presentation-state owner. Existing other HSE operations retain their current owner; their broad legacy read/workflow audit remains required.

InvestigatorAssignmentState reviews the actual current FIELD selection, requests the SDK review for that exact incident/FIELD and requires matching account/company/database/FIELD/selection context. It copies each complete-key participant choice into a generation-specific read-only list and requires the exact displayed choice instance for selection. A fabricated or earlier-generation record with identical keys cannot adopt a fresh review. No app-user, name, contact, qualification or employment mapping is invented.

Submission freezes the original review and selected role before awaits, reserves the target and rechecks the actual FIELD review before contacting the command. One in-flight write blocks another write through this state, including after navigation/scope changes. A pre-send binding change sends no mutation. A command acknowledgment clears choices and requires explicit read-only outcome review; it does not report the participant as assigned from the acknowledgment alone. A lost/rejected response retains the original target/review/selection in this state and never performs an automatic retry.

Outcome review reports the **currently recorded** investigator. Matching BA/party identity is reported as a current-record match, not as a durable receipt proving which intent/writer produced it. A changed roster/authority revision with a different current investigator clears the earlier selection and explicitly says that the old submission is not attributed by this review. An unchanged revision without a recorded match keeps the outcome unconfirmed and blocks further assignment. No new selection is made automatically after reconciliation.

Company and FIELD events invalidate visible evidence, cancel reads and advance the generation, including switching away and back. Incident navigation invalidates visible evidence even during a write. Late reads or acknowledgments cannot restore stale choices/messages, while write reservations and unconfirmed target outcomes survive those changes inside this state instance. Disposal discards late reads and unsubscribes events.

## Source owners

- `Beep.OilandGas.Web/Services/IHSEServiceClient.cs`: extends the actual HSE presentation client with the shared review/choice contracts.
- `Beep.OilandGas.Web/Services/HSEServiceClient.cs`: delegates reviewed investigator operations to the actual Field SDK interface.
- `Beep.OilandGas.Web/Services/InvestigatorAssignmentState.cs`: generation-bound choices, fresh FIELD check, single write reservation and explicit retained outcome review. It uses the existing app permission constant as a presentation hint; the API independently enforces actual authority. It is not a custom authentication/authorization store or durable intent ledger.
- `Beep.OilandGas.Web/Program.cs`: actual SDK facade and state registrations, currently source-only pending host/DI acceptance.
- `Beep.OilandGas.Repository.Tests/InvestigatorAssignmentStateTests.cs`: deterministic lifetime/context/outcome checks against actual state and shared contracts; the high-level client/selector are explicitly mocked boundaries.

No SQL, business EF path, new business entity/schema, live migration, reference/grant seeding or deployment. Business schema ownership remains Beep MigrationManager; identity/access registry EF ownership is unchanged.

## UI design and required guidance

The intended incident form is a MudBlazor card with explicit current-investigator evidence, one recorded-role selector and a submit button. It requires loading/disabled/empty/error/unconfirmed/current-record-match states, explicit outcome review before another change, standard ASP.NET roles consistent with the incident page, accessible labels and focus, theme-token CSS in app.css and no page-local styles/scripts. Complete record keys belong in the selected evidence; UI text must not imply qualification or employment. Keyboard, screen-reader, responsive, browser/OIDC and real-provider workflows remain required.

Razor markup has not been changed. [AGENTS.md](../../AGENTS.md) says **“Before modifying code in these areas, read the corresponding skill”**. The required `.github/Skills/mudblazor-api/SKILL.md` and `.github/Skills/ui-ux-pro-max/SKILL.md` are absent; the configured installed skill roots and adjacent BeepWeb repository (including ignored files) contain no copies. The available installed UI Components/Form Validation guidance was located; a question asks the user to supply the required paths or authorize that substitute. That question remains pending. No dependent Razor work proceeds without the required guidance or explicit override. Independent client/state implementation proceeds within the existing approved plan.

## Verification

Initial actual state/SDK checkpoint: **37 passed**, zero failed/skipped, terminal exit 0 (232ms), log `%TEMP%/oilgas-investigator-state-checkpoint.log`. Eighteen state cases cover frozen original submission, no repeated write, explicit current outcome, unchanged uncertain evidence, superseding roster, stale choices, company/FIELD/incident switching, pre-send selection revision, permission hints, malformed review, no selected FIELD and disposal. The existing nineteen SDK cases retain transport/contract proof.

The affected state/SDK/shared company/PPDM transport/current-FIELD/lease-portfolio gate passed **142 cases**, zero failed/skipped, terminal exit 0 (24s), log `%TEMP%/oilgas-investigator-state-affected.log`. It includes the initial eighteen state and nineteen SDK cases and existing shared lifecycle/transport owners. Three subsequent test-only lifetime-edge cases verify that missing company sends no FIELD/incident reads, failed refresh cannot retain earlier choices and failed outcome review retains the uncertain reservation. Production state/client/shared-contract source did not change after the affected gate. The rebuilt final state/SDK gate passed **40 cases** (all twenty-one state plus nineteen SDK cases), zero failed/skipped, terminal exit 0 (281ms), log `%TEMP%/oilgas-investigator-state-final.log`. Scopes overlap and are not a unique summed count.

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~InvestigatorAssignmentStateTests|FullyQualifiedName~InvestigatorClientTests|FullyQualifiedName~CompanyClientScopeTests|FullyQualifiedName~PpdmDataClientTests|FullyQualifiedName~CompanyFieldSelectionTests|FullyQualifiedName~LeasePortfolioUiTests' --logger 'console;verbosity=minimal'
```

Final state/SDK verification:

```powershell
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~InvestigatorAssignmentStateTests|FullyQualifiedName~InvestigatorClientTests' --logger 'console;verbosity=minimal'
```

Focused compilation has zero errors and the known analyzer/HSE-nullable/refusal-handler warnings. No missing external project was replaced.

Actual state, shared contracts/validator and SDK helper compile in the focused target. High-level selector/HSE client callbacks are mock boundaries; this is not proof of the complete HSE Web adapter, full SDK facade assembly, Program/host DI, rendered Razor or browser/authorization/provider behavior. Earlier Implementation 71 API/HSE results retain their source scope; no unrelated API command source changed here.

Documentation validation: nine scoped documents decode as UTF-8; all 302 local links resolve; no pending test-gate placeholders remain. `git -c core.safecrlf=false diff --check` passes. All focused test sessions are terminal; no repository testhost remains live. The separate required UI-guidance question is still pending.

## Remaining complete-plan work

Implement the actual investigator component/page consumption after required UI guidance, verify the complete SDK/Web adapter/facade/host wiring and render/keyboard/screen-reader/responsive/browser/HTTP/OIDC transitions. Retained pending outcomes here live only in this state instance; page/circuit/process restart persistence, stable assignment intent, durable ledger/status/reconciliation/supersession and outbox remain open. Read-only current-record matching does not certify original intent attribution or provider concurrency/fault recovery.

New participant party creation, personal/app-user/medical disclosure and qualification/employment/country privacy, attachment/blob ownership, configuration administration/revision, canonical new corrective-action plan creation, process approval/closure/consistent handoffs, exposure/unit/calendar/jurisdiction/provenance inputs and installed-provider/performance/host acceptance remain required. Other multinational, finance, engineering, operational and release owners remain part of the full plan.

## Independent backend continuation

[Implementation 73](Implementation-73-Atomic-Corrective-Action-Plan-Creation.md) extends the existing corrective-action command with actual atomic plan creation and shared canonical incident write authority. Its command-only scope and separate final verification do not resolve this document's missing required UI guidance or rendered/host/browser acceptance gates. The root tracker owns current evidence.
