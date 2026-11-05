# Implementation 113: Canonical recorded work-order review

## Actual data path

The existing WorkOrderService list/detail read path now reads canonical WORK_ORDER_COMPONENT relationships filtered by the confirmed FIELD and active indicator, then each linked canonical WORK_ORDER header by its recorded identity. It uses the existing company-bound repository factories and before/after binding checks. PROJECT column assumptions were removed from these read methods; there is no parallel or legacy read fallback.

RecordedWorkOrderRows validates actual canonical entity types, FIELD/order/component identities, active indicators, unique compound component keys, unique headers and complete component-to-header coverage. An orphan or ambiguous relationship cannot become an empty successful result. Multiple recorded components contribute one header. These records establish a recorded FIELD relationship, not a legal ownership interest.

The existing WorkOrderSummary/detail and accompanying DTOs now live once in TheTechIdea.Data with their original namespace. Summary state no longer defaults to Draft. Projection preserves recorded WORK_ORDER_NUMBER, WORK_ORDER_TYPE, REQUEST_DATE, DUE_DATE, COMPLETE_DATE and INSTRUCTIONS; it explicitly marks workflow evidence unavailable. Dates are not silently classified as planned start or accepted return to service. No status/approval/cost/inspection evidence is inferred from the header.

The existing maintenance landing has a distinct workflow-state-unavailable column for these actual records. Opening a recorded order shows its header, FIELD, recorded number/type/dates and instructions. It does not query unsupported companion mappings or show default budget/actual zeros for this review. Unknown timezone, workflow state, acceptance, inspection, permit and cost evidence are explicit. Existing context clearing and late-response checks remain. Read-only source review admits an explicitly identified canonical header without workflow state; child actions and mutations still require confirmed state. State filtering refuses when the canonical source does not establish state.

## Focused evidence

Actual canonical Data build: zero warnings/errors, 1.60 seconds. Scripts/Test-WorkOrderListState.ps1: **128 passed**, zero failures/skips, terminal exit 0, 0.9645 seconds. Log: %TEMP%/oilgas-recorded-workorder-113.log. This gate compiles the actual changed read service and all five current companion service sources, current Data/DTO/workflow/state sources and canonical PPDM dependencies. Sixteen new actual header/component and UI-state cases cover preservation without invented state, wrong/duplicate/inactive relationships, orphan headers, duplicate/wrong headers, one header for multiple components, actual empty results, readable canonical detail and refused transition without workflow evidence. The previous 112 cases pass alongside them.

Current source definitions take precedence over old cached DTO collisions in the focused project. This is canonical model/projection/state evidence and changed-source compilation, not actual provider/HTTP/controller/DI/full Razor/browser execution. Whitespace diff check passed.

## Full maintenance workflow still required

Creation, transition and companion mutation implementations still assume unsupported PROJECT/child fields; this read correction does not deliver them. Reconcile the canonical WORK_ORDER header/components with the existing process-instance/task owner using explicit recorded relationships and approved definitions. The seeded maintenance processes are equipment-oriented and their step states cannot be relabeled as order states without reviewed mapping. Do not create a parallel process store or infer linkage by an equipment ID alone.

Complete durable order creation/read-back, scheduling and material/permit prerequisites, actual inspection capture, independent supervisor acceptance, return-to-service and cost/accounting handoff. Verify approved company roles, two companies with colliding IDs, physical providers, original-context retries, current authority and browser/RTL/keyboard. The component/header read must also undergo paging/performance and transactional-snapshot checks. Any required business schema change goes through Beep MigrationManager; no handwritten SQL or business EF lane.

No schema/live migration, grants, persona/phase/goal closure.
