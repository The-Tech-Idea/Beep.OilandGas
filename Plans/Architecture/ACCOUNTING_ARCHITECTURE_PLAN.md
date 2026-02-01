# Beep.OilandGas Accounting - Architecture Plan

## Executive Summary

**Goal**: Provide the core accounting foundation (GL, AP, AR, Inventory, Reporting, Period Close) that underpins production accounting and all financial workflows.

**Key Principle**: Use **PPDM-aligned Data Classes** as the authoritative ledger and subledger records, with strict double-entry controls and auditability.

**Scope**: General ledger, AP/AR, inventory, period close, IFRS/GAAP reporting, and shared financial controls.

---

## Architecture Principles

### 1) Financial Integrity
- Enforce double-entry posting: debits must equal credits.
- Only posted entries affect balances.
- Full audit trail on every financial change.

### 2) Standards Alignment
- Support IFRS and GAAP reporting requirements.
- Preserve policy changes and corrections with effective dates.

### 3) Shared Core Services
- Core accounting services are reused by ProductionAccounting.
- Standard invoice flow via `InvoiceService`.

### 4) PPDM39 Alignment
- Data entities are ALL_CAPS with audit columns.
- Reference tables enforce consistent code sets.

---

## Target Project Structure

```
Beep.OilandGas.Accounting/
├── Services/
│   ├── GLAccountService.cs
│   ├── JournalEntryService.cs
│   ├── TrialBalanceService.cs
│   ├── PeriodClosingService.cs
│   ├── APInvoiceService.cs
│   ├── APPaymentService.cs
│   ├── ARService.cs
│   ├── InventoryService.cs
│   ├── InvoiceService.cs
│   ├── ReconciliationService.cs
│   └── CashFlowService.cs
├── Constants/
│   └── DefaultGlAccounts.cs
├── Validation/
│   ├── JournalEntryValidator.cs
│   ├── InvoiceValidator.cs
│   └── PeriodCloseValidator.cs
└── Exceptions/
    ├── AccountingException.cs
    └── PostingException.cs
```

---

## Data Model Requirements (PPDM-Aligned)

Create/verify these entities in `Beep.OilandGas.Models.Data.Accounting`:

### General Ledger
- GL_ACCOUNT
- JOURNAL_ENTRY
- JOURNAL_ENTRY_LINE
- GL_ENTRY
- ACCOUNT_BALANCE

### AP / AR
- AP_INVOICE
- AP_PAYMENT
- AR_INVOICE
- AR_PAYMENT
- INVOICE
- INVOICE_LINE

### Inventory
- INVENTORY_ITEM
- INVENTORY_TRANSACTION
- INVENTORY_VALUATION

### Period Close + Reporting
- PERIOD_CLOSE
- TRIAL_BALANCE
- CASH_FLOW_STATEMENT
- FINANCIAL_STATEMENT

### Policies + Adjustments
- ACCOUNTING_POLICY
- POLICY_CHANGE
- ERROR_CORRECTION
- ADJUSTING_ENTRY

---

## Service Interface Standards

```csharp
public interface IJournalEntryService
{
    Task<JOURNAL_ENTRY> CreateEntryAsync(JOURNAL_ENTRY entry, string userId);
    Task<bool> PostEntryAsync(string entryId, string userId);
    Task<JOURNAL_ENTRY?> GetEntryAsync(string entryId);
}
```

---

## Implementation Phases

### Phase 1: Core Ledger (Week 1)
- GL accounts, journal entries, and posting logic.
- Trial balance validation.

### Phase 2: Subledgers (Week 2)
- AP/AR services and invoice workflow.
- Inventory transactions and valuation.

### Phase 3: Close + Reporting (Week 3)
- Period close workflow and financial statements.

### Phase 4: Policies + Compliance (Week 4)
- Policy changes, corrections, and audit reporting.

---

## Best Practices Embedded

- **Double-entry enforcement**: no unbalanced postings.
- **Auditability**: all actions tracked by user and timestamp.
- **Standardized invoices**: single invoice flow used across modules.

---

## API Endpoint Sketch

```
/api/accounting/
├── /journal-entries
│   ├── POST
│   ├── POST /{id}/post
│   └── GET /{id}
├── /ap
│   ├── POST /invoices
│   └── POST /payments
├── /ar
│   ├── POST /invoices
│   └── POST /payments
└── /close
    └── POST /period/{periodId}
```

---

## Success Criteria

- Double-entry integrity enforced on all postings.
- Core accounting services reusable by ProductionAccounting.
- IFRS/GAAP reporting workflows are auditable and repeatable.

---

**Document Version**: 1.0  
**Last Updated**: February 2026  
**Status**: Draft (Phase 1 ready)
