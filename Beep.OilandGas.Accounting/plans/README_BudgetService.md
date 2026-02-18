# BudgetService

## Overview

The `BudgetService` enables financial planning and variance analysis. It allows users to define budgets for specific accounts and periods, then compares them against actual GL activity.

## Responsibilities

- **Planning**: Create/Edit Month/Quarter/Year budgets.
- **Reporting**: Generate "Budget vs Actual" reports.
- **Forecasting**: Predict year-end figures based on current run rates (Linear Trend, etc.).

## Key Methods

### `CreateBudgetAsync`
Defines a new budget with line items for specific GL accounts.

### `GenerateBudgetVarianceReportAsync`
Compares Actual vs Budget for a specific date.
*   Calculates Variance $ and %.
*   Flags items as `OVER_BUDGET` or `ON_TRACK`.

### `ForecastBudgetAsync`
Projects end-of-year results.
*   Uses YTD averages to extrapolate full-year performance.

## Models
- `Budget`
- `BudgetVarianceReport`
- `BudgetForecast`
