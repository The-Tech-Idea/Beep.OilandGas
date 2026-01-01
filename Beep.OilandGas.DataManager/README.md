# Beep.OilandGas.DataManager

A unified interface for managing and executing database scripts across all modules in the Beep.OilandGas solution.

## Overview

The `Beep.OilandGas.DataManager` project provides a centralized system for:
- Executing database scripts for any module
- Managing script dependencies and execution order
- Supporting checkpoint/resume functionality for long-running operations
- Validating scripts before execution
- Checking for errors after execution
- Comprehensive logging of all operations

## Features

- **Module-Based Script Execution**: Each module implements `IModuleData` to define its scripts
- **Dependency Resolution**: Automatically resolves and executes modules in the correct order
- **Checkpoint/Resume**: Save execution state and resume from where you left off
- **Pre-Execution Validation**: Validate scripts for syntax errors and missing dependencies
- **Post-Execution Error Checking**: Verify that objects were created successfully
- **Comprehensive Logging**: Log to file, ILogger, and in-memory buffer
- **Multi-Database Support**: Works with SQL Server, SQLite, PostgreSQL, Oracle, MySQL, and MariaDB
- **Consolidated Script Support**: Handles both individual table scripts and consolidated scripts (TAB.sql, PK.sql, FK.sql, etc.)

## Installation

Add a project reference to `Beep.OilandGas.DataManager`:

```xml
<ProjectReference Include="..\Beep.OilandGas.DataManager\Beep.OilandGas.DataManager.csproj" />
```

## Setup

### Dependency Injection

Register DataManager services in your `Startup.cs` or `Program.cs`:

```csharp
using Beep.OilandGas.DataManager.DependencyInjection;

// For file-based state store (default)
services.AddDataManager(options =>
{
    options.StateStoreType = StateStoreType.File;
    options.StateDirectory = @"C:\DataManager\ExecutionStates";
});

// For database-based state store
services.AddDataManagerWithDatabaseStateStore(
    sp => sp.GetRequiredService<IDataSource>(),
    options =>
    {
        options.StateStoreType = StateStoreType.Database;
    });
```

## Usage

### Basic Module Execution

```csharp
using Beep.OilandGas.DataManager.Core.Interfaces;
using Beep.OilandGas.DataManager.Core.Models;
using TheTechIdea.Beep.Editor;

// Get services
var dataManager = serviceProvider.GetRequiredService<IDataManager>();
var dataSource = serviceProvider.GetRequiredService<IDataSource>();

// Create module data implementation
var moduleData = new ProductionAccountingModuleData();

// Execute module
var options = new ScriptExecutionOptions
{
    ContinueOnError = false,
    ValidateBeforeExecution = true,
    CheckErrorsAfterExecution = true
};

var result = await dataManager.ExecuteModuleAsync(moduleData, dataSource, options);

if (result.Success)
{
    Console.WriteLine($"Module executed successfully: {result.SuccessfulScripts}/{result.TotalScripts} scripts");
}
else
{
    Console.WriteLine($"Module execution failed: {result.ErrorMessage}");
}
```

### Executing Multiple Modules

```csharp
var modules = new List<IModuleData>
{
    new SecurityModuleData(),
    new CommonModuleData(),
    new ProductionAccountingModuleData()
};

var results = await dataManager.ExecuteModulesAsync(modules, dataSource, options);

foreach (var (moduleName, result) in results)
{
    Console.WriteLine($"{moduleName}: {result.Success}");
}
```

### Checkpoint and Resume

```csharp
// Start execution with checkpointing enabled
var options = new ScriptExecutionOptions
{
    EnableCheckpointing = true,
    CheckpointInterval = TimeSpan.FromMinutes(5),
    StateStore = serviceProvider.GetRequiredService<IExecutionStateStore>()
};

var result = await dataManager.ExecuteModuleAsync(moduleData, dataSource, options);

// Save the execution ID for later
var executionId = result.ExecutionId;

// Later, resume execution
var resumeOptions = new ScriptExecutionOptions
{
    ExecutionId = executionId,
    EnableCheckpointing = true,
    StateStore = serviceProvider.GetRequiredService<IExecutionStateStore>()
};

var resumedResult = await dataManager.ResumeModuleExecutionAsync(
    executionId, 
    dataSource, 
    resumeOptions);
```

### Pre-Execution Validation

```csharp
// Validate scripts before execution
var validation = await dataManager.ValidateScriptsAsync(
    moduleData, 
    "sqlserver", 
    dataSource);

if (!validation.IsValid)
{
    Console.WriteLine("Validation errors:");
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"  {error.ScriptFileName}: {error.Message}");
    }
}
```

### Post-Execution Error Checking

```csharp
// After execution, check for errors
var errorCheck = await dataManager.CheckForErrorsAsync(executionId, dataSource);

if (errorCheck.HasErrors)
{
    Console.WriteLine("Errors detected:");
    
    foreach (var scriptError in errorCheck.ScriptErrors)
    {
        Console.WriteLine($"  Script: {scriptError.ScriptFileName} - {scriptError.ErrorMessage}");
    }
    
    foreach (var objectError in errorCheck.ObjectErrors)
    {
        Console.WriteLine($"  Object: {objectError.ObjectName} ({objectError.ObjectType}) - {objectError.ErrorMessage}");
    }
}
```

