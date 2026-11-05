# Implementation 244 — Native commercial preparation and independent decision persistence

## Delivery and persona boundary

The existing ProductionAccounting service now owns PrepareSalesContractTermsAsync and DecideSalesContractTermsAsync. They persist the canonical Implementation 243 preparation/decision evidence on the exact native SALES_CONTRACT instead of constructing an approval only in memory. This is native command-owner delivery; authenticated API command endpoints, retained Web state/forms and actual assigned approval tasks remain to be connected. Implementation 242 remains the currently visible source review.

The commands preserve recorded native price/currency/effective dates and creator provenance. Proposed effective terms remain separately recorded evidence; approve/reject does not silently rewrite the original source price or create an invoice, recognized amount or ledger entry. One original preparation and optional independent decision are supported per contract. A different intent or amendment cannot replace them; version cycles/history expansion remains future work.

## Native proof and provisioning

Canonical SALES_CONTRACT gains nullable TERMS_WORKFLOW_JSON with MaxLength 524288. SalesContractTermsNativeProof retains the complete copied frozen preparation, optional decision, server command UTC timestamps and digest, together with the exact native row/audit snapshot excluding only the envelope itself. Its digest covers that complete evidence. Read-back requires exact active seller-company/native identity, recorded changed actor/time consistent with the command, preserved native business/creator facts, matching row snapshot and canonical serialized envelope. Changed or ambiguous/extra serialized payload facts refuse.

The existing PRODUCTION module's ProductionAccountingModuleSetup already declares typeof(SALES_CONTRACT) in EntityTypes. Its canonical type now includes the evidence column, so schema evolution stays in the existing explicit-type Beep MigrationManager path. An actual Beep ClassCreator check confirms the string type, nullable declaration and full capacity. The writer refreshes native schema before each command confirmation and refuses missing, undersized, nonnullable or incorrectly typed evidence columns. It never auto-migrates on save. No live migration plan execution or provider provisioning is performed by this slice.

Fresh reviewer capture includes the persisted envelope/audit change caused by preparation. RestoreOriginalReview first validates the complete current native proof and original preparation ownership, confirms unchanged native business facts, and restores only the frozen original contract for comparison with its original revenue/contract review. This permits the writer's own audited preparation/decision change without concealing external buyer/price/source/binding drift. Revenue source facts still undergo the canonical complete-source revision check.

## Transaction, authority and retry behavior

Submitted preparation, expected record and decision are copied before awaits. Both methods require nonnull fresh source and current-authority callbacks supplied by the API owner. Guards run before, during and after the command; changed datasource identity refuses. The upcoming API adapter must independently bind current app-owned roles/grants, company/account/FIELD and PRODUCTION connection fingerprint. This callback boundary does not itself prove HTTP authorization, and no HTTP write route is added here.

The existing PPDMDataSourceTransaction owner supplies an owned Serializable Beep transaction on the same datasource. Borrowed transactions or ReadCommitted isolation refuse. A narrowly scoped existing PPDMGenericRepository row policy restricts mutation to the exact SALES_CONTRACT key and seller, compares the complete previous row, requires server audit stamping and builds/validates the native proof after audit stamping. Repository provider errors are not accepted as success.

Within the transaction, the writer checks refreshed schema, unique typed native contract, exact agreement between native and freshly reviewed row, original frozen source, prior preparation/decision ownership and current authority. It confirms the complete written row and envelope, then recaptures closing evidence before commit. After commit it again checks native row, complete proof, source and authority; uncertain evidence returns failure and requires reconciliation.

An exact original preparation retry returns the recorded result without a second write. An exact decision retry against its original undecided review returns the already recorded decision, including after an uncertain commit; changed decision intent cannot overwrite it. A different preparation refuses. Independent-maker and explicit-term rules remain the canonical Implementation 243 checks. The original preparation UTC and source snapshot remain intact when recording the separate reviewer decision.

## Focused verification

Actual canonical Data builds with zero warnings/errors in 1.61 seconds. Eighty-nine focused tests pass with zero failures/skips; final test duration is three seconds. The gate compiles the exact new production partial, canonical model/proof and existing repository/transaction owners against actual Data. Cached source/dependency composition warnings remain. No full host/solution build is claimed.

Sixteen new checks comprise one actual Beep ClassCreator metadata check, ten preparation cases and five decision cases. They cover prepare/retry, refusal of replacement intent, schema capacity, initial authority, post-write revocation, provider write error, corrupt native read-back, borrowed transaction, wrong isolation, changed revenue source, changed binding, native approve/reject, retained original preparation, preserved native maker/current audit, unchanged native price, exact decision retry, uncertain commit recovery, changed buyer and tampered envelope. Transaction fixture read-back and rollback operate on copied native rows; tests verify failed outcomes do not leave a preparation where rollback is required, and exact recovery adds no write. Seventy-three preceding domain/source/API/SDK/rendered checks remain green.

Reproduce with Scripts/Test-PurchaseOrderOwnership.ps1, actual canonical Data DLL, IncludeWorkflowInbox and FullyQualifiedName~FieldRevenueSourceTests. Native datasource behavior is exercised using the controlled Beep transaction fixture. Live provider/database isolation, transactional completeness of all source reads, production provisioning, concurrency/performance and browser/OIDC/multinational/accessibility acceptance remain open.

## Required continuation

Expose preparation/decision review and command endpoints through existing company/native readers with independent current grants and role checks. Preserve retained client intents and uncertain-command reconciliation across company/FIELD/authentication lifecycle. Add commercial preparation and reviewer screens showing full source, proposed terms, exact recorded outcome and original audit; route actual configured persona approval work through existing task/process owners. Continue invoice/recognition, amendment/history, royalty/partner queues and the wider thirteen-persona plan.

No handwritten SQL, business EF lane, live business/schema mutation, permission assignment, deployment or commit/push is performed. Native writes occur only inside focused fixtures. No CA/FG phase or overall goal closes.
