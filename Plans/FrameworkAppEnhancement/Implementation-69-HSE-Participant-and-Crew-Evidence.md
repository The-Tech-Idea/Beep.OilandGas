# Implementation 69 — actual HSE participant and crew evidence

Date: 2026-10-06. Continues [Implementation 68](Implementation-68-HSE-Restricted-Case-Visibility.md). CA-03/05/06/32; FG-10/11/12/20/40/42. The [root tracker](../../MASTER-TODO-TRACKER.md) owns the full plan; no phase is closed by this checkpoint.

## Implemented source and behavior

HSE_INCIDENT_BA has the full INCIDENT_ID/PARTY_OBS_NO/PARTY_ROLE_OBS_NO key. Its actual vocabulary references are R_INCIDENT_BA_ROLE.BA_ROLE and R_EMPLOYEE_STATUS.EMPLOYEE_STATUS. INVOLVED_PARTY_BA_ID references BUSINESS_ASSOCIATE. The table's recorded purpose includes crew members, whole crews and other parties. COMPANY_BA_ID, UWI, PERIOD_OBS_NO and DETAIL_OBS_NO describe optional crew context; WELL_DRILL_PERIOD_CREW uses UWI/PERIOD_OBS_NO/CREW_COMPANY_BA_ID/DETAIL_OBS_NO, with actual crew-member/company BA identities and a WELL_DRILL_PERIOD parent. The metadata currently represents some relationships by the ID column alone; focused logical full-key validation does not certify installed composite foreign keys.

The existing company PPDM row policy/evidence reader owns these checks; there is no additional participant store or permission engine. Current case visibility remains mandatory. Current participant predicates must use the complete three-column key, recorded role/status/BA/company references and explicit date windows. Unknown, orphan, ambiguous and inconsistent evidence refuses. A company BA is a recorded participant reference, not proof of personal app identity or an automatic grant. Third-party companies are supported through an actually recorded case/crew relationship; equality with the active app company is not guessed as a universal participation rule.

Optional crew context requires complete exact identity, actual current member or whole-crew evidence and the actual period parent. Its WELL must have current existing app/resource authority and the case's actual primary FIELD. Crew references cannot promote authority to an unrelated WELL. Frozen roster/reference evidence survives provider awaits and is compared with returned payloads and fresh evidence. A final stored-roster read after all reference awaits closes the roster interval even when the provider materializes copies instead of aliased objects. Canonical empty/expired rosters may yield no current participant; missing reference evidence cannot masquerade as empty data.

The actual incident owner's investigator selection now rejects multiple current recorded investigator observations and uses exact approved role identity instead of case-insensitive first-row selection. Generic participant/investigator writes remain fenced; typed assignment/roster creation, personal identity mapping and medical/attachment/jurisdiction decisions keep their existing owners and remain required.

## Verification and remaining full-plan work

**Pre-closing-roster-check participant acceptance: 46 passed, zero failed/skipped, terminal exit 0, duration 1m11s.** Forty new policy/repository/actual incident-owner cases cover external companies, member/whole crews, complete observation keys, missing/duplicate/inactive/expired person/role/status/company/crew/period/WELL evidence, foreign WELL operator/FIELD refusal, canonical key metadata drift, expired/future participant exclusion, exact investigator role identity and ambiguity, same-key payload substitution, aliased mid-read roster mutation and withholding before personal roster access. Six new loopback HTTP cases exercise the existing HSE controller, actual facade/incident/policy/profile owners and refusal middleware: valid external participant, anonymous, denied FIELD, foreign incident, missing person and ambiguous investigator. The recording Beep provider, fixture authentication and orchestrator/access restoration are explicit test boundaries. This is not installed provider, OIDC, Program DI or browser evidence.

The initial seven-case checkpoint failed two reads because policy predicate matching accepted only strings; complete numeric observation keys now reuse the evidence reader's invariant decimal comparison. The corrected checkpoint passed seven cases. The initial 36-case participant run passed 34 and failed two: a stale request fixture after removing a permission, and a stored-roster change after the last roster materialization. The final fixture refreshes the company scope; a closing stored-roster comparison now checks the roster after all reference awaits and the original one-shot mutation test is restored. Subsequent final-source gates own this correction. Failed runs remain recorded as failed; subsequent tests do not relabel them.

### Commands and logs

`dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-restore --filter 'FullyQualifiedName~CompanyHseRowAccessTests.Participant_|FullyQualifiedName~Actual_HSE_participant_HTTP' --logger 'console;verbosity=minimal'`

Log: `%TEMP%/oilgas-hse-participant-final-acceptance.log`. Compilation: zero errors, nine existing warnings (four absent analyzer project references, four existing HSE nullable assignments, existing unnamed-enum refusal-handler warning); no new warning from this checkpoint and no external-project substitution. Existing identity/access test registry EF ownership remains unchanged. No business schema or migration is executed here; Beep MigrationManager remains the business schema owner.

