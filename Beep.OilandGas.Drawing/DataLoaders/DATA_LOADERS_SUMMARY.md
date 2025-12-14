# Data Loaders - Implementation Summary

## Overview
The Data Loaders system provides a comprehensive, extensible framework for loading oil and gas data from various sources including databases (PPDM38, SeaBed), file formats (LAS, WITSML), and other standard data sources.

## Architecture

### Core Interfaces

#### IDataLoader<T>
Base interface for all data loaders with common functionality:
- Connection management (Connect, Disconnect, ValidateConnection)
- Data loading (Load, LoadAsync)
- Identifier management (GetAvailableIdentifiers)
- Disposable pattern

#### ILogLoader
Specialized interface for loading log data:
- LoadLog(wellIdentifier, logName)
- LoadLogs(wellIdentifier, logNames)
- GetAvailableLogs(wellIdentifier)
- Full async support

#### ISchematicLoader
Specialized interface for loading well schematic data:
- LoadSchematic(wellIdentifier)
- LoadSchematics(wellIdentifiers)
- GetAvailableWells()
- Full async support

### Data Models

#### LogData
Represents log data with:
- Well identifier and log name
- Depth information (start, end, step, unit)
- Curve data (dictionary of curve name to values)
- Depth values array
- Curve metadata (units, descriptions, min/max)
- General metadata
- Helper methods for accessing curve values

#### LogCurveMetadata
Metadata for individual log curves:
- Unit
- Description
- Min/Max values
- Null value

## Implementations

### LasLogLoader
**Purpose**: Loads log data from LAS (Log ASCII Standard) files.

**Features**:
- Parses LAS file format
- Extracts well information, curve information, and ASCII data
- Handles metadata and curve descriptions
- Supports standard LAS sections (~VERSION, ~WELL, ~CURVE, ~ASCII)

**Usage**:
```csharp
var loader = new LasLogLoader("path/to/log.las");
loader.Connect();
var logData = loader.LoadLog("WELL-001", "GR");
```

### DatabaseLogLoader
**Purpose**: Loads log data from database sources.

**Features**:
- Generic database connection support
- Connection factory pattern for flexibility
- Async operations
- Query-based data loading

**Usage**:
```csharp
var loader = new DatabaseLogLoader(connectionString, () => new SqlConnection());
loader.Connect();
var logData = loader.LoadLog("WELL-001", "GR");
```

### Ppdm38SchematicLoader
**Purpose**: Loads well schematic data from PPDM38 database.

**Features**:
- PPDM38 database schema support
- Loads boreholes, casing, tubing, equipment, perforations
- Connection factory pattern
- Async operations

**Usage**:
```csharp
var loader = new Ppdm38SchematicLoader(connectionString, () => new SqlConnection());
loader.Connect();
var wellData = loader.LoadSchematic("WELL-001");
```

### SeaBedSchematicLoader
**Purpose**: Loads well schematic data from SeaBed system.

**Features**:
- SeaBed database/API support
- Flexible connection (database or API)
- Loads complete well schematics
- Async operations

**Usage**:
```csharp
// Database connection
var loader = new SeaBedSchematicLoader(connectionString: connectionString);

// API endpoint
var loader = new SeaBedSchematicLoader(apiEndpoint: "https://api.seabed.com");

loader.Connect();
var wellData = loader.LoadSchematic("WELL-001");
```

## Factory Pattern

### DataLoaderFactory
Provides factory methods for creating loaders:

```csharp
// Create log loader from file (auto-detects format)
var logLoader = DataLoaderFactory.CreateLogLoaderFromFile("log.las");

// Create log loader by type
var logLoader = DataLoaderFactory.CreateLogLoader(
    "path/to/log.las", 
    DataLoaderType.LasFile);

// Create schematic loader
var schematicLoader = DataLoaderFactory.CreateSchematicLoader(
    connectionString, 
    DataLoaderType.Ppdm38,
    () => new SqlConnection());
```

## Data Loader Types

- **LasFile**: LAS file format
- **Database**: Generic database
- **Ppdm38**: PPDM38 database
- **SeaBed**: SeaBed system
- **Witsml**: WITSML format (future)
- **Excel**: Excel files (future)
- **Csv**: CSV files (future)

## Usage Examples

