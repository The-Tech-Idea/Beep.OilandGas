# Beep.OilandGas.Client Developer Guide

This guide provides comprehensive instructions for using the `Beep.OilandGas.Client` SDK in your applications.

## Table of Contents

1. [Installation](#installation)
2. [Quick Start](#quick-start)
3. [Configuration](#configuration)
4. [Authentication](#authentication)
5. [Connection Management](#connection-management)
6. [Using Services](#using-services)
7. [Application-Specific Examples](#application-specific-examples)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

## Installation

### NuGet Package

```bash
dotnet add package Beep.OilandGas.Client
```

### Project Reference (Local Development)

```xml
<ItemGroup>
  <ProjectReference Include="..\Beep.OilandGas.Client\Beep.OilandGas.Client.csproj" />
</ItemGroup>
```

## Quick Start

### Basic Usage

```csharp
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Authentication;

// Create app instance
var app = new BeepOilandGasApp(
    baseUrl: "https://api.beep-oilandgas.com",
    authProvider: new TokenProvider("your-token")
);

// Use services
var chokeResult = await app.Analysis.CalculateDownholeChokeFlowAsync(
    new GasChokeProperties
    {
        GasSpecificGravity = 0.65m,
        UpstreamPressure = 2000m,
        DownstreamPressure = 1500m,
        Temperature = 520m,
        ZFactor = 0.85m,
        FlowRate = 5000m
    }
);

Console.WriteLine($"Flow Rate: {chokeResult.FlowRate} Mscf/day");
```

## Configuration

### Remote Mode (HTTP API)

The SDK connects to a remote API service:

```csharp
var app = new BeepOilandGasApp(
    baseUrl: "https://api.beep-oilandgas.com",
    authProvider: new TokenProvider("your-access-token")
);
```

### Local Mode (Dependency Injection)

The SDK uses directly injected services:

```csharp
// In your Startup.cs or Program.cs
services.AddBeepOilandGasClient(options =>
{
    options.UseLocalMode = true;
    // Register your local services
});

var app = serviceProvider.GetRequiredService<IBeepOilandGasApp>();
```

## Authentication

### Token-Based Authentication

```csharp
using Beep.OilandGas.Client.Authentication;

// Static token
var authProvider = new TokenProvider("your-access-token");

// Token with refresh
var authProvider = new TokenProvider(
    token: "initial-token",
    refreshToken: "refresh-token",
    refreshCallback: async () => await RefreshTokenAsync()
);

var app = new BeepOilandGasApp(
    baseUrl: "https://api.beep-oilandgas.com",
    authProvider: authProvider
);
```

### Username/Password Authentication

```csharp
using Beep.OilandGas.Client.Authentication;

var authProvider = new CredentialsAuthenticationProvider(
    baseUrl: "https://api.beep-oilandgas.com",
    username: "your-username",
    password: "your-password"
);

var app = new BeepOilandGasApp(
    baseUrl: "https://api.beep-oilandgas.com",
    authProvider: authProvider
);
```

### Custom Authentication Provider

```csharp
public class CustomAuthProvider : IAuthenticationProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Your custom token retrieval logic
        return Task.FromResult<string?>("your-token");
    }
}

var app = new BeepOilandGasApp(
    baseUrl: "https://api.beep-oilandgas.com",
    authProvider: new CustomAuthProvider()
);
```

## Connection Management

### Single Connection

```csharp
var app = new BeepOilandGasApp(
    baseUrl: "https://api.beep-oilandgas.com",
    authProvider: new TokenProvider("token")
);

// All operations use the default connection
var result = await app.Analysis.CalculateDownholeChokeFlowAsync(request);
```

### Multiple Connections

```csharp
// Add connections
await app.Connection.AddConnectionAsync("Production", new ConnectionInfo
{
    Name = "Production",
    BaseUrl = "https://prod-api.beep-oilandgas.com",
    DatabaseType = "SQLServer",
    ConnectionString = "Server=prod-server;Database=Production;..."
});

await app.Connection.AddConnectionAsync("Development", new ConnectionInfo
{
    Name = "Development",
    BaseUrl = "https://dev-api.beep-oilandgas.com",
    DatabaseType = "SQLServer",
    ConnectionString = "Server=dev-server;Database=Development;..."
});

// Switch between connections
await app.Connection.SwitchConnectionAsync("Production");
var prodData = await app.Well.GetWellsAsync();

await app.Connection.SwitchConnectionAsync("Development");
var devData = await app.Well.GetWellsAsync();

// Get current connection
var currentConnection = app.Connection.CurrentConnection;
Console.WriteLine($"Current: {currentConnection?.Name}");
```

### Connection Testing

```csharp
// Test a connection
var isConnected = await app.Connection.TestConnectionAsync("Production");
if (isConnected)
{
    Console.WriteLine("Connection successful!");
}
```

## Using Services

### Analysis Services

```csharp
// Choke Analysis
var chokeResult = await app.Analysis.CalculateDownholeChokeFlowAsync(
    new GasChokeProperties { /* ... */ }
);

// Compressor Analysis
var compressorResult = await app.Analysis.AnalyzeCompressorAsync(
    new CompressorOperatingConditions { /* ... */ }
);

// Pipeline Analysis
var pipelineResult = await app.Analysis.AnalyzePipelineAsync(
    new PipelineProperties { /* ... */ }
);

// Well Test Analysis
var wellTestResult = await app.Analysis.AnalyzeBuildUpAsync(
    new WellTestData { /* ... */ }
);

// Gas Lift Analysis
var gasLiftResult = await app.Analysis.DesignGasLiftAsync(
    new GasLiftWellProperties { /* ... */ }
);

// Pump Performance
var pumpResult = await app.Analysis.AnalyzePumpPerformanceAsync(
    new ESPDesignProperties { /* ... */ }
);

// Prospect Identification
var prospect = await app.Analysis.IdentifyProspectAsync(
    new PROSPECT { /* ... */ }
);
```

### Pumps Services

```csharp
// Hydraulic Pump Design
var jetPumpResult = await app.Pumps.DesignHydraulicJetPumpAsync(
    new HYDRAULIC_JET_PUMP_PROPERTIES { /* ... */ }
);

// Plunger Lift Design
var plungerResult = await app.Pumps.DesignPlungerLiftAsync(
    new PlungerLiftWellProperties { /* ... */ }
);

// Sucker Rod Pumping
var suckerRodResult = await app.Pumps.DesignSuckerRodPumpAsync(
    new SuckerRodSystemProperties { /* ... */ }
);
```

### Properties Services

```csharp
// Oil Properties
var oilResult = await app.Properties.CalculateOilPropertiesAsync(
    new OilPropertyConditions { /* ... */ }
);

// Gas Properties
var zFactor = await app.Properties.CalculateGasZFactorAsync(
    new GasComposition { /* ... */ }
);

// Heat Map
var heatMap = await app.Properties.GenerateHeatMapAsync(
    new HEAT_MAP_CONFIGURATION { /* ... */ }
);
```

### Calculations Services

```csharp
// Flash Calculations
var flashResult = await app.Calculations.PerformIsothermalFlashAsync(
    new FlashConditions { /* ... */ }
);

// Nodal Analysis
var nodalResult = await app.Calculations.PerformNodalAnalysisAsync(
    new ReservoirProperties { /* ... */ }
);

// Economic Analysis
var npv = await app.Calculations.CalculateNPVAsync(
    new List<CashFlow> { /* ... */ }
);
```

### Production Services

```csharp
// Production Accounting
var volumes = await app.Production.GetProductionVolumesAsync(
    wellId: "WELL-001",
    dateRange: new DateRangeRequest
    {
        StartDate = DateTime.Now.AddMonths(-1),
        EndDate = DateTime.Now
    }
);

// Production Forecasting
var forecast = await app.Production.CreateForecastAsync(
    new ReservoirForecastProperties { /* ... */ }
);

// Production Operations
var operation = await app.Production.CreateOperationAsync(
    new PRODUCTION_COSTS { /* ... */ }
);
```

### LifeCycle Services

```csharp
// Exploration
var exploration = await app.LifeCycle.CreateExplorationProjectAsync(
    new EXPLORATION_PROGRAM { /* ... */ }
);

// Development
var development = await app.LifeCycle.CreateDevelopmentPlanAsync(
    new DEVELOPMENT_COSTS { /* ... */ }
);

// Decommissioning
var decommissioning = await app.LifeCycle.CreateDecommissioningPlanAsync(
    new DECOMMISSIONING_STATUS { /* ... */ }
);
```

### Drilling Services

```csharp
// Drilling Operations
var drillingOp = await app.Drilling.CreateDrillingProgramAsync(
    new DRILLING_OPERATION { /* ... */ }
);

// Enhanced Recovery
var eorResult = await app.Drilling.AnalyzeEORAsync(
    new EORAnalysisRequest { /* ... */ }
);
```

### Data Management Services

#### CRUD Operations

```csharp
// Get entities with filters
var entities = await app.DataManagement.GetEntitiesAsync(
    tableName: "WELL",
    request: new { filters = new[] { 
        new { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } 
    }}
);

// Get single entity
var entity = await app.DataManagement.GetEntityAsync(
    tableName: "WELL",
    id: "WELL-001"
);

// Insert entity
var newEntity = await app.DataManagement.InsertEntityAsync(
    tableName: "WELL",
    request: wellEntity,
    userId: "USER-001"
);

// Update entity
var updatedEntity = await app.DataManagement.UpdateEntityAsync(
    tableName: "WELL",
    id: "WELL-001",
    request: wellEntity,
    userId: "USER-001"
);

// Delete entity (soft delete)
await app.DataManagement.DeleteEntityAsync(
    tableName: "WELL",
    id: "WELL-001",
    userId: "USER-001"
);
```

#### Defaults Management (Per Data Source)

```csharp
// Get a default value for a specific data source
var activeInd = await app.DataManagement.GetDefaultValueAsync(
    key: "ACTIVE_IND_YES",
    databaseId: "DATABASE-GUID-001",
    userId: "USER-001" // Optional: user-specific override
);

// Set or update a default value
await app.DataManagement.SetDefaultValueAsync(
    key: "DEFAULT_ROW_QUALITY",
    value: "GOOD",
    databaseId: "DATABASE-GUID-001",
    userId: null, // null = system default
    category: "System",
    valueType: "String",
    description: "Default row quality indicator"
);

// Get all defaults for a category
var systemDefaults = await app.DataManagement.GetDefaultsByCategoryAsync(
    category: "System",
    databaseId: "DATABASE-GUID-001"
);

// Get all defaults for a data source
var allDefaults = await app.DataManagement.GetDefaultsForDatabaseAsync(
    databaseId: "DATABASE-GUID-001",
    userId: "USER-001" // Optional: user-specific defaults
);

// Initialize system defaults for a new database
await app.DataManagement.InitializeSystemDefaultsAsync(
    databaseId: "DATABASE-GUID-001",
    userId: "SYSTEM"
);

// Reset user overrides to system defaults
await app.DataManagement.ResetToSystemDefaultsAsync(
    databaseId: "DATABASE-GUID-001",
    userId: "USER-001"
);

// Get standard PPDM default values (hardcoded constants)
var standardDefaults = await app.DataManagement.GetStandardDefaultsAsync();
```

#### Batch Operations

```csharp
// Insert multiple entities in a batch
var entities = new List<object> { well1, well2, well3 };
var inserted = await app.DataManagement.InsertBatchAsync(
    tableName: "WELL",
    entities: entities,
    userId: "USER-001",
    batchSize: 100
);

// Update multiple entities in a batch
var updated = await app.DataManagement.UpdateBatchAsync(
    tableName: "WELL",
    entities: entitiesToUpdate,
    userId: "USER-001",
    batchSize: 100
);

// Delete multiple entities by IDs
var deletedCount = await app.DataManagement.DeleteBatchAsync(
    tableName: "WELL",
    ids: new List<string> { "WELL-001", "WELL-002", "WELL-003" },
    userId: "USER-001",
    softDelete: true, // true = soft delete (set ACTIVE_IND = 'N')
    batchSize: 100
);

// Bulk upsert (insert or update)
var upserted = await app.DataManagement.UpsertBatchAsync(
    tableName: "WELL",
    entities: entitiesToUpsert,
    userId: "USER-001",
    batchSize: 100
);
```

#### Import/Export

```csharp
// Import data from CSV file
var importResult = await app.DataManagement.ImportFromCsvAsync(
    tableName: "WELL",
    csvFilePath: "C:\\Data\\wells.csv",
    userId: "USER-001",
    columnMapping: new Dictionary<string, string>
    {
        { "Well Name", "WELL_NAME" },
        { "UWI", "WELL_UWI" },
        { "Status", "STATUS" }
    },
    skipHeaderRow: true,
    validateForeignKeys: true
);

// Export data to CSV file
var exportedCount = await app.DataManagement.ExportToCsvAsync(
    tableName: "WELL",
    csvFilePath: "C:\\Data\\wells_export.csv",
    filters: new List<object>
    {
        new { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
    },
    includeHeaders: true
);
```

#### Pagination & Aggregation

```csharp
// Get entities with pagination
var paginatedResult = await app.DataManagement.GetPaginatedAsync(
    tableName: "WELL",
    filters: new List<object>
    {
        new { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
    },
    pageNumber: 1,
    pageSize: 50,
    sortField: "WELL_NAME",
    sortDirection: "ASC"
);

// Get count of entities
var count = await app.DataManagement.GetCountAsync(
    tableName: "WELL",
    filters: new List<object>
    {
        new { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
    }
);

// Get aggregated value (SUM, AVG, MIN, MAX, COUNT)
var totalDepth = await app.DataManagement.GetAggregateAsync(
    tableName: "WELL",
    fieldName: "TOTAL_DEPTH",
    aggregationType: "SUM",
    filters: new List<object>
    {
        new { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
    }
);

// Get grouped aggregated results
var depthByStatus = await app.DataManagement.GetGroupedAggregateAsync(
    tableName: "WELL",
    groupByField: "STATUS",
    aggregateField: "TOTAL_DEPTH",
    aggregationType: "AVG",
    filters: new List<object>
    {
        new { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
    }
);

// Get distinct values for a field
var distinctStatuses = await app.DataManagement.GetDistinctAsync(
    tableName: "WELL",
    fieldName: "STATUS",
    filters: new List<object>
    {
        new { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
    }
);
```

#### Relationship Navigation

```csharp
// Get related entities through a foreign key relationship
var wellbores = await app.DataManagement.GetRelatedEntitiesAsync(
    tableName: "WELL",
    entityId: "WELL-001",
    relatedTableName: "WELLBORE",
    foreignKeyColumn: "WELL_ID" // Optional: auto-detected if not provided
);

// Get parent entity through a foreign key relationship
var well = await app.DataManagement.GetParentEntityAsync(
    tableName: "WELLBORE",
    entityId: "WELLBORE-001",
    parentTableName: "WELL",
    foreignKeyColumn: "WELL_ID" // Optional: auto-detected if not provided
);

// Get all relationships for an entity
var relationships = await app.DataManagement.GetEntityRelationshipsAsync(
    tableName: "WELL",
    entityId: "WELL-001"
);

// Get child entities by parent key
var children = await app.DataManagement.GetChildrenByParentKeyAsync(
    tableName: "WELLBORE",
    parentTableName: "WELL",
    parentKey: "WELL-001"
);
```

#### Validation & Quality

```csharp
// Validate entity
var validationResult = await app.DataManagement.ValidateEntityAsync(
    tableName: "WELL",
    entity: wellEntity
);

// Validate batch of entities
var batchValidation = await app.DataManagement.ValidateBatchAsync(
    tableName: "WELL",
    entities: new List<object> { well1, well2, well3 }
);

// Calculate quality score
var qualityScore = await app.DataManagement.CalculateQualityScoreAsync(
    tableName: "WELL",
    entity: wellEntity
);

// Get quality metrics for a table
var qualityMetrics = await app.DataManagement.CalculateTableQualityMetricsAsync(
    tableName: "WELL"
);

// Find quality issues
var issues = await app.DataManagement.FindQualityIssuesAsync(
    tableName: "WELL",
    fieldNames: new List<string> { "WELL_NAME", "UWI" } // Optional: specific fields
);
```

#### Versioning

```csharp
// Create a version of an entity
var version = await app.DataManagement.CreateVersionAsync(
    tableName: "WELL",
    entity: wellEntity,
    userId: "USER-001",
    versionLabel: "Initial Version"
);

// Get all versions of an entity
var versions = await app.DataManagement.GetVersionsAsync(
    tableName: "WELL",
    entityId: "WELL-001"
);

// Get a specific version
var version1 = await app.DataManagement.GetVersionAsync(
    tableName: "WELL",
    entityId: "WELL-001",
    versionNumber: 1
);

// Compare two versions
var comparison = await app.DataManagement.CompareVersionsAsync(
    tableName: "WELL",
    entityId: "WELL-001",
    version1: 1,
    version2: 2
);

// Rollback to a specific version
await app.DataManagement.RollbackToVersionAsync(
    tableName: "WELL",
    entityId: "WELL-001",
    versionNumber: 1,
    userId: "USER-001"
);
```

#### List of Values (LOV)

```csharp
// Get LOV entries by type
var statusLOV = await app.DataManagement.GetLOVAsync("WELL_STATUS");

// Get LOV by code
var status = await app.DataManagement.GetLOVByCodeAsync(
    lovType: "WELL_STATUS",
    code: "ACTIVE"
);

// Create LOV entry
var newLOV = await app.DataManagement.CreateLOVAsync(
    lovEntry: new { LOV_TYPE = "WELL_STATUS", CODE = "NEW", DESCRIPTION = "New Well" },
    userId: "USER-001"
);

// Get reference table data
var referenceData = await app.DataManagement.GetReferenceTableDataAsync("STATUS");
```

#### Metadata

```csharp
// Get table metadata
var tableMetadata = await app.DataManagement.GetTableMetadataAsync("WELL");

// Get all tables metadata
var allTables = await app.DataManagement.GetAllTablesMetadataAsync();

// Get column metadata
var columnMetadata = await app.DataManagement.GetColumnMetadataAsync(
    tableName: "WELL",
    columnName: "WELL_NAME"
);
```

## Application-Specific Examples

### Blazor Server Application

```csharp
// Program.cs or Startup.cs
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Authentication;
using Beep.OilandGas.Client.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure Beep Oil & Gas Client
builder.Services.AddBeepOilandGasClient(options =>
{
    options.BaseUrl = builder.Configuration["BeepOilandGas:BaseUrl"];
    options.UseLocalMode = false; // Use remote API
});

// Register authentication provider
builder.Services.AddSingleton<IAuthenticationProvider>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new TokenProvider(config["BeepOilandGas:AccessToken"]);
});

var app = builder.Build();

// Use in Blazor component
@inject IBeepOilandGasApp BeepApp

@code {
    private ChokeFlowResult? chokeResult;

    protected override async Task OnInitializedAsync()
    {
        var request = new GasChokeProperties
        {
            GasSpecificGravity = 0.65m,
            UpstreamPressure = 2000m,
            DownstreamPressure = 1500m,
            Temperature = 520m,
            ZFactor = 0.85m,
            FlowRate = 5000m
        };

        chokeResult = await BeepApp.Analysis.CalculateDownholeChokeFlowAsync(request);
    }
}
```

### Blazor WebAssembly Application

```csharp
// Program.cs
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Authentication;
using Beep.OilandGas.Client.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure Beep Oil & Gas Client
builder.Services.AddBeepOilandGasClient(options =>
{
    options.BaseUrl = builder.Configuration["BeepOilandGas:BaseUrl"];
    options.UseLocalMode = false;
});

// Register authentication provider
builder.Services.AddSingleton<IAuthenticationProvider>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new TokenProvider(config["BeepOilandGas:AccessToken"]);
});

await builder.Build().RunAsync();
```

### WinForms Application

```csharp
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Authentication;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private IBeepOilandGasApp? _app;

    public MainForm()
    {
        InitializeComponent();
        InitializeBeepClient();
    }

    private void InitializeBeepClient()
    {
        _app = new BeepOilandGasApp(
            baseUrl: "https://api.beep-oilandgas.com",
            authProvider: new TokenProvider("your-token")
        );
    }

    private async void btnCalculate_Click(object sender, EventArgs e)
    {
        try
        {
            var request = new GasChokeProperties
            {
                GasSpecificGravity = decimal.Parse(txtGravity.Text),
                UpstreamPressure = decimal.Parse(txtUpstreamPressure.Text),
                DownstreamPressure = decimal.Parse(txtDownstreamPressure.Text),
                Temperature = decimal.Parse(txtTemperature.Text),
                ZFactor = decimal.Parse(txtZFactor.Text),
                FlowRate = decimal.Parse(txtFlowRate.Text)
            };

            var result = await _app!.Analysis.CalculateDownholeChokeFlowAsync(request);
            
            MessageBox.Show($"Flow Rate: {result.FlowRate} Mscf/day", 
                "Calculation Result", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", 
                "Error", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);
        }
    }
}
```

### WPF Application

```csharp
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Authentication;
using System.Windows;
using System.Windows.Input;

public partial class MainWindow : Window
{
    private IBeepOilandGasApp? _app;

    public MainWindow()
    {
        InitializeComponent();
        InitializeBeepClient();
    }

    private void InitializeBeepClient()
    {
        _app = new BeepOilandGasApp(
            baseUrl: "https://api.beep-oilandgas.com",
            authProvider: new TokenProvider("your-token")
        );
    }

    private async void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var request = new GasChokeProperties
            {
                GasSpecificGravity = decimal.Parse(GravityTextBox.Text),
                UpstreamPressure = decimal.Parse(UpstreamPressureTextBox.Text),
                DownstreamPressure = decimal.Parse(DownstreamPressureTextBox.Text),
                Temperature = decimal.Parse(TemperatureTextBox.Text),
                ZFactor = decimal.Parse(ZFactorTextBox.Text),
                FlowRate = decimal.Parse(FlowRateTextBox.Text)
            };

            var result = await _app!.Analysis.CalculateDownholeChokeFlowAsync(request);
            
            ResultTextBlock.Text = $"Flow Rate: {result.FlowRate} Mscf/day";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
```

### ASP.NET Core API Service

```csharp
// Program.cs
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Authentication;
using Beep.OilandGas.Client.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Beep Oil & Gas Client
builder.Services.AddBeepOilandGasClient(options =>
{
    options.BaseUrl = builder.Configuration["BeepOilandGas:BaseUrl"];
    options.UseLocalMode = false;
});

builder.Services.AddSingleton<IAuthenticationProvider>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new TokenProvider(config["BeepOilandGas:AccessToken"]);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Controller
[ApiController]
[Route("api/[controller]")]
public class ChokeAnalysisController : ControllerBase
{
    private readonly IBeepOilandGasApp _beepApp;

    public ChokeAnalysisController(IBeepOilandGasApp beepApp)
    {
        _beepApp = beepApp;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<ChokeFlowResult>> CalculateChokeFlow(
        [FromBody] GasChokeProperties request)
    {
        try
        {
            var result = await _beepApp.Analysis.CalculateDownholeChokeFlowAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

### Console Application

```csharp
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Authentication;

class Program
{
    static async Task Main(string[] args)
    {
        var app = new BeepOilandGasApp(
            baseUrl: "https://api.beep-oilandgas.com",
            authProvider: new TokenProvider("your-token")
        );

        // Example: Calculate choke flow
        var request = new GasChokeProperties
        {
            GasSpecificGravity = 0.65m,
            UpstreamPressure = 2000m,
            DownstreamPressure = 1500m,
            Temperature = 520m,
            ZFactor = 0.85m,
            FlowRate = 5000m
        };

        var result = await app.Analysis.CalculateDownholeChokeFlowAsync(request);
        Console.WriteLine($"Flow Rate: {result.FlowRate} Mscf/day");
        Console.WriteLine($"Downstream Pressure: {result.DownstreamPressure} psia");
    }
}
```

## Best Practices

### 1. Dependency Injection

Always use DI when possible:

```csharp
// Register in DI container
services.AddBeepOilandGasClient(options => { /* ... */ });

// Inject in your classes
public class MyService
{
    private readonly IBeepOilandGasApp _beepApp;

    public MyService(IBeepOilandGasApp beepApp)
    {
        _beepApp = beepApp;
    }
}
```

### 2. Error Handling

Always wrap service calls in try-catch:

```csharp
try
{
    var result = await app.Analysis.CalculateDownholeChokeFlowAsync(request);
    // Process result
}
catch (HttpRequestException ex)
{
    // Handle HTTP errors
    Console.WriteLine($"HTTP Error: {ex.Message}");
}
catch (Exception ex)
{
    // Handle other errors
    Console.WriteLine($"Error: {ex.Message}");
}
```

### 3. Cancellation Tokens

Always pass cancellation tokens for async operations:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var result = await app.Analysis.CalculateDownholeChokeFlowAsync(
        request, 
        cts.Token
    );
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
```

### 4. Connection Management

Use connection switching for multi-tenant scenarios:

```csharp
// Switch to production
await app.Connection.SwitchConnectionAsync("Production");
var prodData = await app.Well.GetWellsAsync();

// Switch to development
await app.Connection.SwitchConnectionAsync("Development");
var devData = await app.Well.GetWellsAsync();
```

### 5. Resource Disposal

Dispose the app instance when done:

```csharp
using var app = new BeepOilandGasApp(
    baseUrl: "https://api.beep-oilandgas.com",
    authProvider: new TokenProvider("token")
);

// Use app...
// Automatically disposed when exiting using block
```

## Troubleshooting

### Common Issues

#### 1. Authentication Errors

**Problem**: `401 Unauthorized` errors

**Solution**: 
- Verify your token is valid
- Check token expiration
- Ensure token is being sent in headers

```csharp
// Check token
var token = await authProvider.GetAccessTokenAsync();
if (string.IsNullOrEmpty(token))
{
    // Refresh or re-authenticate
}
```

#### 2. Connection Errors

**Problem**: `Connection refused` or timeout errors

**Solution**:
- Verify base URL is correct
- Check network connectivity
- Verify firewall settings

```csharp
// Test connection
var isConnected = await app.Connection.TestConnectionAsync();
if (!isConnected)
{
    // Handle connection failure
}
```

#### 3. Entity Type Errors

**Problem**: `Type not found` or compilation errors

**Solution**:
- Ensure `Beep.OilandGas.Models` is referenced
- Check namespace imports
- Verify entity class exists

```csharp
// Correct namespace imports
using Beep.OilandGas.Models.ChokeAnalysis;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
```

#### 4. Null Reference Errors

**Problem**: `NullReferenceException` when accessing services

**Solution**:
- Ensure app is initialized
- Check service is registered in DI
- Verify connection is established

```csharp
// Check app initialization
if (app == null)
{
    throw new InvalidOperationException("App not initialized");
}

// Check connection
if (app.Connection.CurrentConnection == null)
{
    await app.Connection.SwitchConnectionAsync("Default");
}
```

## Additional Resources

- **API Documentation**: See API controller documentation
- **Entity Reference**: See `Beep.OilandGas.Models` documentation
- **Examples**: Check the examples folder in the repository
- **Support**: Contact support@beep-oilandgas.com

## License

This SDK is licensed under the MIT License. See LICENSE file for details.

