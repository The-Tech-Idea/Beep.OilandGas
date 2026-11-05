# Implementation 116: Recorded well-to-maintenance components

## Visible production and maintenance journey

The existing maintenance create form now accepts an optional recorded well UWI independently of its optional equipment ID. Production follow-up carries the reviewed well query into WellId; it does not copy a well into EquipmentId. The recorded detail shows each FIELD component identity and its separately recorded well and equipment references. A multi-component order retains every contributing component rather than arbitrarily choosing a scalar equipment or well identity.

The existing intervention decision screen adds an optional recorded work-order ID to link when approving. Approval without a link remains available and does not create an order or AFE. Linking requires an existing corrective recorded order, the original FIELD and an actual WORK_ORDER_COMPONENT.UWI matching the reviewed well. A matching equipment ID alone refuses. Automatic intervention header creation remains refused; the user creates the maintenance header separately and explicitly selects its recorded identity for linking.

TheTechIdea.Data owns WorkOrderComponentReference in the existing WorkOrder contract owner and the shared component checks. The existing canonical projection fills Components from actual WORK_ORDER_COMPONENT rows; GetById carries that collection to detail. List/detail view states validate compound order/component identities and original FIELD before publishing. Creation freezes the optional well and equipment references and accepts success only when the saved single component returns exactly those submitted references, alongside the existing number/type/instructions/readback checks. An absent component is not confirmation of creation. Older successful header reads with no returned references show component evidence unavailable rather than inventing relationships.

## Existing persistence and authorization owners

Canonical creation writes UWI and EQUIPMENT_ID to their separate component columns through the existing WorkOrderService and Beep repositories. The existing company gateway and owned serializable transaction from Implementation 115 remain the command boundary. A requested well requires refreshed independent WELL READ access, plus an exact single active WELL in the original company datasource inside that transaction. Missing, inactive, ambiguous or wrong-type provider rows refuse. No well, equipment, activity or reference catalog is created implicitly.

Recording the FIELD component establishes that order's declared component relationship; it does not infer a preexisting WELL-to-FIELD hierarchy or an intervention activity foreign key. WELL_ACTIVITY_SET_ID/SOURCE/TYPE_ID are not fabricated. Exact physical provider foreign keys and reference behavior still need native verification.

The production API reviews the explicitly selected order through its current company-bound owner, with independent WORK_ORDER READ checks before and after the read and unchanged original FIELD checks. Shared checks require canonical WORK_ORDER source, corrective type, valid unique compound components and an actual UWI match. Before writing the decision, the API rereads the order and compares the complete returned well/equipment component set. Existing source-revision, WELL WRITE and independent-maker approval gates remain. The existing financial mapping now uses the recorded work-order number rather than substituting its technical identity, and leaves property unavailable rather than inferring it from a well or equipment; broader financial connection/command verification remains open and the screen does not request an AFE.

These preflight and closing checks are not an atomic transaction across the production decision and work-order owner. They do not prove durable outcome receipt, recorded decision readback, concurrency isolation or next-role acceptance. No fallback order is created when the asserted order is absent or mismatched.

## Focused verification

Actual isolated TheTechIdea.Data source build: zero warnings/errors, 1.56 seconds. Scripts/Test-WorkOrderListState.ps1: **202 passed**, zero failed/skipped, terminal exit 0, 1.0389 seconds. Twenty-eight additional cases cover actual canonical multi-component projection, separate UWI/equipment columns, equipment-only mismatch, wrong FIELD/order/source/type, duplicate/missing/malformed components, list/detail refusal, frozen client intent, exact saved-reference confirmation and active/absent/ambiguous native WELL row validation.

The focused gate compiles current actual work-order DTO/check/projection/view-state/preparation sources and all five service owners against actual Data and cached actual PPDM dependencies. Its reference tests exercise actual WELL model rows, not a live datasource. Scripts/Test-ProductionInterventionDecisionState.ps1: **27 passed**, zero failed/skipped, terminal exit 0, 0.8480 seconds. Its retained-intent/outcome cases remain green. Logs: %TEMP%/oilgas-workorder-components-116.log and %TEMP%/oilgas-intervention-link-116.log. Whitespace diff check passed.

Parent Razor pages, controller/DI/HTTP wiring, resource stores and provider transaction execution were not compiled/executed by these focused gates. They remain source-only wiring; none of these results certify the full browser or role-enforced handoff.

## Remaining complete persona gates

Verify native WORK_ORDER/WELL resource admission and revocation, physical CORE/LIFECYCLE connections, provider compound references/serializable transaction/rollback/readback, multiple companies with colliding identities and current type/equipment/well catalogs. Validate the optional-link UI and deep-link navigation in the actual browser, keyboard and RTL experience. Production decisions need durable original-source command/readback receipts and atomic outcome handling before reliable retry. Explicit order-to-process linkage, planning/material/permit evidence, inspections, independent supervisor acceptance, accepted return to service and accounting handoff remain open. Other persona workflows remain in the master scope.

No handwritten SQL, schema change, live migration, new business EF lane or seeded grants. Business schema changes remain Beep MigrationManager-owned. No persona, phase or goal closure.
