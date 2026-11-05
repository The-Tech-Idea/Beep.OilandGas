# Implementation 203 — Correction save API and typed client

## Existing AP save boundary

POST api/accounting/traditional/ap/{id}/correction-commands extends the existing AP controller and invokes its existing native correction owner. The body uses TheTechIdea.Data's exact submission contract; no client model, store or direct database path is added. The route uses the existing exact recorded identity reader and must match the submitted invoice. The original company, account and production binding must match current request authority.

Current standard ASP.NET roles must match the authoritative company role set; Accounting.CorrectInvoice is required alongside the existing AP/purchase read authority. Full role and permission sets are frozen and checked through the native operation and response boundary. IdentityServer remains authentication-only. The submitted review is validated but does not supply authoritative current business facts: the API supplies server-owned native invoice, purchase, allocation, policy and global receipt-consumption captures through the existing double-capture financial reader.

The native owner retains the owned Serializable Beep transaction, typed schema/global identity checks, narrow header/line/history writes, exact retry and durable full-source confirmation from Implementation 202. Original proposal/controller evidence stays recorded. Changed authority, source or contributors cannot publish success. Forbidden authority returns 403; unconfirmed/changed source or commit returns an actionable 409 telling the caller to retain and reconcile its exact original intent.

## Existing typed client

IAccountingServiceClient and its existing implementation expose RecordAPInvoiceCorrectionAsync. The client freezes the full submission before awaiting, validates the exact command/review, and requires its original selected company/account. It escapes the recorded invoice identity and submits through the existing ApiClient with selected-company headers.

After response, the company/account snapshot must still match. The canonical result must prove the exact submitted command, recorded final history entry, complete current header/lines and preserved preceding reviewed source. Missing lines or a substituted command cannot confirm a correction. A committed response under a changed context remains unconfirmed rather than publishing to that context. The scoped editor has not yet been wired to save or retain uncertain command intent in this slice.

This endpoint changes billed source values/history only. It does not approve financial matching, derive payment/balance/tax/ledger effects or complete correction workflow work. Independent rereview and confirmed correction task evidence remain required future work.

## Focused verification

The affected gate passes **1535 checks with zero failures/skips in 3.1460 minutes**, including the initial 18 new HTTP/client cases. All **22 final HTTP/client cases pass in 32.3508 seconds**, including four additional response/context cases. The gates overlap and have different scopes; the later targeted run covers the added cases. All handles reached terminal exit and whitespace verification passes. The overlapping source gate passes **519 checks in 8.2127 seconds**. Actual canonical Data builds with zero warnings/errors in 1.71 seconds. Composition reports 374 source/cache/package warnings, three more existing-style source/cache conflicts at this endpoint's permission/refusal references than Implementation 202.

Actual Kestrel API/typed-client cases cover native recording and exact retry without extra writes, subsequent complete source reads, absent/revoked grants, role/binding changes, foreign route/company/account, changed source/line/global contributor evidence, drift after line writes with rollback, uncertain commit and exact reconciliation, changed retry intent/contributors, original selected-context refusal before HTTP, company/account changes after commit and altered response lines/command. The first composition compile used an incorrect source client method name in the new test; it was corrected to the existing GetAPInvoiceAsync method before runtime verification.

Native provider/physical transaction and authoritative RBAC capture remain explicit fixture seams; these tests do not prove live provider/schema, production grants, browser/OIDC/accessibility or a full solution pass. No SQL, new business EF lane, live migration/grant or phase closure is introduced.

## Next implementation target

Wire saving into the existing accountant correction editor/scoped owner with one frozen original intent, busy/conflicting-command guards, retained unknown outcome and fresh complete source/history confirmation. Reject late publication across source/company/account changes and provide exact original-context recovery. Require independent rereview and confirmed corrected-invoice evidence before correction task completion. Live definition provisioning, lifetime recovery, tax/non-PO/book/period/journal evidence, policy tasks, stock/returns, other personas/dashboards and multinational acceptance remain active.
