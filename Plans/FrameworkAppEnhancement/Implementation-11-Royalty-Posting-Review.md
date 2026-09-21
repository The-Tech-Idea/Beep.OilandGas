# Royalty posting evidence and durable accrual linkage — 2026-09-21

## Implemented

A pending royalty reservation cannot establish whether its journal posted before a later write failed. Added a read-only review to the existing royalty service and API at GET `/api/accounting/royalty/calculations/{royaltyId}/posting-review`. The API requires the local authenticated actor, app-owned Accounting.PostJournal permission and access to the calculation's source-ticket field before reading posting evidence.

The result covers the accrual and every persisted linked payment, with recorded status, amount, stored journal ID, candidate IDs and a finding: NoJournalRequired, MissingJournal, AmbiguousJournals, JournalMismatch, IncompletePosting or MatchingPostedEvidence. A matching finding is evidence for investigation, not permission to retry or proof of a safe recovery transaction.

JournalEntryService reads exact generated descriptions, then each candidate's journal lines and ledger entries from its bound database. Read failures propagate instead of becoming missing evidence. Reversed/inactive headers and incomplete ledger writes remain visible. Royalty review checks source module, journal linkage, active indicators, header totals, exactly the expected debit/credit pair, unique line/ledger identities and POSTED status. Missing/extra ledger entries cannot be reported as matching posted evidence. Payment identities must be present and unique.

Successful positive accruals now persist JOURNAL_ENTRY_ID on the existing calculation entity. Updated all six ProductionAccounting provider bootstrap scripts. A failed final accrual write can still leave this link absent; the exact generated description supplies candidates for investigation without silently repairing the row.

## Validation

Service regressions cover matching, missing and ambiguous journals, wrong account/link/total, draft headers, missing/duplicate ledger entries and a pending payment whose journal posted. Reviews assert no writes or journal creation. The real journal reader is exercised through the PPDM repository over a mocked provider for scoped reads and provider failure propagation. API regressions cover authentication, local identity, posting permission and field access. Existing royalty and SQLite bootstrap tests continue to run.

Focused royalty tests: **73 passed**. Full API suite: **946 passed, 10 skipped, zero failed** (956 total). Final Web build result is recorded in the master tracker. API evidence: `Beep.OilandGas.ApiService.Tests/TestResults/royalty-posting-review-full.trx`. Logs: `%TEMP%/beep-posting-review-full.log` and `%TEMP%/beep-posting-review-web.log`.

## Recovery plan and remaining gates

1. Apply entity/bootstrap schema updates through development module setup and verify deployed key enforcement; no live database has been altered by this slice.
2. Introduce a persisted recovery/version claim enforced by the database and shared by ordinary posting and recovery. A reviewer must not race a still-running original operation or a new payment.
3. Capture durable journal source identifiers at creation and revalidate the journal/ledger under the same controlled operation. Descriptions and independent read snapshots are diagnostic evidence, not a sufficient mutation guard.
4. Implement approved, audited finalization for confirmed postings, and explicit handling of incomplete/absent/reversed postings. Do not delete reservation rows or blindly repost.
5. Verify provider races, crash points, approvals/segregation of duties, closed-period behavior and UI acceptance.

This slice does not mutate pending records, add automatic recovery, or assert atomic multi-table reads. Existing fixed default-account posting and review share the same account assumptions; configured account mapping and currency-specific rules remain part of accounting hardening. FG-41 remains in progress.