**Pre-closing-roster-check HSE-family/controller gate: 172 passed, zero failed/skipped, terminal exit 0, duration 4m27s.** Includes all 117 HSE row cases (40 new participant cases plus all existing restriction, population, incident/RCA and inherited-field tests), six participant HTTP cases and 49 HSE controller/account refusal cases. Its scope overlaps the 46-case acceptance gate; counts must not be summed as unique coverage. Log: `%TEMP%/oilgas-hse-participant-family-final.log`.

`dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-build --no-restore --filter 'FullyQualifiedName~CompanyHseRowAccessTests|FullyQualifiedName~Actual_HSE_participant_HTTP|FullyQualifiedName~HSEControllerTests' --logger 'console;verbosity=minimal'`

**Pre-closing-roster-check shared-policy gate: 183 passed, zero failed/skipped, terminal exit 0, duration 10m06s.** Includes 31 existing company PPDM row cases, 83 HAZOP authority cases, 68 corrective-action authority cases and actual atomic addition/retry/intent collision. Log: `%TEMP%/oilgas-hse-participant-dependent-final.log`.

`dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-build --no-restore --filter 'FullyQualifiedName~CompanyPPDMRowAccessTests|FullyQualifiedName~CompanyHazopAuthorityTests|FullyQualifiedName~CompanyCorrectiveActionAuthorityTests|FullyQualifiedName~Addition_atomically_records_the_step' --logger 'console;verbosity=minimal'`

**Final closing-roster checkpoint: four passed, zero failed/skipped, terminal exit 0, duration 41s.** Rebuilds the final source and proves the original one-shot stored-roster mutation refuses alongside actual positive external-company, member and whole-crew reads. Log: `%TEMP%/oilgas-hse-participant-closing-checkpoint.log`. **Final affected source gate: 174 passed, zero failed/skipped, terminal exit 0, duration 3m36s.** Includes all 117 HSE row cases (40 participant cases plus all existing restrictions/current population/incident/RCA tests), six participant HTTP cases, 49 HSE controller/account refusal cases, actual recorded-project CA read and actual atomic addition/retry/intent collision. This run uses the rebuilt closing-roster source and original one-shot mutation test. Broader pre-closing results above retain their exact source boundary; overlapping counts are not unique totals. Log: `%TEMP%/oilgas-hse-participant-final-source.log`.

`dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true --no-build --no-restore --filter 'FullyQualifiedName~CompanyHseRowAccessTests|FullyQualifiedName~Actual_HSE_participant_HTTP|FullyQualifiedName~HSEControllerTests|FullyQualifiedName~Actual_CA_owner_reuses_actual_project_and_reports_recorded_evidence|FullyQualifiedName~Addition_atomically_records_the_step' --logger 'console;verbosity=minimal'`

All test processes completed before any later rebuild. No source edits followed the final affected gate.

### Remaining full-plan work

Participant evidence does not establish app-user/person identity, employment, medical disclosure rights or country-specific privacy policy. Participant disclosure policy, typed investigator/roster writes, attachments/blob access, configuration revision/administration, canonical new-plan creation, durable workflow/outbox and scientific exposure/unit/calendar/provenance inputs remain open. Logical composite checks do not certify installed physical foreign keys. Provider/host/OIDC/browser/performance and actual transactional reporting snapshots remain unverified. The 256-row bounded evidence limit remains explicit; no silent pagination or large-roster readiness claim. No phase is closed and the full plan remains active.

### Source owners

- `Beep.OilandGas.ApiService/Services/CompanyPPDMRowAccessPolicy.cs`: complete participant predicates, roster/reference/crew evidence and frozen-payload delivery/access validation through the existing case policy.
- `Beep.OilandGas.ApiService/Services/CompanyPPDMEvidenceReader.cs`: invariant decimal predicate comparison reused by row policy; canonical evidence capture remains the shared owner.
- `Beep.OilandGas.PPDM39.DataManagement/Services/HSE/HSEIncidentService.cs`: exact, unique current investigator selection.
- `Beep.OilandGas.Repository.Tests/CompanyHseRowAccessTests.cs`: real row/incident owners and explicit recording-provider acceptance; actual BA/role records added to existing fixture seeds.
- `Beep.OilandGas.Repository.Tests/CurrentFieldLifecycleGateTests.cs`: six actual controller/facade HTTP acceptance cases through its existing host harness.

## Subsequent recorded-party assignment

[Implementation 70](Implementation-70-Recorded-Investigator-Assignment.md) adds typed investigator assignment through the existing incident owner, shared participant/case policy and owned Beep transaction/repository lane. It preserves the exact recorded party and prior role history. Historical gate results above retain their source scope. New party creation, client revision/stable intent across intervening assignments, disclosure/personal identity/qualification, attachments/configuration, new-plan creation and provider/host/browser/scientific acceptance remain open.
