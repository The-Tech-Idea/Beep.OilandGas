# Persisted royalty payments — 2026-09-21

## Implemented

Replaced the royalty controller's in-memory payment and caller-supplied calculation paths with one stored-calculation service. POST `/api/accounting/royalty/calculations/{royaltyId}/payments` takes `{ requestId, amount }`. The request ID is a nonempty UUID retained for retries; changing its amount is a conflict. GET on the same path reads persisted payments, including pending records. Both require an authenticated local actor and source-ticket field access; writes additionally require the app-owned Accounting.PostJournal permission. IdentityServer remains authentication-only. Removed old payments/service-payments and unscoped by-allocation routes; no Web consumers referenced them.

- Reload the active stored accrued calculation and derive owner, interest and lease from it. Reject nonpositive amounts, unsupported sub-cent precision and amounts above the remaining balance.
- Add ROYALTY_CALCULATION_ID, PAYMENT_REQUEST_ID and JOURNAL_ENTRY_ID to the existing module-owned ROYALTY_PAYMENT entity and its six ProductionAccounting provider bootstrap scripts. No parallel payment store or DTO calculation is introduced.
- Sum linked completed payments. Partial settlement leaves the obligation Accrued; exact final settlement marks it Paid. A completed request replay returns its stored payment without a second journal entry.
- Reserve each next payment position using a deterministic primary key derived from calculation ID and the count of retained payment records. Concurrent contenders for the same position cannot both insert under primary-key enforcement. An insert failure prevents journal posting. All reservation rows must be retained; inactive, ambiguous or pending payments block further settlement pending reconciliation.
- Post debit accrued royalties / credit cash and require a POSTED journal receipt with an ID. Keep the payment pending until calculation status and final payment writes complete. Exceptions or unconfirmed receipts stop automatic replay. This records accounting settlement; it does not initiate bank transfers or certify bank clearance.
- Repair missing inline primary keys in SQLite ProductionAccounting royalty calculation/payment bootstrap scripts. The previous companion scripts asserted those keys existed, but the table definitions omitted them.

## Verification

New service tests cover partial/final settlement, unchanged replay, changed replay amount, overpayment, invalid amounts, journal errors, draft receipts, calculation-write failure, payment-write failure and simulated concurrent attempts against the same balance. API tests cover missing local identity, unauthenticated access, denied posting permission and source-field authorization. Two real SQLite tests execute the bootstrap scripts and prove duplicate reservation keys fail; the payment test also reads the new linkage columns.

Full API suite: **930 passed, 10 skipped, zero failed** (940 total). Web build: **zero errors, 310 existing warnings**. Results are also recorded in MASTER-TODO-TRACKER.md. API command: `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --no-restore -v quiet --logger 'trx;LogFileName=royalty-payments-full.trx'`. Web command: `dotnet build Beep.OilandGas.Web/Beep.OilandGas.Web.csproj --no-restore -v quiet`. Logs: `%TEMP%/beep-payment-full.log`, `%TEMP%/beep-payment-web.log`. Whitespace check passed.

## Remaining gates

Bootstrap scripts and entity-driven setup describe the development target; no existing database was migrated or recreated during this slice. Apply the updated schema through the module setup process before exercising payments against a development database. Non-SQLite provider DDL was not executed. The service concurrency test simulates uniqueness; live provider race/transaction validation remains open.

A pending reservation after any uncertain journal/write outcome requires controlled reconciliation. There is no automatic rollback or cross-repository transaction claim. Durable accrual journal linkage, recovery actions, approvals/segregation-of-duties execution, closed-period enforcement, payment-date/bank-reference workflow, currency-specific rounding and UI acceptance remain open. Payment records cannot be deleted to retry. FG-41 remains in progress.
