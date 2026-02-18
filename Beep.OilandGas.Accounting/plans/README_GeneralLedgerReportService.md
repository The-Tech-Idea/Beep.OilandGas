# GeneralLedgerReportService

## Overview

The `GeneralLedgerReportService` provides comprehensive reporting capabilities for the General Ledger. It allows users to view detailed transaction histories or high-level summaries.

## Reports

### `GL Detail Report`
- Lists every transaction (Journal Entry Line) for a specific account.
- Shows Date, Reference, Description, Debit, Credit, and Running Balance.
- Essential for auditing specific accounts.

### `GL Summary Report`
- Lists all accounts with their current balances.
- Segregates by Account Type (Asset, Liability, etc.).
- Verifies if the GL is balanced (Total Debits = Total Credits).

## Key Methods

### `GenerateGLDetailReportAsync`
Fetches and formats line-item details for an account.

### `GenerateGLSummaryReportAsync`
Produces a trial-balance-like summary of all accounts.

## Exports
- Methods exist to export reports as formatted text strings (useful for logs or simple displays).
