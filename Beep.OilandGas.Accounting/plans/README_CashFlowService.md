# CashFlowService

## Overview

The `CashFlowService` generates the **Statement of Cash Flows** (IAS 7). unlike the P&L or Balance Sheet which are often snapshots or simple aggregations, the Cash Flow statement requires analyzing the *nature* of changes in cash.

## Methodology

Currently implements a **Direct/Indirect hybrid approach** by analyzing GL entries:
1.  Identifies all Journal Entries affecting `Cash`.
2.  Analyzes the *offsetting* legs of those entries to determine the "Reason" for cash movement.
    - Offset to `Revenue/Expense` -> **Operating**
    - Offset to `Asset` -> **Investing**
    - Offset to `Liability/Equity` -> **Financing**

## Key Methods

### `GenerateCashFlowAsync`
Generates the statement for a given period.
*   Returns `Operating`, `Investing`, and `Financing` cash flow totals.
*   Provides a breakdown of line items.

## Logic Details
*   It assumes `GL_ENTRY` contains the source of truth.
*   It uses `IAccountMappingService` (or fallback) to identify the Cash account to analyze.
