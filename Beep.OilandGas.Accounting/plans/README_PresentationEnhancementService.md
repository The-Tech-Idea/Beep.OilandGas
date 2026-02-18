# PresentationEnhancementService

## Overview

The `PresentationEnhancementService` supports the upcoming **IFRS 18** standard (Presentation and Disclosure in Financial Statements). It focuses on new categories (Operating, Investing, Financing) and Management-defined Performance Measures (MPMs).

## Key Functionality

- **IFRS 18 Package**: Generates a presentation package tailored to IFRS 18 requirements.
- **Disclosure Recording**: Logs the generation of IFRS 18 compliant reports.

## Key Methods

### `BuildIFRS18PackageAsync`
Builds a financial reporting package with IFRS 18 specific structures.

## Dependencies
- `FinancialStatementService`
- `ACCOUNTING_COST` table
