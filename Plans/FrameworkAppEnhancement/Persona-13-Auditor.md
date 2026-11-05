# Auditor: company data workflow and screens

Updated 2026-10-06. **Planned journey; existing source surfaces inventoried, end-to-end delivery remains open.** Task owners: CA-13/41/44; FG-52. Extends [Feature-11-Release-and-Operational-Evidence.md](Feature-11-Release-and-Operational-Evidence.md). [Persona delivery index](Company-Persona-Data-Workflows.md) supplies shared requirements; root master tracker owns status.

## Scope and role responsibilities

Explicit company, period/resource, evidence classification and approved export scope.

Independent read/review/export rights; auditor persona must not imply posting, correction, grant administration or personal-data disclosure.

Business duties above are not assertions that matching ASP.NET role names are seeded. Map them to the selected company's approved existing app-role catalog and assignments. Persona preference never grants rights. Standard roles guard pages/components; APIs independently enforce current company, permissions and resource rights. Multiple duties do not bypass separation of duties.

## Existing screens and landing decision

| Responsibility | Verified route(s) | Existing screen |
|---|---|---|
| Audit evidence | `/data/audit` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Data/AccessAudit.razor) |
| Close history | `/ppdm39/accounting/period-close` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Accounting/PeriodClose.razor) |
| Review requests | `/ppdm39/tasks/inbox`, `/tasks/inbox` | [Source](../../Beep.OilandGas.Web/Pages/PPDM39/Workflow/TaskInbox.razor) |

Extend the appropriate current experience root and reconcile overlapping process forms before adding routes. Current source routes do not prove authorized runtime behavior or a completed landing. Missing dedicated screens remain explicit coverage gaps.

## Data workflow and handoff

| Step | Source data shown | Permitted action | Recorded outcome / next screen |
|---|---|---|---|
| Scope to sample | Authorized population/period/source links | Select sample; inspect approval/posting chain | Reproducible sample without leakage |
| Evidence to finding | Actor, version, time, correction and approval history | Record finding through governance owner | Finding assigned to accountable owner |
| Finding to follow-up | Response/action/closure evidence | Review remediation; accept/reject closure | Independent recorded follow-up |
| Review to export | Permitted source records and provenance | Generate scoped evidence export | Authorized reproducible package |

Finance/operations/admin provides evidence; management remediates; auditor reviews independently.

Reuse the existing workflow/task owner and inbox. Each recipient sees the assigned entity/version, evidence, due date, permitted actions and rejection/return reason. Detail screens retain company/resource context, validate reviewed versions at the API and reload actual persisted outcomes. Corrections preserve source and reasons; uncertain writes retain the original intent for explicit reconciliation, without automatic duplicate submission.

## Persona landing dashboard

| Queue / KPI | Source and required drill-down |
|---|---|
| Findings/responses due | Recorded action/state/owner; drill into evidence |
| Evidence gaps | Missing recorded links; never fabricate compliance |
| Audit reviews | Current authorized assignments and due dates |

Show company/resource/period, currency or units, source/as-of time and freshness/approval status. A card opens the exact contributing authorized records with identical filters. Prioritize assigned work, actionable exceptions and permitted next actions. Missing/stale/restricted data is explicit, never a fabricated zero or completion. Company switches clear prior values and reject late responses.

## First visible delivery slice

Reconcile a read-only audit landing with existing history screens -> scoped sample -> finding -> response -> review. Generic administrator write screens are not auditor workspaces.

1. Name existing Data/service/API/client/screen/task owners and verify exact company role mappings before coding.
2. Connect landing queue -> source/detail -> permitted action -> next-role task/outcome -> authoritative reload -> KPI drill-down. Implement only the missing backend path required for this journey.
3. Use API-first clients and TheTechIdea.Data contracts; business schema changes use Beep MigrationManager. No handwritten SQL or new business EF migration lane.
4. Focused checks cover allowed/denied roles, direct deep links/API denial, two companies with colliding IDs, self-approval rejection where applicable, missing/stale data, validation/failure, uncertain outcome/retry, reload, reassignment and company switch during a request.
5. Record state/client/API checks separately from rendered browser, keyboard/RTL, OIDC and real-provider evidence. Backend tests alone do not deliver this persona or close its CA/FG tasks.


Implementation 107: [Company persona landing queues](Implementation-107-Company-Persona-Landing-Queues.md), 38 focused state cases pass. Nine additional existing experience roots now show their current catalog-matched assigned-work/source queue; native authority/catalog/browser and complete persona action/read-back/handoff gates remain open.

Implementation 128: [Maintenance process source review](Implementation-128-Maintenance-Process-Source-Review.md) connects recorded order/component/inspection facts to the existing process action review and next-role screen. Header, component, requirement/result changes invalidate the reviewed revision; wrong definitions and original-context mismatches refuse. Data builds cleanly; 41 API review, 38 page-state, eight source-render, 325 work-order regression and eight inspection-render checks pass. Full controller/parent-page/HTTP/browser/provider, duty mapping and independent acceptance/return-to-service remain open. No phase closes.

