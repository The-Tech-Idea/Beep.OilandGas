# Implementation 342: Development AFE investment review

2026-10-25. Scoped app slice locally verified under CA-22 / FG-52. Extends [341](Implementation-341-Development-AFE-Request.md) and the existing [asset-delivery owner](Feature-07-Asset-Delivery-and-Interventions.md). The full goal remains active.

Deliver the actual finance queue -> exact approved-FDP/AFE/book source -> independent explicit approve/reject rationale -> frozen confirmation -> retained original -> canonical APPROVAL_WORKFLOW decision -> authoritative outcome/reload/history across finance and engineering readers. AFE remains the original DRAFT request. Approval records a budget decision; it does not create a commitment, payment, actual cost or operations approval.

Reuse current company/standard-role authority, exact original FIELD grants/resources, configured books, AfeService and AuthorizationWorkflowService. TheTechIdea.Data owns shared contracts and nullable decision lineage on the existing APPROVAL_WORKFLOW entity through its PRODUCTION Beep manifest. No SQL, new business EF lane, duplicate finance store or compatibility endpoints.

Accounting.ReviewDevelopmentBudget plus existing finance/development/workflow View and original FIELD WRITE admits a new independent decision. Original request maker cannot decide. Public outcome reads and exact original recovery use current View/FIELD READ. The existing encrypted tab journal retains each decision before transport. Failed/uncertain transport cannot invent success or replace the original. Finance queue labels recorded/pending decisions from native facts; it does not invent a lifecycle task or assigned recipient.

Acceptance includes actual rendered queue/source/decision/recovery, independent current authority, exact amount/currency/source/book/audit, schema/transaction/SDK boundaries, rollback, duplicate/retagged/corrupt evidence, loss recovery, encrypted retention and context/late fences. Run affected focused tests after every coherent change and retain the relevant AFE/baseline/book/work-order regressions. Provider/migration/host/browser and later explicit lifecycle routing/notification remain separate gates until observed.

## Implemented journey and owners

The accounting landing exposes Company development investment work through standard ASP.NET role checks and the selected company's Accounting.View, DevelopmentPlanning.View and Workflow.View capabilities. The contextless `/economics/afe` parent renders the actual current company/FIELD request queue. Each card shows the recorded AFE/approved FDP identity, exact book/currency/precision, amount, calendar date, maker/UTC and pending or recorded decision. Its fixed link opens the exact source and AFE anchor. The queue never labels available work as an assigned lifecycle task.

The same source parent embeds DevelopmentInvestmentPanel for each native request and the just-confirmed request receipt. Explicit inspection loads current source/decision evidence; the original maker is blocked. An eligible independent reviewer chooses approve/reject, enters a rationale and confirms a frozen snapshot. The existing encrypted tab journal retains it before HTTP. Only a matching native receipt or checked absence clears that original. Unknown transport/receipt/cleanup outcomes block replacement and support original recovery after page recreation. Public outcome facts omit private rationale, command and intent.

DevelopmentInvestmentState and DevelopmentInvestmentQueueState fence company/account/persona/route changes and late responses. A queued refresh during an old company read fetches the current company after discarding the old answer. The actual parent encodes native names as text. Planning CAPEX retains all recorded decimal digits in millions of its source currency; investment money uses the selected native book precision.

