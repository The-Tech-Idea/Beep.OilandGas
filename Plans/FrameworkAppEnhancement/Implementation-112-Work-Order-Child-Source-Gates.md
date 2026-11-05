# Implementation 112: Original work-order source gates for child operations

## API changes

Every existing WorkOrderController route containing an instanceId now verifies the original work-order source through the existing IWorkOrderService before invoking its action or child owner. The current selected FIELD and authenticated application actor supply the access context. The existing IAccessControlService independently checks FIELD READ for reads and WRITE for mutations; a missing grant prevents even the source read. Returned order identity, FIELD, subtype and state must be present and match the route context. Missing orders refuse; mismatched source data cannot authorize child operations. The shared helper checks authority again after reading the source.

The guard covers existing detail/transitions, deletion, scheduling/rescheduling/conflict review, contractor assignment/removal, cost/AFE operations and checklist recording. Read endpoints close source/authority review again before returning success. These checks supplement the existing authenticated/current-FIELD filter; they do not create a new role mechanism or authorization authority. The selected-company gateway and current application FIELD grant owner remain authoritative.

The two existing work-order AFE endpoints now explicitly pass the current company gateway's verified connection to their existing Accounting owner rather than accepting that owner's PPDM39 default. Contractor qualification requests require an explicit jurisdiction; the API no longer substitutes USA. Regional qualification/template correctness remains unverified.

## Focused evidence

Scripts/Test-WorkOrderListState.ps1 passed **112 cases**, zero failed/skipped, terminal exit 0, 1.1038 seconds. Log: %TEMP%/oilgas-workorder-source-112.log. Twelve new actual WorkOrderSourceChecks cases cover before/after authority, denial before source transport, wrong FIELD/identity, incomplete state/subtype, missing source/context, revocation and propagation of read/authority failures. The preceding 100 state/workflow/binding checks pass in the same gate, which also compiles all five changed work-order service sources from Implementation 111.

The helper test callbacks are fixtures for the existing source/access owners, not evidence of native authorization, HTTP or provider execution. Controller wiring and the existing AFE connection overload signatures were inspected in source. The native controller/DI/CompanyModuleConnection and AFE transport/provider paths were not compiled or executed in this focused gate. Missing production projects were not replaced. Whitespace diff check passed.

## Remaining native requirements

Reconcile the actual PPDM PROJECT/child relationship and missing-column assumptions before claiming work-order persistence. Verify direct API denial with two company accounts and colliding order IDs, membership/role revocation, selected FIELD changes, provider evidence containment and actual API wiring. Explicit connection assertions close the work-order endpoints' default AFE target path; the wider WorkOrderAccountingService's other callers still require review, and its internal writes do not yet supply an atomic company/version contract.

Source checks do not validate every child row's compound identity or give transaction consistency. General contractor-license visibility, equipment/resource checks, current approved action roles, independent supervisor acceptance, recorded inspection history, reviewed regional templates, permit/material prerequisites, durable command receipts and next-role/cost handoffs remain open. A source check before a mutation is not atomic authorization/state comparison. No persona, phase or goal closure.

No SQL, schema, new business EF lane, live migration or grants. Business schema changes remain Beep MigrationManager-owned.