Implementation 129: [Process review response contract](Implementation-129-Process-Review-Response-Contract.md) reuses shared original-company/actor/process, action and source-evidence validation at the existing API helper/Web state and typed-client source. Malformed JSON, ambiguous choices and empty responses cannot publish actions. Data builds cleanly; 64 JSON/state, 41 API review and eight source-render checks pass. Full typed-client/HTTP/controller/parent-page/browser/provider and independent maintenance acceptance remain open; no phase closes.

Implementation 134: [Reviewed definition start and readback](Implementation-134-Reviewed-Definition-Start-and-Readback.md) connects configured landing definitions to recorded steps/responsibilities and an original company/FIELD/source start review. Null/mismatched outcomes retain the page-session attempt; navigation requires matching recorded instance readback. API helper validates active definition identity/type/structure/content revision, while the existing endpoint source independently checks FIELD/source WRITE authority. Data builds cleanly; 94 state, 60 API helper/review and 16 actual source-render checks pass. Full parent page/client/controller/mapper/HTTP/browser/provider, duty mapping, atomic concurrency and durable recovery remain open; no phase closes.

Implementation 135: [Recorded process initiation source](Implementation-135-Recorded-Process-Initiation-Source.md) requires recorded journal header/lines or work-order header/FIELD components before starting from the existing definition screen. Original-scope content reviews are rechecked by the API source, and changed/failed/late reviews clear controls without releasing uncertain starts. Journal FIELD applicability remains explicitly unconfirmed. Data builds cleanly; 103 state, 82 API helper/review and 19 actual source-render checks pass. Full parent page/client/controller/HTTP/browser/provider, additional source families, execution duties, atomic concurrency, durable recovery and complete persona handoffs remain open; no phase closes.

Implementation 136: [Company-owned procurement source workbench](Implementation-136-Company-Owned-Procurement-Source-Workbench.md) adds supplier-filtered `/procurement` and encoded PO header drill-down through the existing accounting client/API, explicitly showing missing currency/downstream evidence. PO/line/receipt types move to Data; nullable owner columns use the existing PRODUCTION Beep manifest and scoped canonical service. The API replaces its anonymous/traditional path with fresh app-role/permission checks; unreviewed legacy creation refuses. Data builds cleanly; 29 ownership/service/Beep-metadata/state/render checks pass. Migration/reconciliation, full parent/controller/client/HTTP/browser/provider, buyer/receiver duties, receipts/stock/AP and durable writes remain open; no phase closes.

Implementation 137: [Recorded procurement lines and receipts](Implementation-137-Recorded-Procurement-Lines-and-Receipts.md) extends the same owned-PO detail with actual lines, exact receipt-to-line membership, active/inactive history and recorded provenance. Shared Data source reviews freeze and validate content; fresh schema requires a unique nonnullable header key. Independent Procurement.View plus current company roles replaces the prior accounting permission boundary without assigning grants. Data builds cleanly; 63 focused ownership/service/authority/metadata/JSON/state/render checks pass. Full controller/client/parent/HTTP/browser/provider, Beep migration/reconciliation, configured duties, stock/AP handoffs and durable writes remain open; no phase closes.

## Verified legacy access-history gaps (2026-10-21)

The current /data/audit page still calls the legacy DataManagementService audit methods. Source inspection of PPDM39DataAccessAuditController and PPDMDataAccessAuditService shows generic authentication, unscoped statistics/entity history and the service's default PPDM39 connection, without a selected-company envelope or independent auditor/export admission. User history alone cannot prove company ownership. These paths are not qualified company audit evidence and must not be promoted as an auditor journey.

The page labels one-day statistics as 30-day KPIs, initializes unconfirmed counts to zero, substitutes READ/one affected record/COMPLETE for missing event facts, converts timestamps using the host zone, retains old data after failed reads, and reports export queued without an export operation. Existing DataAccessEvent and AccessStatistics contracts have no selected-company proof. These are observed source gaps, not runtime/provider findings.

The existing DATA_ACCESS_AUDIT model already declares BUSINESS_ASSOCIATE_ID, while PPDMDataAccessAuditService.RecordAccessAsync does not populate it; there is no tenant/binding ownership proof. Resolve and enforce that existing field's company meaning before adding another company column. The active PPDM_CORE CorePpdmModule extension inventory currently includes work-order and intervention native types, not DATA_ACCESS_AUDIT. Any required audit ownership/schema extension belongs in that existing Beep module/migration path. Move the current DataAccessEvent, AccessStatistics and DATA_ACCESS_AUDIT owners into TheTechIdea.Data while preserving their active namespaces/contracts; do not introduce parallel legacy/scoped copies. These are source observations and implementation requirements, not an applied migration or owned-event population.

Deliver the existing access-history owner and route through canonical Data-owned scoped contracts and API-first reads. Define auditable company/resource correlation and classification before enabling history; ambiguous legacy events must remain unowned rather than be relabeled. Keep native business persistence under the existing Beep migration/schema owner, reusing DATA_ACCESS_AUDIT rather than a parallel ledger. Independently check current app roles, read/export rights and resource scope; use an explicit UTC period, source/as-of context, bounded complete population and matching drill-down. Remove fabricated outcome/count/operation values and false export acknowledgement. Preserve assigned review work while adding finding/response/independent closure through the existing governance/task owner. Native two-company/resource privacy, real export/read-back, failure/uncertainty, and actual rendered persona checks remain required; a safer empty or disabled page does not complete this journey.