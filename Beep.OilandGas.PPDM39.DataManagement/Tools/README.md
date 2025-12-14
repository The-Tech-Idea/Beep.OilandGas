# PPDM39 Metadata Generator Tool

## Purpose

This tool parses PPDM SQL scripts (TAB.sql, PK.sql, FK.sql) **ONCE** and generates:
- C# class with all metadata (fast, compile-time)
- JSON file with all metadata (flexible, runtime)

The generated metadata is then used by repositories - **no SQL parsing needed at runtime**.

## How to Use

### Option 1: Run the Generator Tool

```csharp
// In GenerateMetadata.cs Main method, update paths:
var scriptFolder = @"C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\ppdm39_SqlServerScript\ms";
var tabScript = Path.Combine(scriptFolder, "TAB.sql");
var pkScript = Path.Combine(scriptFolder, "PK.sql");
var fkScript = Path.Combine(scriptFolder, "FK.sql");

// Run the tool
var generator = new PPDMMetadataGenerator(tabScript, pkScript, fkScript);
generator.GenerateCSharpMetadata("PPDM39Metadata.Generated.cs");
generator.GenerateJsonMetadata("PPDM39Metadata.json");
```

### Option 2: Use Programmatically

```csharp
var generator = new PPDMMetadataGenerator(
    @"C:\Path\To\TAB.sql",
    @"C:\Path\To\PK.sql",
    @"C:\Path\To\FK.sql"
);

// Generate C# class
generator.GenerateCSharpMetadata(@"Core\Metadata\PPDM39Metadata.Generated.cs");

// Or generate JSON
generator.GenerateJsonMetadata(@"Core\Metadata\PPDM39Metadata.json");
```

## What Gets Generated

### C# Class (`PPDM39Metadata.Generated.cs`)
```csharp
public static class PPDM39Metadata
{
    public static Dictionary<string, PPDMTableMetadata> GetMetadata()
    {
        // All 2600+ tables with:
        // - Table names
        // - Primary keys
        // - Foreign keys
        // - Common columns
        // - Module assignments
    }
}
```

### JSON File (`PPDM39Metadata.json`)
```json
{
  "STRAT_UNIT": {
    "TableName": "STRAT_UNIT",
    "PrimaryKeyColumn": "STRAT_UNIT_ID",
    "Module": "Stratigraphy",
    "ForeignKeys": [...],
    "CommonColumns": [...]
  },
  ...
}
```

## Using Generated Metadata

### From C# Class (Recommended - Fastest)
```csharp
// In dependency injection
services.AddSingleton<IPPDMMetadataRepository>(sp =>
    PPDMMetadataRepository.FromGeneratedClass()
);
```

### From JSON File
```csharp
// In dependency injection
services.AddSingleton<IPPDMMetadataRepository>(sp =>
    PPDMMetadataRepository.FromJsonFile(@"path\to\PPDM39Metadata.json")
);
```

## Benefits

1. ✅ **Parse Once** - SQL scripts parsed only during generation
2. ✅ **Fast Startup** - No SQL parsing at runtime
3. ✅ **Data Source Agnostic** - Works with SQL, NoSQL, APIs, etc.
4. ✅ **Version Controlled** - Generated files can be committed
5. ✅ **Type Safe** - C# class provides compile-time safety

## Workflow

1. **One Time**: Run generator tool with SQL scripts
2. **Generated**: C# class or JSON file created
3. **Runtime**: Use generated metadata (no SQL parsing)
4. **Update**: Re-run generator if SQL scripts change

