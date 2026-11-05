# Implementation 149: AP receipt allocation review screen

## Accountant source-to-receipt review

The existing AP invoice detail now offers Review recorded receipt allocations after the exact invoice line's owned PO source has been reviewed. It requires both Accounting.View and Procurement.View under a current selected-company app role. The same source panel shows recorded allocation identity, exact receipt/order-line IDs, allocated quantity/unit, activity and recorded/changed provenance. A confirmed empty list is distinct from unavailable evidence and does not establish matching completion.

The existing PO receipt history remains visible alongside the allocation history. Its caption identifies all-order history separately from the reviewed invoice-line allocations. Different recorded units remain raw; no quantity comparison, conversion, aggregation, receipt acceptance, effective allocation, global consumption limit, match tolerance, approval, stock, posting or payment is inferred. No allocation-writing action is exposed.

## Canonical API and client path

APInvoiceReadChecks.ReadReceiptAllocationsAsync extends the existing read orchestration. It pins original company and PRODUCTION binding, standard roles and both permissions across the entire read. Each capture uses the existing owned AP/PO source reader and requires the exact displayed invoice and order revisions. Allocation history is then read through APInvoiceService.GetReceiptAllocationsForLineAsync with authority checks before and after. Two complete captures must have the same allocation/source content revision before publication. This observes changes but is not an atomic provider snapshot.

The existing AP controller adds GET invoice-line receipt-allocations with the two expected revisions. It supplies fresh current-company access, actor verification and binding plus the actual AP/PO service readers; malformed revisions refuse, denied authority returns Forbid and unavailable/changed source returns fixed Conflict. The existing typed accounting client encodes original IDs/revisions, captures the original selection and validates the Data allocation review plus exact order revision. No alternate HTTP client, store or source model is added.

## Existing state and UI behavior

APInvoiceWorkspaceState retains a readonly frozen allocation projection, revision and UTC retrieval provenance. It requires the current reviewed invoice/line and owned PO revision. AP/company/source changes clear allocation evidence, and separate allocation generations reject overlapping or late responses. A failed allocation read clears prior history while preserving current invoice/order evidence. New purchase-source review clears allocations. Public clear operations let the parent clear evidence before fresh authority evaluation.

PayablesWorkbench checks current company roles and both permissions again for the action, passes the displayed revisions and provides separate progress/failure feedback. Denied authority clears the nested purchase/allocation evidence and disables its procurement eligibility. The actual panel hides the action, allocation history and provenance when procurement eligibility is false, including when a state still contains previously reviewed rows. No custom CSS/JS is introduced.

## Focused evidence

The final affected actual-source gate passes **301 tests**, zero failed/skipped, terminal exit 0, in 1.5056 seconds. Log: `%TEMP%/oilgas-allocation-review-149.log`. It includes the previous 258 cases plus 43 new checks; overlapping totals are not summed. The existing 175 dependency/source-precedence warnings remain. Data sources are unchanged from the previous clean complete-source build (zero warnings/errors, 1.55 seconds).

Thirty-two API helper checks cover original source arguments and repeated authority/captures; twenty-nine revocations of role, accounting/procurement permission, company or binding at relevant allocation boundaries; and changed expected order/closing allocation evidence. Seven state cases cover exact displayed revisions/frozen history, three late-context/source changes, failed/changed-order reload clearing and overlapping reads. Four actual Razor render cases cover permission eligibility with nonempty and confirmed empty allocation history, exact IDs/units/provenance and explicit unresolved consumption/matching claims.

The gate compiles the actual API helper, current AP/PO services, Data reviews, repository/interface/transaction helpers, states and source panels. Full controller, typed client and parent page, production current-company/binding callbacks, auth/DI/HTTP/OIDC, browser/keyboard/RTL and native provider consistency remain source-only or unverified. This is not proof of a complete accountant journey. No allocation is written, migration executed, role granted, SQL introduced or business EF path added.

## Remaining delivery

Review/apply/reconcile the new allocation Beep schema and verify actual endpoint/client/parent HTTP and browser/provider behavior. Establish independently configured allocation preparer/reviewer duties, unit/currency/catalog and acceptance rules, required matching cardinality/tolerances, globally concurrent receipt consumption, correction/history and durable command receipts/readback. Complete invoice approval/posting/payment and buyer/receiver/stock handoffs, plus every other company/persona/multinational requirement. No feature, persona or phase closes.
