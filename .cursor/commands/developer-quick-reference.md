# Developer Quick Reference - API & Web Service Clients

## Quick Start

### Using Service Clients in Blazor Pages/Components

#### Calculation Services
```csharp
@inject ICalculationServiceClient CalculationService

// Gas Lift Analysis
var result = await CalculationService.AnalyzeGasLiftPotentialAsync(
    wellProperties, minRate, maxRate, numberOfPoints);

// Nodal Analysis
var analysis = await CalculationService.PerformNodalAnalysisAsync(
    wellUWI, analysisParameters);

// Production Forecasting
var forecast = await CalculationService.GenerateForecastAsync(
    wellUWI, fieldId, forecastMethod, forecastPeriod);

// Economic Analysis
var npv = await CalculationService.CalculateNPVAsync(cashFlows, discountRate);
```

#### Operations Services
```csharp
@inject IOperationsServiceClient OperationsService

// Prospect Identification
var prospects = await OperationsService.GetProspectsAsync(filters);
var evaluation = await OperationsService.EvaluateProspectAsync(prospectId);

// Enhanced Recovery
var eorAnalysis = await OperationsService.AnalyzeEORPotentialAsync(
    fieldId, eorMethod);

// Lease Acquisition
var leases = await OperationsService.GetAvailableLeasesAsync(filters);
var leaseId = await OperationsService.CreateLeaseAcquisitionAsync(
    leaseRequest, userId);

// Drilling Operations
var operations = await OperationsService.GetDrillingOperationsAsync(wellUWI);
var operation = await OperationsService.CreateDrillingOperationAsync(createDto);
```

#### Pump Services
```csharp
@inject IPumpServiceClient PumpService

// Hydraulic Pump
var design = await PumpService.DesignHydraulicPumpSystemAsync(
    wellUWI, pumpType, wellDepth, desiredFlowRate);

// Plunger Lift
var plungerDesign = await PumpService.DesignPlungerLiftSystemAsync(
    wellUWI, wellProperties);

// Sucker Rod Pumping
var rodDesign = await PumpService.DesignSuckerRodPumpSystemAsync(
    wellUWI, wellProperties);
```

#### Properties Services
```csharp
@inject IPropertiesServiceClient PropertiesService

// Heat Map
var heatMap = await PropertiesService.GenerateHeatMapAsync(
    dataPoints, configuration);
var productionHeatMap = await PropertiesService.GenerateProductionHeatMapAsync(
    fieldId, startDate, endDate);
```

## Service Client Registration

All service clients are already registered in `Program.cs`:
- `ICalculationServiceClient` → `CalculationServiceClient`
- `IOperationsServiceClient` → `OperationsServiceClient`
- `IPumpServiceClient` → `PumpServiceClient`
- `IPropertiesServiceClient` → `PropertiesServiceClient`

## API Endpoints

### Base URL Pattern
All endpoints follow: `/api/{controller}/{action}`

### Example API Calls (using HttpClient directly)
```csharp
// POST request example
var request = new { WellUWI = "12345", ... };
var response = await httpClient.PostAsJsonAsync(
    "/api/gaslift/analyze-potential", request);
var result = await response.Content.ReadFromJsonAsync<GasLiftPotentialResult>();
```

## Error Handling

### Service Client Error Handling
All service clients include error handling and logging:
```csharp
try
{
    var result = await CalculationService.AnalyzeGasLiftPotentialAsync(...);
    // Use result
}
catch (Exception ex)
{
    // Error is already logged by the service client
    // Handle user-facing error message
}
```

### API Controller Error Responses
All controllers return consistent error format:
```json
{
  "error": "Error message here"
}
```

HTTP Status Codes:
- `200 OK` - Success
- `400 Bad Request` - Invalid input
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## Authentication

All endpoints require authentication. The service clients automatically include the authentication token from the current user session.

## DTOs Location

All DTOs are located in:
- `Beep.OilandGas.Models.DTOs` - Main DTO namespace
- `Beep.OilandGas.Models.DTOs.Lease` - Lease-specific DTOs
- Service-specific model namespaces (e.g., `Beep.OilandGas.GasLift.Models`)

## Common Patterns

### Getting User ID
User ID is automatically extracted from the authenticated user's claims. If you need to pass it explicitly:
```csharp
var userId = "USER_ID_HERE";
await OperationsService.CreateLeaseAcquisitionAsync(leaseRequest, userId);
```

### Filtering Results
Many services support optional filters:
```csharp
var filters = new Dictionary<string, string>
{
    { "status", "active" },
    { "fieldId", "FIELD123" }
};
var results = await OperationsService.GetProspectsAsync(filters);
```

### Saving Results
Most services have save methods that persist results to the database:
```csharp
await CalculationService.SaveNodalAnalysisResultAsync(result, userId);
await CalculationService.SaveForecastAsync(forecast, userId);
```

## File Locations

### API Controllers
- Calculations: `Beep.OilandGas.ApiService/Controllers/Calculations/`
- Operations: `Beep.OilandGas.ApiService/Controllers/Operations/`
- Pumps: `Beep.OilandGas.ApiService/Controllers/Pumps/`
- Properties: `Beep.OilandGas.ApiService/Controllers/Properties/`

### Service Clients
- Interfaces: `Beep.OilandGas.Web/Services/I{ServiceName}ServiceClient.cs`
- Implementations: `Beep.OilandGas.Web/Services/{ServiceName}ServiceClient.cs`

## Additional Resources

- **API Endpoints Reference**: `api-endpoints-reference.md`
- **Integration Summary**: `api-and-web-integration-summary.md`
- **Verification Checklist**: `integration-verification-checklist.md`

## Support

For issues or questions:
1. Check the API endpoints reference for available endpoints
2. Review the integration summary for implementation details
3. Check service client interfaces for method signatures
4. Review controller implementations for request/response formats

