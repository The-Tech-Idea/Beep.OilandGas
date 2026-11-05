# Implementation 114: Recorded work-order to existing workflow evidence

## Visible handoff

The existing canonical work-order detail now loads a separate typed process review from /api/field/current/workorder/{instanceId}/processes. The recorded header renders first. Workflow loading, confirmed absence and failure have distinct presentation; failure does not erase the header or become an empty successful list. Recorded process name/identity/status/raw state/current step and UTC retrieval are shown. Each link opens the existing process-instance detail route using the encoded actual instance identity.

TheTechIdea.Data owns the review envelope and link projection. It identifies the original company/account, FIELD, work-order identity and retrieval time. WorkOrderDetailState validates that envelope and each unique link, clears it with header/company changes and rejects late responses. Raw process states remain raw; they are not converted into Draft/Planned/Completed work-order states.

The API queries the existing IProcessService for an explicit ENTITY_TYPE=WORK_ORDER and ENTITY_ID equal to the recorded order. Only the selected FIELD contributes links. It requires each contributing definition to be a WORK_ORDER process for a WORK_ORDER entity; equipment-oriented seeded definitions do not establish this relationship. Malformed, duplicate and mismatched provider records refuse. No equipment-ID inference, alternate process store, automatic process creation, seeded grants or new workflow definition is introduced.

Every contributing instance is checked through the existing CompanyProcessReadService, which independently checks FIELD and source entity READ authority. The API closes those checks and compares actual state/step/status before delivery, then rechecks original order authority. Existing process-detail role/source/action gates remain in force after navigation. Company membership is refreshed through the existing company gateway.

## Focused verification

Actual canonical Data build: zero warnings/errors, 1.91 seconds. Scripts/Test-WorkOrderListState.ps1: **152 passed**, zero failed/skipped, terminal exit 0, 1.2105 seconds. Log: %TEMP%/oilgas-workorder-process-114.log. Twenty-four new actual process projection/view-state cases cover encoded identity, preserved raw state/status/step, wrong entity/definition, duplicate/missing relationships, another FIELD, original company/account/order envelope, actual empty review, failure preserving the header and company-switch late-response rejection. The preceding 128 cases pass alongside them.

The gate compiles current projection/state/DTO sources and the five current service sources against actual Data and cached PPDM/model dependencies, with actual source precedence for old cached DTO collisions. ProcessInstance/Definition dependencies are cached actual model types; native process repository execution was not performed. Controller/typed-client/full Razor/DI, CompanyProcessReadService with actual access stores and provider/browser execution remain source-only wiring, not HTTP or authorization certification. Whitespace diff check passed.

## Remaining complete journey

Verify native WORK_ORDER resource admission, two identities/companies with colliding IDs, physical CORE/LIFECYCLE bindings, current definitions and role revocation before accepting this handoff. Existing equipment-oriented maintenance definitions require reviewed explicit order linkage; absence of such a link is reported rather than silently starting a process. Required role assignments and source access cannot be guessed from duty names.

Canonical creation, process/action command ownership, scheduling/material/permit evidence, recorded inspections, independent supervisor acceptance, durable outcome/retry receipts, accepted return-to-service and accounting handoff remain open. Preflight/source closing checks are not a transactional snapshot or provider atomic command. Performance/paging and browser/keyboard/RTL gates remain active. No persona, phase or goal closure.

No handwritten SQL, schema, new business EF lane, live migration or grants. Business schema changes remain Beep MigrationManager-owned.
