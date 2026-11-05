# Reservoir and petroleum engineer: company data workflow and screens

Updated 2026-10-06. **Planned journey; existing source surfaces inventoried, end-to-end delivery remains open.** Task owners: CA-20/21; FG-52. Extends [Feature-06-Engineering-Studies-and-Quality.md](Feature-06-Engineering-Studies-and-Quality.md). [Persona delivery index](Company-Persona-Data-Workflows.md) supplies shared requirements; root master tracker owns status.

## Scope and role responsibilities

Company, FIELD/pool/reservoir, study/input version, effective date, units and forecast scenario.

Analyst prepares studies; technical reviewer approves; reserves approval and portfolio publication require separate authority.

Business duties above are not assertions that matching ASP.NET role names are seeded. Map them to the selected company's approved existing app-role catalog and assignments. Persona preference never grants rights. Standard roles guard pages/components; APIs independently enforce current company, permissions and resource rights. Multiple duties do not bypass separation of duties.

## Existing screens and landing decision

| Responsibility | Verified route(s) | Existing screen |
|---|---|---|
| Reservoir landing | `/reservoir/workspace` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Reservoir/ReservoirEngineerWorkspace.razor) |
| Characterization | `/reservoir/characterization` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Reservoir/CharacterizationSummary.razor) |
| Reserves | `/reservoir/reserves` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Reservoir/ReservesClassification.razor) |
| Forecasts | `/production/forecasting` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Production/ProductionForecasting.razor) |

Extend the appropriate current experience root and reconcile overlapping process forms before adding routes. Current source routes do not prove authorized runtime behavior or a completed landing. Missing dedicated screens remain explicit coverage gaps.

## Data workflow and handoff

| Step | Source data shown | Permitted action | Recorded outcome / next screen |
|---|---|---|---|
| Evidence to model | Well/test/PVT/pressure/geology source versions | Validate dimensions/quality; state assumptions | Reviewed inputs |
| Model to forecast | History, explicit scenario and solver configuration | Run; compare; save benchmark evidence | Versioned forecast and uncertainty |
| Forecast to decision | Approved scenario and classification evidence | Submit technical review; address rejection | Recorded classification/decision |
| Decision to development | Approved forecast, assumptions and constraints | Hand off to FDP/economics | Recipient traces source/result versions |

Production provides accepted history; development uses approved scenarios; management receives approved scoped metrics.

Reuse the existing workflow/task owner and inbox. Each recipient sees the assigned entity/version, evidence, due date, permitted actions and rejection/return reason. Detail screens retain company/resource context, validate reviewed versions at the API and reload actual persisted outcomes. Corrections preserve source and reasons; uncertain writes retain the original intent for explicit reconciliation, without automatic duplicate submission.

## Persona landing dashboard

| Queue / KPI | Source and required drill-down |
|---|---|
| Studies awaiting review | Recorded task/version and reviewer |
| Forecast versus actual | Comparable windows/units; drill into source points |
| Reserves exceptions | Recorded evidence/classification gaps; calculation alone is not approval |

Show company/resource/period, currency or units, source/as-of time and freshness/approval status. A card opens the exact contributing authorized records with identical filters. Prioritize assigned work, actionable exceptions and permitted next actions. Missing/stale/restricted data is explicit, never a fabricated zero or completion. Company switches clear prior values and reject late responses.

## First visible delivery slice

Reservoir landing -> assigned study -> input evidence -> saved forecast -> technical approval -> development handoff.

1. Name existing Data/service/API/client/screen/task owners and verify exact company role mappings before coding.
2. Connect landing queue -> source/detail -> permitted action -> next-role task/outcome -> authoritative reload -> KPI drill-down. Implement only the missing backend path required for this journey.
3. Use API-first clients and TheTechIdea.Data contracts; business schema changes use Beep MigrationManager. No handwritten SQL or new business EF migration lane.
4. Focused checks cover allowed/denied roles, direct deep links/API denial, two companies with colliding IDs, self-approval rejection where applicable, missing/stale data, validation/failure, uncertain outcome/retry, reload, reassignment and company switch during a request.
5. Record state/client/API checks separately from rendered browser, keyboard/RTL, OIDC and real-provider evidence. Backend tests alone do not deliver this persona or close its CA/FG tasks.


Implementation 107: [Company persona landing queues](Implementation-107-Company-Persona-Landing-Queues.md), 38 focused state cases pass. Nine additional existing experience roots now show their current catalog-matched assigned-work/source queue; native authority/catalog/browser and complete persona action/read-back/handoff gates remain open.

## Engineering app landing checkpoint (2026-10-25)

[Implementation 107](Implementation-107-Company-Persona-Landing-Queues.md#current-engineering-landings-and-relevant-source-filters-2026-10-25) now delivers drilling/reservoir landing cards organized around each role's work, current company/persona assigned-source queues and relevant recorded source filters. Hardcoded alerts and zero-on-error KPI values are removed; unavailable engineering/financial totals are explicit. **188 focused checks passed**, including 18 actual engineering parent-page cases and affected inbox/landing/HTTP regressions; all 22 landing links resolve. Existing source-screen approval, saved-result/read-back and independent handover journeys, live role catalog/provider and browser/keyboard/RTL remain open. This is an app landing slice, not persona or phase completion.
