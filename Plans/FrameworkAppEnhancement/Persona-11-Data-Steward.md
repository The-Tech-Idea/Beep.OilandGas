# Data steward: company data workflow and screens

Updated 2026-10-06. **Planned journey; existing source surfaces inventoried, end-to-end delivery remains open.** Task owners: CA-01/02/42; FG-20/50/52. Extends [Feature-01-Coverage-Architecture-and-CI.md](Feature-01-Coverage-Architecture-and-CI.md). [Persona delivery index](Company-Persona-Data-Workflows.md) supplies shared requirements; root master tracker owns status.

## Scope and role responsibilities

Company/database/module, permitted dataset, source version, job and validation rule revision.

Importer stages data; steward reviews mapping/quality; domain/master-data reviewer approves publication; migration authority is separately assigned.

Business duties above are not assertions that matching ASP.NET role names are seeded. Map them to the selected company's approved existing app-role catalog and assignments. Persona preference never grants rights. Standard roles guard pages/components; APIs independently enforce current company, permissions and resource rights. Multiple duties do not bypass separation of duties.

## Existing screens and landing decision

| Responsibility | Verified route(s) | Existing screen |
|---|---|---|
| Data landing | `/ppdm39/data-management` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/DataManagementHub.razor) |
| Import review | `/ppdm39/data-import` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/DataImport.razor) |
| Validation | `/data/validation` | [Source](../../Beep.OilandGas.Web/Pages/Data/Validation.razor) |
| Quality | `/data/quality-dashboard` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Data/DataQualityDashboard.razor) |
| History | `/data/audit` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Data/AccessAudit.razor) |

Extend the appropriate current experience root and reconcile overlapping process forms before adding routes. Current source routes do not prove authorized runtime behavior or a completed landing. Missing dedicated screens remain explicit coverage gaps.

## Data workflow and handoff

| Step | Source data shown | Permitted action | Recorded outcome / next screen |
|---|---|---|---|
| Source to preview | Declared source/schema/keys/units/mapping | Validate; preview mapping and errors | Reviewed import intent |
| Preview to publish | Accepted mapping, duplicate/FK checks and approval | Submit canonical import; track/reconcile | Persisted records with provenance |
| Quality to correction | Executed rule failures and affected identities | Assign; correct through owner; rerun | Resolved issue with original/corrected evidence |
| Master-data change | Reference vocabulary and affected workflows | Propose; review impact; approve via BeepDiA | Effective governed data for clients |

Business users report issues; steward proposes correction; owning domain reviewer accepts; admin controls setup.

Reuse the existing workflow/task owner and inbox. Each recipient sees the assigned entity/version, evidence, due date, permitted actions and rejection/return reason. Detail screens retain company/resource context, validate reviewed versions at the API and reload actual persisted outcomes. Corrections preserve source and reasons; uncertain writes retain the original intent for explicit reconciliation, without automatic duplicate submission.

## Persona landing dashboard

| Queue / KPI | Source and required drill-down |
|---|---|
| Blocked/failed jobs | Actual job state/stage; drill into row evidence |
| Quality exceptions | Executed outcomes and validation time; missing run is unavailable |
| Unapproved changes | Recorded proposal/approval and affected scope |

Show company/resource/period, currency or units, source/as-of time and freshness/approval status. A card opens the exact contributing authorized records with identical filters. Prioritize assigned work, actionable exceptions and permitted next actions. Missing/stale/restricted data is explicit, never a fabricated zero or completion. Company switches clear prior values and reject late responses.

## First visible delivery slice

Data landing -> job/quality exception -> preview/source evidence -> approved correction/publish -> authoritative reload and downstream impact.

1. Name existing Data/service/API/client/screen/task owners and verify exact company role mappings before coding.
2. Connect landing queue -> source/detail -> permitted action -> next-role task/outcome -> authoritative reload -> KPI drill-down. Implement only the missing backend path required for this journey.
3. Use API-first clients and TheTechIdea.Data contracts; business schema changes use Beep MigrationManager. No handwritten SQL or new business EF migration lane.
4. Focused checks cover allowed/denied roles, direct deep links/API denial, two companies with colliding IDs, self-approval rejection where applicable, missing/stale data, validation/failure, uncertain outcome/retry, reload, reassignment and company switch during a request.
5. Record state/client/API checks separately from rendered browser, keyboard/RTL, OIDC and real-provider evidence. Backend tests alone do not deliver this persona or close its CA/FG tasks.


Implementation 107: [Company persona landing queues](Implementation-107-Company-Persona-Landing-Queues.md), 38 focused state cases pass. Nine additional existing experience roots now show their current catalog-matched assigned-work/source queue; native authority/catalog/browser and complete persona action/read-back/handoff gates remain open.
