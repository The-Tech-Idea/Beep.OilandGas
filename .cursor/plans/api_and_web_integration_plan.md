# API Service and Web Integration Plan

## Overview
This plan addresses the integration of newly implemented oil & gas calculation services into the ApiService (API controllers) and Web (service clients) projects.

## Current Status

### ApiService (Beep.OilandGas.ApiService)
- ✅ All services registered in `Program.cs` (lines 690-908)
- ✅ Existing controllers: `GasPropertiesController`, `OilPropertiesController`, `FlashCalculationController`, `ProductionOperationsController`
- ✅ **COMPLETE**: All 13 controllers created and integrated

### Web Project (Beep.OilandGas.Web)
- ✅ Generic `ApiClient` service exists
- ✅ Service client pattern established (`LifeCycleService`, `AccountingServiceClient`)
- ✅ **COMPLETE**: All 4 service clients created and registered

## ✅ Implementation Status: COMPLETE

All API controllers and Web service clients have been successfully implemented. See `api-and-web-integration-summary.md` for details.

## Phase 1: Create Missing API Controllers

### 1.1 Controller Pattern
Follow existing pattern from `GasPropertiesController.cs`:
- Use `[ApiController]`, `[Route("api/[controller]")]`, `[Authorize]`
- Inject service interface via constructor
- Use `GetUserId()` helper method for user identification
- Return appropriate HTTP status codes
- Include error handling with logging

### 1.2 Controllers to Create

#### Calculations Controllers
1. **GasLiftController** (`Controllers/Calculations/GasLiftController.cs`)
   - `POST /api/gaslift/analyze-potential` - Analyze gas lift potential
   - `POST /api/gaslift/design-valves` - Design gas lift valves
   - `POST /api/gaslift/design` - Save gas lift design
   - `GET /api/gaslift/performance/{wellUWI}` - Get gas lift performance

2. **NodalAnalysisController** (`Controllers/Calculations/NodalAnalysisController.cs`)
   - `POST /api/nodalanalysis/analyze` - Perform nodal analysis
   - `POST /api/nodalanalysis/optimize` - Optimize system
   - `POST /api/nodalanalysis/result` - Save analysis result
   - `GET /api/nodalanalysis/history/{wellUWI}` - Get analysis history

3. **ProductionForecastingController** (`Controllers/Calculations/ProductionForecastingController.cs`)
   - `POST /api/productionforecasting/generate` - Generate forecast
   - `POST /api/productionforecasting/decline-curve` - Perform decline curve analysis
   - `POST /api/productionforecasting/forecast` - Save forecast

4. **PipelineAnalysisController** (`Controllers/Calculations/PipelineAnalysisController.cs`)
   - `POST /api/pipelineanalysis/analyze-flow` - Analyze pipeline flow
   - `POST /api/pipelineanalysis/pressure-drop` - Calculate pressure drop
   - `POST /api/pipelineanalysis/result` - Save analysis result

5. **EconomicAnalysisController** (`Controllers/Calculations/EconomicAnalysisController.cs`)
   - `POST /api/economicanalysis/npv` - Calculate NPV
   - `POST /api/economicanalysis/irr` - Calculate IRR
   - `POST /api/economicanalysis/analyze` - Comprehensive analysis
   - `POST /api/economicanalysis/npv-profile` - Generate NPV profile
   - `POST /api/economicanalysis/result` - Save analysis result
   - `GET /api/economicanalysis/result/{analysisId}` - Get analysis result

#### Properties Controllers
6. **HeatMapController** (`Controllers/Properties/HeatMapController.cs`)
   - `POST /api/heatmap/generate` - Generate heat map
   - `POST /api/heatmap/configuration` - Save heat map configuration
   - `GET /api/heatmap/configuration/{heatMapId}` - Get heat map configuration
   - `POST /api/heatmap/production` - Generate production heat map

#### Operations Controllers
7. **ProspectIdentificationController** (`Controllers/Operations/ProspectIdentificationController.cs`)
   - `POST /api/prospect/evaluate/{prospectId}` - Evaluate prospect
   - `GET /api/prospect` - Get prospects (with optional filters)
   - `POST /api/prospect` - Create prospect
   - `POST /api/prospect/rank` - Rank prospects

