# SeparateFinancialStatementService

## Overview

The `SeparateFinancialStatementService` supports **IAS 27** separate financial statements. It is used when an entity elects or is required to present separate financial statements (in addition to consolidated ones).

## Key Functionality

- **Statement Generation**: Wraps `PresentationService` to build a package specifically labeled as "Separate Statements".
- **Recording**: Logs the generation event for audit trails.

## Key Methods

### `BuildSeparateStatementsAsync`
Generates the separate financial statement package.

## Dependencies
- `PresentationService`
