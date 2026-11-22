# ACC-01 — Accounting dashboard

| Field | Value |
|---|---|
| Stage | STG-10 — Production accounting and financials |
| Area | Accounting |
| Status | Gate met - see [evidence](../../evidence/STG-10-ACC-01/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Accounting`, `Beep.OilandGas.ProductionAccounting` |
| Desktop project | `Modules/Accounting/Dashboard/` |
| External packages | none |
| Depends on | ACC-02 shell, WFL-01 |
| Blocks | — |
| Web reference | `Pages/PPDM39/Accounting/AccountingDashboard.razor`, `Pages/PPDM39/Admin/AdminWorkspace.razor` |

## 1. Purpose

The accounting persona's landing: the invoice and payment queues awaiting action, the current
period state, and the balances that need attention.

## 2. Source components

| Source | Reuse |
|---|---|
| `Accounting/Services/DashboardService.cs` | KPI aggregation |
| `Accounting/Services/APInvoiceService.cs`, `APPaymentService.cs`, `ARService.cs` | queue counts |
| `ProductionAccounting/Services/PeriodClosingService.cs` | period state |
| ACCT_* process definitions | process queues |

## 3. Screens

- KPI cards: open AP, open AR, overdue, unposted journals, period state, cash position.
- Queue panels: supplier invoices, purchase orders, sales invoices, receipts awaiting match.
- Period strip: current period, close state, blocking items count.
- Attention list: documents past due, unmatched receipts, failed postings.

## 4. Data and contracts

`OgAccountingKpis { OpenAp, OpenAr, Overdue, UnpostedJournals, PeriodState, Currency }`.

## 5. States

| State | Behaviour |
|---|---|
| Failed read | failure panel; no zero balances |
| Period closed | banner with the period name |
| Empty queue | "nothing awaiting you" with the last-refreshed time |
| Permission limited | queue panel omitted with the reason, not shown empty |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| View | `accounting.read` |
| Open a queue | that document's permission |
| Refresh | any signed-in user |

## 7. Acceptance criteria

- AC-1 Balances come from records; a failed read is never zero.
- AC-2 Queue counts match the queue page's total for the same filter.
- AC-3 Period state is shown for the current period of the session.
- AC-4 A restricted queue is omitted with a stated reason.
- AC-5 The last-refreshed time is always visible.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ACC-01"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-10-ACC-01/observed-evidence.json). The dashboard writes nothing and
reads through the seven accounting services ACC-02..ACC-08 built, plus one direct read of the configured cash account for the
cash position.

**The open question, answered.** The queue is built from the **records' own states**, not from `ACCT_*` process definitions.
This app creates no process instances for those definitions, so a queue built from them would have been empty by
construction - and building it from both would have created a second source for the same count. The states the queues report
are the facts a person acts on: a draft invoice, an invoice awaiting approval, a match exception, a draft receipt, unapplied
cash, an unposted journal, unposted tax, an unposted bill, a payable royalty, an open stock hold.

**AC-2 is true by construction, not by a matching predicate written twice.** The dashboard calls each feature's own `LoadAsync`
and applies the predicate to the workspace it returns, so the count is computed from the same records - and the same
conditions on those records - that the queue's own page lists. The gateway test proves it by loading all seven workspaces and
comparing every one of the twelve counts with them:

| Queue | Predicate over the workspace |
|---|---|
| sales-draft / sales-review | `IsDraft` / `IsIssued && !IsApproved` |
| ap-open / ap-exceptions / po-draft | `!IsPaid` / `NeedsExceptionApproval` / `IsDraft` |
| receipts-draft / unapplied-cash | `IsDraft` / `Unapplied > 0` |
| journals-draft / tax-unposted / jib-unposted | `IsDraft` / `!IsPosted` / `!IsPosted` |
| royalties-payable / stock-holds | `CanPay` / `HoldsStock` |

**AC-1 is proven by breaking it.** Every money figure and the unposted count are nullable, and a null prints
`unavailable - <reason>` rather than `0.00`. The gateway test drops the three sales tables and asserts that `OpenAr` is
**null**, that its text contains `unavailable` and not `0.00`, that `Overdue` is null too (it needs both sides), that the
payables side still reads, and that the sales queue says `(not read)` instead of looking empty.

**AC-4 distinguishes a denial from an unknown.** A valid permission set that lacks the right is withheld **naming the
permission**; a set whose lookup failed (`PermissionSourceAvailable` false) is withheld and the reason says it is **unknown,
not denied** - because an empty permission set from a failed lookup is not the same fact as a person without the role.

**Two messaging defects the unit test caught.** A queue whose count could not be read was reported with the same sentence as
an empty queue (`nothing awaiting you`), and the cash breakdown appended the currency twice. Both are corrected: an unread
queue prints `(not read)` with its reason, and the breakdown prints `opening 5,000.00 + posted movement 250.00 USD`.

**The cash position is the one figure computed here.** It is the configured chart's `OPENING_BALANCE` for the cash account
plus the posted movements on that account taken from the ledger workspace - because the ledger service reports a period's
trial balance, not a position across periods.

**Not built, and stated as gaps:** no bank reconciliation and no unpresented-cheque adjustment; the dashboard re-reads each
whole register on every refresh (the services' own `LoadAsync`), so cost grows with the registers; attention is capped at 25
rows with no count of the remainder; the queues are fixed in code and an operator cannot add, reorder or hide one; there is no
drill-through from a queue row, so the link is the navigation menu; and the dashboard writes nothing, so it has no audit
trail or segregation-of-duties rule of its own.

## 9. Open questions and risks

- The Web accounting dashboard is a "supplier-invoice source queue" rather than a financial
  dashboard. This feature defines the desktop as a true KPI dashboard; record the divergence.
