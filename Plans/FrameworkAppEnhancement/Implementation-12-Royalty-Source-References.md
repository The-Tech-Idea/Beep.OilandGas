# Structured royalty journal source references - 2026-09-21

## Implemented

Royalty journals now persist the calculation or payment reservation ID in JOURNAL_ENTRY.REFERENCE_NUMBER at creation. The existing SOURCE_MODULE is PRODUCTION_ACCOUNTING. The generated description remains descriptive text, not the reconciliation key.

JournalEntryService exposes a referenced balanced-entry operation and routes it and the existing balanced-entry operation through one private creation/posting implementation. The referenced operation requires a nonblank reference within the existing 255-character journal column limit. Royalty accrual and payment calls supply their server-generated reservation IDs; neither accepts a client override for the journal reference.

Posting evidence queries now filter by both REFERENCE_NUMBER and SOURCE_MODULE. Royalty review independently checks the reference against the stored reservation identity, plus all prior header/line/ledger checks. Description changes no longer lose the link; another module's matching reference is excluded. Duplicate journals within the production source remain ambiguous. There is no description fallback.

## Validation

Extended existing tests to assert accrual/payment journal calls carry their actual reservation IDs. New tests exercise actual JournalEntryService creation, header/line/ledger persistence and lookup using PPDM repositories over the mocked provider; they verify edited descriptions and matching references from unrelated modules. Blank, whitespace and overlength references fail before writes. All prior royalty, authorization and schema regressions remain in the suite.

API full-suite evidence: Beep.OilandGas.ApiService.Tests/TestResults/royalty-source-reference-full.trx. Commands: `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --no-restore -v quiet --logger 'trx;LogFileName=royalty-source-reference-full.trx'` and `dotnet build Beep.OilandGas.Web/Beep.OilandGas.Web.csproj --no-restore -v quiet`. Logs: `%TEMP%/beep-royalty-reference-full.log`, `%TEMP%/beep-royalty-reference-web.log`. Full API suite: **950 passed, 10 skipped, zero failed** (960 total). Final incremental Web build: **zero errors, 92 warnings**. Existing dependency/analyzer warnings remain; incremental warning counts do not indicate clean-build warning elimination. Counts are recorded in the master tracker.

## Remaining recovery boundary

The inspected IPPDMGenericRepository.UpdateAsync(entity, userId) contract has no expected-version/compare-and-swap argument. Adding an in-process lock would not protect another server or a crashed operation. Recovery status mutations remain disabled until ordinary posting and recovery share a database-enforced claim/version contract, with provider-specific affected-row verification and crash/race tests.

The reference is durable correlation, not a unique-key guarantee or automatic journal idempotency. Existing royalty reservation primary keys still arbitrate ordinary duplicate submissions. The next recovery work must add the shared atomic claim, revalidate evidence under that operation, enforce approvals/closed periods and record the recovery audit before exposing finalization actions. No live database was altered. Journals created before structured references need explicit development reconciliation; they are not silently matched by descriptions. Schema deployment from earlier slices, account mappings, currency rules and live provider verification remain open. FG-41 remains in progress.
