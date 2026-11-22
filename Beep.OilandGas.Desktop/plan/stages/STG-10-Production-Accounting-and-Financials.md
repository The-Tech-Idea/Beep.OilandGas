# STG-10 — Production accounting and financials

| Field | Value |
|---|---|
| Stage | STG-10 |
| Features | 8 (ACC-01 … ACC-08) |
| Depends on | STG-05, STG-09 |
| Status | Gate met - ACC-01..ACC-08 gate met ([ACC-01](../evidence/STG-10-ACC-01/observed-evidence.json), [ACC-02](../evidence/STG-10-ACC-02/observed-evidence.json), [ACC-03](../evidence/STG-10-ACC-03/observed-evidence.json), [ACC-04](../evidence/STG-10-ACC-04/observed-evidence.json), [ACC-05](../evidence/STG-10-ACC-05/observed-evidence.json), [ACC-06](../evidence/STG-10-ACC-06/observed-evidence.json), [ACC-07](../evidence/STG-10-ACC-07/observed-evidence.json), [ACC-08](../evidence/STG-10-ACC-08/observed-evidence.json)); exit-gate items SG-10-1..SG-10-9 met, with **SG-10-9** proven separately ([gate](../evidence/STG-10-GATE/observed-evidence.json)). |
| Exit gate | A period can be closed with balanced journals, revenue recognised, royalties and JIB posted, and tax provision calculated |

---

## 1. Objective

The financial close surface. Eight features covering the dashboard, period close and ledger,
revenue and sales invoicing, receivables, payables and procurement, warehouse and stock,
royalties / JIB / cost allocation, and tax.

This is the highest-risk stage because it writes money. Every command follows ADR-010.

---

## 2. Scope

**In**

- Accounting dashboard as a source queue over supplier invoices and process instances.
- Period close: checklist, blocking issues, close and reopen with reason.
- Ledger accounts, journals, trial balance, financial statements.
- Revenue recognition, sales invoices, credit notes, receivables, payments.
- Payables, purchase orders, receipts, three-way match, payments.
- Warehouse: items, receipts, stock intents, supplier returns.
- Royalties, joint interest billing, cost allocation, COPAS overhead.
- Tax provision and deferred tax.

**Out**

- External ERP integration (none exists).
- Bank connectivity (bank reconciliation record only, no import).

---

## 3. Source projects consumed

| Source | Used for |
|---|---|
| `Beep.OilandGas.Accounting` | ~120 services: `GLAccountService`, `JournalEntryService`, `ARService`, `APInvoiceService`, `APPaymentService`, `InvoiceService`, `PeriodClosingService`, `PurchaseOrderService`, `InventoryService`, `CostCenterService`, `TrialBalanceService`, `FinancialStatementService`, `TaxProvisionService`, `FixedAssetLifecycleService`, `LeaseAccountingService`, `AnomalyDetectionService` |
| `Beep.OilandGas.ProductionAccounting` | `IRevenueService`, `IRoyaltyService`, `RoyaltyDisputeService`, `JointInterestBillingService`, `CostAllocationService`, `ProductionTaxService`, `PricingService`, `ImbalanceService`, `CopasOverheadService`, `MeasurementService`, `InventoryLcmService` |
| `Beep.OilandGas.LifeCycle` | `PPDMAccountingService`, `AccountingModuleSetup` (`ACCOUNTING`), ACCT_* process definitions |
| `Beep.OilandGas.Repository` | `FinancialOperationClaimStore` for segregation-of-duties |
| `Beep.OilandGas.Client` | `.Accounting` |

---

## 4. Features

