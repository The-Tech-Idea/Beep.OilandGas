# Implementation 190 - Durable invoice variance proposal recording and retry

Date: 2026-10-07. Status: Data/native/API/typed-client recording and reconciliation implemented and focused-verified. Screen submission/reconciliation, independent controller disposition and persona task handoff remain incomplete. No persona phase is closed.

## Chosen source and persistence owners

The existing canonical AP_INVOICE in TheTechIdea.Data receives one nullable notifying VARIANCE_PROPOSAL_JSON column, bounded at 32768 characters. The existing PRODUCTION Beep entity manifest already lists AP_INVOICE (Beep.OilandGas.ProductionAccounting/Modules/ProductionAccountingModuleSetup.cs); its explicit type now exposes the new column to Beep MigrationManager schema evolution. No parallel invoice, proposal table, repository, EF migration or handwritten SQL is introduced. Live migration/preflight/application remains open; an absent, wrong-type, undersized or nonnullable proposal column refuses recording.

APInvoiceVarianceCommand, APInvoiceVarianceProof, Submission and Result are canonical Data contracts. Command binds original company/account/invoice, the exact proposal and preparation revision, production binding and explicit GUID intent. Preparation revision is derived through the existing proposal-review owner so changed explanations, reason, actor or binding cannot retain the original revision. Shape checks validate exact dispositions, hashes, bounded explanations/reason and intent before a transaction can start.

Proof retains the original invoice header and exact command, plus the complete recorded row digest. Recording may change only the new JSON and canonical accountant change audit. Source maker/creation audit remains intact; amounts, currency, status, payment balances and all other source values cannot change. Invoice status remains DRAFT. Proof is app-recorded provider evidence, not a cryptographic signature or financial approval. Unknown JSON members, changed command/row, foreign ownership and mismatched expected submission refuse adoption.

## Exact retry and current evidence

The existing AP invoice source revision now includes the proposal JSON, and ownership checks enforce its bounded source length. A recorded proposal changes the current invoice and financial revisions. APInvoiceFinancialReview.OriginalVarianceEvidence reconstructs the original header solely from verified own-command proof, rebuilding current own purchase/allocation/contributor references around that header. Current lines, other invoices, allocations, purchase sources, receiver decisions and policy history remain current. They must still reproduce the exact original financial/preparation revisions. The reconstruction is restricted to reconciliation of the same original account/intent; it is not a controller approval shortcut or historical-source fallback.

APInvoiceService.RecordVarianceProposalAsync extends the existing native owner. It freezes command/displayed preparation, requires fresh original submission authority and request-bound source identity, and owns a Serializable Beep transaction. Every review reads complete financial evidence and globally identifies the invoice, refusing duplicate/foreign identities or disagreement with the owned capture. A restricted existing PPDMGenericRepository update policy checks unchanged original row and exact actor/table/operation, then stamps proof after the canonical common-column handler. Stored read-back, repeated complete evidence review and final authority guard precede commit. Exact durable row/proof and fresh authority are required after commit before returning success.

The first proposal cannot overwrite an existing recorded intent. Exact original retry reconstructs only its verified own header and performs no additional update. Uncertain commit preserves the original intent for reconciliation. Changed contributors, policy, receipt, invoice lines, proposal proof, binding or authority refuse retry without repairing/replacing history. Corrections and future controller decisions require their own reviewed lifecycle design.

## AP API and typed client

The existing APController adds POST `{id}/variance-proposal-commands` with canonical Submission/Result. It independently enforces the original company/account/invoice/binding, current standard ASP.NET assigned app role, Accounting.View, Accounting.PrepareInvoiceVariance and the new explicit Accounting.RecordInvoiceVariance grant. Purchase sources independently require Procurement.View. Roles and permission sets are frozen and rechecked at every boundary; complete financial reads use the existing double-capture owner on the same request-bound production source, including inside the native transaction and original retries.

Authorization refusal returns 403; unconfirmed/changed command outcome returns fixed reconciliation guidance with 409. The API verifies the complete returned result against the original submission. No live grant assignment or IdentityServer authorization change is made.

AccountingServiceClient extends its existing invoice-variance partial with RecordAPInvoiceVarianceAsync. It freezes/validates the submission, enforces original selected company/account, submits the same intent and validates the exact full result. Company/account change during the call cannot publish success. Caller state and the forthcoming screen must retain unknown original submissions; the client does not invent or replace intents on retry.

## Focused verification

Actual Data build: zero warnings/errors, 2.98 seconds; log `%TEMP%/oilgas-variance-190-data.log`.

Final affected gate: **1236 passed, zero failed/skipped, terminal exit 0**, 56.6560 seconds; log `%TEMP%/oilgas-variance-190-http.log`. Smaller source gate: **519 passed, zero failed/skipped, terminal exit 0**, 6.1438 seconds; log `%TEMP%/oilgas-variance-190-source.log`. Counts overlap. Composition has 358 warnings, including source/cached-type conflicts from native repository/transaction and API grant/refusal references. Actual Data is warning-free. Whitespace checks pass.

Adds 46 cases over the previous 1190: 36 native/Data and 10 actual HTTP/typed-client cases. Native cases cover exact first recording/retry, original values/status/audit, changed intent/actor/binding/proposal/preparation, changed invoice/line/purchase/allocation/policy/receiver sources, foreign/duplicate global identity, four fresh schema faults, failed write/corrupt read-back/late source/authority/binding with rollback, borrowed/insufficient-isolation refusal, unknown commit and exact retry, changed proof/contributors/another intent refusal, missing post-commit durable row, lost initial/post-commit authority, notifying Beep metadata/source revision and changed result/row/proof adoption. HTTP cases cover independently guarded success/retry, missing prepare/record grants, missing assigned role, binding mismatch, changed contributor evidence/schema, and unknown commit followed by exact original retry or current grant/source refusal. Proposal commands issue one invoice update on success; retries never add another. No payment/posting or controller task is fabricated.

The initial composition exposed missing test Beep.ConfigUtil imports and was repaired before execution. First execution passed 1219 with one foreign-invoice fixture failure: its helper dereferenced a missing owned invoice. The helper now reports unavailable evidence explicitly. Expanded native checkpoint passed 1226; final API/client gate passed 1236. Physical provider schema/isolation/durability, host/OIDC/DI, JS/browser and production concurrency remain seams or unverified acceptance. No broad missing-project solution build is claimed.

## Next persona delivery and broader roadmap

Wire the existing scoped AP workspace and invoice page to submission, original-intent retention, current grant checks, exact result/current-source confirmation and reconciliation after uncertain outcomes and page recreation. Display verified recorded proposal evidence separately from unrecorded preparation. Then implement independent controller disposition and the configured persona task handoff through existing invoice/source/lifecycle owners, with exact evidence, independent preparer/reviewer, durable history and recovery.

Separate tax/non-PO and book/period/journal evidence remain required before financial posting/payment. Policy tasks/retirement/correction, lifetime/new-circuit recovery, stock/returns, broader persona dashboards, multinational requirements and live provider/browser/release gates remain open.

Beep MigrationManager owns business schema. No live migration/grant, handwritten SQL, business EF lane, commit, push or deployment occurs in this slice.
