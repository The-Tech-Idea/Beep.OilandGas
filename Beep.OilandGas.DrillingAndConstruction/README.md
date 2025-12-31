# Beep.OilandGas.DrillingAndConstruction

A comprehensive service library for managing drilling operations and construction activities in oil and gas fields, following industry best practices and PPDM standards.

## Overview

The `Beep.OilandGas.DrillingAndConstruction` project provides services for managing the complete drilling and construction lifecycle, from well planning through completion. It integrates with the LifeCycle project and follows the established patterns using `PPDMGenericRepository` for data access.

## Features

### Core Functionality

- **Drilling Operation Management**: Complete lifecycle management of drilling operations
- **Drilling Reports**: Daily drilling reports and activity tracking
- **Well Construction**: Casing, completion, and facility construction tracking
- **Cost Management**: Integration with AFE (Authorization for Expenditure) and cost tracking
- **Safety and Compliance**: Safety incident tracking and regulatory compliance
- **Real-time Monitoring**: Drilling parameter monitoring and alerting

### Service Architecture

The project follows the LifeCycle service patterns:

- **Service Interfaces**: Defined in `Beep.OilandGas.Models.Core.Interfaces`
- **Repository Pattern**: Uses `PPDMGenericRepository` for all data access
- **Common Column Handling**: Automatic audit column management via `ICommonColumnHandler`
- **Metadata Integration**: Uses `IPPDMMetadataRepository` and `IPPDM39DefaultsRepository`
- **Logging**: Comprehensive logging with `ILogger<T>`
- **Connection Management**: Supports multiple database connections

## Installation

```bash
dotnet add package Beep.OilandGas.DrillingAndConstruction
```

## Usage Examples

### Basic Service Registration

```csharp
using Beep.OilandGas.DrillingAndConstruction.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

// Register services
services.AddScoped<IDrillingOperationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var logger = sp.GetService<ILogger<DrillingOperationService>>();
    
    return new DrillingOperationService(
        editor,
        commonColumnHandler,
        defaults,
        metadata,
        connectionName: "PPDM39",
        logger: logger);
});
```

### Get All Drilling Operations

```csharp
var drillingService = serviceProvider.GetRequiredService<IDrillingOperationService>();

// Get all active drilling operations
var operations = await drillingService.GetDrillingOperationsAsync();

// Get operations for a specific well
var wellOperations = await drillingService.GetDrillingOperationsAsync("123456789012345");
```

### Create a New Drilling Operation

```csharp
var createDto = new CreateDrillingOperationDto
{
    WellUWI = "123456789012345",
    PlannedSpudDate = DateTime.UtcNow.AddDays(30),
    TargetDepth = 10000m,
    DrillingContractor = "ABC Drilling Company",
    RigName = "Rig-001",
    EstimatedDailyCost = 50000m
};

var operation = await drillingService.CreateDrillingOperationAsync(createDto);
```

### Create a Drilling Report

```csharp
var reportDto = new CreateDrillingReportDto
{
    ReportDate = DateTime.UtcNow,
    Depth = 2500m,
    Activity = "Drilling",
    Hours = 24m,
    Remarks = "Drilled 500 feet today. No issues encountered."
};

var report = await drillingService.CreateDrillingReportAsync("123456789012345", reportDto);
```

### Update Drilling Operation

```csharp
var updateDto = new UpdateDrillingOperationDto
{
    Status = "Active",
    CurrentDepth = 5000m,
    DailyCost = 52000m,
    CompletionDate = null
};

var updatedOperation = await drillingService.UpdateDrillingOperationAsync("123456789012345", updateDto);
```

## Service Interface

The service interface is defined in `Beep.OilandGas.Models.Core.Interfaces.IDrillingOperationService`:

```csharp
public interface IDrillingOperationService
{
    Task<List<DrillingOperationDto>> GetDrillingOperationsAsync(string? wellUWI = null);
    Task<DrillingOperationDto?> GetDrillingOperationAsync(string operationId);
    Task<DrillingOperationDto> CreateDrillingOperationAsync(CreateDrillingOperationDto createDto);
    Task<DrillingOperationDto> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperationDto updateDto);
    Task<List<DrillingReportDto>> GetDrillingReportsAsync(string operationId);
    Task<DrillingReportDto> CreateDrillingReportAsync(string operationId, CreateDrillingReportDto createDto);
}
```

## Data Models

### DrillingOperationDto

Represents a drilling operation with all associated information:

- `OperationId`: Unique identifier (UWI)
- `WellUWI`: Well unique well identifier
- `WellName`: Well name
- `SpudDate`: Spud date
- `CompletionDate`: Completion date
- `Status`: Operation status (Active/Inactive)
- `CurrentDepth`: Current drilling depth
- `TargetDepth`: Target drilling depth
- `DrillingContractor`: Drilling contractor name
- `RigName`: Rig name
- `DailyCost`: Daily drilling cost
- `TotalCost`: Total drilling cost
- `Currency`: Currency code
- `Reports`: List of drilling reports

### DrillingReportDto

Represents a daily drilling report:

- `ReportId`: Unique report identifier
- `OperationId`: Associated operation ID (UWI)
- `ReportDate`: Report date
- `Depth`: Current depth
- `Activity`: Activity description
- `Hours`: Hours worked
- `Remarks`: Additional remarks
- `ReportedBy`: User who created the report

## Integration Points

### LifeCycle Integration

The service integrates with the LifeCycle project for field development phase management:

- Drilling operations are linked to field development phases
- Cost tracking integrates with LifeCycle cost management
- Well completion triggers transition to production phase

### ProductionAccounting Integration

- AFE (Authorization for Expenditure) integration for cost approval
- Cost allocation to wells and fields
- Budget vs actual cost reporting

### PermitsAndApplications Integration

- Drilling permit tracking
- Environmental permit compliance
- Regulatory reporting

## PPDM Tables Used

The service uses the following PPDM tables:

- **WELL**: Well master data
- **WELL_DRILL_REPORT**: Drilling reports and activities

## Best Practices

1. **Always use the service interface**: Use `IDrillingOperationService` from `Beep.OilandGas.Models.Core.Interfaces`
2. **Connection management**: Specify connection name when creating service instances
3. **Error handling**: All service methods throw appropriate exceptions
4. **Logging**: Enable logging for production environments
5. **Audit trail**: All operations automatically maintain audit columns via `ICommonColumnHandler`

## Architecture

The service follows the established patterns:

```
IDrillingOperationService (Interface in Models)
    ↓
DrillingOperationService (Implementation)
    ↓
PPDMGenericRepository (Data Access)
    ↓
PPDM Database Tables
```

## Dependencies

- `Beep.OilandGas.Models`: DTOs and interfaces
- `Beep.OilandGas.PPDM39`: PPDM models
- `Beep.OilandGas.PPDM39.DataManagement`: Repository pattern implementation
- `Microsoft.Extensions.Logging`: Logging support

## Future Enhancements

Planned enhancements include:

- Drilling cost service with AFE integration
- Drilling safety service for incident tracking
- Real-time drilling monitoring integration
- Advanced reporting and analytics
- Integration with rig scheduling systems
- Environmental compliance tracking

## License

MIT License

## Contributing

Contributions are welcome! Please follow the project's coding standards and submit pull requests.