### Loading Log Data from LAS File

```csharp
using Beep.OilandGas.Drawing.DataLoaders;
using Beep.OilandGas.Drawing.DataLoaders.Implementations;

// Create loader
var loader = new LasLogLoader("well_log.las");
loader.Connect();

// Load log
var logData = loader.LoadLog("WELL-001", "GR");

// Access curve data
var gammaRayValues = logData.Curves["GR"];
var depths = logData.Depths;

// Get value at specific depth
var value = logData.GetCurveValueAtDepth("GR", 5000.0);

loader.Dispose();
```

### Loading Well Schematic from PPDM38

```csharp
using Beep.OilandGas.Drawing.DataLoaders;
using Beep.OilandGas.Drawing.DataLoaders.Implementations;
using System.Data.SqlClient;

// Create loader with connection factory
var loader = new Ppdm38SchematicLoader(
    connectionString,
    () => new SqlConnection());

await loader.ConnectAsync();

// Load schematic
var wellData = await loader.LoadSchematicAsync("WELL-001");

// Access well data
foreach (var borehole in wellData.BoreHoles)
{
    // Use borehole data...
}

loader.Dispose();
```

### Using Factory Pattern

```csharp
using Beep.OilandGas.Drawing.DataLoaders;

// Auto-detect file format
var logLoader = DataLoaderFactory.CreateLogLoaderFromFile("log.las");
logLoader.Connect();
var logData = logLoader.Load();

// Create schematic loader
var schematicLoader = DataLoaderFactory.CreateSchematicLoader(
    connectionString,
    DataLoaderType.Ppdm38,
    () => new SqlConnection());
```

### Loading Multiple Logs

```csharp
var loader = new DatabaseLogLoader(connectionString, connectionFactory);
loader.Connect();

// Load all logs for a well
var allLogs = loader.LoadLogs("WELL-001");

// Load specific logs
var specificLogs = loader.LoadLogs("WELL-001", new List<string> { "GR", "RES", "DEN" });

// Async loading
var logsAsync = await loader.LoadLogsAsync("WELL-001");
```

## Integration with Drawing Framework

Data loaders integrate seamlessly with the drawing framework:

```csharp
// Load well schematic
var schematicLoader = DataLoaderFactory.CreateSchematicLoader(
    connectionString,
    DataLoaderType.Ppdm38);
var wellData = schematicLoader.LoadSchematic("WELL-001");

// Create visualization
var engine = WellSchematicBuilder.Create()
    .WithWellData(wellData)
    .Build();
```

## Extensibility

### Creating Custom Loaders

1. **Implement the appropriate interface**:
   - `ILogLoader` for log data
   - `ISchematicLoader` for schematic data

2. **Implement required methods**:
   - Connection management
   - Data loading
   - Identifier retrieval

3. **Register with factory** (optional):
   - Add to `DataLoaderFactory` if desired

Example:
```csharp
public class CustomLogLoader : ILogLoader
{
    // Implement interface methods...
}
```

## Future Enhancements

### Planned Implementations

1. **WitsmlLogLoader**: WITSML format support
2. **ExcelLogLoader**: Excel file support
3. **CsvLogLoader**: CSV file support
4. **ApiSchematicLoader**: Generic REST API loader
5. **FileSchematicLoader**: File-based schematic loading

### Additional Features

- **Caching**: Data caching for performance
- **Validation**: Data validation and error handling
- **Transformation**: Data transformation pipelines
- **Filtering**: Advanced filtering capabilities
- **Streaming**: Streaming support for large datasets

## Best Practices

1. **Always Dispose**: Use `using` statements or call `Dispose()`
2. **Validate Connection**: Check `IsConnected` before loading
3. **Handle Errors**: Wrap loading operations in try-catch
4. **Use Async**: Prefer async methods for better performance
5. **Connection Management**: Reuse connections when possible

## Summary

The Data Loaders system provides a robust, extensible foundation for loading oil and gas data from various sources. It supports:

- ✅ Multiple data source types (files, databases, APIs)
- ✅ Standard formats (LAS, PPDM38, SeaBed)
- ✅ Async operations
- ✅ Factory pattern for easy creation
- ✅ Extensible architecture
- ✅ Integration with drawing framework

The system is designed to be easily extended with new loaders and data sources as needed.

