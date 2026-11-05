# Implementation 187 — Effective policy and invoice variance evidence

Date: 2026-10-07. Persona priority: accountant invoice review and controller financial evidence, supporting Feature 04/Persona 01. Verified effective policy now participates in the existing complete invoice financial review/API/client/screen. Independent invoice variance decisions, financial commands and task handoff remain open. No phase closes.

## Existing complete financial review owner

APInvoiceFinancialReview extends its existing contract with the complete CompanyMatchingPolicyReview snapshot. The financial content revision includes its history revision, and Require independently validates exact company/account and complete policy content. Missing policy in a source-only Data diagnostic remains explicitly unavailable; the actual API always requires the native policy history owner. No global or latest-policy fallback is introduced.

PolicySelection requires an active recorded invoice, explicit currency and full UTC invoice date. It filters complete history by active APPROVED status, exact currency and inclusive recorded effective/expiry window, requiring exactly one candidate. That candidate must have complete verified independent approval proof. Missing date/UTC, expired/future policy, wrong currency, inactive/draft/unverified/changed policy or ambiguous candidates cannot select rules. This selects diagnostic rules; it does not approve an invoice or prove statutory financial treatment.

## Explicit quantity and price variance

The existing canonical APSourceComparison owner adds separate policy-price and policy-quantity diagnostics. They compare invoice unit price/quantity with the full recorded PO line basis under the verified percentage limits. Active PO header/line, exact units and currencies, nonnegative price and positive quantity/basis are required. No unit/currency conversion is inferred. Quantity evidence explicitly names the full PO-line basis rather than claiming remaining balance, receipt availability or globally consumed capacity.

The testable criterion is absolute invoice-versus-PO difference times 100 less than or equal to PO basis times the recorded tolerance percentage. Decimal coefficients/scales are compared as BigInteger values, preserving exact thresholds without decimal product overflow, underflow or rounding. Zero PO basis and zero invoice quantity are unavailable rather than defaulting a ratio.

Existing exact header/line/price/allocation and receiving diagnostics remain independent. The supported policy explicitly requires separate tax/non-PO review and exact source amounts. Tax/non-PO financial authorization stays unavailable; a confirmed source-amount rule does not replace the arithmetic checks or establish posting/payment. Ledger and independent invoice financial decision remain unavailable.

## API, typed client and accountant screen

APInvoiceReadChecks requires the existing AccountingPolicyService read callback when capturing financial evidence. Native complete policy history joins each invoice/purchase/allocation capture under fresh company/account/binding/role/permission guards. A changed policy history changes the financial revision and prevents publication across the double capture. The existing AP endpoint consumes its registered policy owner; the existing typed client/workspace validate and freeze the extended Data contract automatically.

APInvoiceSourcePanel shows verified policy selection/evidence and a policy/variance table alongside the existing exact source-diagnostic table. Both views filter the same canonical Checks collection; no duplicate model or alternate endpoint exists. This makes policy evidence visible immediately rather than hiding it behind the source table's pagination. MudBlazor/theme conventions are reused; no custom CSS/JS or frontend database access is added.

## Focused verification

Actual Data build: zero warnings/errors, 1.72 seconds; log %TEMP%/oilgas-policy-187-data.log.

Final affected gate: 1144 passed, zero failed/skipped, terminal exit 0, 40.9174 seconds; log %TEMP%/oilgas-policy-187-final.log. Adds 32 cases to the previous 1112 (24 Data/native-policy cases and 8 actual HTTP/complete-page cases). Smaller source gate: 519 passed, zero failed/skipped, terminal exit 0, 9.0968 seconds; log %TEMP%/oilgas-policy-187-source.log. Counts overlap. The composition retains 350 warnings, with no new policy financial-source warning. Whitespace checks pass.

New checks exercise native preparation/independent approval before effective selection, unknown/invalid/ambiguous history and dates/currency/proof, exact inclusive/two-sided thresholds, tiny/maximum decimal values, zero basis/quantity, unit/currency/activity refusal and mutated snapshot rejection. Actual AP API/client checks cover fresh complete policy capture, changed policy during capture, unverified approval status, access/binding/schema refusal. Complete invoice-page handlers render applicable policy/variance evidence and clear it after failed reload. Native fixture writes remain exactly the two policy lifecycle commands; invoice status stays DRAFT and diagnostics issue no invoice financial command.

Initial gate passed 1111 with one rendered-page label assertion failing; updating the expected label still exposed that the new policy rows were beyond the first source-table page. Data-expanded checkpoint passed 1134 with that same rendered-page failure. The canonical policy rows now have a visible dedicated table; the native HTTP/page checkpoint passed 1143. Final 1144 includes the zero-invoice-quantity case. Default HTTP fixtures provide an explicit empty native policy history, not a fabricated approved policy. Physical datasource/metadata/isolation/durability, authentication and JS/browser remain seams; full production host/DI/OIDC/browser/provider/concurrency are unverified.

## Next persona delivery

Implement accountant variance review/proposal and independent controller decision through existing invoice/source/task owners, bound to the exact invoice/purchase/allocation/global-receipt/policy evidence and current app-role/grant/binding. Keep positive diagnostics separate from a durable invoice matching/approval decision. Resolve separate tax/non-PO evidence and book/period/journal ownership before granting financial posting/payment commands.

Policy-specific tasks, retirement/correction, lifetime/new-circuit recovery, stock/returns, wider persona dashboards, multinational requirements and live release gates remain active. Beep MigrationManager remains the business schema owner. This slice adds no business schema, SQL, business EF lane, live migration/grant, commit, push or deployment.
