# JournalEntryService

## Overview

The `JournalEntryService` is the core engine for recording financial transactions. It ensures the integrity of the General Ledger by enforcing double-entry accounting rules.

## Responsibilities

- **Entry Creation**: Creates `JOURNAL_ENTRY` headers and `JOURNAL_ENTRY_LINE` items.
- **Validation**: Enforces `Total Debits = Total Credits` (Balance Tolerance: 0.01%).
- **Posting**: Commits DRAFT entries to POSTED status and generates immutable `GL_ENTRY` records.
- **Reversal**: Creates reversing entries for corrections.

## Key workflows

### Creating a Manual Entry
1.  **Stage**: Call `CreateEntryAsync` with a list of lines. Status = `DRAFT`.
2.  **Review**: (Optional) User reviews the draft.
3.  **Post**: Call `PostEntryAsync`. Status -> `POSTED`. `GL_ENTRY` rows are created.

### Creating a System Entry (Quick Post)
Use `CreateBalancedEntryAsync` to create and post a simple 2-line entry in one step.

### Reversing an Entry
Call `ReverseEntryAsync`. This:
1.  Finds the original entry.
2.  Creates a new entry with swapped Debits and Credits.
3.  Links the new entry to the old one via Reference Number.
4.  Updates original entry status to `REVERSED`.

## Data Model
- **JOURNAL_ENTRY**: The document header (Date, Description, Status, Book ID).
- **JOURNAL_ENTRY_LINE**: The detail lines (Account, Debit, Credit).
- **GL_ENTRY**: The posted impact on account balances (derived from lines upon posting).
