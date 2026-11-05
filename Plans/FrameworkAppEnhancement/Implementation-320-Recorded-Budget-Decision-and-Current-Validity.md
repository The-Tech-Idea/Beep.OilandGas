# 320 - Recorded budget decision and current validity

Current actual labor valuation checkpoint: [321 - recorded labor actual-cost valuation](Plans/FrameworkAppEnhancement/Implementation-321-Recorded-Labor-Actual-Cost-Valuation.md) adds financial recorder workflow through the existing ACCOUNTING_COST/AfeService owner. Actual native CORE labor facts preserve quantity/unit/employee/duty/interval/audit without private executor report or intent. Current original work scope/company book/independent approval and complete cost populations are reviewed; explicit rate in the recorded unit/currency, actual valuation UTC, category and rationale are retained before transport. Omitted JSON rate/unit/currency/date cannot default. Amount uses exact source quantity and configured book rounding; duplicate labor correlation, budget overrun, unknown/legacy/unowned cost population, changed approval/source or missing schema refuses.

One serializable cost insert/readback leaves original draft/approval/labor intact, records no stock/GL mutation and does not infer header actual totals. Same physical CORE/PRODUCTION closes labor inside owned proof; separate CORE/book closes before/after and original lost acknowledgement stays recoverable. API review/write/reference recovery and actual typed client/circuit/MudBlazor rate screen are connected to native detail, enforce standard app roles/current explicit Accounting.RecordWorkOrderActualCost plus Accounting.View, FIELD/order WRITE and native bindings, retain book/AFE/labor/intent before send and fence context changes. No SQL, business EF, parallel ledger or live migration.

Current Data builds zero warnings/errors; focused actual API source zero errors (20.74 seconds, existing focused warnings). Native actual-cost seventeen pass (7.7359 seconds); client/form/state twenty-one pass (6.9673 seconds); native detail fourteen pass (1.7736 seconds). Protected actual labor-cost API passes one (30.7816 seconds) before final JSON/capacity rebuild: one cost, no-write recovery/retry, original draft retained; configured-book/already-recorded start/labor/auth/provider seams are explicit. Final protected cost rerun after required JSON/capacity facts passes one (32.0960 seconds), zero failures/skips, terminal exit 0 in oilgas-321-labor-cost-http-final.log. Session 99206 is closed; no HTTP process is running. Loaded shared HTTP output was preserved throughout the live run. Counts overlap. Material/contractor/equipment valuation, invoice/commitment correlations/reversals, budget lines/amendments/adoption, full public actual/variance/conversion/posting/queues/dashboards, operational verification/return/lifecycle and all remaining financial/persona/multinational acceptance remain open. No live provider/OIDC/browser/RTL/accessibility/atomicity certification. Goal active; commit/push follows full completion audit.


Finance viewers and budget makers can now read the original independent budget outcome separately from its validity for the current work scope and company book. This extends implementations 318/319 and prepares source-backed cost admission; it does not implement actual costing, posting or all financial dashboards.

## Persona data workflow

The financial viewer opens an authorized native work-order detail and supplies the original company book and AFE identity. The read shows original budget amount/currency, recorded approve/reject outcome, workflow ID, independent reviewer and UTC audit. It also shows current comparison time and explicit blockers. Rejection, an unrecorded outcome, changed original work scope or a changed configured book cannot become current approval. A recorded approval remains visible when later scope changes block its current admission.

Only Accounting.View, standard app-owned roles and current FIELD/order READ are required for this public read. Preparation and private independent decision/recovery endpoints retain their own explicit capabilities and WRITE grants. IdentityServer remains authentication-only. Current company/account/FIELD/capability changes clear screen values and fence late replies. The read neither creates an approval nor records a cost.

## Canonical implementation

- Data-owned CompanyWorkOrderBudgetDecisionFacts exposes immutable outcome/audit correlation only. It omits private reviewer rationale, original command and recovery intent.
- CompanyWorkOrderBudgetDecisionStatus combines complete current AFE/order/book evidence with recorded facts and derived blockers. Its current-approved property validates the source contract, including comparison times after original audits. It never infers approval from absent data.
- AuthorizationWorkflowService pairs complete original AFE decision-population reads through the existing APPROVAL_WORKFLOW owner. Headers close before private proof validation. A complete known-empty population yields unrecorded; missing schema, malformed proof, ambiguous/unowned correlation or changed observations remain unavailable/refused.
- CompanyWorkOrderBudgetService observes actual current order/book/AFE evidence and decision facts twice, compares revisions, and independently refreshes app RBAC, preference, bindings and READ grants. The existing controller exposes `/api/field/current/workorder/{workOrderId}/budget/decision/status` with no-store and READ authorization.
- The existing typed WorkOrderServiceClient, scoped status state and MudBlazor status panel render recorded facts and current blockers. Native work-order detail shows the viewer surface separately from draft preparation and independent review controls.

No raw SQL, EF business store, parallel approval/cost ledger or live migration is introduced. Current validity is scoped to original work-scope and book proof; actual-cost/period/source admission needs its own complete reviewed evidence.

## Focused evidence

| Gate | Result | Scope |
|---|---|---|
| Current isolated Data build | Zero warnings/errors | Public fact/status contracts and source validation |
| Native independent decision and visibility | 15 passed, 7.9914 seconds | Historical public outcome for another authorized actor; no rationale/intent leak; recorded approval vs current scope drift; complete known-empty vs missing schema; existing decision safeguards |
| Public status client/state/form | 16 passed, 6.5725 seconds | Visibility capability, late company/FIELD replies, approved/rejected/unrecorded/changed states, no mutations or private rationale |
| Native work-order detail composition | 13 passed, 1.8795 seconds | Viewer-only entry point, distinct preparation/review controls, current context/source behavior |
| Protected approve/reject plus public status | 2 passed, 44.4192 seconds, zero failures/skips, terminal exit 0 | Actual MVC/app RBAC/native order/AFE/decision owners; current scope drift, READ-only visibility after preparation/review capability removal. Book source/auth/provider seams remain explicit. |

Two preliminary protected runs refused stale company selection after the fixture changed role permissions. The corrected viewer host rebinds its actual company/profile/source owners to the refreshed selection. No production refresh or authorization bypass is added. Both outcomes and READ-only viewer admission pass in the third run. Session 77829 is closed; the loaded shared HTTP output was preserved until terminal success.

## Required remaining work

- Actual source-backed labor/material/contractor/equipment cost quantities, rates, valuation dates and currencies through the existing cost owner; no inferred rate or conversion.
- Duplicate prevention/correlation, budget lines and controlled amendments/adoption, commitments/actuals/variance, recorded conversion/rounding and posting.
- Finance and operational responsible queues/dashboard drill-down with complete authorized source populations.
- Independent work verification, return acceptance and lifecycle advancement; remaining persona, multinational and acceptance requirements.

Focused seams do not certify live provider/migration/concurrency, OIDC, browser/RTL/accessibility or cross-provider atomicity. The full goal remains active. Commit/push follows its complete audit.
