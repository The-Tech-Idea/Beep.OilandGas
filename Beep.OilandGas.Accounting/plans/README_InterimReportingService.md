# InterimReportingService

## Overview

The `InterimReportingService` supports **IAS 34** interim financial reporting. It orchestrates the generation of a condensed set of financial statements for an interim period.

## Key Functionality

- **Package Generation**: Generates Income Statement, Balance Sheet, and Cash Flow Statement for the interim period.
- **Disclosure List**: Returns a list of required disclosures for the interim report.

## Key Methods

### `BuildInterimPackageAsync`
Generates the full interim reporting package.

## Dependencies
- `FinancialStatementService`
- `ACCOUNTING_COST` (to log the report generation event)