8. **EnhancedRecoveryController** (`Controllers/Operations/EnhancedRecoveryController.cs`)
   - `POST /api/enhancedrecovery/analyze-eor` - Analyze EOR potential
   - `POST /api/enhancedrecovery/recovery-factor` - Calculate recovery factor
   - `POST /api/enhancedrecovery/injection` - Manage injection

9. **LeaseAcquisitionController** (`Controllers/Operations/LeaseAcquisitionController.cs`)
   - `GET /api/lease/evaluate/{leaseId}` - Evaluate lease
   - `GET /api/lease/available` - Get available leases
   - `POST /api/lease` - Create lease acquisition
   - `PUT /api/lease/{leaseId}/status` - Update lease status

10. **DrillingOperationController** (`Controllers/Operations/DrillingOperationController.cs`)
    - `GET /api/drilling/operations` - Get drilling operations
    - `GET /api/drilling/operations/{operationId}` - Get drilling operation
    - `POST /api/drilling/operations` - Create drilling operation
    - `PUT /api/drilling/operations/{operationId}` - Update drilling operation
    - `GET /api/drilling/operations/{operationId}/reports` - Get drilling reports
    - `POST /api/drilling/operations/{operationId}/reports` - Create drilling report

#### Pump Controllers
11. **HydraulicPumpController** (`Controllers/Pumps/HydraulicPumpController.cs`)
    - `POST /api/hydraulicpump/design` - Design pump system
    - `POST /api/hydraulicpump/analyze-performance` - Analyze pump performance
    - `POST /api/hydraulicpump/design/save` - Save pump design
    - `GET /api/hydraulicpump/performance-history/{pumpId}` - Get performance history

12. **PlungerLiftController** (`Controllers/Pumps/PlungerLiftController.cs`)
    - `POST /api/plungerlift/design` - Design plunger lift system
    - `POST /api/plungerlift/analyze-performance` - Analyze performance
    - `POST /api/plungerlift/design/save` - Save plunger lift design

13. **SuckerRodPumpingController** (`Controllers/Pumps/SuckerRodPumpingController.cs`)
    - `POST /api/suckerrodpumping/design` - Design pump system
    - `POST /api/suckerrodpumping/analyze-performance` - Analyze performance
    - `POST /api/suckerrodpumping/design/save` - Save pump design

### 1.3 Request/Response DTOs
Create request DTOs in each controller file (following pattern from `GasPropertiesController`):
- Keep DTOs simple and focused
- Use existing DTOs from `Beep.OilandGas.Models.DTOs` where possible
- Create request DTOs only when needed for API-specific parameters

## Phase 2: Create Web Service Clients

### 2.1 Service Client Pattern
Follow pattern from `LifeCycleService.cs` and `AccountingServiceClient.cs`:
- Create interface (e.g., `ICalculationServiceClient`)
- Create implementation using `ApiClient`
- Use async/await pattern
- Include error handling
- Return strongly-typed DTOs

### 2.2 Service Clients to Create

#### Calculation Service Clients
1. **ICalculationServiceClient** (`Services/CalculationServiceClient.cs`)
   - Methods for: GasLift, NodalAnalysis, ProductionForecasting, PipelineAnalysis, EconomicAnalysis
   - Group related calculation services together

2. **IPropertiesServiceClient** (`Services/PropertiesServiceClient.cs`)
   - Methods for: HeatMap
   - Extend existing properties services if needed

#### Operations Service Clients
3. **IOperationsServiceClient** (`Services/OperationsServiceClient.cs`)
   - Methods for: ProspectIdentification, EnhancedRecovery, LeaseAcquisition, DrillingOperation

4. **IPumpServiceClient** (`Services/PumpServiceClient.cs`)
   - Methods for: HydraulicPump, PlungerLift, SuckerRodPumping

### 2.3 Registration in Web Program.cs
Register all new service clients:
```csharp
builder.Services.AddScoped<ICalculationServiceClient, CalculationServiceClient>();
builder.Services.AddScoped<IPropertiesServiceClient, PropertiesServiceClient>();
builder.Services.AddScoped<IOperationsServiceClient, OperationsServiceClient>();
builder.Services.AddScoped<IPumpServiceClient, PumpServiceClient>();
```

## Phase 3: Implementation Details

