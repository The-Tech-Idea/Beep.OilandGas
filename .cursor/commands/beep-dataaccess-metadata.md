# Metadata Management

## Overview

The metadata management system provides access to PPDM39 table metadata, including table definitions, column information, relationships, and subject area organization.

## Key Components

### PPDMMetadataRepository

Provides access to PPDM39 table metadata.

**Key Methods:**
- `GetTableMetadataAsync`: Gets metadata for a table
- `GetAllTablesAsync`: Gets all table metadata
- `GetTablesBySubjectAreaAsync`: Gets tables by subject area

**Example:**
```csharp
var metadataRepo = new PPDMMetadataRepository(editor);
var tableMetadata = await metadataRepo.GetTableMetadataAsync("WELL");
Console.WriteLine($"Table: {tableMetadata.TableName}");
Console.WriteLine($"Primary Key: {tableMetadata.PrimaryKeyColumn}");
```

### PPDMMetadataLoader

Loads metadata from JSON files.

**Key Methods:**
- `LoadMetadataAsync`: Loads metadata from file

### PPDMTreeBuilder

Builds tree structures from metadata.

**Key Methods:**
- `BuildTreeAsync`: Builds complete tree structure

## Metadata Structure

Metadata includes:
- Table names and descriptions
- Column definitions
- Primary keys
- Foreign keys
- Relationships
- Subject areas
- Modules

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Generic Repository](beep-dataaccess-generic-repository.md) - Repository usage

