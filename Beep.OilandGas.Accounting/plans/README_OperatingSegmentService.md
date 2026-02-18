# OperatingSegmentService

## Overview

The `OperatingSegmentService` handles **IFRS 8** operating segments. It records key metrics (revenue, profit, assets) for identified reportable segments to support segment reporting disclosures.

## Key Functionality

- **Metric Recording**: Stores segment-specific financial data.
  - Stored in `ACCOUNTING_COST` with `COST_TYPE = 'SEGMENT_REPORT'`.

## Key Methods

### `RecordSegmentMetricAsync`
Records a specific metric for a segment.

## Dependencies
- `ACCOUNTING_COST` table
