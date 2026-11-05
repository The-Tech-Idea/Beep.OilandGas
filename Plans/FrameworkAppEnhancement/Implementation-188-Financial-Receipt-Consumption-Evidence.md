# Implementation 188 - Complete receipt consumption in invoice financial review

Date: 2026-10-07. Status: implemented and focused-verified prerequisite for accountant variance proposal and independent controller decision. No persona phase is closed.

## Persona outcome

The existing whole-invoice financial review now includes complete receipt contributors across owning invoices. Accountants and controllers see competing allocations, including inactive history, beside verified receiver acceptance and the existing policy/variance evidence. Contributor buttons open the existing `/accounting/payables/invoices/{Id}` screen; the owned purchase-order links remain available. No matching approval, stock balance, reservation, posting or payment is inferred.

## Canonical evidence and API ownership

APInvoiceFinancialReview in TheTechIdea.Data now owns a frozen ReceiptConsumption collection of the existing APReceiptConsumptionReview contract. Every allocated receipt on every recorded purchase line must appear exactly once per invoice-line/receipt pair. The assembly verifies that the current invoice's allocations appear unchanged in the complete contributor history, and a receipt shared across multiple invoice lines has identical global contributor history. Full contributor revisions are included in the financial revision. Missing, duplicated, changed or inconsistent evidence prevents adoption. Legacy source-only diagnostic constructors explicitly report unavailable global evidence; the production API always supplies the required native reader.

APInvoiceReadChecks calls the existing APInvoiceService.GetReceiptConsumptionAsync for every relevant allocated receipt, including inactive allocation history. That owner queries the full company-owned receipt contributor set, resolves each owning invoice and validates exact order/line/receipt membership. Each capture retains current company/account/binding/app-role/permission guards. The existing double financial capture compares the extended revision and refuses contributor changes during loading. No new endpoint, parallel repository, business entity or authorization authority is introduced.

The global diagnostic sums active allocations in exact recorded receipt units and compares them with verified accepted quantity from receiver decision proof. Unknown activity, incompatible units, missing or altered receiver proof and overflow produce unavailable evidence. Excessive competing consumption is visible even when this invoice's individual allocation is within acceptance. The separate individual-invoice acceptance check retains its narrower meaning.

## Screen delivery

APInvoiceSourcePanel shows a dedicated complete receipt consumption diagnostic table, per-receipt contributor/source/allocation revisions and UTC retrieval time, contributor invoice links, and every allocation's quantity, unit, activity and creation/change audit fields. This reuses the existing financial review/client/workspace owners and MudBlazor components. Source, policy and global tables filter canonical diagnostic collections without duplicating business models. No custom CSS/JS or client database access is added.

## Focused verification

Actual Data build: zero warnings/errors, 1.78 seconds; log `%TEMP%/oilgas-receipts-188-data.log`.

Final affected gate: **1158 passed, zero failed/skipped, terminal exit 0**, 39.3696 seconds; log `%TEMP%/oilgas-receipts-188-final.log`. Smaller source gate: **519 passed, zero failed/skipped, terminal exit 0**, 8.3011 seconds; log `%TEMP%/oilgas-receipts-188-source.log`. Counts overlap. Composition retains 350 warnings. Whitespace checks pass.

Adds 14 cases to the previous 1144: native global contributor comparisons, excessive consumption, inactive history, unknown activity/unit/receiver proof, incomplete/duplicate/omitted-own histories, contributor revision changes and snapshot tampering, shared receipts across own invoice lines, changed own allocation refusal, and actual API/typed-client stable and changed-global-capture checks. Existing complete-page tests now seed competing receipt consumption and assert rendered contributor links, allocation evidence, accepted/global quantities and excessive-consumption status. Read tests issue no allocation or invoice writes; existing page fixtures retain exactly two policy lifecycle writes and invoice DRAFT status.

The first build exposed namespace qualification in the new Data helper and was repaired with global namespace qualification. The initial affected composition exposed missing test client imports and was repaired before execution. The first successful gate passed 1156 checks; final 1158 adds shared-own-line/global-history and changed-own-allocation cases and verifies the corrected contributor route. Physical datasource/provider metadata/durability, host/OIDC/DI, concurrency and JS/browser remain test seams or unverified production acceptance; no broad missing-project build is claimed.

## Remaining persona and company roadmap

Next: accountant variance review/proposal and independent controller financial decision through existing invoice/source/task owners, bound to exact invoice/purchase/allocation/global-receipt/policy revisions and current app-role/grant/binding. Resolve separate tax/non-PO and book/period/journal evidence before financial posting/payment. Policy-specific tasks, retirement/correction, lifetime/new-circuit recovery, stock/returns, wider persona dashboards, multinational requirements and live release gates remain open.

Beep MigrationManager remains the business schema owner. This slice adds no business schema, handwritten SQL, business EF lane, live migration/grant, commit, push or deployment.
