# ACC-05 — Payables and procurement

| Field | Value |
|---|---|
| Stage | STG-10 — Production accounting and financials |
| Area | Accounting |
| Status | Gate met - see [evidence](../../evidence/STG-10-ACC-05/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.Accounting` |
| Desktop project | `Modules/Accounting/Payables/` |
| External packages | none |
| Depends on | ACC-02 |
| Blocks | ACC-06, ECO-02 |
| Web reference | `Pages/PPDM39/Accounting/PayablesWorkbench.razor`, `ProcurementWorkbench.razor`, plus `Components/Workflow/WorkOrder*Cost`, `…Material*`, `…SourceReview` panels |

## 1. Purpose

Buy and pay correctly: purchase order, receipt, invoice, three-way match, approval, payment — with
the commitment visible against the AFE.

## 2. Source components

| Source | Reuse |
|---|---|
| `Accounting/Services/PurchaseOrderService.cs` (+6 partials) | ordering |
| `Accounting/Services/APInvoiceService.cs` (+5) | invoice capture |
| `Accounting/Services/APPaymentService.cs` | payment |
| `Accounting/Services/InventoryService.cs` | receipt linkage |
| `ProductionAccounting/Services/AfeService.cs` | commitment |
| `Components/BusinessProcess/APInvoiceCorrectionEditor.razor`, `…CorrectionDecisionPanel`, `…SourcePanel` | correction behaviour |

## 3. Screens

`Accounting → Payables`

Tabs: Orders · Receipts · Invoices · Match · Payments · Commitments.

- Order editor: vendor, lines, quantities, prices, delivery, AFE reference, approvals.
- Invoice capture: vendor invoice, date, amounts, tax, PO reference, attachments.
- Three-way match: order vs receipt vs invoice with tolerances and exceptions.
- Correction: for a mismatched invoice, a correction editor with a decision panel.
- Payment run: select due invoices, propose a run, approve, post.

## 4. Data and contracts

`OgPurchaseOrder { Number, VendorId, Lines[], Total, Currency, AfeRef, State }`
`OgApInvoice { Id, VendorId, Number, Date, Amount, Tax, Currency, PoRef, MatchState }`
`OgMatchResult { Matched, Exceptions[] { Kind, Ordered, Received, Invoiced, Tolerance } }`.

## 5. States

| State | Behaviour |
|---|---|
| Quantity above order | refused by match |
| Price outside tolerance | exception requiring approval |
| Duplicate invoice number | refused |
| AFE over commitment | blocked unless a supplement exists |
| Lost acknowledgement | retained original |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| Raise order | `accounting.payable.write` | yes |
| Capture invoice | `accounting.payable.write` | yes |
| Approve match exception | `accounting.payable.approve` + SoD | yes |
| Payment run | `accounting.payable.pay` + SoD | yes |

## 7. Acceptance criteria

- AC-1 Three-way match refuses a receipt quantity above the order.
- AC-2 A price outside tolerance becomes an exception, not a silent pass.
- AC-3 A duplicate invoice number is refused.
- AC-4 A commitment above the AFE remaining is refused.
- AC-5 Payments create exactly one journal entry.
- AC-6 Author cannot approve their own exception or payment.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~ACC-05"
```

---

## Implementation notes

Evidence: [observed-evidence.json](../../evidence/STG-10-ACC-05/observed-evidence.json). Orders, receipts, supplier invoices
and payments are read and written through `UnitofWork<T>` over the ProductionAccounting tables; the AFE commitment is the
same `AFE_COMMITMENT` row ECO-02 writes, and the payment journal goes to the ledger's `JOURNAL_ENTRY`/`JOURNAL_ENTRY_LINE`.

**The open question, answered.** The six purchase-order partials and five AP partials do **not** map one-to-one onto the six
tabs. They are workflow engines (inventory links, source reviews, receipt allocations, correction decisions) built for the
company-owned process; what the desk needs from them is the *records* they write, which are all present and registered. So
the tabs are built over the tables, and the partials are read for behaviour rather than reproduced:

| Tab | Owner records used |
|---|---|
| Orders | `PURCHASE_ORDER`, `PO_LINE_ITEM` |
| Receipts | `PO_RECEIPT` (accepted quantity is what counts; rejected is never invoiced) |
| Invoices | `AP_INVOICE`, `AP_INVOICE_LINE` |
| Match | both, plus `PO_RECEIPT` and the `ACCOUNTING_METHOD` policy |
| Payments | `AP_PAYMENT`, and the new `AP_PAYMENT_RUN` header |
| Commitments | `AFE` and `AFE_COMMITMENT` |

**The tolerances were already recorded - in a shape that omits a third of the match.** The owner's
`CompanyMatchingPolicyRules` carries `QuantityTolerancePercent` and `PriceTolerancePercent`, and
`APInvoiceFinancialReview.MatchingPolicy` compares them with exact scaled-integer arithmetic, which `ThreeWayMatch` reuses so
no rounding decides a tolerance. But that policy compares the invoice against the **full ordered quantity** and says so: it
does not consider "accepted receipt capacity or consumption by other invoices". An invoice inside the order but above what
arrived would therefore have matched. This feature adds that ceiling and reports the two quantity exceptions separately, so
a reviewer can see which ceiling was passed.

**Exactly one policy must apply.** That is the owner's rule - "no newest-row fallback is applied" - so an overlapping or
missing policy is a configuration error the capture reports, naming whether it found none, several, or one that cannot be
read. A malformed row is reported through the failure reporter with a reference and skipped; a corrupt policy never silently
becomes an assumed tolerance.

**AC-5 needed a header.** `AP_PAYMENT` is one amount against one invoice, so a run settling several invoices had no single
record. `AP_PAYMENT_RUN` is that header, and its journal reference is `PAYRUN-<run id>` - a key that exists before any
numbering - so a retry after a lost acknowledgement adopts the entry, and an invoice already at a zero balance is skipped, so
the retry cannot pay it twice.

**AC-4 reuses ECO-02's commitment rows** rather than a second notion of commitment: the AFE's remaining is its approved
amount less the sum of `AFE_COMMITMENT`, and a capture writes one so the authorisation shows what the invoice commits.

**Not built, and stated as gaps:** the correction/variance cycle for a mismatched invoice (an exception is approved, not
corrected and re-issued); netting off quantities already invoiced by other invoices; mixed-currency or partial payments
inside a run; releasing an AFE commitment when an invoice is voided (there is no void); tax computed from codes rather than
stated; and amending an approved order.

## 9. Open questions and risks

- Six purchase-order partials plus five AP partials; map one partial to one tab before coding.