### 3.1 API Controller Implementation
For each controller:
1. Create controller class with proper attributes
2. Inject service interface and logger
3. Implement endpoints matching service methods
4. Add `GetUserId()` helper method
5. Include proper error handling
6. Use appropriate HTTP verbs (GET, POST, PUT, DELETE)
7. Return proper status codes (200, 201, 400, 404, 500)

### 3.2 Service Client Implementation
For each service client:
1. Create interface with all service methods
2. Implement interface using `ApiClient`
3. Map service methods to API endpoints
4. Handle serialization/deserialization
5. Include error handling and logging
6. Return strongly-typed results

### 3.3 Error Handling
- Use consistent error response format: `{ error: string, details?: object }`
- Log errors with appropriate level (Error, Warning)
- Return appropriate HTTP status codes
- Include user-friendly error messages

## Phase 4: Testing Considerations

### 4.1 API Testing
- Test each endpoint with valid data
- Test error scenarios (invalid data, missing resources)
- Test authentication/authorization
- Verify proper status codes

### 4.2 Service Client Testing
- Test successful API calls
- Test error handling
- Test serialization/deserialization
- Verify proper exception handling

## Implementation Order

1. **Priority 1: Core Calculation Services**
   - GasLiftController
   - NodalAnalysisController
   - ProductionForecastingController
   - EconomicAnalysisController
   - CalculationServiceClient

2. **Priority 2: Operations Services**
   - ProspectIdentificationController
   - EnhancedRecoveryController
   - LeaseAcquisitionController
   - DrillingOperationController
   - OperationsServiceClient

3. **Priority 3: Pump Services**
   - HydraulicPumpController
   - PlungerLiftController
   - SuckerRodPumpingController
   - PumpServiceClient

4. **Priority 4: Additional Services**
   - PipelineAnalysisController
   - HeatMapController
   - PropertiesServiceClient (if needed)

## Files to Create

### ApiService Controllers (13 files)
- `Beep.OilandGas.ApiService/Controllers/Calculations/GasLiftController.cs`
- `Beep.OilandGas.ApiService/Controllers/Calculations/NodalAnalysisController.cs`
- `Beep.OilandGas.ApiService/Controllers/Calculations/ProductionForecastingController.cs`
- `Beep.OilandGas.ApiService/Controllers/Calculations/PipelineAnalysisController.cs`
- `Beep.OilandGas.ApiService/Controllers/Calculations/EconomicAnalysisController.cs`
- `Beep.OilandGas.ApiService/Controllers/Properties/HeatMapController.cs`
- `Beep.OilandGas.ApiService/Controllers/Operations/ProspectIdentificationController.cs`
- `Beep.OilandGas.ApiService/Controllers/Operations/EnhancedRecoveryController.cs`
- `Beep.OilandGas.ApiService/Controllers/Operations/LeaseAcquisitionController.cs`
- `Beep.OilandGas.ApiService/Controllers/Operations/DrillingOperationController.cs`
- `Beep.OilandGas.ApiService/Controllers/Pumps/HydraulicPumpController.cs`
- `Beep.OilandGas.ApiService/Controllers/Pumps/PlungerLiftController.cs`
- `Beep.OilandGas.ApiService/Controllers/Pumps/SuckerRodPumpingController.cs`

### Web Service Clients (4 files)
- `Beep.OilandGas.Web/Services/CalculationServiceClient.cs`
- `Beep.OilandGas.Web/Services/OperationsServiceClient.cs`
- `Beep.OilandGas.Web/Services/PumpServiceClient.cs`
- `Beep.OilandGas.Web/Services/PropertiesServiceClient.cs` (if needed)

### Web Service Interfaces (4 files)
- `Beep.OilandGas.Web/Services/ICalculationServiceClient.cs`
- `Beep.OilandGas.Web/Services/IOperationsServiceClient.cs`
- `Beep.OilandGas.Web/Services/IPumpServiceClient.cs`
- `Beep.OilandGas.Web/Services/IPropertiesServiceClient.cs` (if needed)

## Notes

- All controllers should follow RESTful conventions
- Use consistent naming: `{ServiceName}Controller`, `I{ServiceName}ServiceClient`
- Maintain consistency with existing code patterns
- Ensure proper authentication/authorization on all endpoints
- Include comprehensive error handling
- Use async/await throughout
- Follow established logging patterns

