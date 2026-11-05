# Implementation 94: recorded closing journal read-back

Updated 2026-10-06. Continues accountant period/source read-back. Supporting actual owner correction; full close/lock/ledger outcome remains open.

## Concrete mismatch corrected

The existing ProductionAccounting.PeriodClosingService writes a journal through JournalEntryService with reference CLOSE-{FIELD}-{yyyyMM}, source module PERIOD_CLOSING and an ordinary generated journal entry number/type. ProductionAccountingService.GetPeriodStatusAsync instead queried PC-{FIELD}-{yyyyMM} in ENTRY_NUMBER and PERIOD_CLOSE in ENTRY_TYPE. Those criteria did not match the active writer. Any returned row also counted as CLOSED regardless of posting status.

Both active writer and reader now use PeriodCloseJournalRecords for the same reference and evidence interpretation. The reader queries actual REFERENCE_NUMBER, SOURCE_MODULE and ACTIVE_IND. Exactly one matching active journal with the exact entry date and POSTED status reports the existing CLOSED code. A DRAFT reports CLOSING; a genuine empty recorded result reports OPEN. Null reads, duplicate/foreign/inactive/wrong-date/unidentified records and unknown statuses refuse confirmation rather than reporting closed or empty success.

The writer's existing duplicate check uses the same evidence interpretation before accepting an already-recorded posted journal. It no longer silently accepts multiple posted duplicates or a null provider read. This is duplicate detection after a read, not atomic create/idempotency or a complete close transaction.

## Multinational consistency

The machine journal reference uses invariant Gregorian yyyyMM, independent of the caller's current calendar/culture. The typed client's period-check date query also uses invariant yyyy-MM-dd. Display localization remains a separate concern. Existing data written under an alternate calendar is not silently matched through a compatibility reference; provider reconciliation remains an explicit gate.

## Ownership and migration boundary

This extends the existing journal-backed closing owner and its status reader; no second period/history store is created. Installed Migration guidance was consulted while checking schema ownership. This correction needs no schema or migration application. Journal close evidence still does not supply an authoritative accounting-period lock or book/fiscal-period identity.

## Focused evidence

Test-PeriodCloseJournalRecords.ps1 compiles actual helper/constants sources against actual Models/Data dependencies: **16 passed**, zero failures/skips, terminal exit 0, 0.9665 seconds. Log `%TEMP%/oilgas-close-record-status-final.log`. Cases cover query alignment with writer reference/module, posted versus draft, genuine empty result, null/ambiguous/foreign/invalid records, required active indicator, and invariant references under ar-SA, th-TH and en-US. Whitespace diff check passed.

The actual reader/writer and client wiring are source-only; the focused helper tests do not execute provider repositories, the typed client or whole service. No native/provider/browser claim is made and missing production dependencies are not substituted.

## Remaining complete outcome

The existing CLOSED value means a matching posted closing journal header under this reader, not proof of posted ledger lines, balanced receipts, finished multi-stage close, enforced period lock or atomic book-bound closure. Zero-activity close currently skips journal creation and therefore does not supply close read-back. Full persisted close/history/lock and complete ledger/source command evidence remain open.

Verify exact selected-company accounting binding with two companies/colliding field IDs, actual write -> recorded header/lines/ledger reload, maker/checker authorization, original reviewed version, no-activity outcomes, provider concurrency and durable reconciliation. Surface authoritative close outcomes on the period screen only after these gates pass. Other accountant and remaining persona workflows stay in scope.

No SQL, schema change, business EF lane, role grants, fabricated outcomes or phase/goal closure.
