# Implementation 184 — Company matching-policy workspace

Date: 2026-10-07. Persona priority: accountant/controller company landing and invoice preparation; Feature 04 and Persona 01. The native read prerequisite from Implementation 183 now has API, typed client, workspace and actual Mud page integration. Independent policy approval, effective policy selection and variance/financial decision commands remain open. No feature or phase closes.

## API and existing owner

The existing APController exposes GET api/accounting/traditional/ap/matching-policies. It uses AccountingPolicyService, not a separate store or policy model. Current app-owned company role and Accounting.View permission are required through the existing standard ASP.NET role/read checks. Original company/account/PRODUCTION binding and full role/permission sets are checked before and after both native reads. The complete policy history revision must remain unchanged. Lost authority returns forbidden; missing, invalid or changed evidence returns a fixed conflict response.

ApiService registers the existing policy owner with the existing ModuleConnectionResolver for PRODUCTION. This is source integration; focused tests register the native owner explicitly rather than proving full production host/DI. IdentityServer remains authentication-only. Business schema remains owned by Beep MigrationManager; no SQL or business EF is added.

## Typed client and workspace

The existing IAccountingServiceClient and AccountingServiceClient consume the same Data review. The shared ApiClient company boundary checks each request; the policy method validates original company/account and complete response evidence. Foreign actor/company, altered content, null response and changed company cannot be adopted. The existing company boundary cancels a stale response rather than turning it into a valid policy history.

APInvoiceWorkspaceState owns frozen policy review. Reload clears prior visible evidence before awaiting. Company/source clear, disposal and competing loads invalidate late results; exposed reviews are copies. No policy store, approval state or compatibility path is duplicated.

## Accountant/controller screens

The existing PayablesWorkbench and APInvoiceSourcePanel provide Review company matching policies from both the company queue landing and invoice detail. A fresh authenticated standard-role/Accounting.View check precedes the handler. Loading disables the policy action and shows labelled progress; unavailable, empty and confirmed history are distinct states. Failed evidence clears previous policy display; denied authority clears owned workspace evidence. Existing handlers refuse concurrent policy activity.

History shows all active/inactive records with identity/version, recorded status, effective/expiry dates, currency, explicit quantity/price percentage tolerances, unit/currency/tax/rounding/non-PO rules and recorded/changed provenance, plus retrieval time and complete content revision. MudBlazor/theme conventions are reused; no CSS or JavaScript is added.

Recorded APPROVED status still cannot prove independent approval. No latest/effective policy is chosen, no tolerance is silently applied to invoice comparisons, and no allocation, receiving or generic workflow approval becomes financial authorization. Existing unavailable financial diagnostics remain explicit until the independent policy/decision workflow is delivered.

## Focused verification

Actual Data build: zero warnings/errors, 1.27 seconds; log %TEMP%/oilgas-policy-184-data.log.

Final affected gate: 1039 passed, zero failed/skipped, terminal exit 0, 36.1721 seconds; log %TEMP%/oilgas-policy-184-complete.log. This adds 25 cases to the previous 1014. The composition has 342 existing warnings and no new policy-screen compiler warning. Smaller source checkpoint: 519 passed, zero failed/skipped, terminal exit 0, 8.2296 seconds; log %TEMP%/oilgas-policy-184-source.log. Counts overlap; the final affected gate covers the final landing-page placement and all source checks.

New cases exercise actual native policy owner, AP controller, complete typed client and rendered complete payables page/handler on landing and detail; explicit empty/invalid evidence; standard role/permission/binding/revocation fences; changed/foreign/schema evidence; frozen state, failed reload and late competing/company responses; and typed client null/foreign/account/content/stale-company refusal. Physical datasource/metadata, authentication and browser/JS are test seams. The policy landing-page test supplies an unselected mocked persona context for the adjacent inbox and the actual inbox client; it does not prove a new persona task journey.

Initial run passed 1032 and failed one screen assertion expecting different text from the shared error formatter. The assertion now verifies the actual error and cleared state; a checkpoint passed 1033. Added stale-company tests initially expected InvalidOperationException where the shared company boundary correctly throws OperationCanceledException; that expectation was corrected. Landing verification exposed missing adjacent persona/inbox services in the render harness; they were supplied. The final full affected run passes all 1039. Whitespace checks pass.

## Next delivery and remaining scope

Implement independent accountant/controller matching-policy creation/review/approval with explicit company/account/binding and original content, then evidence-bound variance review/financial decisions through the existing source/task owners. Keep policy selection, effective dating, currency/unit/tax/rounding/non-PO rules and independent authority explicit.

Corrections/returns, stock, book/period/journal linkage, posting/payment, durable recovery/outbox, receipt-specific workflow policy, routing/delegation, broader persona dashboards and multinational requirements remain active. Full host/DI/OIDC/browser/keyboard/RTL/accessibility, live Beep migration and provider/concurrency acceptance remain unverified. No live migration/grant, commit, push or deployment occurred.
