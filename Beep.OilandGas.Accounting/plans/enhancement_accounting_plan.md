# Accounting System Enhancement Plan

This document outlines the strategic roadmap for further strengthening the `Beep.OilandGas.Accounting` system. Having established a comprehensive baseline of services covering major IFRS and US GAAP standards, the next phase focuses on automation, integration, and advanced analytics.

## Completed / Implemented (Phase 1)

### 1.4 Dynamic GL Account Mapping (Completed)
**Goal**: Eliminate hardcoded GL account constants (`DefaultGlAccounts.cs`) to support multi-client configurations.
- **Action**: Refactor `AccountMappingService`.
- **Status**: **Implemented**.
- **Features**:
  - **Database Storage**: Mappings stored in `GL_ACCOUNT_MAPPING`.
  - **Seeding**: `SeedDefaultMappingsAsync` populates defaults on startup.
  - **GL Account Generation**: `GenerateDefaultAccountsAsync` creates ~50 standard physical accounts if missing.
  - **Deprecation**: `DefaultGlAccounts.cs` marked as `[Obsolete]`.

## Phase 1: Deep Integration & Automation (Months 1-3)

### 1.1 Automated Period Close Workflow
**Goal**: Reduce the manual effort required to close financial periods.
- **Action**: Enhance `PeriodClosingService` to function as a workflow engine.
- **Features**:
  - Dependency mapping (e.g., AP must close before Inventory).
  - Automated validation checks (Trial Balance = 0).
  - "Soft Close" vs "Hard Close" flags.
  - Automatic accrual reversals for the next period.

### 1.2 Subledger-to-GL Reconciliation Dashboard
**Goal**: Provide real-time visibility into data integrity.
- **Action**: Build a UI dashboard on top of `ReconciliationService`.
- **Features**:
  - Red/Green status indicators for AR, AP, Inventory, and Fixed Assets.
  - Drill-down capability to identify specific variance transactions.
  - Automated daily alerts for out-of-balance conditions.

### 1.3 Multi-Book Accounting Engine
**Goal**: Seamlessly handle IFRS/GAAP/Tax differences.
- **Action**: Refine `AccountingBasisPostingService`.
- **Features**:
  - "Ledger Groups" configuration (e.g., "Statutory", "Management", "Tax").
  - Automated rule-based posting (e.g., if booked to IFRS 16 lease, auto-book ASC 842 entry to GAAP book).
  - Delta reporting (System generates "IFRS to GAAP Reconciliation" report automatically).

### 2.1 Lease Accounting Engine (IFRS 16 / ASC 842)
**Goal**: Full lifecycle management for complex leases.
- **Action**: Expand `LeaseAccountingService`.
- **Features**:
  - Automatic remeasurement upon index rate changes (CPI updates).
  - Modification accounting wizard (extensions, terminations).
  - Disclosure generation (Maturity analysis tables).

### 2.2 Revenue Recognition Wizard (IFRS 15 / ASC 606)
**Goal**: Simplify the 5-step model for non-accountants.
- **Action**: Enhance `PerformanceObligationService`.
- **Features**:
  - Contract combination logic.
  - Standalone Selling Price (SSP) calculator (Residual approach, Cost plus margin).
  - Variable consideration estimation (Expected value vs Most likely amount).

### 2.3 Tax Provisioning Automation (IAS 12)
**Goal**: Automate the bridge between Book and Tax.
- **Action**: Upgrade `TaxProvisionService`.
- **Features**:
  - Automated temporary difference tracking (e.g., CCA vs Depreciation).
  - ETR (Effective Tax Rate) reconciliation report.
  - Return-to-provision adjustments.

## Phase 3: Analytics & Reporting (Months 7+)

### 3.1 Financial Data Warehouse
**Goal**: High-performance reporting.
- **Action**: Create a denormalized reporting schema.
- **Features**:
  - Pre-aggregated cubes for "Revenue by Region", "Cost by Well".
  - Fast generation of Financial Statements (under 1 second).

### 3.2 AI-Assisted Anomaly Detection
**Goal**: Proactive fraud and error detection.
- **Action**: Implement ML models on `JOURNAL_ENTRY` data.
- **Features**:
  - "Unusual Posting Time" alerts (e.g., manuals entries on Sunday at 2 AM).
  - "Benford's Law" analysis on invoice amounts.
  - "Rare Account Combination" detection.

## Priorities for Immediate Attention

1.  **Unit Test Coverage**: The service layer is robust, but comprehensive unit tests are critical for an accounting system.
    - *Target*: 90% coverage for all "Calculation" methods (Interest, Depreciation, Tax).
2.  **Audit Log Viewer**: A frontend component to visualize the `ROW_CREATED_BY` / `ROW_CHANGED_DATE` trail is essential for compliance.
3.  **Permissions System**: Granular locking (e.g., "Can Post Journal" vs "Can Approve Journal").

## Technical Debt & Maintenance

- **Nuget Dependencies**: Regular updates to PPDM core libraries.
- **Performance Tuning**: Index optimization on `ACCOUNTING_COST` and `JOURNAL_ENTRY` tables, specifically on `COST_DATE` and `FINANCE_ID` columns.
