# InvestmentDisclosureService

## Overview

The `InvestmentDisclosureService` facilitates **IFRS 12** disclosures of interests in other entities (subsidiaries, joint arrangements, associates, etc.).

## Key Functionality

- **Disclosure Recording**: Logs qualitative and quantitative disclosures regarding interests in other entities.
  - Stored in `ACCOUNTING_COST` with `COST_TYPE = 'IFRS12_DISCLOSURE'`.

## Key Methods

### `RecordDisclosureAsync`
Records a disclosure item.

## Dependencies
- `ACCOUNTING_COST` table
