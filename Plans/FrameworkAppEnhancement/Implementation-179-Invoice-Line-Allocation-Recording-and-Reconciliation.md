# Implementation 179 — Invoice-line allocation recording and reconciliation

Date: 2026-10-07. Status: actual invoice-line screen, shared circuit state, typed API submission/reconciliation and refreshed source evidence are implemented and focused verified. Financial approval, receiver acceptance and the broader purchase-to-payment/persona plans remain open.

## Existing screen and circuit owner

PayablesWorkbench extends its current invoice/PO/receipt proposal workflow with an explicit bounded reason, recording action, progress, confirmation and original-intent reconciliation. The existing APInvoiceWorkspaceState is extended as a partial class and registered once per circuit. The page consumes this owner and clears visible source evidence on disposal rather than destroying its retained unresolved command. Page recreation therefore preserves recovery without another store or client model.

The screen independently checks current standard ASP.NET company roles and Accounting.View, Procurement.View and Accounting.RecordReceiptAllocation before submission/reconciliation. The API remains independently authoritative. Reason validation distinguishes an action that could not start from a submitted outcome that cannot be confirmed. Mud components provide labels, disabled/loading/error/status states; no page CSS, JavaScript or theme palette is introduced.

## Original intent and unknown outcome

The state freezes the full bound proposal review and creates one original command before transport. It exposes copied command, receipt and contributor evidence. Busy and unresolved intent prevent another command or proposal review. An explicit retry preserves the original reason, quantity, receipt, revisions, company/account/binding and GUID rather than replacing them with current form input.

Company/account/source changes and page disposal invalidate visible success and source frames while retaining unresolved intent. Late responses cannot be adopted. Returning to the original company/account/invoice enables reconciliation; the current company/account can open the retained original invoice from the queue. The screen subscribes to state completion so a recreated page sees when an older in-flight operation finishes. Retention is circuit-local; browser/new-circuit and historical durable recovery remain open.

## Confirmed read-back before success

After a valid API receipt, the state stages refreshes through the existing invoice, purchase, allocation and whole-invoice owners. It requires unchanged original invoice/order revisions and the exact stored row in the owned invoice-line allocation set. It separately reloads complete receipt contributors, verifies that same row and explicit recorded active quantity, and requires the financial review's selected allocation revision to agree. No partial refresh is published as success.

Only then does the screen show the recorded allocation, actor/time, refreshed contributors, contributing invoice links and whole-invoice diagnostics. Failure or drift preserves the original unresolved intent and clears stale proposal/allocation/financial evidence. Recorded acceptance, matching policy, book/period/journal evidence and independent financial decisions remain unavailable where not implemented; allocation recording cannot imply approval, posting or payment.

## Receipt-consumption read

The existing AP controller and complete AccountingServiceClient add a read-only invoice-line receipt-consumption endpoint. It reuses canonical AP/PO source reads and APInvoiceService.GetReceiptConsumptionAsync, checks original company/account/binding and unchanged role/permission sets, captures contributors twice and refuses changed content. Accounting.View and Procurement.View suffice; preparation or recording grants are not required for this read. This lets recording/reconciliation refresh evidence without silently making the read-only preparation grant mandatory.

## Focused verification

Actual Data source build: zero warnings/errors, 1.29 seconds. Log: %TEMP%/oilgas-allocation-179-data.log.

Scripts/Test-PurchaseOrderOwnership.ps1 with actual Data and -IncludeWorkflowInbox: **893 passed, zero failed/skipped, terminal exit 0, 23.7888 seconds**. Log: %TEMP%/oilgas-allocation-179-final.log. Twenty new cases extend the prior 873-case gate; counts overlap prior slices and are not added.

New cases cover actual API/state line allocation, all receipt contributors and financial read-back; copied snapshots; uncertain commit/original reason/retry; busy duplicate refusal; late company/account/source changes; missing line/contributor/financial read-back; invalid reason without intent or transaction; complete parent-page recording, permission denial, reconciliation and page recreation; and read-only consumption access, role/permission/binding/role-set/revision/receipt refusals. Native writers execute in the actual HTTP/client/page chain with explicit provider fixtures.

The smaller source composition also passes **517 checks**, zero failed/skipped, exit 0, 6.5461 seconds. Log: %TEMP%/oilgas-allocation-179-source.log. Its previously omitted ProcessStepRoleChecks dependency is now included in both compositions. These counts overlap the larger gate.

The complete composition retains 330 warnings, including two additional current-source precedence warnings from consumption-read refusal references. No new changed-source nullable or other compiler warning is reported. Actual source and parent handlers compile and execute; the Program scoped registration is present in source and the focused page fixtures exercise scoped state reuse. Current account/module resolution, physical provider metadata/isolation/durability and static JS/auth boundaries remain explicit seams. Real host/DI/OIDC, browser/keyboard/RTL/accessibility and provider concurrency are not claimed. Whitespace checks pass.

## Next delivery and remaining roadmap

Implement authoritative receiver accept/reject quantity and decision evidence through the existing procurement receipt owner and screen, with company/role separation, original intent, Beep transaction/read-back and receiver-to-accountant handoff. Keep physical received quantity, accepted quantity, invoice allocation, stock movement and financial approval distinct. Expose verified acceptance to the existing whole-invoice evidence review rather than deriving it from a receipt status or workflow completion.

Company matching/tax/rounding policy, book/period/journal linkage, independent financial decision/post/payment, stock/valuation, durable new-circuit recovery/outbox, configured routing/delegation, broader dashboards and other personas, multinational requirements and live host/browser/provider checks remain active. Beep MigrationManager remains the business schema owner. This slice adds no business schema, SQL, EF migration, live migration/grant, commit, push or deployment. No feature or phase closes.
