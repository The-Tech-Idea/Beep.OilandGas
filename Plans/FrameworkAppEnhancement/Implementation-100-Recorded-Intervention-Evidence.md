# Implementation 100: Recorded intervention evidence

## Engineer review and maintenance planning

The existing intervention API filled absent deferred production, days deferred and USD cost with zero and priority with MEDIUM. The detail then assumed a 70-dollar oil price, seven-day duration, two-month payout, current-day-derived onset and a proceed recommendation. These values were not source evidence. This slice replaces them in the existing engineer-to-maintenance review rather than adding another screen.

## Shared data and existing API

ProductionInterventionCandidateDto now has one Data owner; the old API and client copies and page's duplicate candidate/detail records are removed. The existing activity projection carries selected company, actor, FIELD, recorded source/latest observation numbers, actual source START_DATE and UTC retrieval time. Retrieval is not transactional source-as-of time. Activity start is not interpreted as rate-drop onset and no source timezone is assumed. UWI is displayed as the recorded well identity, not a fabricated well name.

Absent rate, cost, days and priority remain nullable. The API does not assign zero, MEDIUM or a WORKOVER type to missing evidence. The shared validator requires original company/actor/FIELD, nonempty unique well identity, source/latest observation identity and valid UTC retrieval. Invalid numeric metrics are refused. The current client screen validates these returned rows before publication, including company/FIELD collisions.

## Existing screen and actual component

ProductionInterventionEvidence renders actual source references, recorded note/type/start date, retrieval, company/FIELD and optional rate/day/cost/priority values. Missing values say Unavailable. Actual zero rates and costs retain BOPD and USD units; costs are not relabeled thousands. No oil-price calculation, generated onset, invented duration/payout/root cause or proceed recommendation remains in this detail.

The existing list shows recorded UWI and nullable metric empty states. Missing rate history, diagnostics and comparable cases now have explicit unavailable messages. Diagnostic metadata is displayed without an invented Normal/Abnormal judgment; diagnostic lookup failures use the existing failure reporter and do not silently become a successful empty panel. Existing work-order/AFE/process links remain source references, not proof those downstream records were loaded or authorized.

## Focused evidence

Actual Data isolated build succeeded, zero warnings/errors, 1.53 seconds; `%TEMP%/oilgas-intervention-evidence-data-build.log`.

Test-ProductionInterventionEvidence.ps1 compiles the actual Razor component against the actual current Data assembly and actual Web MudBlazor package version. **19 passed**, zero failures/skips, terminal exit 0, 0.9697 seconds; `%TEMP%/oilgas-intervention-evidence.log`. Five static rendering cases cover unavailable versus genuine zeros/units, source-date timezone limits, missing source and encoded notes. Fourteen shared validation cases cover valid nullable/empty data and company/actor/FIELD/well/source/latest/retrieval/offset/duplicate/null/invalid-rate/cost/day refusals. Any JS request fails in the explicit static-render fixture; no browser success is simulated. Whitespace diff check passed; source search confirms one production candidate DTO owner.

## Remaining persona gates

The actual API projection/controller/filter/provider, current FIELD-to-well authority, typed HTTP transport and full parent page/browser/OIDC/keyboard/RTL remain unexecuted. Candidate observation references are not an optimistic-concurrency token or independent-approval review. The source provider's well membership and stable database binding still require proof. This slice does not validate workflow marker references, certify the separate decommissioning command, or complete original-context URL handoffs and durable maintenance/financial commands. Engineering-approved scope, trustworthy metric source/as-of/units and planning material/permit/execution/return-to-service evidence remain required before certifying the complete persona journey.

Implementation 99's unsupported candidate-detail gate is advanced here; its native/source-version/durable action gates remain open. All accountant and other persona plans remain active. No SQL, schema, migration, business EF lane, grants or phase/goal closure.
