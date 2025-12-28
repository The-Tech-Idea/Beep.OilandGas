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
- **Calculation Service** (`PPDMCalculationService`) - Performs DCA, physics-based forecasting, economic analysis, nodal analysis, well test analysis, and flash calculations
- **Accounting Service** (`PPDMAccountingService`) - Production accounting operations (volume reconciliation, royalty calculations, Successful Efforts/Full Cost accounting, amortization, BOE conversions)
- **Permit Management Service** (`PermitManagementService`) - Manages permit applications (drilling, environmental, injection) across multiple jurisdictions
- **Well Comparison Service** (`WellComparisonService`) - Compares wells side-by-side

## Dependencies

### Core Dependencies
- `Beep.OilandGas.PPDM39` - Core interfaces, DTOs, and metadata
- `Beep.OilandGas.PPDM39.DataManagement` - Core data access (PPDMGenericRepository, etc.)

### Calculation Libraries
- `Beep.OilandGas.DCA` - Decline Curve Analysis library
- `Beep.OilandGas.ProductionForecasting` - Physics-based forecasting library
- `Beep.OilandGas.EconomicAnalysis` - Economic evaluation (NPV, IRR, payback period)
- `Beep.OilandGas.NodalAnalysis` - IPR/VLP analysis for well performance optimization
- `Beep.OilandGas.WellTestAnalysis` - Pressure transient analysis (PTA)
- `Beep.OilandGas.FlashCalculations` - Phase equilibrium and flash calculations

### Operational Libraries
- `Beep.OilandGas.GasLift` - Gas lift analysis, valve design, and spacing
- `Beep.OilandGas.PipelineAnalysis` - Pipeline capacity and flow analysis
- `Beep.OilandGas.CompressorAnalysis` - Compressor power and pressure calculations
- `Beep.OilandGas.ChokeAnalysis` - Choke flow analysis and sizing
- `Beep.OilandGas.SuckerRodPumping` - Sucker rod load and power analysis

### Accounting & Permits
- `Beep.OilandGas.Accounting` - Successful Efforts and Full Cost accounting
- `Beep.OilandGas.PermitsAndApplications` - Permit application management

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

## Integrated Features

### Calculation Service Features
- **Decline Curve Analysis (DCA)**: Arps, exponential, hyperbolic, harmonic decline models
- **Production Forecasting**: Physics-based forecasting using reservoir models
- **Economic Analysis**: NPV, IRR, payback period, ROI calculations with cash flow modeling
- **Nodal Analysis**: IPR/VLP analysis for production system optimization
- **Well Test Analysis**: Pressure transient analysis for permeability, skin factor, reservoir pressure
- **Flash Calculations**: Phase equilibrium calculations for vapor-liquid separation

### Development Service Features
- **Gas Lift**: Potential analysis, valve design (US/SI units), valve spacing calculations
- **Pipeline Analysis**: Capacity analysis and flow calculations for gas/liquid pipelines
- **Compressor Analysis**: Power and pressure calculations for centrifugal/reciprocating compressors

### Production Service Features
- **Choke Analysis**: Flow rate calculations and sizing for downhole/uphole chokes
- **Sucker Rod Pumping**: Load analysis and power requirements for sucker rod systems

### Accounting Service Features
- **Successful Efforts Accounting**: FASB Statement No. 19 compliant accounting
- **Full Cost Accounting**: Alternative accounting method support
- **Amortization**: Units-of-production amortization calculations
- **BOE Conversions**: Barrels of Oil Equivalent conversions for production and reserves
- **Cost Recording**: Exploration, development, and acquisition cost tracking
- **Impairment Tracking**: Unproved property impairment management

### Permit Management Features
- **Drilling Permits**: New well drilling, re-entry, horizontal drilling permits
- **Environmental Permits**: Waste management, pits, discharges, NORM permits
- **Injection Permits**: Enhanced recovery, disposal, gas storage, CO2 storage permits
- **Multi-Jurisdiction Support**: Texas RRC, TCEQ, Alberta AER, Federal BLM, USACE, EPA, and more

## Integration Status

✅ **Fully Integrated** (11 libraries):
- DCA, ProductionForecasting, EconomicAnalysis, NodalAnalysis, WellTestAnalysis, FlashCalculations
- GasLift, PipelineAnalysis, CompressorAnalysis, ChokeAnalysis, SuckerRodPumping
- Accounting, PermitsAndApplications

All integrations follow consistent patterns:
- Data retrieval from PPDM tables
- Calculation execution using library methods
- Results storage in appropriate PPDM tables
- Comprehensive error handling and logging
- XML documentation for all public methods
