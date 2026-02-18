# AccountingPolicyService

## Overview

The `AccountingPolicyService` manages **IAS 8** accounting policies. It provides a formal mechanism for defining, updating, and versioning accounting methods and policies.

## Key Functionality

- **Policy Management**: effective dating of policies (Active/Expired).
- **Audit Trail**: Tracks who changed a policy and why.
- **Retrieval**: Fetches the currently active policy for a given method type.

## Key Methods

### `RecordPolicyChangeAsync`
Retires the old policy and creates a new one.

### `GetActivePolicyAsync`
Retrieves the current policy.

## Dependencies
- `ACCOUNTING_METHOD` table