### Executing a Single Script

```csharp
var scriptInfo = new ModuleScriptInfo
{
    FileName = "USER_TAB.sql",
    FullPath = @"C:\Scripts\Sqlserver\Security\USER_TAB.sql",
    ScriptType = ScriptType.TAB,
    TableName = "USER",
    ExecutionOrder = 1
};

var result = await dataManager.ExecuteScriptAsync(scriptInfo, dataSource, options);
```

## Implementing IModuleData

Each module should implement `IModuleData` to define its scripts:

```csharp
using Beep.OilandGas.DataManager.Core.Interfaces;
using Beep.OilandGas.DataManager.Core.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

public class ProductionAccountingModuleData : IModuleData
{
    public string ModuleName => "ProductionAccounting";
    public string Description => "Production Accounting module scripts";
    public string ScriptBasePath => "ProductionAccounting";
    public int ExecutionOrder => 10;
    public bool IsRequired => true;

    public IEnumerable<string> GetDependencies() => new[] { "Security", "Common" };

    public async Task<IEnumerable<ModuleScriptInfo>> GetScriptsAsync(string databaseType)
    {
        var scripts = new List<ModuleScriptInfo>();
        var scriptsPath = Path.Combine(
            AppContext.BaseDirectory,
            "Scripts",
            databaseType,
            ScriptBasePath);

        if (!Directory.Exists(scriptsPath))
        {
            return scripts;
        }

        // Scan for individual table scripts (e.g., TABLE_TAB.sql)
        var individualScripts = Directory.GetFiles(scriptsPath, "*_TAB.sql")
            .Concat(Directory.GetFiles(scriptsPath, "*_PK.sql"))
            .Concat(Directory.GetFiles(scriptsPath, "*_FK.sql"))
            .Concat(Directory.GetFiles(scriptsPath, "*_IX.sql"));

        foreach (var filePath in individualScripts)
        {
            var fileName = Path.GetFileName(filePath);
            var scriptType = DetermineScriptType(fileName);
            var tableName = ExtractTableName(fileName);

            scripts.Add(new ModuleScriptInfo
            {
                FileName = fileName,
                FullPath = filePath,
                RelativePath = Path.GetRelativePath(scriptsPath, filePath),
                ScriptType = scriptType,
                TableName = tableName,
                IsConsolidated = false,
                ExecutionOrder = GetExecutionOrder(scriptType),
                IsRequired = true,
                FileSize = new FileInfo(filePath).Length,
                LastModified = File.GetLastWriteTime(filePath)
            });
        }

        // Scan for consolidated scripts (e.g., TAB.sql, PK.sql, FK.sql)
        var consolidatedScripts = new[] { "TAB.sql", "PK.sql", "FK.sql", "IX.sql", "CK.sql" };
        foreach (var scriptName in consolidatedScripts)
        {
            var filePath = Path.Combine(scriptsPath, scriptName);
            if (File.Exists(filePath))
            {
                var scriptType = DetermineScriptType(scriptName);
                var tableNames = await ExtractTableNamesFromConsolidatedScript(filePath);

                scripts.Add(new ModuleScriptInfo
                {
                    FileName = scriptName,
                    FullPath = filePath,
                    RelativePath = scriptName,
                    ScriptType = scriptType,
                    TableNames = tableNames,
                    IsConsolidated = true,
                    ExecutionOrder = GetExecutionOrder(scriptType),
                    IsRequired = true,
                    FileSize = new FileInfo(filePath).Length,
                    LastModified = File.GetLastWriteTime(filePath)
                });
            }
        }

        return scripts.OrderBy(s => s.ExecutionOrder);
    }

    private ScriptType DetermineScriptType(string fileName)
    {
        if (fileName.Contains("_TAB") || fileName.Equals("TAB.sql", StringComparison.OrdinalIgnoreCase))
            return ScriptType.TAB;
        if (fileName.Contains("_PK") || fileName.Equals("PK.sql", StringComparison.OrdinalIgnoreCase))
            return ScriptType.PK;
        if (fileName.Contains("_FK") || fileName.Equals("FK.sql", StringComparison.OrdinalIgnoreCase))
            return ScriptType.FK;
        if (fileName.Contains("_IX") || fileName.Equals("IX.sql", StringComparison.OrdinalIgnoreCase))
            return ScriptType.IX;
        if (fileName.Contains("_CK") || fileName.Equals("CK.sql", StringComparison.OrdinalIgnoreCase))
            return ScriptType.CK;
        
        return ScriptType.Unknown;
    }

    private string? ExtractTableName(string fileName)
    {
        // Extract from patterns like "TABLE_NAME_TAB.sql"
        var match = Regex.Match(fileName, @"^([A-Z_]+)_(TAB|PK|FK|IX|CK)\.sql$", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private async Task<List<string>> ExtractTableNamesFromConsolidatedScript(string filePath)
    {
        var tableNames = new List<string>();
        var content = await File.ReadAllTextAsync(filePath);
        
        // Extract table names from CREATE TABLE statements
        var matches = Regex.Matches(content, @"CREATE\s+TABLE\s+(\[?)(\w+)(\]?)", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            tableNames.Add(match.Groups[2].Value);
        }
        
        return tableNames.Distinct().ToList();
    }

    private int GetExecutionOrder(ScriptType scriptType)
    {
        return scriptType switch
        {
            ScriptType.TAB => 1,
            ScriptType.PK => 2,
            ScriptType.FK => 3,
            ScriptType.IX => 4,
            ScriptType.CK => 5,
            _ => 99
        };
    }
}
```

