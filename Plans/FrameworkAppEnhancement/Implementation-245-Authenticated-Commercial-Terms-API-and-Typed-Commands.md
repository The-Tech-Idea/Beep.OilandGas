# Implementation 245 — Authenticated commercial terms API and typed commands

## Commercial persona workflow now available through the API

The existing CompanyFieldRevenueReads owner now connects the Implementation 244 native command methods to authenticated commercial preparation/decision endpoints. Existing recorded source and sales-contract reviews remain the source entry point. This slice delivers actual API/client/native prepare, independent approve/reject and recorded outcome read-back. Commercial preparation/reviewer forms, retained Web state and assigned workflow tasks are still to be implemented; the existing page does not yet expose these writes.

The additive routes under `/api/fields/{fieldId}/accounting/commercial-terms` are GET for exact transaction/date-window review, POST prepare for the canonical frozen preparation and POST decide for the canonical original preparation plus independent decision. FIELD identity is taken from the exact once-decoded request route and must match the frozen source. No caller-supplied database connection is accepted. Existing DI registration for CompanyFieldRevenueReads supplies the new partial implementation without a parallel service owner.

Canonical Data owns SalesContractTermsReadReview and SalesContractTermsDecisionSubmission. Read review holds the current complete source/contract graph and optional native recorded preparation/decision. A null result is confirmed only after the same native evidence-column schema preflight used by the writer. If recorded evidence exists, review validates the exact native envelope, company, selected transaction, source window, original preparation, complete source and recorded outcome. Missing provisioning is an error, not an empty preparation. Review of a saved preparation currently requires its exact captured transaction/window; it does not claim amendment or cross-window discovery.

## Independent authority and source checks

The controller requires authentication. The adapter captures the existing authoritative selected-company context and requires matching actor/company, a standard ASP.NET principal role matching the app-owned role set and Accounting.View. Preparation additionally requires the existing ProductionAccounting.Adjust grant; decision requires ProductionAccounting.Approve. Both independently require exact FIELD WRITE; fresh source reads also retain Accounting.View and FIELD READ checks. No persona name is used as authorization and IdentityServer business roles are not introduced.

The adapter freezes sorted authoritative roles/grants, actor/company, binding and the request-resolved PRODUCTION connection. Its callback repeatedly verifies that original authority and connection before, during and after the native owned transaction. Fresh contract/revenue captures use the existing complete source read path; the native owner retains original evidence, same-maker refusal, narrow row policy, schema preflight, native audit and committed read-back. Foreign actor/company, route mismatch, revoked grants, FIELD write denial or source drift cannot produce a success.

The native evidence-column schema check is shared between read and mutation. Its canonical model declaration and existing PRODUCTION explicit-type migration list remain the Beep MigrationManager provisioning path. The API never performs migration during a read/write request.

## Typed client and command fidelity

IAccountingServiceClient adds exact commercial review, preparation and decision operations; AccountingServiceClient.CommercialTerms supplies them using the existing ApiClient. Route/query values are escaped and stored calendar dates use invariant date formatting. Commands are copied before awaits and original selected company/account are checked before submission and after response.

The client validates canonical read scope, transaction, source/native proof and recorded result. Preparation acknowledgement must retain the exact original frozen preparation. Decision acknowledgement must retain the exact decision command, original preparation and preparation UTC. Null responses, a different intent/proposal/decision, malformed evidence or changed company context refuse success. Error messages instruct callers to retain and reconcile original intents; this client does not yet provide a durable Web retained-intent state owner. The native endpoint's exact retry reconciles uncertain commits without another write.

API refusals are explicit: anonymous callers are challenged, unauthorized authority is forbidden, route/window argument mismatch is bad request and unconfirmed/stale native source/outcome is conflict. Responses do not reveal provider exception text. Application-wide refusal handling remains responsible for other existing infrastructure refusal types.

## Focused evidence

Actual canonical Data builds with zero warnings/errors in 1.68 seconds. One hundred thirteen focused cases pass, zero failures/skips, final duration seven seconds. Current-source composition includes actual canonical models, native commercial writer, authorization adapter, controllers and typed client. Existing cached source/dependency warnings remain; no full host build is claimed.

Twenty-four new checks comprise sixteen local Kestrel/actual typed-client journey cases and eight SDK response checks. Journeys cover native prepare/retry/read-back then a distinct reviewer approve/reject/retry/read-back, uncertain commit and exact recovery, escaped slash/Unicode/percent FIELD identity, anonymous/standard-role/preparation-grant/FIELD-write denial, forged actor, foreign company, mismatched route, changed source, post-write revocation, missing approval grant, same preparer decision and missing read-time schema capacity. They verify unchanged original native price/preparation, preserved actor/time and no extra persistent write on exact retry. SDK response checks independently refuse null, different preparation intent, changed proposed terms or decision intent, and late company selection for each command. Eighty-nine preceding source/domain/native/API/SDK/rendered checks remain green.

Reproduce using Scripts/Test-PurchaseOrderOwnership.ps1 with actual canonical Data DLL, IncludeWorkflowInbox and FullyQualifiedName~FieldRevenueSourceTests. HTTP uses local Kestrel with the actual controller and client, controlled company authority and Beep transaction/native datasource fixtures. Live OIDC/claims bridge, real native database/provider/transaction isolation, concurrency/performance, browser/keyboard/RTL/accessibility and production provisioning remain acceptance work.

## Next delivery and scope

Connect these commands to retained Web commercial preparation/reviewer screens. Show exact recorded source and proposed terms, explicit pricing/date basis, current authority, preparation/decision audit and confirmed persisted outcome. Preserve pending original intent across errors/context changes and prevent reissuing a new intent while reconciliation is unresolved. Route real approval work through the configured persona task/process owner; the current company process path still supports only AP_INVOICE/PURCHASE_ORDER and must be extended coherently before claiming commercial task completion.

Continue commercial amendments/history, invoice/recognition, royalty/partner source queues and wider thirteen-persona/multinational/native/provider/browser acceptance. No SQL, business EF lane, live schema/business mutation, grant assignment, deployment or commit/push occurs. Writes run only against focused native fixtures. No CA/FG phase or overall goal closes.
