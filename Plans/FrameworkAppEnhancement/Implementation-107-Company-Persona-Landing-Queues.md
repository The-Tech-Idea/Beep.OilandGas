# Implementation 107: Remaining company persona landing queues

## Current landing surfaces

The existing shared PersonaTaskInbox is connected to these current experience roots, using each page's actual declared routes:

| Existing root | Catalog-matched routes |
|---|---|
| Commercial/revenue workbench | `/accounting/revenue-workbench` |
| Procurement/cost workbench | `/accounting/cost-workbench` |
| Drilling engineering | `/drilling/workspace` |
| Reservoir engineering | `/reservoir/workspace` |
| Exploration prospect board | `/exploration/prospect-board` |
| HSE/compliance | `/hse/dashboard`, `/ppdm39/hse` |
| Executive/manager | `/executive/dashboard` |
| Data steward | `/ppdm39/data-management` |
| Company administration | `/admin/dashboard` |
| Audit | `/data/audit` |

These extend the existing accountant, production engineer, operations and maintenance roots; no duplicate persona dashboard, inbox, model, grant or backend queue was introduced. The existing selected-company catalog's DefaultRoute controls workspace matching. Duty names in plans are not treated as seeded roles or persona codes. All modified roots now require standard authenticated ASP.NET authorization; this does not assert their role-specific domain actions are authorized. The shared API independently resolves current company/persona/roles and each task/source resource.

The queue shows its actual company/persona-bound assigned-work counts, exact contributing task filters and encoded source-process links. Wrong catalog route, unavailable source or changed company cannot fetch/publish another persona's queue. General inbox stays available for any current eligible persona. Workbench navigation does not change persona preference or grant rights.

## Initial focused evidence

Test-PersonaTaskInboxState.ps1 with actual state/context sources and actual Data: **38 passed**, zero failures/skips, terminal exit 0, 0.9988 seconds; `%TEMP%/oilgas-persona-landing-all-queues.log`. Ten additional actual declared route aliases now exercise catalog matching and exact source task link publication alongside 28 existing scoped/count/filter/failure cases. Fixture persona codes remain opaque and deliberately independent of duty names; this proves route behavior, not seeded production assignments or runtime role admission. Whitespace diff check passed.

At that initial checkpoint, actual full parent pages/DI/API transport/current catalog/native roles/provider/browser/OIDC/keyboard/RTL were not executed. The current engineering checkpoint below records the later parent-render and controlled HTTP evidence separately. Existing domain dashboard metrics, resource screens and commands are not certified by adding their queue.

## Current engineering landings and relevant source filters (2026-10-25)

The existing drilling and reservoir engineer pages now lead with the canonical company/persona-matched assigned-work inbox. Drilling follows design/program -> investment/review -> execution/handover; reservoir follows evidence -> analysis/scenarios -> reserves/recovery/review. Both use responsive MudBlazor cards and existing routes. Drilling now has the standard authenticated page guard. The shared inbox retains current app-role validation and independent API admission; no role names or grants are invented.

Removed hardcoded AFE/well/pool alerts, unsupported page-local KPI types and direct KPI HTTP calls that caught errors and displayed zero. Missing performance totals are explicit, including period/unit/currency or assumption requirements. Links open existing source screens; they do not imply approved designs, funding or readiness.

The existing PersonaTaskInboxState now exposes only source categories with actual contributing assignments. PersonaAssignedWorkPanel reuses its existing source counts, scope matching and drill-down; irrelevant zero-count financial/maintenance filters disappear. All-source and confirmed empty totals remain. Company/persona changes clear filters and evidence; recorded maintenance reports remain available independently of pending-task counts.

Focused verification: **188 passed**, zero failures/skips, 21.2497 seconds. This includes **18 actual engineering parent-page cases** (current, confirmed empty, failure, missing company, wrong workspace, foreign company/account, company change and late response), shared state/source-filter/render cases, existing maintenance/executive landing cases and affected inbox HTTP regressions. All **22 navigation links** match declared routes. The initial source-filter run retained three failures from older zero-filter expectations; the corrected assertions verify empty totals and preserved original reports. [Observed evidence, source/runtime hashes and retained logs](evidence/107-engineering-landings/observed-evidence.json).

Actual Razor parents/shared components are compiled and rendered using controlled auth/transport/native fixtures. Full solution, live OIDC/company-role catalog, browser/keyboard/RTL/responsive checks and full source-screen workflows remain unverified. Forecast/classification/FDP, design/AFE approval, execution and independent handover outcomes remain open under their existing owners. No database/schema change, new SQL/EF lane, persona grant, phase/goal closure, commit or push.

## Remaining persona outcomes

Each recipient still needs a complete source -> permitted action -> independent review where required -> durable result/read-back -> next-role task/KPI drill-down journey. Revenue/partner settlement, procurement/material/receipt/invoice, reservoir/classification/FDP, exploration/drilling/development, HSE/permit/regulatory, management decisions, data-quality/import approvals, company role administration and audit source evidence remain open under their existing plans. Accountant/operator/maintenance action/provider gates remain active.

Procurement now has the company-owned `/procurement` workbench under Implementations 136/137; its remaining receipt/stock/AP and provider gates stay with those owners. Company-administrator and auditor catalogs/routes must be verified live; authentication alone is not domain authorization. Existing unsupported legacy page metrics need their own corrections rather than being presumed valid beside the task queue.

No SQL, schema/migration, business EF lane, grants or phase/goal closure.
