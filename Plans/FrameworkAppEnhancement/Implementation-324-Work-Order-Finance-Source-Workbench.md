# 324 - Work-order finance source discovery and landing workbench

CA-10 and CA-14 require finance users to find original transactions and drill into recorded budgets and costs from their company workspace. The earlier work-order financial panels required manual book/AFE identities. The accounting landing now includes a read-only work-order source workbench with authorized native order discovery, recorded budget/decision/cost evidence and direct links to the existing workflows.

## Workflow

1. A user with current standard app roles and Accounting.View reloads the authorized native work orders for the selected company/FIELD. The existing WorkOrderListState validates the complete response; the finance owner reuses it and omits legacy PROJECT rows from this native workflow.
2. Selecting an order clears the prior financial evidence. A read-only API query independently checks current stored selection, direct FIELD/order READ, app roles/Accounting.View and original CORE/PRODUCTION bindings. No book or AFE identity is guessed.
3. Existing AfeService reads complete owned original budget membership twice, validates schema/private proof and returns public original book/AFE/amount/currency/maker/version facts. The API reads each actual configured book, recorded independent decision/current scope and complete mixed labor/material cost population through the existing owners, then compares two complete observations.
4. The workbench displays each original budget and recorded actual amount in its own currency/precision, budget remaining, independent outcome/current blockers and contributing cost count. Legacy BudgetAmt/ActualAmt header defaults do not feed amounts. An empty owned budget population offers recorded company books for explicit preparation; it does not claim zero actual costs or commitments.
5. Exact order/book/AFE links open existing native detail, preparation, independent review and actual labor/material valuation panels. The public status panel receives the original financial query identities. Visible action entry points follow current capabilities, original maker independence and recorded approval; their existing APIs enforce fresh write authority separately.

## Architecture and states

Data owns the financial query/discovery contract. CaptureOrder extends the existing financial authority boundary without fabricated book identities. Native AFE discovery reuses the same repository/schema/proof reader as preparation/recovery. The scoped financial workspace reuses WorkOrderListState; the typed client uses the existing work-order budget resource. No new store, duplicate financial entity, raw SQL, business EF or live migration is introduced.

MudBlazor provides controls and focus behavior; no custom CSS or JavaScript is added. Default/unconfirmed, loading, disabled, error, known-empty and confirmed sources are explicit. Company/account/FIELD/auth/capability changes clear evidence and fence late results. Budgets/costs remain read-only here; no intent, executor rationale or valuation rationale is exposed, and no workflow completion, stock mutation or journal posting occurs. Amounts in different books/currencies are not aggregated.

## Focused verification

- Actual isolated Data: zero warnings/errors, 2.02 seconds.
- Native budget/discovery/material/labor gate: final 104 passed, 13.9741 seconds. Public discovery checks original book/AFE, known-empty membership, private rationale/intent exclusion, missing schema, revoked authority, wrong binding, foreign headers and changed observations. The initial expanded run passed 103/104 because the mutation fixture changed after its closing observation; moving it between observations verifies the intended paired-read refusal. Paired reads do not claim a transactional reporting snapshot.
- Workbench/status typed client/state/forms: 39 passed, 9.9607 seconds. Includes recorded amounts despite poisoned header totals, original query links, empty/preparation choices, current approval/review controls and account/FIELD/permission/late-result fences.
- Existing complete landing/inbox/native workflow journey: 49 passed, 23.5773 seconds. Existing accounting route compiles with the workbench; explicit focused FIELD/client seams remain declared.
- Final targeted workbench/status form checks: 22 passed, 8.2469 seconds, explicitly verifying source identities arrive as parameters without manually setting input fields. Actual accounting landing/inbox parent checks: 2 passed, 6.9909 seconds, explicitly asserting the financial workbench entry point on both AP and PO controller landings. Counts overlap earlier checks.
- Protected actual discovery approval/rejection/read-only viewer/mixed-cost journey: 3 passed, 1.8366 minutes, zero failures/skips, terminal exit 0 in oilgas-324-financial-sources-http.log. Session 54825 is closed; no HTTP test remains live. Read-only viewers discover the original book/AFE/decision, known-empty preparation uses recorded books, and mixed totals include both actual sources without additional writes or private valuation rationale. Shared HTTP output was preserved throughout the run.

Live provider/migration/concurrency, full solution, OIDC, browser/RTL/keyboard/screen-reader and cross-provider atomicity acceptance remain open. Original recorded start/labor/material, configured-book/auth/provider and unused posting dependencies are controlled seams in the protected cost fixture.

## Remaining full-plan work

This implements discoverable native work-order financial sources and accounting landing entry points with focused verification. It does not close all accounting dashboards or CA-10/14: commitments/invoice correlations, contractor/equipment valuation, controlled corrections, budget lines/amendments/adoption, currency conversion, posting and complete responsible-person queues remain required. Independent maintenance verification/return/lifecycle and all remaining persona/multinational acceptance remain required. The full goal stays active; commit and push follow its complete requirement-by-requirement audit.
