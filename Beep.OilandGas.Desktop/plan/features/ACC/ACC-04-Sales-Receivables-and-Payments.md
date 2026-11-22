# ACC-04 — Sales receivables and payments

| Field | Value |
|---|---|
| Stage | STG-10 — Production accounting and financials |
| Area | Accounting |
| Status | Gate met - see [evidence](../../evidence/STG-10-ACC-04/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Accounting` |
| Desktop project | `Modules/Accounting/Receivables/` |
| External packages | none |
| Depends on | ACC-03 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Accounting/SalesReceivables.razor`, `Components/Workflow/SalesReceivableWorkbench.razor`, `…ReviewWorkbench`, `…PaymentWorkbench`, `…PaymentReviewWorkbench` |

## 1. Purpose

Collect what is owed: see the ageing, allocate receipts to invoices, and post the cash.

## 2. Source components

| Source | Reuse |
|---|---|
| `Accounting/Services/ARService.cs` (+6 partials) | receivables |
| `Accounting/Services/APPaymentService.cs` (pattern) | payment shape |
| `Accounting/Services/JournalEntryService.cs` | posting |

## 3. Screens

`Accounting → Receivables`

- Ageing grid: customer, current, 30, 60, 90+, total.
- Open invoice list with balance and due date.
- Receipt entry: customer, amount, currency, date, reference, bank account.
- Allocation: apply the receipt across invoices, with remaining unapplied cash shown.
- Payment review: approve or return with reason.
- Posted ledger panel.

## 4. Data and contracts

`OgReceipt { Id, CustomerId, Amount, Currency, Date, Reference, BankAccount, Allocations[],
Unapplied }`.

## 5. States

| State | Behaviour |
|---|---|
| Over-allocation | refused with the excess |
| Currency mismatch | refused |
| Unapplied cash | shown explicitly, never discarded |
| Partial payment | invoice keeps a balance |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Record receipt | `accounting.receivable.write` | yes |
| Allocate | `accounting.receivable.write` | yes |
| Approve | `accounting.receivable.approve` + SoD | yes |

## 7. Acceptance criteria

- AC-1 Allocation total equals the receipt amount or leaves explicit unapplied cash.
- AC-2 Ageing buckets are computed from due dates.
- AC-3 A partial payment leaves the correct balance.
- AC-4 Over-allocation is refused.
- AC-5 Posting creates exactly one journal entry.
- AC-6 Author cannot approve their own receipt.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ACC-04"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-10-ACC-04/observed-evidence.json). Receipts and their allocations are
read and written through `UnitofWork<T>` over the ProductionAccounting AR tables; the cash journal is written to the
ledger's `JOURNAL_ENTRY`/`JOURNAL_ENTRY_LINE`.

**The model had an allocation but no receipt.** `AR_PAYMENT` is one amount against one invoice, which is exactly what an
allocation is - so it was reused as one and only gained `RECEIPT_ID`. What was missing was the header: one bank payment
covering three invoices had no single record, and **unapplied cash had no owner at all**. `AR_RECEIPT` is that header, with
`UNAPPLIED_AMOUNT` as a column, so "allocation total equals the receipt or leaves explicit unapplied cash" is a fact about
the row rather than a convention the screen has to remember.

**The ageing rule had to be written down.** The project carries an `ARAgingSummary` request shape but no rule, so any screen
would have invented its own boundaries. `ReceivableAgeing` states them once:

| Bucket | Rule |
|---|---|
| CURRENT | not yet past the due date |
| 30 | 1 to 30 days past due |
| 60 | 31 to 60 days past due |
| 90+ | more than 60 days past due |

Inclusive at each upper bound, and an invoice with no due date falls back to its invoice date - stated, not silent. Both the
grid and the totals read the same function, so a row cannot disagree with the total under it.

**One journal, and a retry that neither duplicates nor double-applies.** As in ACC-03, the journal's `REFERENCE_NUMBER` is
`RECEIPT-<receipt id>` - a key that exists before the receipt is numbered - so a retry adopts the entry. This feature adds
the other half: an allocation that is already `POSTED` is **skipped** when applying cash, so the retry path cannot reduce an
invoice a second time. The test clears the receipt's journal link and status by hand, re-posts, and asserts both the journal
count and every invoice's paid amount.

**A data-layer constraint found and recorded.** `UnitofWork.Update` and `Delete` locate a row through **the unit of work's
own read buffer**. Reading with `ReadAllAsync` and then updating through a fresh unit throws `Object not found` - which is
exactly how the first run of this feature's gateway test failed. Every write now reads through the unit that performs it.

**Not built, and stated as gaps:** there is no customer master (the customer is the stated id); **unapplied cash cannot be
applied later** - a second receipt is the only path, so cash arriving before its invoice stays unapplied; a receipt cannot be
unposted or reversed; the bank account is a free reference while posting always debits the house cash account; and the
journal is written already posted rather than through the ledger's draft/post cycle.

## 9. Open questions and risks

- Bank reconciliation has no statement import; this feature records cash only.
