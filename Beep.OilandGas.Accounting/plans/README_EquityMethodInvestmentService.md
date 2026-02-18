# EquityMethodInvestmentService

## Overview

The `EquityMethodInvestmentService` manages investments in associates and joint ventures using the equity method as prescribed by **IAS 28**.

## Key Functionality

- **Initial Recognition**: Records the investment at cost.
  - GL: Debit Investment in Associate/JV, Credit Cash.
- **Share of Earnings/Losses**: Adjusts the investment value based on the investor's share of the investee's profit or loss.
  - Profit: Debit Investment, Credit Equity Method Earnings.
  - Loss: Debit Equity Method Loss, Credit Investment.
- **Dividends**: Reduces the carrying amount of the investment upon receipt of dividends.
  - GL: Debit Cash, Credit Investment.

## Key Methods

### `RecordInvestmentAsync`
Records the initial investment.

### `RecordEquityMethodEarningsAsync`
Records share of profit/loss.

### `RecordDividendAsync`
Records dividend receipt.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
