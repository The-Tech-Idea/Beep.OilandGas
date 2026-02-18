# AccountMappingService

## Overview

The `AccountMappingService` facilitates **Dynamic GL Account Mapping**, allowing the system to resolve logical account keys (e.g., "Cash", "Revenue") to physical GL account numbers (e.g., "1000", "4001") stored in the database. 

This replaces the hardcoded `DefaultGlAccounts` class, enabling multi-client configurations without code changes.

## Key Features

- **Database-Driven**: Mappings are stored in the `GL_ACCOUNT_MAPPING` table.
- **Auto-Seeding**: On startup, if no mappings exist, the service automatically populates the table with standard defaults.
- **Caching**: Mappings are cached in memory for high performance.
- **Fail-Safe**: If the database is unreachable or keys are missing, it falls back to hardcoded defaults to prevent system crash.

## Configuration

To change a mapping (e.g., change Cash from 1000 to 1005):
1.  Update the `GL_ACCOUNT_NUMBER` in the `GL_ACCOUNT_MAPPING` table for `MAPPING_KEY = 'Cash'`.
2.  Restart the application (or re-initialize the service) to refresh the cache.

## Usage

```csharp
// Inject the service
public class MyService(IAccountMappingService mapping) 
{
    public void Post() 
    {
        // Resolve valid GL Account ID
        string cashAccountId = mapping.GetAccountId(AccountMappingKeys.Cash);
        
        // Use in journal entry...
    }
}
```

## Dependencies
- `GL_ACCOUNT_MAPPING` table
- `IDMEEditor` (for data access)
