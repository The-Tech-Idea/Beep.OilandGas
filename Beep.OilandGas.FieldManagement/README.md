# Beep.OilandGas.FieldManagement

## Overview

**Beep.OilandGas.FieldManagement** is a comprehensive, integrated field management system that unifies all oil and gas engineering, analysis, and operational tools into a single cohesive platform. It uses **PPDM39** as the default data repository model, providing industry-standard data management while integrating all specialized calculation and analysis libraries.

## Key Features

- **Unified Field Management**: Single platform for all field operations
- **PPDM39 Integration**: Industry-standard data model as the foundation
- **Module Integration**: Seamless integration of all Beep.OilandGas projects
- **Real-Time Monitoring**: Real-time data processing and monitoring
- **Comprehensive Analytics**: Integrated analytics and reporting
- **API-First Design**: REST, GraphQL, and gRPC APIs
- **Workflow Engine**: Automated workflows for common operations
- **Event-Driven Architecture**: Event-based inter-module communication

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Field Management System                   │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────┐      ┌──────────────┐      ┌──────────┐ │
│  │   Services   │──────▶│ Integration │──────▶│ Analysis │ │
│  │   Layer      │      │    Layer     │      │  Modules  │ │
│  └──────────────┘      └──────────────┘      └──────────┘ │
│         │                       │                            │
│         │                       │                            │
│         ▼                       ▼                            │
│  ┌──────────────┐      ┌──────────────┐                    │
│  │ Data Access  │──────▶│   PPDM39     │                    │
│  │    Layer     │      │  Repository   │                    │
│  └──────────────┘      └──────────────┘                    │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

## Integrated Modules

### Analysis Modules
- **NodalAnalysis**: Well performance analysis
- **DCA**: Decline curve analysis
- **ProductionForecasting**: Production forecasting
- **WellTestAnalysis**: Well test analysis
- **EconomicAnalysis**: Economic analysis

### Calculation Modules
- **GasProperties**: Gas property calculations
- **OilProperties**: Oil property calculations
- **FlashCalculations**: Phase behavior calculations

### Artificial Lift Modules
- **PumpPerformance**: ESP and pump analysis
- **GasLift**: Gas lift analysis
- **SuckerRodPumping**: Sucker rod pumping
- **PlungerLift**: Plunger lift analysis
- **HydraulicPumps**: Hydraulic pump analysis

### Operational Modules
- **ProductionAccounting**: Production accounting
- **ChokeAnalysis**: Choke flow analysis
- **CompressorAnalysis**: Compressor analysis
- **PipelineAnalysis**: Pipeline analysis

### Visualization Modules
- **Drawing**: Well schematics, logs, reservoir visualization
- **HeatMap**: Heat map visualizations

## Project Structure

```
Beep.OilandGas.FieldManagement/
├── Core/                    # Foundation layer
│   ├── FieldManager.cs
│   ├── DataIntegration/
│   ├── EventBus/
│   └── Configuration/
│
├── Services/                # Business logic layer
│   ├── WellManagementService/
│   ├── ProductionManagementService/
│   ├── ReservoirManagementService/
│   ├── EquipmentManagementService/
│   ├── AnalysisService/
│   └── ReportingService/
│
├── Integration/            # Module integration layer
│   ├── NodalAnalysisIntegration/
│   ├── DCAIntegration/
│   ├── ProductionForecastingIntegration/
│   ├── ArtificialLiftIntegration/
│   ├── AccountingIntegration/
│   └── DrawingIntegration/
│
├── DataAccess/             # Data access layer
│   ├── PPDM39Repository/
│   ├── WellRepository/
│   ├── ProductionRepository/
│   ├── ReservoirRepository/
│   └── QueryBuilders/
│
├── API/                    # API layer
│   ├── REST/
│   ├── GraphQL/
│   └── gRPC/
│
└── Models/                 # Domain models
    ├── WellModels/
    ├── ProductionModels/
    └── AnalysisModels/
```

## Getting Started

### Prerequisites

- .NET 8.0 or later
- SQL Server or compatible database
- All Beep.OilandGas project dependencies

### Installation

```bash
# Clone the repository
git clone <repository-url>

# Navigate to the project
cd Beep.OilandGas.FieldManagement

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

### Configuration

Configure the system in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PPDM39": "Server=localhost;Database=PPDM39;Integrated Security=true;"
  },
  "FieldManagement": {
    "CacheEnabled": true,
    "EventBusEnabled": true,
    "RealTimeProcessingEnabled": true
  }
}
```

## Usage Examples

### Well Management

```csharp
// Get well management service
var wellService = serviceProvider.GetService<IWellManagementService>();

// Create a new well
var well = new WellModel
{
    UWI = "123456789012345",
    WellName = "Well-001",
    WellType = WellType.Oil
};

await wellService.CreateWellAsync(well);

// Get well with analysis
var wellWithAnalysis = await wellService.GetWellWithAnalysisAsync(well.UWI);
```

### Production Management

```csharp
// Get production service
var productionService = serviceProvider.GetService<IProductionManagementService>();

// Record production
var production = new ProductionRecord
{
    WellUWI = "123456789012345",
    Date = DateTime.Now,
    OilProduction = 1000.0,
    GasProduction = 500.0,
    WaterProduction = 200.0
};

await productionService.RecordProductionAsync(production);
```

### Analysis Integration

```csharp
// Get analysis service
var analysisService = serviceProvider.GetService<IAnalysisService>();

// Run nodal analysis
var nodalAnalysis = await analysisService.RunNodalAnalysisAsync(
    wellUWI: "123456789012345",
    options: new NodalAnalysisOptions { ... }
);

// Run DCA
var dcaAnalysis = await analysisService.RunDCAAsync(
    wellUWI: "123456789012345",
    options: new DCAOptions { ... }
);
```

## API Endpoints

### Wells

- `GET /api/v1/wells` - Get all wells
- `GET /api/v1/wells/{id}` - Get well by ID
- `POST /api/v1/wells` - Create new well
- `PUT /api/v1/wells/{id}` - Update well
- `DELETE /api/v1/wells/{id}` - Delete well
- `POST /api/v1/wells/{id}/nodal-analysis` - Run nodal analysis
- `POST /api/v1/wells/{id}/dca` - Run DCA

### Production

- `GET /api/v1/production` - Get production data
- `POST /api/v1/production` - Record production
- `GET /api/v1/production/{wellId}/forecast` - Get production forecast

## Documentation

- [Implementation Plan](./IMPLEMENTATION_PLAN.md) - Comprehensive implementation roadmap
- [API Documentation](./docs/API.md) - API reference documentation
- [Architecture Guide](./docs/ARCHITECTURE.md) - System architecture details
- [Integration Guide](./docs/INTEGRATION.md) - Module integration guide

## Contributing

Please read [CONTRIBUTING.md](./CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE.txt) file for details.

## Support

For support, please open an issue in the repository or contact the development team.

---

**Status**: Planning Phase  
**Version**: 0.1.0  
**Last Updated**: 2024

