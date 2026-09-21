# Stored royalty accrual and retry reservations — 2026-09-21

## Implemented

Royalty accrual previously accepted caller-supplied allocation amounts and created a fresh obligation on every call. The canonical service now accepts an allocation-detail ID and reloads exactly one active stored detail from the bound module repository. The accounting cycle uses this same contract.

The new POST `/api/accounting/royalty/allocations/{allocationDetailId}/accrue` endpoint requires the authenticated local user, app-owned `Accounting.PostJournal` permission and access to the field resolved from the stored allocation's source ticket. It accepts no amount, actor or connection override. The old `service/calculate` action is removed.

- Return an existing active Accrued/Paid calculation without recalculation or another journal entry.
- Reject incomplete, reversed or ambiguous recorded calculations pending reconciliation.
- Derive a stable calculation primary key from the stored allocation-detail ID and insert the reservation before calling the journal service. Concurrent submissions contend on the existing calculation primary key; a rejected insert cannot post a journal. Corrections require a new allocation detail.
- Require a nonempty journal ID and POSTED receipt before marking a positive obligation Accrued. Zero obligations complete without a journal.
- Include calculation and allocation IDs in the journal description. If posting or the final status update fails, the reservation blocks automatic replay; it does not imply that the journal rolled back.

## Verification

- Focused royalty and accounting-cycle tests: **48 passed**, zero failures. `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --no-restore -v quiet --filter 'FullyQualifiedName~Royalty|FullyQualifiedName~ProductionAccountingServiceProcessCycleTests' --logger 'trx;LogFileName=royalty-accrual.trx'`.
- Full API suite: **913 passed, 10 skipped, zero failed**, 923 total. `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --no-build --no-restore -v quiet`. Log: `%TEMP%/beep-accrual-full.log`.
- Web build: **zero errors, 310 warnings**. `dotnet build Beep.OilandGas.Web/Beep.OilandGas.Web.csproj --no-restore -v quiet`. Log: `%TEMP%/beep-accrual-web.log`.
- `git diff --check` passed.

New regressions cover successful replay with changed source amounts, missing stored inputs, source-ticket field resolution, posting exceptions, unposted receipts, failed final status writes, concurrent reservation contention, missing local identity, denied permission and denied field access. Repository tests use mocked IDataSource; the concurrency test simulates a unique-key rejection. The existing ROYALTY_CALCULATION primary-key schema was inspected, but a live database race was not exercised. Ten LocalDB tests remain skipped.

## Remaining work / next slice

FG-41 remains in progress. Payment recording still has caller-supplied calculation and in-memory paths. Replace them with one persisted-calculation/payment contract, authoritative permission and field checks, linked payment records, outstanding-balance validation and retry/recovery rules. Do not certify partial-payment status, overpayment handling or payment reporting yet.

Accrual reservation is not an atomic calculation/journal transaction or automatic recovery mechanism. Add durable journal linkage and controlled reconciliation of interrupted reservations through the existing repository/journal architecture. Validate deployed primary-key enforcement and provider transaction behavior before release. Approval/closed-period gates, browser/OIDC journeys and clean-CI dependency checks remain open.