- TheTechIdea.Data owns DevelopmentInvestmentReview/Command/Receipt/public facts, the company queue/index projections, and nullable FDP_BASELINE_ID, DEVELOPMENT_DECISION_JSON and DEVELOPMENT_DECISION_REVISION on existing APPROVAL_WORKFLOW. Null values preserve existing work-order decision serialization/digests. Existing PRODUCTION manifest discovery owns this same entity; no new financial table or SQL/business EF lane.
- AfeService shares its existing physical binding/schema/owned Serializable Beep transaction, typed SDK conversion, exact original request and complete request-population readers with AuthorizationWorkflowService inside the production assembly. The new native owner records one deterministic first decision per company/AFE and preserves the original DRAFT request. Exact current book/currency/amount/independence, source/body/audit, complete native membership, SDK insertion/readback and closing checks are required. An unexpected companion request during insertion rolls back the whole operation.
- AuthorizationWorkflowService handles exact original retry/recovery and public decision reads. Inactive/foreign/retagged/duplicate/altered decision rows cannot become an absence or silent repair. The selected COMPANY_LEDGER_BOOK, AFE and APPROVAL_WORKFLOW share one native transaction. Core/FDP/lifecycle/resource evidence brackets it; no cross-source atomicity or live concurrency certification is claimed.
- CompanyDevelopmentAfeService remains the one API admission owner, with investment and queue partials. It reuses actual request-account resolution, app-owned current standard company roles/capabilities, exact original FIELD grants/resources and core/planning/lifecycle/production bindings. New decisions require Accounting.ReviewDevelopmentBudget and FIELD WRITE; maker self-approval is forbidden. View/READ admits public outcomes and the exact original actor's recovery after review/write revocation.
- The queue qualifies current company/account/role/FIELD access before private request proof. Native AFE candidates include canonical ownership markers even after a changed source tag. Company books are read once per queue and approved source scope once per process group. A closing native AFE/decision observation refuses a decision changed during loading. An authorized foreign company with the same actor, role and FIELD cannot read the original company's private request.
- The current controller, IAfeServiceClient/AfeServiceClient, DevelopmentDraftIntentStore and shared failure/refusal reporting are extended. Existing API DI already registers the canonical authorization-workflow service. No alternate approval engine or successful empty fallback is introduced.

## API additions

Under authenticated, no-store `/api/accounting/cost/afe`:

- GET `development/queue` returns the current bounded company/account/FIELD queue.
- GET `development/{afeId}/investment?processId=...` returns the exact approved source/request and recorded investment outcome.
- POST `development/investments` records the frozen independent decision.
- POST `development/investments/recover` checks its exact retained original without another write.

The queue is bounded to 256 requests and 128 eligible FIELD grants, with refusal rather than silent truncation. Provider paging/scaling beyond these bounds remains a separate gate. These routes take no caller-selected datasource connection.

## Verification

**827 focused tests pass**, zero failures/skips, **515.532 seconds (8.5922 minutes)**, terminal exit 0. Separate work-order budget/decision/costing regressions: **64 pass**, zero failures/skips, **14.9542 seconds**, terminal exit 0. Data builds with zero errors and one existing missing IdentityServer-analyzer warning (**2.57 seconds**).

Incremental native/AFE/book 52, initial API 13, queue/source UI/index 56 and accounting landing 3 passed during construction; the final combined gate retains all affected cases plus later hardening. Counts overlap and are not added. [Exact filters, class counts, logs, source/generated/runtime hashes, fixture correction and limits](evidence/342-development-investment-review/observed-evidence.json).

The landing fixture initially resolved bUnit services before authorization registration. Registration was moved before service resolution; its three role/capability cases and the final clean gate pass. Product authorization and native transaction guards were not weakened. Provider/auth/resource/JS/diagnostics seams remain explicit; no browser, full-host or live-provider acceptance is claimed.

## Remaining delivery work

1. Add explicit lifecycle assignment/notifications and any configured tier/amount policy through the existing framework owners. This available-work queue and native first decision do not fabricate a persisted lifecycle assignment or multi-stage investment process.
2. The residual automatic work-order AFE bridge/card and its old controller/client/process/shim paths are retired in [343](Implementation-343-Retire-Automatic-Work-Order-Finance.md), with current native financial controls and focused regressions retained.
3. Continue approved source-backed design/program/scenario, drilling/construction actuals, changes and independent operations handover under the existing feature/persona owners. Investment approval alone does not complete them.
4. Qualify deployed Beep migration/provider LOB/constraint support, real isolation/concurrency/reconciliation, full hosted app/OIDC composition, keyboard/accessibility/RTL/localization and multinational acceptance. No full goal closure, staging, commit or push.