## Script Execution Options

The `ScriptExecutionOptions` class provides extensive configuration:

```csharp
var options = new ScriptExecutionOptions
{
    // Error handling
    ContinueOnError = false,              // Stop on first error
    EnableRollback = false,               // Rollback on error (if supported)
    
    // Script filtering
    ExecuteOptionalScripts = false,       // Include optional scripts
    IncludedScriptTypes = new List<ScriptType> { ScriptType.TAB, ScriptType.PK },
    ExcludedScriptTypes = new List<ScriptType> { ScriptType.SYN },
    
    // Validation
    ValidateBeforeExecution = true,      // Validate scripts before execution
    CheckErrorsAfterExecution = true,    // Check for errors after execution
    VerifyObjectsCreated = true,          // Verify tables/indexes exist
    
    // Checkpointing
    EnableCheckpointing = true,          // Enable checkpoint/resume
    ExecutionId = null,                  // Execution ID for resuming
    CheckpointInterval = TimeSpan.FromMinutes(5),
    StateStore = stateStore,             // State store implementation
    
    // Logging
    Logger = logger,                     // ILogger instance
    LogFilePath = @"C:\Logs\datamanager.log",
    
    // Parallel execution (future feature)
    EnableParallelExecution = false,
    MaxParallelTasks = null
};
```

## State Store

### File-Based State Store

```csharp
var stateStore = new FileExecutionStateStore(
    stateDirectory: @"C:\DataManager\ExecutionStates",
    logger: logger);
```

### Database-Based State Store

```csharp
var stateStore = new DatabaseExecutionStateStore(
    dataSource: dataSource,
    logger: logger);
```

## Logging

The `DataManagerLogger` provides comprehensive logging:

```csharp
var logger = new DataManagerLogger(
    logger: serviceProvider.GetService<ILogger<DataManager>>(),
    logFilePath: @"C:\Logs\datamanager.log");

// Logs are automatically written for:
// - Module execution start/complete
// - Script execution start/complete/failure
// - Progress updates
// - Errors and warnings

// Get in-memory log
var logEntries = logger.GetInMemoryLog();
```

## Error Handling

The DataManager provides comprehensive error handling:

- **Validation Errors**: Caught during pre-execution validation
- **Execution Errors**: Caught during script execution
- **Object Verification Errors**: Caught during post-execution checking

All errors are logged and included in the result objects.

## Best Practices

1. **Always validate before execution**: Set `ValidateBeforeExecution = true`
2. **Use checkpointing for long operations**: Enable checkpointing for modules with many scripts
3. **Check for errors after execution**: Set `CheckErrorsAfterExecution = true`
4. **Handle dependencies correctly**: Ensure dependent modules are executed first
5. **Use appropriate logging**: Configure file and ILogger logging for production
6. **Test with ContinueOnError**: Test error scenarios with `ContinueOnError = true` to see all errors

## Architecture

```
Beep.OilandGas.DataManager/
├── Core/
│   ├── Interfaces/
│   │   ├── IDataManager.cs          # Main interface for script execution
│   │   └── IModuleData.cs           # Interface for module script definitions
│   ├── Models/
│   │   ├── ModuleScriptInfo.cs      # Script metadata
│   │   ├── ScriptExecutionOptions.cs # Execution configuration
│   │   ├── ModuleExecutionResult.cs  # Execution results
│   │   ├── ExecutionState.cs         # Checkpoint state
│   │   ├── ValidationResult.cs      # Validation results
│   │   └── ErrorCheckResult.cs       # Error check results
│   ├── State/
│   │   ├── IExecutionStateStore.cs  # State persistence interface
│   │   ├── FileExecutionStateStore.cs
│   │   └── DatabaseExecutionStateStore.cs
│   └── Exceptions/
│       ├── DataManagerException.cs
│       └── ScriptExecutionException.cs
├── Services/
│   ├── DataManager.cs                # Main implementation
│   └── ScriptValidator.cs            # Validation and error checking
├── Logging/
│   └── DataManagerLogger.cs          # Comprehensive logging
└── DependencyInjection/
    └── DataManagerServiceCollectionExtensions.cs
```

## Dependencies

- `TheTechIdea.Beep.Editor` - For `IDataSource` interface
- `Microsoft.Extensions.Logging.Abstractions` - For logging
- `Beep.OilandGas.Models` - For shared models
- `Beep.OilandGas.PPDM39.DataManagement` - For `ScriptType` enum and `ScriptExecutionResult`

## License

See LICENSE file in the solution root.
