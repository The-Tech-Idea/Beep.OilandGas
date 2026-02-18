# EventsAfterReportingService

## Overview

The `EventsAfterReportingService` manages **IAS 10** events after the reporting period. It distinguishes between adjusting events (those appearing to reflect conditions that existed at the balance sheet date) and non-adjusting events.

## Key Functionality

- **Event Recording**: Logs subsequent events.
- **Adjusting Events**: If marked as adjusting, posts a GL entry to reflect the financial impact.
- **Non-Adjusting Events**: Recorded for disclosure purposes only.

## Key Methods

### `RecordSubsequentEventAsync`
Records an event and optionally posts adjustments.

### `GetSubsequentEventsAsync`
Retrieves events for disclosure.

## Dependencies
- `AccountingBasisPostingService`
- `ACCOUNTING_COST` table (used for storage)
