# SubsidiaryDisclosureService

## Overview

The `SubsidiaryDisclosureService` captures disclosures required by **IFRS 19** (Subsidiaries without Public Accountability: Disclosures). It allows eligible subsidiaries to provide reduced disclosures.

## Key Functionality

- **Disclosure Recording**: Captures specific disclosure notes for subsidiaries.
  - Stored in `ACCOUNTING_COST` with `COST_TYPE = 'IFRS19_DISCLOSURE'`.

## Key Methods

### `RecordDisclosureAsync`
Records a disclosure note for a specific topic.

## Dependencies
- `ACCOUNTING_COST` table