| ID | Feature | Document |
|---|---|---|
| ACC-01 | Accounting dashboard | [ACC-01](../features/ACC/ACC-01-Accounting-Dashboard.md) |
| ACC-02 | Period close and ledger | [ACC-02](../features/ACC/ACC-02-Period-Close-and-Ledger.md) |
| ACC-03 | Revenue recognition and sales invoices | [ACC-03](../features/ACC/ACC-03-Revenue-Recognition-and-Sales-Invoices.md) |
| ACC-04 | Sales receivables and payments | [ACC-04](../features/ACC/ACC-04-Sales-Receivables-and-Payments.md) |
| ACC-05 | Payables and procurement | [ACC-05](../features/ACC/ACC-05-Payables-and-Procurement.md) |
| ACC-06 | Warehouse, receipt stock and returns | [ACC-06](../features/ACC/ACC-06-Warehouse-Receipt-Stock-and-Returns.md) |
| ACC-07 | Royalties, joint interest billing and cost allocation | [ACC-07](../features/ACC/ACC-07-Royalties-JIB-and-Cost-Allocation.md) |
| ACC-08 | Tax workbench | [ACC-08](../features/ACC/ACC-08-Tax-Workbench.md) |

---

## 5. New code introduced

`.Modules/Accounting` — eight pairs. `.Controls` gains `MoneyCell` (right-aligned, decimal,
invariant parsing), `BalanceBadge` (debit = credit), `PeriodPicker`, `ApprovalBar`.

---

## 6. Architecture notes

- **Segregation of duties is enforced.** The journal author cannot approve; the payment creator
  cannot post. `FinancialOperationClaimStore` and `SodConflictDetector` provide the facts;
  the view model must check them before enabling a command.
- **Balanced before posted.** A journal that does not balance is refused client-side *and* the
  service refusal is surfaced, not overwritten.
- **Retained-original on every posting** (ADR-010). A lost posting acknowledgement must never
  produce a second entry.
- **Period lock is checked before any write.** A write into a closed period is refused with the
  period name.
- **Money never passes through `double`.** Use `decimal` end to end, including JSON mapping.

---

## 7. Dependencies and sequencing

ACC-02 → ACC-03 → ACC-04. ACC-05 → ACC-06. ACC-07 needs ACC-03. ACC-08 needs ACC-02.
ACC-01 needs the others to have something to show, but its shell can be built first.

---

## 8. Exit gate

| ID | Gate item | Evidence |
|---|---|---|
| SG-10-1 | An unbalanced journal is refused with the exact imbalance | test `ACC-02/T03` |
| SG-10-2 | A duplicate posting attempt does not create a second entry | test `ACC-02/T07` |
| SG-10-3 | A write into a closed period is refused and names the period | test `ACC-02/T05` |
| SG-10-4 | Author cannot approve their own journal | test `ACC-02/T09` |
| SG-10-5 | Credit note reduces the invoice balance by exactly the credit amount | test `ACC-03/T05` |
| SG-10-6 | Three-way match refuses a receipt quantity above the order | test `ACC-05/T04` |
| SG-10-7 | Royalty and JIB post to the configured accounts and are traceable to the source volume | test `ACC-07/T03` |
| SG-10-8 | Tax provision refuses when the fiscal year is incomplete | test `ACC-08/T02` |
| SG-10-9 | All amounts round-trip through persistence without precision loss | test `STG-10/T09` |

---

## 9. Test plan

`ACC-01/T02`, `ACC-02/T03`, `ACC-02/T05`, `ACC-02/T07`, `ACC-02/T09`, `ACC-03/T05`,
`ACC-04/T02`, `ACC-05/T04`, `ACC-06/T03`, `ACC-07/T03`, `ACC-08/T02`, `STG-10/T09`.

---

## 10. Risks and open questions

| Risk | Mitigation |
|---|---|
| ~120 services with overlapping invoice concepts (`InvoiceService`, `ARService`, `APInvoiceService`) | Map each to exactly one feature tab; no view model calls two invoice services |
| Web has 23 accounting pages with several "workbench shells" | Consolidate into eight features and record the route mapping |
| Reconciliation against bank statements has no import | State as a limitation |
| SoD may be advisory in the service | Enforce in the view model and record the refusal |
