# ACC-03 — Revenue recognition and sales invoices

| Field | Value |
|---|---|
| Stage | STG-10 — Production accounting and financials |
| Area | Accounting |
| Status | Gate met - see [evidence](../../evidence/STG-10-ACC-03/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.ProductionAccounting` |
| Desktop project | `Modules/Accounting/Revenue/` |
| External packages | none |
| Depends on | ACC-02, PRD-05 |
| Blocks | ACC-04, ACC-07, ECO-01 |
| Web reference | `Pages/PPDM39/Accounting/RevenueRecognition.razor`, `SalesInvoices.razor`, `SalesInvoiceReview.razor`, `RevenueWorkbench.razor`, plus `Components/Workflow/Sales*Workbench.razor` |

## 1. Purpose

Recognise revenue from sold volumes, raise sales invoices, issue credit notes, and record the
ledger effect — traceable from invoice line back to the volume that produced it.

## 2. Source components

| Source | Reuse |
|---|---|
| `ProductionAccounting/Services/RevenueService.cs` | recognition |
| `ProductionAccounting/Services/ReserveAccountingService.cs` | overlift/underlift |
| `Accounting/Services/InvoiceService.cs` (+5), `ARService.cs` (+6) | invoicing |
| `Accounting/Services/PricingService` / `ProductionAccounting/Services/PricingService.cs` | pricing |
| `Components/Workflow/SalesInvoiceWorkbench.razor`, `…Queue`, `…ReviewWorkbench`, `…JournalPostingWorkbench`, `…JournalReviewWorkbench`, `…PostedLedgerPanel` | behaviour |

## 3. Screens

`Accounting → Revenue`

- Workbench: revenue source queue (sold volumes awaiting invoicing).
- Invoice editor: buyer, contract, commodity, quantity (+unit), price, tax, terms; lines linked to
  volume records.
- Issue: assigns the number (server-owned), posts to the ledger, shows the journal reference.
- Credit note: amount, reason, resulting balance.
- Posted ledger panel: the journal behind an invoice.
- Review: reviewer approves or returns with a reason.

## 4. Data and contracts

`OgSalesInvoice { Id, Number, BuyerId, Date, DueDate, Currency, Lines[], Subtotal, Tax, Total,
BalanceDue, JournalId, State }`
`OgCreditNote { InvoiceId, Amount, Currency, Reason, State }`.

## 5. States

| State | Behaviour |
|---|---|
| Volume without a price | invoice line blocked with the reason |
| Currency mismatch with contract | refused |
| Issue acknowledgement lost | retained original; no second journal |
| Review return | reason shown on the offending line |
| Invoice with payments | deletion refused |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Build invoice | `accounting.revenue.write` | yes |
| Issue | `accounting.revenue.issue` | yes |
| Credit note | `accounting.revenue.credit` | yes |
| Review | `accounting.revenue.approve` + SoD | yes |

## 7. Acceptance criteria

- AC-1 A credit note reduces the balance by exactly its amount.
- AC-2 Issuing an invoice creates exactly one journal entry.
- AC-3 The invoice line traces to a volume record.
- AC-4 Author cannot approve their own invoice.
- AC-5 Tax is computed by the owning service, not the view.
- AC-6 A lost acknowledgement does not duplicate the journal.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ACC-03"
```

## 9. Open questions and risks

- The Web has six sales components; only four behaviours are needed. Map them explicitly here.

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-10-ACC-03/observed-evidence.json). Invoices, lines and credit memos are
read and written through `UnitofWork<T>` over the ProductionAccounting AR tables; the invoice journal is written to the
ledger's `JOURNAL_ENTRY`/`JOURNAL_ENTRY_LINE`.

**The open question, answered - four of the six behaviours.** This feature reproduces the **queue/editor** (build a draft
from priced lines), the **issue** (number, journal, ledger reference), the **credit note** (amount, reason, resulting
balance) and the **review** (approve or return with a reason). The journal-posting *workbench* and the posted-ledger
*panel* of the Web are not reproduced as separate screens: the journal is created by the issue itself and its reference and
dates are shown on the invoice, so a second posting screen would have had nothing left to do.

**A table had to be added.** The AR model had a header and a credit memo but **no invoice lines**, so AC-3 (a line traces to
a volume) and the per-line tax had nowhere to live. `AR_INVOICE_LINE` is that table, and `VOLUME_REFERENCE` is the link.

**Tax is the owning project's, because it did not exist there.** `Beep.OilandGas.Accounting` has income-tax provision and
deferral records but no sales tax on an invoice line, so `SalesInvoiceTax` was added: net, tax rounded to the currency's
minor unit **away from zero**, line total, and the invoice totals from the lines. The screen computes nothing (AC-5).

**One journal, and a retry that cannot make a second.** The journal's `REFERENCE_NUMBER` is `SALES-<invoice id>` - a key
that exists *before* the invoice is numbered - so if the acknowledgement is lost after the journal is written but before the
invoice is updated, the next issue finds that reference and adopts the entry. That is why the test can clear
`JOURNAL_ENTRY_ID` by hand, re-issue, and still see exactly one journal.

**A credit note reduces the balance by exactly its amount (SG-10-5).** The stored balance is recomputed as total less paid
less credited in decimal, the credit is refused when it exceeds what is outstanding, and a credit against a draft is
refused because a draft is not yet receivable.

**Not built, and stated as a gap:** a credit note does **not** post its own journal - it reduces the balance and the
receivable figure, but the revenue and tax reversal is left to the accounting close. Also absent: a sold-volume table (the
volume reference is a stated id), a `SALES_CONTRACT` currency lookup (the contract currency is stated on the screen),
payments (ACC-04), and tax codes or multi-rate invoices.
