# Implementation 140: Company-owned payables source workbench

## Accountant's vendor invoice journey

The existing accounting landing now links eligible company users to `/accounting/payables`. The supplier source workbench lists actual active company-owned vendor invoices, with encoded links to `/accounting/payables/invoices/{Id}`. The same page embeds the existing assigned-work queue when the current configured persona maps to that root. Direct detail shows actual recorded header amount, paid amount, balance, currency, status, activity and creation/change provenance. No missing amount is substituted with zero, and a recorded zero remains zero.

Empty supplier results do not establish no historical bills or no liability. Recorded header balances/status do not verify actual payment, posting, independent approval or matching. Invoice lines, exact PO/receipt linkage and currency catalog validity remain unconfirmed. No conversion, aggregation, approval, invoice creation or payment action is offered. Supplier identity is not owning-company identity and cannot link a vendor bill to an order.

The state freezes UI projections, binds original company/actor/invoice/content and clears failed or invalid reloads. Company switches clear the supplier and return a detail to the root instead of adopting a colliding local ID. Away-and-back responses cannot repopulate earlier evidence. The parent follows the existing workbench lifecycle pattern and uses the current typed accounting client, standard ASP.NET roles and MudBlazor; no client database, custom CSS/JS or alternate task owner is added.

## Canonical domain owner and service

AP_INVOICE moves from Models to TheTechIdea.Data under its existing namespace, with no second domain definition. It declares its existing AP_INVOICE_ID key and nullable bounded OWNER_TENANT_ID (64), OWNER_COMPANY_BA_ID (128) and CURRENCY_CODE (32). Existing legacy rows are not assigned to the supplier or selected company. The explicit PRODUCTION Beep manifest already includes this type. Migration planning, capabilities, physical schema/key verification and reviewed legacy reconciliation remain required; no migration or assignment was executed.

Shared APInvoiceOwnership validates exact tenant/company/header identity, active supplier results, duplicate/null rows and bounded recorded currency. APInvoiceSourceReview freezes the actual header and hashes explicit business/audit fields, excluding tracking internals/retrieval time. It binds the original company, actor, identity and actual UTC capture time and survives Web JSON. The UI and typed consumer reuse this single contract.

The existing asynchronous APInvoiceService now requires CompanyScope on its business operations and records ownership on its internal create path. Header reads validate ownership; supplier reads filter both owner columns and validate returned rows. Shared default PPDM39 fallback is removed. Fresh repository metadata requires a single nonnullable AP identity and owner/currency columns. Missing schema must refuse rather than silently return no bills. The source-review read is added to this same owner, not a separate AP store.

Currency-less/mixed-currency aging aggregation is explicitly unavailable until reviewed currency-bound contributing records exist. Existing internal receive/payment commands are not certified for independent duties, account mapping, atomic posting, retry recovery or provider consistency and are not exposed by this slice. Their CompanyScope change does not make their financial lifecycle complete.

## Existing controller and typed client

The existing APController previously called the overlapping traditional facade and could create a bill and post ledger entries without the reviewed company-owned command. It now requires authentication, current selected-company roles recognized through standard User.IsInRole and independent Accounting.View permission. Procurement.View alone is insufficient. IdentityServer remains authentication-only; no new role/grant or administrator override is added.

GET detail uses the canonical asynchronous AP owner, captures source evidence twice and rechecks fresh company authority and PRODUCTION binding. Supplier GET is added to the same controller. The existing AccountingServiceClient/interface consume these routes and validate original company snapshots and shared source ownership/revision. The detail response now uses the Data source-review contract; external client shape/transport acceptance remains unverified.

HTTP creation is fenced with an explicit conflict before any invoice/posting operation. The old AccountsPayableCompatibility read/create implementations also explicitly refuse; their company-less repository fallback and create-and-post routing are removed. This is retirement of an overlapping owner, not a second read/write lane. Other legacy/generic accounting surfaces require broader reconciliation.

## Focused verification and limits

Actual current Data sources build with zero warnings/errors (1.98 seconds). The existing `Scripts/Test-PurchaseOrderOwnership.ps1` gate now includes payables source contracts/state/panel/read-authority checks. Final result: **111 passed**, zero failed/skipped, terminal exit 0 (1.9047 seconds); `%TEMP%/oilgas-payables-sources-140.log`.

The 81 prior procurement cases are included, not additional unique verification. Thirty new AP cases cover canonical Data ownership/actual Beep field/key metadata; five foreign/unassigned ownership refusals; four supplier list failures; frozen Web JSON review; five content changes; four context/provenance failures; independent standard role/Accounting.View checks; frozen nullable/zero detail projection; three invalid reloads; company away/back rejection; and four actual MudBlazor detail/list/empty/unconfirmed renders with encoded identity/source text.

The gate compiles current Data, AP read-authority helper, actual AP UI state/panel and the previously scoped procurement sources. It does **not** compile or execute the complete APInvoiceService, APController, typed client, parent workbench/accounting landing, retired facade or host. Those changes remain source-only. The existing ledger binding test's AP read signature was updated but that broader suite was not rerun. Native AP metadata refusal/successful query, full HTTP/OIDC, browser events/keyboard/RTL, migration/reconciliation and financial transactions remain unverified. Two captures are not an atomic database snapshot. No phase/persona is certified.

## Next handoff

Compile and focus-test the complete canonical AP service and controller/client transport using real dependencies. Verify parent/browser/company-switch flows and native schema acceptance. Establish authoritative supplier/currency references and Data-owned invoice lines with exact PO line/receipt relationships; supplier equality alone is never matching evidence. Then implement configured AP/requester/buyer/receiver/reviewer duties, approved invoice creation, matching variances, independent approval/posting/payment, recorded readback and durable command recovery. Complete the remaining persona and multinational roadmap. No SQL, business EF lane, live writes, role grants or deployment occurred.
