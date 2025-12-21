# Beep.OilandGas.LifeCycle

This project contains all services related to the **oil field lifecycle management**, including field orchestration, phase-specific services (Exploration, Development, Production, Decommissioning), calculations, accounting, and well comparison.

## Purpose

This project was extracted from `Beep.OilandGas.PPDM39.DataManagement` to separate lifecycle management concerns from data management concerns. This separation provides:

- **Clearer Architecture**: Lifecycle services are distinct from data access/validation services
- **Better Organization**: All field lifecycle-related logic is in one place
- **Easier Maintenance**: Changes to lifecycle logic don't affect data management services

## Services Included

### Core Orchestration
- **FieldOrchestrator** - Manages the complete lifecycle of a single active field, coordinating all phase services

### Phase Services
- **Exploration Service** (`PPDMExplorationService`) - Manages prospects, seismic surveys, and exploratory wells
- **Development Service** (`PPDMDevelopmentService`) - Manages pools, facilities, pipelines, and development wells
- **Production Service** (`PPDMProductionService`) - Manages production data and reserves
- **Decommissioning Service** (`PPDMDecommissioningService`) - Manages well abandonment and facility decommissioning

### Supporting Services
- **Calculation Service** (`PPDMCalculationService`) - Performs DCA, physics-based forecasting, and provides helper functions for retrieving well/reservoir data
- **Accounting Service** (`PPDMAccountingService`) - Production accounting operations (volume reconciliation, royalty calculations)
- **Well Comparison Service** (`WellComparisonService`) - Compares wells side-by-side

## Dependencies

- `Beep.OilandGas.PPDM39` - Core interfaces, DTOs, and metadata
- `Beep.OilandGas.PPDM39.DataManagement` - Core data access (PPDMGenericRepository, etc.)
- `Beep.OilandGas.DCA` - Decline Curve Analysis library
- `Beep.OilandGas.ProductionForecasting` - Physics-based forecasting library

## Usage

All services are registered in `Program.cs` of the API service:

```csharp
// Field Orchestrator (scoped per request)
builder.Services.AddScoped<IFieldOrchestrator>(sp => 
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<FieldOrchestrator>();
    return new FieldOrchestrator(
        editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});

// Other lifecycle services can be registered similarly...
```

## Migration Notes

This project was created by moving services from `Beep.OilandGas.PPDM39.DataManagement.Services` to `Beep.OilandGas.LifeCycle.Services`. The following namespaces were changed:

- `Beep.OilandGas.PPDM39.DataManagement.Services` → `Beep.OilandGas.LifeCycle.Services`
- `Beep.OilandGas.PPDM39.DataManagement.Services.Exploration` → `Beep.OilandGas.LifeCycle.Services.Exploration`
- `Beep.OilandGas.PPDM39.DataManagement.Services.Development` → `Beep.OilandGas.LifeCycle.Services.Development`
- `Beep.OilandGas.PPDM39.DataManagement.Services.Production` → `Beep.OilandGas.LifeCycle.Services.Production`
- `Beep.OilandGas.PPDM39.DataManagement.Services.Decommissioning` → `Beep.OilandGas.LifeCycle.Services.Decommissioning`
- `Beep.OilandGas.PPDM39.DataManagement.Services.Calculations` → `Beep.OilandGas.LifeCycle.Services.Calculations`
- `Beep.OilandGas.PPDM39.DataManagement.Services.Accounting` → `Beep.OilandGas.LifeCycle.Services.Accounting`

All interfaces remain in `Beep.OilandGas.PPDM39.Core.DTOs` and `Beep.OilandGas.PPDM39.Core.Interfaces`.
