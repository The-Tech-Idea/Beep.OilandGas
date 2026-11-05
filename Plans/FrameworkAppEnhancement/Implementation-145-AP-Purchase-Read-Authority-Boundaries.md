# Implementation 145: AP purchase read authority boundaries

## Accountant source review

The existing AP invoice-line purchase-source endpoint now delegates its complete two-capture orchestration to APInvoiceReadChecks.ReadPurchaseSourceAsync. This extends the existing API read-check owner rather than adding another AP or procurement service. The endpoint supplies actual current company access, actor verification and the PRODUCTION binding, and calls the existing APInvoiceService and PurchaseOrderService source readers.

The helper requires standard authenticated app roles, Accounting.View and Procurement.View under the original company and binding. It checks authority initially, before each invoice read, before each PO read and before returning the closing review. Revocation after the first order capture therefore stops the second invoice query; a later revocation stops the next order query or final publication. Missing initial binding refuses. The endpoint maps authority refusal to Forbid and unavailable/changed source evidence to its existing fixed Conflict response.

Both invoice captures must match the exact reviewed invoice revision. Each recorded invoice line must resolve through the existing Data-owned cross-source proof to the exact owned order and line for the same supplier. Initial and closing order revisions must agree. No receipt allocation, quantity/currency conversion, matching approval or financial command is inferred. These successive reads do not establish an atomic snapshot.

## Focused verification

The affected actual-source gate passes **211 tests**, zero failed/skipped, terminal exit 0, in 1.3979 seconds. Log: `%TEMP%/oilgas-ap-purchase-read-145.log`. It includes the previous 174 cases plus 37 new cases; overlapping totals are not summed. The existing 175 dependency/source-precedence warnings remain.

New tests compile and execute the exact helper now called by the endpoint. One successful capture checks original company, actor, invoice/order identities, six authority checks and two reads per source. Twenty-nine cases revoke the current role, either permission, company or binding at applicable boundaries and assert exact provider call counts. A company switch is tested after the original scope is captured; the initial authorized scope is the baseline. Six cases refuse missing invoices/orders, changed initial/closing invoices, changed closing orders and a foreign supplier. One invalid reviewed revision refuses before authority or provider work.

The full controller, production current-company/binding callbacks, complete typed client and parent page remain source-only. These tests use explicit authority/source callbacks and do not prove live HTTP authorization, the role bridge, middleware/DI, provider transactions or browser behavior. The existing actual AP/PO services, repository/interface/transaction helpers, Data reviews, UI states and source panels remain included in the affected gate. No schema, SQL, business EF migration, live migration or permission grant is introduced.

## Remaining persona delivery

Verify the full endpoint/client/parent path and live app-role revocation through HTTP and browser/provider gates. Reconcile and apply the AP-line Beep schema through reviewed MigrationManager. Establish canonical invoice-line receipt allocations and the required matching cardinality, discrepancy review and units/currency evidence. Continue buyer/receiver/accountant/reviewer commands, independent approval/posting/payment, recorded outcomes and durable recovery. All other persona/company/multinational requirements remain open; no phase closes.
