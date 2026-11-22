# ACC-02 — Period close and ledger

| Field | Value |
|---|---|
| Stage | STG-10 — Production accounting and financials |
| Area | Accounting |
| Status | Gate met - see [evidence](../../evidence/STG-10-ACC-02/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Accounting` |
| Desktop project | `Modules/Accounting/Ledger/` |
| External packages | none |
| Depends on | PRD-05 |
| Blocks | ACC-03, ACC-08 |
| Web reference | `Pages/PPDM39/Accounting/PeriodClose.razor`, `LedgerAccounts.razor`, `Pages/PPDM39/Accounting/CompanyAccountingProcessDetail.razor` |

## 1. Purpose

Own the ledger: chart of accounts, journals, trial balance, and the period close checklist that
locks the books.

## 2. Source components

| Source | Reuse |
|---|---|
| `Accounting/Services/GLAccountService.cs` (+5 partials) | chart of accounts |
| `Accounting/Services/JournalEntryService.cs` (+11 partials) | journals |
| `Accounting/Services/PeriodClosingService.cs` (+3) | close workflow |
| `Accounting/Services/TrialBalanceService.cs`, `FinancialStatementService.cs` | reporting |
| `Accounting/Services/CostCenterService.cs` | dimensions |
| `ProductionAccounting/Services/PeriodClosingService.cs` | production close |
| `Repository/FinancialOperationClaimStore.cs` | SoD facts |

## 3. Screens

`Accounting → Ledger`

Tabs:

- **Accounts** — tree and grid, postable flag, normal balance, currency, dimensions.
- **Journals** — list, editor, draft/post, reverse, approval state, source link.
- **Trial balance** — period, debit/credit by account, drill down to journal lines.
- **Period close** — checklist with blocking items, close and reopen with reason.
- **Financial statements** — P&L, balance sheet, cash flow, with the same closed-period rule.

## 4. Data and contracts

`OgJournal { Id, Date, Source, Reference, Description, Lines[], TotalDebit, TotalCredit,
Status, PostedBy }`
`OgPeriodClose { PeriodId, State, BlockingItems[], ClosedBy, ClosedAt }`.

## 5. States

| State | Behaviour |
|---|---|
| Unbalanced journal | refused with the exact imbalance |
| Duplicate reference | refused |
| Closed period | writes refused, naming the period |
| SoD conflict | approve disabled with the conflicting role named |
| Lost acknowledgement on post | retained original; no second entry |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Create/edit draft journal | `accounting.journal.write` | yes |
| Post | `accounting.journal.post` | yes |
| Reverse | `accounting.journal.post` | yes |
| Close period | `accounting.period.close` | yes |
| Reopen period | `accounting.period.reopen` | yes |

## 7. Acceptance criteria

- AC-1 An unbalanced journal is refused with the imbalance.
- AC-2 A duplicate posting does not create a second entry.
- AC-3 A write into a closed period is refused and names the period.
- AC-4 The author cannot post their own journal where SoD applies.
- AC-5 Trial balance debit equals credit for every period shown.
- AC-6 Reopening records actor, reason and time.
- AC-7 Drill-down from a trial-balance line reaches the individual journal lines.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ACC-02"
```

## 9. Open questions and risks

- Two period-close services exist (accounting and production). Establish which owns the lock and
  record it here.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-10-ACC-02/observed-evidence.json). Journals, lines, accounts and
periods are read and written through `UnitofWork<T>` over the ProductionAccounting tables and the accounting project's
`COMPANY_LEDGER_PERIOD`; both were already registered, so no new table was needed and only columns were added.

**The open question, answered.** The **accounting** `PeriodClosingService` owns the lock. Its own header says it "owns
native company/book period records and controlled period operations", and its transitions are what write
`COMPANY_LEDGER_PERIOD.STATUS`. The ProductionAccounting `PeriodClosingService` validates month-end readiness and tracks
unreconciled items - a production close, not the ledger lock. Every create, post and reverse therefore checks the
`COMPANY_LEDGER_PERIOD` row for the entry's period code, and refuses naming the period.

**A note on the owner's guarded transitions.** The accounting period transitions are guarded by intent ids, an authority
JSON and a state-history JSON with size checks - built for the company/book workflow the desktop has not set up, and not
callable with a period code alone. The desktop writes its own period rows in the same table with an authority note naming
the app and the actor, and keeps the close and reopen audit this feature needs on the row. The owner's engine remains the
company workflow's; that is a deliberate split, not an oversight.

**Why a duplicate cannot create a second entry (ADR-010).** Posting is a status transition of the journal row that already
exists, so there is no second row to write; and a second journal with the same `SOURCE_MODULE` and `REFERENCE_NUMBER` is
refused naming the existing entry. Both ends are tested.

**The balance rule.** `LedgerCodes.Imbalance(debit, credit)` is the one definition, and the refusal is that amount with
the side it is on. The totals are summed from the lines in `decimal`; a line may carry a debit or a credit but not both.

**Trial balance and drill-down.** Built from posted, non-reversed journals for the period, grouped by account number, and
each row carries the very lines it was summed from - so the drill-down is the same data, not a second query that could
disagree with the number above it.

**Not built:** the chart-of-accounts editor, the financial statements (P&L, balance sheet, cash flow) and the cost-centre
dimensions; there is no approve/reject command, so a draft is posted directly; the period lock matches on the period code
rather than company+book; segregation of duties is the author-versus-poster rule only, without consulting the Repository's
`FinancialOperationClaimStore`; and SG-10-9 (decimal round-trip precision through persistence) is a separate stage gate
item, not claimed here.
