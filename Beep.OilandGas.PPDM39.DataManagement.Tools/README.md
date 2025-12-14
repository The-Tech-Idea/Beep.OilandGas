# PPDM39 Metadata Generator Tool

A console application to parse PPDM 3.9 SQL scripts and generate metadata files for use in the data management layer.

## Purpose

This tool extracts table metadata (table names, primary keys, foreign keys, common columns, and module assignments) from PPDM SQL scripts and generates:
1. **JSON metadata file** - For runtime loading
2. **C# metadata class** - For compile-time metadata access

## Prerequisites

- .NET 8.0 SDK
- SQL scripts in the `ms` folder:
  - `TAB.sql` - CREATE TABLE statements
  - `PK.sql` - PRIMARY KEY constraints
  - `FK.sql` - FOREIGN KEY constraints

## Usage

### Basic Usage (Default Path)

```bash
dotnet run --project Beep.OilandGas.PPDM39.DataManagement.Tools
```

This will use the default script folder:
```
C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\ppdm39_SqlServerScript\ms
```

### Custom Script Folder

```bash
dotnet run --project Beep.OilandGas.PPDM39.DataManagement.Tools -- "C:\Path\To\Your\Scripts"
```

Or after building:

```bash
.\bin\Debug\net8.0\Beep.OilandGas.PPDM39.DataManagement.Tools.exe "C:\Path\To\Your\Scripts"
```

## Output

The tool generates two files in:
```
Beep.OilandGas.PPDM39.DataManagement\Core\Metadata\
```

1. **PPDM39Metadata.json** - JSON format metadata (for runtime loading)
2. **PPDM39Metadata.Generated.cs** - C# static class (for compile-time access)

## Generated Files

### JSON Format
The JSON file contains a dictionary of table metadata with:
- Table name
- Entity type name
- Primary key column
- Module assignment
- Common columns list
- Foreign key relationships

### C# Class Format
The generated C# class provides a static method `GetMetadata()` that returns the metadata dictionary. This can be used in `PPDMMetadataRepository.FromGeneratedClass()`.

## Example Output

```
========================================
PPDM39 Metadata Generator
========================================

Configuration:
  Script Folder: C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\ppdm39_SqlServerScript\ms
  TAB.sql: C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\ppdm39_SqlServerScript\ms\TAB.sql
  PK.sql: C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\ppdm39_SqlServerScript\ms\PK.sql
  FK.sql: C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\ppdm39_SqlServerScript\ms\FK.sql
  Output Folder: C:\...\Beep.OilandGas.PPDM39.DataManagement\Core\Metadata

All SQL scripts found. Starting metadata generation...

Step 1: Generating JSON metadata...
  Parsing TAB.sql...
    Found 500 tables
  Parsing PK.sql...
    Parsed primary keys
  Parsing FK.sql...
    Parsed foreign keys
  ✓ Metadata saved to: ...\PPDM39Metadata.json
  ✓ Total tables: 500

Step 2: Generating C# metadata class...
  ✓ C# metadata class saved to: ...\PPDM39Metadata.Generated.cs
  ✓ Total tables: 500

========================================
Metadata generation complete!
========================================

Generated files:
  - ...\PPDM39Metadata.json
  - ...\PPDM39Metadata.Generated.cs

Next steps:
  1. Review generated files
  2. Use PPDMMetadataRepository.FromGeneratedClass() in your code
  3. Commit generated files to source control
```

## Using Generated Metadata

After generation, you can use the metadata in your code:

```csharp
// Option 1: Use generated C# class (compile-time)
var metadataRepo = PPDMMetadataRepository.FromGeneratedClass();

// Option 2: Load from JSON (runtime)
var jsonPath = "path/to/PPDM39Metadata.json";
var metadataRepo = PPDMMetadataRepository.FromJsonFile(jsonPath);

// Use the repository
var tableMeta = metadataRepo.GetTableMetadata("STRAT_UNIT");
var primaryKey = metadataRepo.GetPrimaryKeyForTable("STRAT_UNIT");
var foreignKeys = metadataRepo.GetForeignKeysForTable("STRAT_UNIT");
```

## Notes

- The generated C# file is marked as auto-generated and should not be edited manually
- Regenerate the metadata whenever the SQL scripts change
- The tool handles large SQL scripts efficiently with progress indicators
- Module assignment is inferred from table name prefixes (e.g., `STRAT_*` → Stratigraphy)

## Troubleshooting

### Missing SQL Scripts
If the tool reports missing files, ensure:
- The script folder path is correct
- All three files (`TAB.sql`, `PK.sql`, `FK.sql`) exist in the folder

### Build Errors
If you encounter build errors, ensure:
- All project dependencies are restored: `dotnet restore`
- The `Beep.OilandGas.PPDM39` project builds successfully (contains metadata classes)
- The `Beep.OilandGas.PPDM39.DataManagement` project builds successfully

