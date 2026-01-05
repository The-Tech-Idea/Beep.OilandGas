# Beep.OilandGas.Client - Service Architecture

## Overview

Clean, organized service structure with:
- **Partial classes** for logical separation
- **One folder per domain**
- **Unified remote/local mode** handling

## Entry Point

```csharp
// Remote mode
var app = new BeepOilandGasApp(httpClient, options, authProvider);

// Local mode
var app = new BeepOilandGasApp(serviceProvider, dmeEditor, options);

// Usage - same API for both modes
await app.Connection.SetCurrentConnectionAsync("MyDatabase");
await app.Pumps.DesignHydraulicPumpAsync(request);
await app.Production.CreateForecastAsync(request);
await app.Analysis.AnalyzeCompressorAsync(request);
await app.DataManagement.ValidateEntityAsync("WELL", wellEntity);
await app.DataManagement.GetQualityDashboardAsync();
```

## Folder Structure

```
App/
├── BeepOilandGasApp.cs              # Main entry point
├── IBeepOilandGasApp.cs             # Main interface
├── AppOptions.cs                    # Configuration
├── ServiceAccessMode.cs             # Remote/Local enum
└── Services/
    ├── ServiceBase.cs               # Base class for all services
    │
    ├── Connection/                  # Core
    │   ├── IConnectionService.cs
    │   └── ConnectionService.cs
    │
    ├── Well/                        # Core
    │   ├── IWellService.cs
    │   └── WellService.cs
    │
    ├── DataManagement/              # Core - PPDM39 Data Management (EXPANDED)
    │   ├── IDataManagementService.cs
    │   ├── DataManagementService.cs
    │   ├── DataManagementService.CRUD.cs
    │   ├── DataManagementService.Validation.cs
    │   ├── DataManagementService.Quality.cs
    │   ├── DataManagementService.Versioning.cs
    │   ├── DataManagementService.Audit.cs
    │   ├── DataManagementService.LOV.cs
    │   ├── DataManagementService.FieldMapping.cs
    │   ├── DataManagementService.Jurisdiction.cs
    │   ├── DataManagementService.Setup.cs
    │   ├── DataManagementService.Workflow.cs
    │   └── DataManagementService.Metadata.cs
    │
    ├── Pumps/                       # Equipment
    │   ├── IPumpsService.cs
    │   ├── PumpsService.cs
    │   ├── PumpsService.HydraulicPump.cs
    │   ├── PumpsService.PlungerLift.cs
    │   └── PumpsService.SuckerRodPumping.cs
    │
    ├── Properties/                  # Equipment
    │   ├── IPropertiesService.cs
    │   ├── PropertiesService.cs
    │   ├── PropertiesService.Oil.cs
    │   ├── PropertiesService.Gas.cs
    │   └── PropertiesService.HeatMap.cs
    │
    ├── Calculations/                # Calculations
    │   ├── ICalculationsService.cs
    │   ├── CalculationsService.cs
    │   ├── CalculationsService.Flash.cs
    │   ├── CalculationsService.NodalAnalysis.cs
    │   └── CalculationsService.Economic.cs
    │
    ├── Analysis/                    # Calculations
    │   ├── IAnalysisService.cs
    │   ├── AnalysisService.cs
    │   ├── AnalysisService.Compressor.cs
    │   ├── AnalysisService.Pipeline.cs
    │   ├── AnalysisService.WellTest.cs
    │   ├── AnalysisService.GasLift.cs
    │   ├── AnalysisService.PumpPerformance.cs
    │   └── AnalysisService.Prospect.cs
    │
    ├── Field/                       # Lifecycle
    │   ├── IFieldService.cs
    │   └── FieldService.cs
    │
    ├── LifeCycle/                   # Lifecycle
    │   ├── ILifeCycleService.cs
    │   ├── LifeCycleService.cs
    │   ├── LifeCycleService.Exploration.cs
    │   ├── LifeCycleService.Development.cs
    │   ├── LifeCycleService.Decommissioning.cs
    │   ├── LifeCycleService.WellManagement.cs
    │   ├── LifeCycleService.FacilityManagement.cs
    │   └── LifeCycleService.WorkOrder.cs
    │
    ├── Operations/                  # Operations
    │   ├── IOperationsService.cs
    │   └── OperationsService.cs
    │
    ├── Drilling/                    # Operations
    │   ├── IDrillingService.cs
    │   ├── DrillingService.cs
    │   ├── DrillingService.Operations.cs
    │   └── DrillingService.EnhancedRecovery.cs
    │
    ├── Production/                  # Production
    │   ├── IProductionService.cs
    │   ├── ProductionService.cs
    │   ├── ProductionService.Accounting.cs
    │   ├── ProductionService.Forecasting.cs
    │   └── ProductionService.Operations.cs
    │
    ├── Accounting/                  # Production
    │   ├── IAccountingService.cs
    │   └── AccountingService.cs
    │
    ├── AccessControl/               # Administrative
    │   ├── IAccessControlService.cs
    │   └── AccessControlService.cs
    │
    ├── Permits/                     # Administrative
    │   ├── IPermitsService.cs
    │   └── PermitsService.cs
    │
    └── Lease/                       # Administrative
        ├── ILeaseService.cs
        └── LeaseService.cs
```

