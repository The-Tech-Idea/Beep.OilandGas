# PresentationService

## Overview

The `PresentationService` is the core engine for **IAS 1** financial statement presentation. It aggregates the Income Statement, Balance Sheet, and Cash Flow Statement into a cohesive reporting package.

## Key Functionality

- **Package Assembly**: Combines generated financial statements into a single `PresentationPackage` object.
- **Disclosure Formatting**: detailed list of required disclosures (currently returned as a static string list for checking compliance).

## Key Methods

### `BuildPresentationPackageAsync`
Generates the full financial statement package.

## Dependencies
- `FinancialStatementService`
