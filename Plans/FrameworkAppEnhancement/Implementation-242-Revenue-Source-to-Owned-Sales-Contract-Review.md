# Implementation 242 — Revenue source to company-owned sales contract review

## Visible commercial and accountant handoff

The recorded FIELD revenue source screen now provides a per-transaction `Review recorded sales contract` action. It uses the selected revenue transaction's recorded CONTRACT_ID rather than a free-text contract selector or broad unscoped trading endpoint. An empty reference disables the action. A missing, inactive, ambiguous, changed or foreign-seller sales contract produces an explicit failure rather than substitute terms.

A successful review displays exact revenue transaction and sales contract identities, contract number, seller/buyer, recorded effective/expiry dates, commodity, currency, nullable base price, pricing method, description and revision. Missing dates/price/other text remain unavailable. Recorded text is encoded by Razor. The panel identifies that this record does not establish approval, price units, effective-term authorization or revenue recognition. Source/contract currencies are presented separately; no exchange rate or cross-currency calculation is inferred.

This moves the commercial workflow from recorded revenue to an exact company-owned native sales contract source. The reader requires SALES_CONTRACT.SALES_CONTRACT_ID to match the recorded revenue reference and SELLER_BA_ID to equal the selected company's business associate identity. It does not establish the semantics of other possible contract tables or pretend that every CONTRACT_ID is a sales contract. No match in this native table refuses the review.

## Existing owners and canonical contracts

The single SALES_CONTRACT definition moves from ProductionAccounting into TheTechIdea.Data while retaining its existing public namespace and fields. Its ID is marked as the native key, string backing values initialize explicitly and unused private date/source/remark shadows are removed. Effective and expiry dates already belong to the canonical ModelEntityBase; those inherited public properties are preserved, with no invented default date. No new persistent business field or approval state is added.

The canonical RevenueSalesContractReview holds a copied original FieldRevenueSourceReview, selected transaction identity, copied native SALES_CONTRACT and complete digest. Validation requires original company/account/FIELD/window/source revision, exact transaction-contract reference, active native contract and selected-company seller. Source and contract mutations alter the required digest.

The existing ProductionAccounting service gains a native generic-repository sales contract reader with exact ID, seller and active filters. It requires the expected sole native primary key and exactly one typed matching result. Malformed identity, duplicate/missing/foreign/inactive records and changed datasource identity refuse publication.

The existing CompanyFieldRevenueReads orchestrator reads the original complete revenue graph and compares its revision to the selected screen evidence. It captures the sales contract twice around fresh revenue/authority reads, then closes with another current company/role/grant/FIELD/binding/source check. Observed changed contract, revenue, binding or revoked access refuses the review. These sequential captures are not a transactional snapshot; repeated native reads and real-provider efficiency remain acceptance work.

The additive `GET /api/fields/{fieldId}/accounting/revenue-source/sales-contract` endpoint takes the exact date window, transactionId and sourceRevision. It independently uses existing selected-company standard-role, Accounting.View and FIELD READ enforcement. It adds no hardcoded persona role or IdentityServer business authorization. The typed accounting client copies the submitted evidence, escapes route/query values, checks the original company selection and validates the returned canonical review. The Razor screen clears contract data/errors with source reload and company/FIELD/authentication changes; its version, company snapshot and original-source reference guards discard late contract success or failure. It uses MudBlazor and introduces no CSS/JavaScript file or direct frontend database access.

## Focused evidence

Actual canonical Data builds with zero warnings/errors (1.77 seconds). Current-source compilation includes the moved canonical model, native reader, orchestration/controller, actual typed client and exact Razor page using the configured MudBlazor version. The source composition still reports cached dependency/source warnings; no full host build is claimed.

Fifty-two focused cases pass with zero failures/skips. Thirty-nine existing revenue source cases remain green. Ten new local Kestrel/actual typed-client contract cases verify successful native seller-owned review with copied nullable/date facts, foreign seller, missing/duplicate/inactive contract, changed contract between captures, stale revenue, unknown selected transaction, removed reference and revoked authority. Three new HtmlRenderer cases invoke the exact review handler and verify contract/source identity and encoded description, explicit contract failure, and suppression of late errors after clearing company context. The native fixture asserts seller and exact contract-ID filters and zero persistent writes.

Reproduce using `Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath <actual Data DLL> -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~FieldRevenueSourceTests'`. No live native provider/database, browser/OIDC, keyboard/RTL/accessibility, concurrency transaction or large-history performance acceptance is claimed. No SQL, business EF lane, live Beep migration, permission assignment, posting, deployment or commit/push is executed.

## Remaining execution

Continue source -> versioned effective-term preparation -> independently authorized approval -> invoice/recognition preparation -> persisted outcome and assigned-task read-back. The current native sales record has no governed approval/version/unit-basis proof; those requirements must reuse the existing process/command ownership rather than treating this read digest as approval. Reconcile royalty and partner surfaces with their source evidence before adding company queues. Wider thirteen-persona delivery, multinational currency/units/time-zone/localization, native authority/provisioning and provider/browser acceptance remain open. This slice closes no CA/FG phase or overall implementation goal.
