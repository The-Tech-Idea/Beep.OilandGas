# Implementation 97: recorded work-order list scope

Updated 2026-10-06. Continues maintenance landing -> recorded order detail from [Implementation 96](Implementation-96-Production-and-Maintenance-Assigned-Work.md). Actual DTO/list-state focused proof; native/rendered order and command flow remains open.

## Existing screen correction

WorkOrderDashboard previously kept old orders after a failed reload, did not listen for company/FIELD changes, swallowed its field-name lookup failure and omitted cancelled orders from the board. The existing list now uses WorkOrderListState, which captures company before FIELD lookup, clears on reload/company invalidation and refuses late responses. The page listens for FIELD/company changes, closes the old create dialog, clears old rows/errors and queues reload if one is running. Changing subtype clears the current list before an explicit Apply.

Only a confirmed response renders board counts. A genuine empty response is an explicit empty state; missing company/FIELD or failed reads are unavailable rather than zero success. Current company/FIELD and loaded record count are visible. The existing cancelled state now has its own column. Each card uses an encoded exact order detail link in a MudButton, replacing a mouse-only card onclick. Create dialogs open only with a confirmed current list; this does not certify the existing creation command.

## Active contracts and owners

The existing WorkOrderServiceClient refuses null list responses rather than coalescing them to an empty list. The new view state replaces the page's local list and validates exact FIELD, requested subtype, unique nonempty order identity and one of the existing recorded work-order states. It adds no order repository or domain model.

Existing WorkOrderSummary requires InstanceId, FieldId and State JSON members, preventing its legacy DRAFT initializer from turning an omitted state into a recorded draft. The DTO stays in its existing Models owner for this change; broader legacy contract ownership migration to Data remains open. No duplicate production DTO is introduced.

## Focused evidence

Test-WorkOrderListState.ps1 compiles actual list state/company context/current WorkOrderDTOs source against actual Data and cached Models base dependencies: **14 passed**, zero failures/skips, terminal exit 0, 0.8537 seconds. Log `%TEMP%/oilgas-work-order-list.log`. Cases cover cancelled records, wrong FIELD/identity/state/subtype/duplicate/null lists, empty versus failed reload, company invalidation/late results, FIELD lookup invalidation, no selected company and missing required JSON members.

The isolated check includes the actual DTO source; CS0436 notices for older cached copies are suppressed while compiled source definitions take precedence. This is not a whole Models/Web/API build or replacement for missing production dependencies. Actual typed-client transport, parent Razor/DI/keyboard rendering, API list/resource authority and provider company isolation remain unexecuted. Whitespace diff check passed.

## Remaining complete maintenance workflow

Verify native list -> detail with selected company/FIELD/subtype, colliding IDs, revoked resource rights, stale states and browser filters/reload/RTL. Existing create/AFE/transition commands still need original FIELD/version intent, independent resource/write rights, concurrency, durable uncertain-outcome recovery and recorded read-back. A cleared dialog does not prove an in-flight create cannot adopt a changed FIELD. Planner/materials/approved permit/execution evidence/supervisor return-to-service/cost handoff remain open through existing workflow/service owners.

No SQL, schema change, business EF lane, role grants, invented orders/counters or phase/goal closure. Accountant finance/provider gates and all other persona plans remain in scope.
