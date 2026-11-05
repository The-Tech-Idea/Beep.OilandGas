# Implementation 115: Canonical maintenance header creation

## Maintenance persona screen and recorded data

The existing maintenance landing create form now requires a recorded work-order number, title, selected FIELD, type and reported jurisdiction. Description is preserved as instructions; optional equipment is explicitly identified as equipment. Planned scheduling is a separate workflow and is no longer accepted by header creation. The result opens the existing recorded detail and linked-process review from Implementation 114. No Draft state, checklist, schedule, approved permit, acceptance or AFE is inferred from saving a header.

The existing WorkOrderService creation owner now writes actual WORK_ORDER and WORK_ORDER_COMPONENT rows through the canonical company-bound Beep repositories, replacing its unsupported PROJECT creation assumptions. TheTechIdea.Data remains the single request/response owner. WORK_ORDER_NUMBER comes from the user; technical IDs are generated. REQUEST_DATE and audit fields record UTC and the authenticated actor. FIELD_ID and optional EQUIPMENT_ID belong to the component. Title, reported jurisdiction and description use one Data-owned instruction format shared by server preparation/readback and client confirmation.

Reported jurisdiction is text, not a certified legal reference. WORK_ORDER_COMPONENT.JURISDICTION participates in the LAND_SALE relationship with LAND_SALE_NUMBER; neither is invented from a free-text country or authority. No default country, provider schema, reference type or grant is seeded.

## Command and review boundaries

Creation requires the existing company gateway binding, authenticated actor and refreshed FIELD WRITE authority; optional equipment requires its own current READ authority. An active exact FIELD and active exact R_WO_TYPE must exist, and both are read again inside the transaction. Existing field-linked recorded orders are checked for the requested number; this uniqueness policy applies to the original FIELD, not globally across other companies or fields.

The existing PPDMDataSourceTransaction owns header/component insertion, in-transaction readback and authority rechecks. Creation explicitly requires a serializable physical provider transaction. A provider that cannot establish that owned isolation refuses before insertion. Current binding and write authority are checked between writes and before commit. After commit, the existing canonical read is repeated and compared with the submitted number, FIELD, type and complete recorded instructions before success is returned. Native rollback and concurrent duplicate behavior still require provider evidence; source compilation does not certify them.

The existing Web state freezes the original request and company/FIELD and retains uncertain outcomes during page lifetime. It now also checks complete recorded instructions before accepting a response. It is not a durable API receipt. A failed post-commit readback leaves an uncertain outcome, not proof of rollback or permission to repeat blindly.

## Production-to-maintenance handoff correction

Intervention approval now records the decision without automatically creating a work order or AFE. Its button says Record Approval and explains the separate maintenance handoff. The previous well query prefill no longer copies a well ID into EquipmentId. WORK_ORDER_COMPONENT has separate UWI and EQUIPMENT_ID columns; equality cannot be inferred. The API refuses a new automatic intervention work-order request before creating records until the canonical well/component relationship is explicitly reviewed. Its unsupported automatic creation branch was removed. Existing linked-order admission still needs canonical relationship review before this handoff can be certified.

The shared decision request retains an explicit recorded-number requirement for a requested new order, and its retained intent preserves that value. These checks do not establish the missing well relationship or make the entire intervention decision/handoff transactional. Approval alone needs no invented order number, jurisdiction or financial record.

## Focused verification

Actual isolated TheTechIdea.Data source build: zero warnings/errors, 2.18 seconds. Scripts/Test-WorkOrderListState.ps1: **174 passed**, zero failed/skipped, terminal exit 0, 1.0078 seconds. The gate compiles actual current preparation/readback, projections, view states and all five work-order service sources against the actual Data assembly and cached actual PPDM dependencies; it does not execute a production datasource transaction. Twenty new preparation/readback cases and two additional client refusal cases cover canonical columns, explicit intent, unsupported scheduling, absent country/legal assumptions and mismatched saved instructions/identity/state/source.

Scripts/Test-ProductionInterventionDecisionState.ps1: **27 passed**, zero failed/skipped, terminal exit 0, 0.8468 seconds. It covers frozen order-number intent, missing-number refusal and approval without downstream creation alongside the existing outcome/company/source checks. It does not compile or execute the parent Razor page or controller. Logs: %TEMP%/oilgas-workorder-create-115.log and %TEMP%/oilgas-intervention-order-number-115.log. Whitespace diff check passed.

## Open delivery gates

Native controller/DI/typed HTTP/full Razor, app role revocation, resource admission and exact physical datasource must be verified. Current active type/equipment catalogs, compound references, provider serializable isolation, rollback, post-commit readback, duplicate-number concurrency and company/actor changes require native provider checks. Add a durable command/outcome receipt before reliable retry across page/session lifetimes. Header creation does not start a process; reviewed explicit WORK_ORDER process linkage, recorded scheduling/materials/permits/inspections, independent supervisor acceptance, return-to-service and accounting handoff remain open. Provider/browser/keyboard/RTL checks and other persona journeys remain active.

No handwritten SQL, live migration, schema change, new business EF lane or grants. Any required business schema work remains Beep MigrationManager-owned. No persona, phase or goal closure.
