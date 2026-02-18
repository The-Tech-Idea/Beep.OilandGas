# RegulatoryDeferralService

## Overview

The `RegulatoryDeferralService` manages **IFRS 14** regulatory deferral accounts. It allows rate-regulated entities to continue recognizing regulatory deferral account balances upon adopting IFRS.

## Key Functionality

- **Recognition**: Records regulatory assets/liabilities and corresponding income/expense.
- **Amortization**: Amortizes the regulatory balances over time.

## Key Methods

### `RecordRegulatoryDeferralAsync`
Recognizes a new regulatory deferral balance.

### `AmortizeRegulatoryDeferralAsync`
Records amortization/reversal of a regulatory balance.

## Dependencies
- `AccountingBasisPostingService`
- `ACCOUNTING_COST` table
