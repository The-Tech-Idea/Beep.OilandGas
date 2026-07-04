# Phase 5 — Boundary Clarification

> **Status:** Not Started | **Depends on:** Phase 1 | **Est. Effort:** 1–2 weeks

---

## The Problem

Two projects handle "accounting":
- **`Beep.OilandGas.Accounting`** — General ledger, AP/AR, financial reporting (IFRS/GAAP)
- **`Beep.OilandGas.ProductionAccounting`** — O&G-specific: revenue, royalties, JIB, AFE, allocation

But the boundary is blurred:
1. `Beep.OilandGas.Accounting` has 78 services covering IFRS topics (Agriculture, InsuranceContracts) that may never be used
2. `Beep.OilandGas.ProductionAccounting` references `Accounting` for `AccountingBasisPostingService`, `DefaultGlAccounts`
3. `LifeCycle` has `PPDMAccountingService` that doesn't delegate to EITHER
4. `InventoryLcmService` exists in BOTH projects (duplicate name)

---

## Task Details

| ID | Task |
|----|------|
| A5-01 | Audit duplicate services between Accounting and ProductionAccounting |
| A5-02 | Define clear ownership document: Accounting = GL/AP/AR/Financial, ProductionAccounting = Revenue/Royalty/JIB/AFE/Allocation |
| A5-03 | Resolve `InventoryLcmService` duplication — keep one, remove other |
| A5-04 | Move O&G-specific services from Accounting to ProductionAccounting (if any) |
| A5-05 | Move generic financial services from ProductionAccounting to Accounting (if any) |
| A5-06 | Wire `PPDMAccountingService` to delegate to ProductionAccounting → Accounting chain |
| A5-07 | Document boundaries in `docs/ACCOUNTING_BOUNDARIES.md` |
