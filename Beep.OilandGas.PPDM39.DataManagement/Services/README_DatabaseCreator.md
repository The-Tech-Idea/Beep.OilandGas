# PPDM Database Creator Service

## Overview

The `PPDMDatabaseCreatorService` provides a comprehensive solution for creating PPDM39 databases by executing SQL scripts in the correct order with proper logging and error handling.

## Features

- **Step-by-step execution** with progress tracking
- **Comprehensive logging** to console and file
- **Module-based categorization** using PPDM module mapping
- **Script type ordering** (TAB → PK → FK → etc.)
- **Multi-database support** (SQL Server, Oracle, PostgreSQL, MySQL, MariaDB, SQLite)
- **Error handling** with optional rollback
- **Resume capability** - can resume from last successful script
- **Validation** - validates script dependencies before execution
- **Filtering** - execute specific modules or script types

## Usage

### Basic Example

```csharp
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using TheTechIdea.Beep.Editor;

var editor = // ... get IDMEEditor instance
var service = new PPDMDatabaseCreatorService(editor, logger);

var options = new DatabaseCreationOptions
{
    DatabaseType = DatabaseType.SqlServer,
    ConnectionString = "Server=localhost;Database=PPDM39;Integrated Security=true;",
    DatabaseName = "PPDM39",
    ScriptsPath = @"C:\Path\To\Beep.OilandGas.PPDM39\Scripts",
    EnableLogging = true,
    LogFilePath = "logs/db_creation.log",
    ContinueOnError = false,
    ExecuteOptionalScripts = false
};

var result = await service.CreateDatabaseAsync(options);

if (result.Success)
{
    Console.WriteLine($"Database created successfully!");
    Console.WriteLine($"Total scripts: {result.TotalScripts}");
    Console.WriteLine($"Successful: {result.SuccessfulScripts}");
    Console.WriteLine($"Duration: {result.TotalDuration.TotalMinutes:F2} minutes");
}
else
{
    Console.WriteLine($"Database creation failed: {result.ErrorMessage}");
}
```

### Execute Specific Modules

```csharp
var options = new DatabaseCreationOptions
{
    DatabaseType = DatabaseType.SqlServer,
    ScriptsPath = @"C:\Path\To\Scripts",
    Categories = new List<string> { "Fields", "Wells" }, // Only execute these modules
    ScriptTypes = new List<ScriptType> { ScriptType.TAB, ScriptType.PK, ScriptType.FK }
};

var result = await service.CreateDatabaseAsync(options);
```

### Discover Scripts

```csharp
var scripts = await service.DiscoverScriptsAsync("Sqlserver", @"C:\Path\To\Scripts");

foreach (var script in scripts)
{
    Console.WriteLine($"{script.FileName} - {script.ScriptType} - {script.Module}");
}
```

### Track Progress

```csharp
var executionId = Guid.NewGuid().ToString();
var options = new DatabaseCreationOptions
{
    // ... other options
    ExecutionId = executionId
};

// Start execution in background
var task = service.CreateDatabaseAsync(options);

// Check progress
var progress = await service.GetProgressAsync(executionId);
Console.WriteLine($"Progress: {progress.ProgressPercentage:F1}%");
Console.WriteLine($"Current: {progress.CurrentScript}");
```

## Script Organization

Scripts are organized by:

1. **Database Type**: Sqlserver, Oracle, PostgreSQL, MySQL, MariaDB, SQLite
2. **Script Type**: TAB, PK, FK, CK, OUOM, UOM, RQUAL, RSRC, TCM, CCM, SYN, GUID
3. **Module**: Fields, Wells, Production & Reserves, Exploration, etc.

### Execution Order

Scripts are executed in this order:

1. TAB.sql (all tables) or individual TABLE_TAB.sql files
2. PK.sql (all primary keys) or individual TABLE_PK.sql files
3. CK.sql (check constraints)
4. FK.sql (foreign keys) or individual TABLE_FK.sql files
5. OUOM.sql, UOM.sql, RQUAL.sql, RSRC.sql
6. Optional: TCM.sql, CCM.sql, SYN.sql, GUID.sql

## Logging

The service provides comprehensive logging:

- **Console logging** via ILogger
- **File logging** to specified log file
- **In-memory logging** for progress tracking

### Log Format

```
[2024-01-15 10:30:45] [INFO] Starting database creation for SQL Server
[2024-01-15 10:30:45] [INFO] Discovered 150 scripts in 12 modules
[2024-01-15 10:30:46] [INFO] Executing: TAB.sql (Fields module)
[2024-01-15 10:30:47] [SUCCESS] TAB.sql completed in 1.2s
[2024-01-15 10:30:47] [INFO] Executing: FIELD_PHASE_TAB.sql
[2024-01-15 10:30:47] [SUCCESS] FIELD_PHASE_TAB.sql completed in 0.1s
[2024-01-15 10:30:47] [INFO] Progress: 2/150 (1.3%) - FIELD_PHASE_PK.sql
```

## Error Handling

- **ContinueOnError**: If true, continues execution after errors
- **EnableRollback**: If true, attempts to rollback on error (future feature)
- **ValidateDependencies**: Validates script dependencies before execution

## Components

### Services

- **PPDMDatabaseCreatorService** - Main service
- **PPDMScriptDiscoveryService** - Discovers and analyzes scripts
- **PPDMScriptCategorizer** - Categorizes scripts by module/type
- **PPDMScriptExecutionOrderManager** - Manages execution order
- **PPDMDatabaseScriptLogger** - Comprehensive logging
- **PPDMScriptExecutionEngine** - Executes scripts

### Models

- **DatabaseCreationOptions** - Configuration options
- **DatabaseCreationResult** - Execution result
- **ScriptInfo** - Script metadata
- **ScriptExecutionResult** - Individual script result
- **ScriptExecutionProgress** - Progress tracking
- **ScriptCategory** - Script categorization

## Integration

The service integrates with:

- **IDMEEditor** - For database connections and execution
- **ILogger** - For logging
- **PPDM39TableToModuleMapping.json** - For module mapping
- **PPDM39SubjectAreasAndModules.json** - For subject area information

## Notes

- Scripts must be organized in the standard PPDM structure
- Metadata files must be accessible for proper categorization
- Connection must be configured in IDMEEditor before execution
- Large scripts may take significant time to execute








