# AccountingBasisPostingService

## Overview

The `AccountingBasisPostingService` simplifies the process of creating and posting balanced journal entries across different accounting bases (IFRS, GAAP, or both). It acts as a facade over `JournalEntryService` and `IAccountMappingService`.

## Responsibilities

- **Dual-Basis Posting**: Create entries for IFRS, GAAP, or both simultaneously.
- **Key-Based Posting**: Use symbolic keys (e.g., "Revenue", "Cash") instead of hardcoded account numbers, relying on `IAccountMappingService` to resolve the actual GL Account ID.
- **Code Reuse**: Eliminates repetitive boilerplate for balanced entry creation.

## Dependencies

- `JournalEntryService`: Handles the actual journal entry creation and validation logic.
- `IAccountMappingService` (Optional): Resolves IFRS account keys.
- `IAccountMappingService` (Optional - for GAAP): Resolves GAAP account keys.

## Key Methods

### `PostBalancedEntryAsync`
Posts a balanced entry using keys.
```csharp
await service.PostBalancedEntryAsync(
    debitKey: AccountMappingKeys.Cash,
    creditKey: AccountMappingKeys.Revenue,
    amount: 1000m,
    description: "Sales Revenue",
    userId: "User1",
    basis: AccountingBasis.Both
);
```

### `PostBalancedEntryByAccountAsync`
Posts a balanced entry using direct account numbers (bypassing mapping).

### `PostEntryAsync`
Posts a complex multi-line entry for a specific basis.

## Usage Example

```csharp
var postingService = new AccountingBasisPostingService(journalEntryService, ifrsMapping, gaapMapping);

// Post a sale to both books
var result = await postingService.PostBalancedEntryAsync(
    "Cash",
    "Revenue",
    500m,
    "Daily Sales",
    "System",
    AccountingBasis.Both
);

// result.IfrsEntry -> Created IFRS entry
// result.GaapEntry -> Created GAAP entry
```