## Service Categories

### Core Services (3)
| Service | Description | Partial Files |
|---------|-------------|---------------|
| **Connection** | Connection management | 1 |
| **Well** | Well operations | 1 |
| **DataManagement** | PPDM39 Data Management (CRUD, Validation, Quality, Versioning, Audit, LOV, Workflow, Metadata) | 12 |

### DataManagement Service Details (EXPANDED)

The DataManagement service now covers all PPDM39.DataManagement project capabilities:

| Area | Methods |
|------|---------|
| **CRUD** | GetEntities, GetEntity, InsertEntity, UpdateEntity, DeleteEntity |
| **Validation** | ValidateEntity, ValidateBatch, GetValidationRules, SaveValidationRule |
| **Quality** | CalculateQualityScore, CalculateTableQualityMetrics, FindQualityIssues, GetQualityDashboard, GetQualityTrends |
| **Versioning** | CreateVersion, GetVersions, GetVersion, CompareVersions, RollbackToVersion |
| **Audit** | RecordAccess, GetAccessHistory, GetUserAccessHistory, GetAccessStatistics |
| **LOV** | GetLOV, GetLOVsByType, GetLOVByCode, CreateLOV, UpdateLOV, DeleteLOV, GetReferenceTableData |
| **FieldMapping** | GetFieldMappings, SaveFieldMapping, ApplyFieldMapping |
| **Jurisdiction** | GetJurisdictions, GetJurisdiction, GetJurisdictionRequirements |
| **Setup** | InitializeDatabase, GetDatabaseStatus, RunMigrations, SeedData |
| **Demo** | CreateDemoDatabase, CleanupDemoDatabase, GetDemoDatabaseStatus |
| **Workflow** | StartWorkflow, GetWorkflowStatus, AdvanceWorkflow, GetPendingWorkflows |
| **Metadata** | GetTableMetadata, GetAllTablesMetadata, GetColumnMetadata |

### Equipment Services (2)
| Service | Description | Methods |
|---------|-------------|---------|
| **Pumps** | Hydraulic, plunger lift, sucker rod | DesignHydraulicPump, AnalyzeHydraulicPumpPerformance, OptimizePlungerLift, DesignSuckerRodPumpingSystem |
| **Properties** | Oil, gas, heat map | CalculateOilFormationVolumeFactor, CalculateGasZFactor, GenerateHeatMap |

### Calculation Services (2)
| Service | Description | Methods |
|---------|-------------|---------|
| **Calculations** | Flash, nodal analysis, economic | PerformIsothermalFlash, PerformNodalAnalysis, CalculateNPV, CalculateIRR |
| **Analysis** | Compressor, pipeline, well test, gas lift, pump performance, prospect | AnalyzeCompressor, AnalyzePipeline, AnalyzeBuildUp, DesignGasLift, AnalyzePumpPerformance, IdentifyProspect |

