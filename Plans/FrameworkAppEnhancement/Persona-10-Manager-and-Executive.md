# Manager and executive: company data workflow and screens

Current executive continuity checkpoint: [108 - account/role-bound executive source journey](Implementation-108-Executive-Company-Portfolio-Evidence.md) validates the typed company/account/FIELD envelope, guards actual portfolio/inbox rendering and clears stale auth/source generations. The protected API now compares company/account/authority values across normal context refreshes, rechecks owned PPDM_CORE/source and current FIELD access, and returns no-store portfolio/assets. Native initial gate passes 13 (20.3411 seconds), final actual typed-client/protected HTTP gate 12 (9.0825 seconds); current app RBAC/row policy/native repository are real, provider/schema/auth/target-admission seams explicit. Prior rendered executive 50 and shared landing/inbox 137 pass, counts overlap. All executive jobs are terminal; mixed-return work remains separate and unverified. No new model/role/endpoint/schema/SQL/domain EF/business write/KPI or theme. Full application/provider/browser/OIDC, approved management KPI/decision/outcome/consolidation and broader goal acceptance remain open. Commit/push follows full goal/plan completion.

Updated 2026-10-21. **Planned journey; existing source surfaces inventoried, end-to-end delivery remains open.** Task owners: CA-40/41; FG-52. Extends [Feature-10-Company-Experience-and-Integrations.md](Feature-10-Company-Experience-and-Integrations.md). [Persona delivery index](Company-Persona-Data-Workflows.md) supplies shared requirements; root master tracker owns status.

## Scope and role responsibilities

Selected entity or explicitly entitled group-reporting scope, portfolio, period, currency and approved scenario.

Operational, investment, finance and HSE decisions require their own approved roles; group reports require explicit entity entitlement, not a persona or administrator shortcut.

Business duties above are not assertions that matching ASP.NET role names are seeded. Map them to the selected company's approved existing app-role catalog and assignments. Persona preference never grants rights. Standard roles guard pages/components; APIs independently enforce current company, permissions and resource rights. Multiple duties do not bypass separation of duties.

## Existing screens and landing decision

| Responsibility | Verified route(s) | Existing screen |
|---|---|---|
| Executive landing | `/executive/dashboard` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/ExecutiveDashboard.razor) |
| Portfolio | `/asset/portfolio` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/AssetPortfolio.razor) |
| Decision queue | `/ppdm39/tasks/inbox`, `/tasks/inbox` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Workflow/TaskInbox.razor) |
| Investment evidence | `/economics/evaluation` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Economics/EconomicEvaluation.razor) |

Extend the appropriate current experience root and reconcile overlapping process forms before adding routes. Current source routes do not prove authorized runtime behavior or a completed landing. Missing dedicated screens remain explicit coverage gaps.

## Data workflow and handoff

| Step | Source data shown | Permitted action | Recorded outcome / next screen |
|---|---|---|---|
| Landing to evidence | Approved production/finance/cost/risk and freshness | Filter; drill into contributing sources | Traceable reconciled metric |
| Inbox to decision | Assigned task/version, delegation and evidence | Approve/reject with reason | Durable decision on exact version |
| Decision to outcome | Downstream execution/posting and history | Track; escalate overdue/rejected work | Observable business outcome |
| Entity to group view | Entitled entities and conversion/consolidation evidence | Review eliminations/currency basis; export | Scoped group report and provenance |

Domain teams submit evidence; approvers return reasons; accountable teams execute and expose recorded outcomes.

Reuse the existing workflow/task owner and inbox. Each recipient sees the assigned entity/version, evidence, due date, permitted actions and rejection/return reason. Detail screens retain company/resource context, validate reviewed versions at the API and reload actual persisted outcomes. Corrections preserve source and reasons; uncertain writes retain the original intent for explicit reconciliation, without automatic duplicate submission.

## Persona landing dashboard

| Queue / KPI | Source and required drill-down |
|---|---|
| Production/cash/cost | Approved metrics, period and units/currency; drill into sources |
| Approval backlog/risk | Assigned tasks and permitted risks only |
| Portfolio variance | Approved baseline versus actuals with conversion versions |

Show company/resource/period, currency or units, source/as-of time and freshness/approval status. A card opens the exact contributing authorized records with identical filters. Prioritize assigned work, actionable exceptions and permitted next actions. Missing/stale/restricted data is explicit, never a fabricated zero or completion. Company switches clear prior values and reject late responses.

## First visible delivery slice

Executive landing -> scoped KPI -> contributing evidence -> assigned decision -> outcome/read-back. Governed multi-company consolidation remains a distinct workflow.

1. Name existing Data/service/API/client/screen/task owners and verify exact company role mappings before coding.
2. Connect landing queue -> source/detail -> permitted action -> next-role task/outcome -> authoritative reload -> KPI drill-down. Implement only the missing backend path required for this journey.
3. Use API-first clients and TheTechIdea.Data contracts; business schema changes use Beep MigrationManager. No handwritten SQL or new business EF migration lane.
4. Focused checks cover allowed/denied roles, direct deep links/API denial, two companies with colliding IDs, self-approval rejection where applicable, missing/stale data, validation/failure, uncertain outcome/retry, reload, reassignment and company switch during a request.
5. Record state/client/API checks separately from rendered browser, keyboard/RTL, OIDC and real-provider evidence. Backend tests alone do not deliver this persona or close its CA/FG tasks.


Implementation 107: [Company persona landing queues](Implementation-107-Company-Persona-Landing-Queues.md), 38 focused state cases pass. Nine additional existing experience roots now show their current catalog-matched assigned-work/source queue; native authority/catalog/browser and complete persona action/read-back/handoff gates remain open.


Implementation 108: [Executive company portfolio evidence](Implementation-108-Executive-Company-Portfolio-Evidence.md) now also validates the typed envelope and current standard account/role before the actual portfolio/inbox render. Final 50-case state/client/rendered executive evidence and earlier 137-case shared inbox/landing regression pass with explicit controlled seams. Unsupported KPI aggregates remain unavailable. Protected actual typed-client/controller/service/native row-policy reads now pass twelve cases with controlled authentication/provider seams. Full application/provider/browser/OIDC and complete management decision/read-back/KPI/consolidation gates remain open.
