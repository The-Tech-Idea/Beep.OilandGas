# EarningsPerShareService

## Overview

The `EarningsPerShareService` performs the calculation and disclosure of Basic and Diluted Earnings Per Share (EPS) in accordance with **IAS 33**.

## Key Functionality

- **Basic EPS**: Net Income / Weighted Average Shares Outstanding.
- **Diluted EPS**: Adjusts the denominator for all potential dilutive ordinary shares (e.g., stock options, convertible bonds).

## Key Methods

### `RecordEarningsPerShareAsync`
Calculates both Basic and Diluted EPS and records the disclosure.

## Dependencies
- PPDM39 Repository (for storage of the calculated record)