### Lifecycle Services (2)
| Service | Description | Methods |
|---------|-------------|---------|
| **Field** | Legacy field operations | SetCurrentField, GetFieldDetails, GetFieldWells |
| **LifeCycle** | Exploration, development, decommissioning, well/facility management, work orders | CreateExplorationProject, CreateDevelopmentPlan, CreateDecommissioningPlan, GetWellStatus, GetFacilities, CreateWorkOrder |

### Operations Services (2)
| Service | Description | Methods |
|---------|-------------|---------|
| **Operations** | Legacy operations | CreateDrillingOperation, CreateProductionOperation, AnalyzeEnhancedRecovery |
| **Drilling** | Drilling programs, BHA, mud, casing, cementing, enhanced recovery | CreateDrillingProgram, GetBHADesign, GetMudProgram, AnalyzeEOR, GetInjectionPlan |

### Production Services (2)
| Service | Description | Methods |
|---------|-------------|---------|
| **Production** | Production accounting, forecasting, operations | GetProductionVolumes, CalculateRoyalties, CreateForecast, GetDeclineCurve, CreateOperation |
| **Accounting** | Legacy accounting | GetProductionData, CalculateRoyalty, GetCostSummary |

### Administrative Services (3)
| Service | Description | Methods |
|---------|-------------|---------|
| **AccessControl** | Roles, permissions | CheckAssetAccess, GetUserRoles, HasPermission |
| **Permits** | Permits and applications | CreatePermitApplication, GetPermitStatus, SubmitPermit, GetJurisdictionRequirements |
| **Lease** | Lease acquisition | CreateLeaseAcquisition, GetLeaseTerms, GetRoyaltyObligations, TransferLease |

## Total: 16 Services, ~60 Partial Files

## Key Architecture Points

1. **ServiceBase** - Abstract base class containing:
   - HTTP methods (GetAsync, PostAsync, PutAsync, DeleteAsync)
   - AccessMode checking
   - Connection name resolution
   - Authentication header injection
   - Retry logic

2. **Partial Classes** - For large services:
   - Main file contains constructor only
   - Separate files for each domain area
   - DataManagement has 12 partial files covering all PPDM39.DataManagement services

3. **Flat Interface** - All methods directly on service interface (no sub-services)
   - `app.Pumps.DesignHydraulicPumpAsync()`
   - `app.Analysis.AnalyzeCompressorAsync()`
   - `app.DataManagement.ValidateEntityAsync()`

4. **Object Types** - Using `object` for DTOs allows flexibility
   - Can cast to specific types when consuming
   - DTOs defined in Beep.OilandGas.Models project

## Project Coverage

| Project | Client Service |
|---------|---------------|
| CompressorAnalysis | Analysis.Compressor |
| DataManager | DataManagement |
| Decommissioning | LifeCycle.Decommissioning |
| DevelopmentPlanning | LifeCycle.Development |
| DrillingAndConstruction | Drilling.Operations |
| EconomicAnalysis | Calculations.Economic |
| EnhancedRecovery | Drilling.EnhancedRecovery |
| FlashCalculations | Calculations.Flash |
| GasLift | Analysis.GasLift |
| GasProperties | Properties.Gas |
| HydraulicPumps | Pumps.HydraulicPump |
| LeaseAcquisition | Lease |
| LifeCycle | LifeCycle |
| NodalAnalysis | Calculations.NodalAnalysis |
| OilProperties | Properties.Oil |
| PermitsAndApplications | Permits |
| PipelineAnalysis | Analysis.Pipeline |
| PlungerLift | Pumps.PlungerLift |
| **PPDM39.DataManagement** | **DataManagement (CRUD, Validation, Quality, Versioning, Audit, LOV, Workflow, Setup, Metadata)** |
| PPDM39 | DataManagement |
| ProductionAccounting | Production.Accounting |
| ProductionForecasting | Production.Forecasting |
| ProductionOperations | Production.Operations |
| ProspectIdentification | Analysis.Prospect |
| PumpPerformance | Analysis.PumpPerformance |
| SuckerRodPumping | Pumps.SuckerRodPumping |
| WellTestAnalysis | Analysis.WellTest |
