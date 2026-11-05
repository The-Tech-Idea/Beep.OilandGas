# Implementation 156: Invoice, PO and receipt source comparisons

## Accountant preparation screen

The existing receipt-allocation proposal review now displays an Invoice / PO / receipt source comparisons table. Each row gives a named comparison, recorded result and the exact source values/basis. The eight rows cover units, currencies, unit prices, invoice quantity, receipt quantity, receipt acceptance, company tolerances and independent reviewer handoff.

Recorded agreement and within-recorded-limit results are descriptive source comparisons, not matching approval or submission readiness. Recorded differences and exceeded limits remain visible rather than becoming automatic acceptance. Missing evidence is explicitly Unavailable. No default conversion, exchange rate, tolerance, acceptance decision or reviewer assignment is substituted.

The table is inside the existing preparation permission and procurement source gates and appears only with the reviewed proposal. It uses MudBlazor and no additional CSS/JS. The current frozen proposal projection retains immutable comparison rows; proposal input changes, source reloads, failed reviews and company changes clear it through existing state guards.

## Canonical Data calculations

APReceiptAllocationComparisons evaluates the existing Data-owned proposal/source/contributor review after validating original company/actor, exact proposal and source hashes. The proposal response exposes derived comparisons; the client/state recompute them from validated source evidence rather than trusting an independent server-supplied result. No persistent comparison model, alternate store or extra API endpoint is introduced.

Unit agreement requires the exact invoice-line, PO-line, receipt and proposal unit codes. Currency agreement requires explicit matching invoice-header, invoice-line and PO-header codes. PO-line currency derives from its recorded header because the current PO line has no separate currency field. Unit prices compare only when units and currencies agree and both recorded prices are nonnegative; zero remains an explicit valid price. No price tolerance is assumed.

The invoice quantity comparison adds the positive proposal to recorded active allocations on that invoice line only when activity and units are confirmed. The receipt quantity comparison uses the reviewed cross-invoice active total. Both compare against positive recorded source quantities and report exceeded limits rather than implying available balances. Quantity arithmetic that exceeds decimal range returns an unavailable result; limit checks avoid overflow. Inactive history remains visible and unconfirmed activity/unit evidence prevents totals.

Receipt acceptance is unavailable even if the raw receipt status says RECEIVED. The accepted quantity/decision has not been established. Company tolerances and the durable independent-review handoff also remain unavailable. Exact recorded agreement never supplies these missing decisions.

## Workflow owner findings

The existing assigned-task inbox requires a FIELD-bound process and entitled FIELD/entity access. The company-wide AP records currently have no authoritative FIELD association. Attaching them to the selected FIELD would fabricate scope; a company accounting process/source access boundary is needed before creating that handoff in the existing process/task owners.

The existing OperationalAccountingWorkflowSeed defines ACCT_PROCURE_TO_PAY on PURCHASE_ORDER and describes a SYSTEM matching step with fixed quantity/price percentages. These legacy seed defaults do not constitute reviewed company policy and are not used by this comparison table. Review and replace that financial step's matching/independent reviewer contract, then connect actual durable submission/task routing and transaction recovery. No workflow seed, active definition or assignment was modified in this slice.

## Focused evidence

Current complete Data sources build with zero warnings/errors in 1.55 seconds. The final actual-source gate passes **437 tests**, zero failed/skipped, terminal exit 0, in 4.7890 seconds. Log: `%TEMP%/oilgas-source-comparisons-156.log`. The 175 existing dependency/source-precedence warnings remain; whitespace verification passes.

Eighteen new Data cases extend the previous 419-case gate: exact and different units/currencies/prices, valid zero price, suppressed price comparison without compatible units/currencies, quantity limits, missing invoice quantity, unknown activity, different active allocation units, missing contributor evidence, numeric overflow, JSON provenance and refusal of mutated source content. Existing complete-controller proposal HTTP coverage now asserts comparison results, and both actual contributor Razor/state cases assert comparison labels, explicit unresolved acceptance/reviewer/tolerances and frozen state. The gate retains 49 complete-controller loopback HTTP cases; totals across runs overlap and are not summed.

Actual Data, controllers, native services/repository, state and shared panel compile/run in the affected gate. Complete typed client/parent/production role bridge/context adapter/host, browser/RTL/accessibility, actual Beep migration/provider and performance acceptance remain unverified. No SQL, business EF, migration application, persisted allocation/task, grant or financial write is added.

## Next independent handoff

Establish authoritative receipt acceptance and reviewed company matching policy, update the existing accounting process/source boundary to company scope, and add durable allocation draft/submission and independent reviewer assignment through existing process/task/Beep command owners. Provider-backed concurrent consumption, ownership reconciliation, recorded/not-recorded/uncertain outcomes, matching/approval/post/payment and buyer/receiver/stock outcomes remain open. This source comparison screen advances preparation but closes no persona journey or roadmap phase.
