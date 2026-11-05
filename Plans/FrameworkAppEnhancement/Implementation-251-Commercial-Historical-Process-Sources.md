# Implementation 251 — Commercial historical process sources

## Original process and version

Company commercial processes now resolve their stored preparation revision rather than displaying the latest contract proposal. The owned process read boundary validates the original start payload, company, process definition identity, deterministic instance identity and original preparer. It compares two complete process and source captures. Current versions continue through the existing live revenue/contract reader; changed current revenue remains a refusal for current work.

An older version resolves only through the current, valid, seller-owned native contract envelope and its preserved PriorVersions. SalesContractTermsNativeProof.HistoricalReview reconstructs the original native proof from the exact historical record, native snapshot, prefix history and amendment origin. The resulting terms retain their original preparation/decision digests, audit evidence and frozen revenue rows. Only the read actor is normalized for the current authenticated reader. Captured source binding and calendar dates remain historical provenance; they are not represented as fresh revenue.

CompanyCommercialProcessSourceReview adds optional CurrentNativeContract evidence and an IsHistorical indicator. Historical reviews must exactly equal reconstruction from that current native envelope. Foreign contracts, altered terms/rows, missing predecessors and substituted historical envelopes refuse. This extends the existing canonical Data contract, existing commercial-source endpoint and typed client validation without introducing another source owner.

## Current authority on historical evidence

Historical reads require current authentication, a standard role from the selected company role set, Accounting.View and FIELD READ for the captured field. The native contract must remain active and seller-owned in the selected company's production binding. Sorted roles/grants, company, actor, production connection/binding and complete native JSON are checked again around the read; the containing process and lifecycle authority are also rechecked. Changes during review fail closed.

Historical reads deliberately do not fetch current revenue rows and substitute them into an old proposal. They therefore remain available when current revenue changes, while live/current-version reads still require exact current source evidence. This is a sequential checked read across existing owners, not a transaction spanning production and lifecycle repositories.

## Original-preparer historical screen

The existing company process page renders historical commercial terms in a read-only MudBlazor panel: contract/version, original FIELD/transaction/window, effective dates, explicit proposed price/currency/unit/method, notes, preparer/time, rejection reviewer/time/reason and preparation revision. A captured revenue table displays stored dates, currency, gross/net revenue and oil price; missing values remain “Not recorded.” The page labels those rows as captured evidence.

Historical pages do not mount the mutable terms workbench or show step submission/reconciliation controls. The workbench independently refuses a historical assigned source if an older panel remains mounted during a reload. The original global FIELD selection remains unchanged. CSS, JavaScript, theme and database access boundaries are unchanged.

## Focused verification

Canonical Data builds with zero warnings/errors. Seven additional actual native/local HTTP cases extend the existing commercial handoff journey. They complete recorded rejection routing and original-preparer rejection review, append an amendment through the native test owner, then open the original process through its typed API client. Frozen old revenue remains available after current revenue changes; the process retains REJECTION_REVIEWED and its original rejected terms. FIELD/grant/role/company denial and mid-read production binding/native changes refuse, with no additional native writes.

The success case renders the original-preparer historical process page, verifies the captured-evidence label and old rejection, and verifies no mutable workbench or review-step controls. It then restores the live source, starts a distinct configured process for the amendment, records a new independent reviewer's native approval and completes that process through the existing API. Both old and new process sources retain their own version-specific outcomes. Actual native write counts and global FIELD preservation are asserted. The amendment append uses the native fixture entry point: this does not certify a public process-bound amendment submission endpoint or form.

Final focused source/Razor verification passes 329 tests with zero failures/skips in 41 seconds. Logs are oilgas-251-data.log and oilgas-251-final-source.log in the local temporary directory. Reproduce with Scripts/Test-PurchaseOrderOwnership.ps1, the actual canonical Data DLL, IncludeWorkflowInbox and the filter FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox. Existing source/dependency composition warnings are scoped test-host limitations; no full solution or production provider build is claimed.

## Next delivery

Implement the process-bound amendment review/submission API with exact closed REJECTION_REVIEWED evidence, current original-preparer role/grants/FIELD authority and a native predecessor fence. Add retained amendment forms, same-intent reconciliation and history navigation to the original-preparer workspace. Verify that public journey through actual persona inbox, forms, native read-back and the new independent handoff. Durable circuit-loss recovery, delivery/effective terms to invoice/recognition and ledger read-back, royalty/partner work and wider thirteen-persona/multinational acceptance remain open.

No schema change, handwritten SQL, new business EF migration lane, live data write, role assignment, commit/push or deployment was performed. Schema remains on Beep Migration. No CA/FG phase or the overall implementation goal closes. Real provider/concurrency, browser/OIDC, keyboard/accessibility/RTL and multinational acceptance remain open.
