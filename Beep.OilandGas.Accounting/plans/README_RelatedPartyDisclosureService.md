# RelatedPartyDisclosureService

## Overview

The `RelatedPartyDisclosureService` aggregates data for **IAS 24** related party disclosures. It scans various transaction tables (Invoices, Leases) to summarize transactions with identified related parties.

## Key Functionality

- **Data Aggregation**: Sums up AR invoices, AP invoices, and Lease counts for specified Related Party Business Entities.

## Key Methods

### `GenerateDisclosureAsync`
Produces a text summary of transactions with a list of related parties.

## Dependencies
- `AR_INVOICE`, `AP_INVOICE`, `INVOICE`, `LEASE_CONTRACT` tables.
