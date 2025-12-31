# Field Lifecycle Management

## Overview

This document outlines patterns for managing oil field lifecycles using the FieldOrchestrator pattern, which coordinates all phases (Exploration, Development, Production, Decommissioning) for a single active field.

## Core Architecture

### FieldOrchestrator: Central Lifecycle Manager

**Purpose**: Manages the complete lifecycle of a single active field, coordinating all phases. Users work with one field at a time and can switch fields as needed.

**Key Responsibilities:**
1. Field Context Management - Maintains current active field (user can switch fields)
2. Phase Coordination - Coordinates all lifecycle phases for the active field
3. Unified Data Access - Uses PPDMGenericRepository to access all field-related data
4. Cross-Phase Queries - Provides methods to query data across phases for the active field
5. Field State Management - Tracks field lifecycle phase transitions

## FieldOrchestrator Interface

```csharp
// In Beep.OilandGas.PPDM39/Core/Interfaces/IFieldOrchestrator.cs
public interface IFieldOrchestrator
{
    string? CurrentFieldId { get; }
    Task<bool> SetActiveFieldAsync(string fieldId);
    Task<object> GetCurrentFieldAsync();
    Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync();
    
    // Phase service access (scoped to current field)
    IFieldExplorationService GetExplorationService();
    IFieldDevelopmentService GetDevelopmentService();
    IFieldProductionService GetProductionService();
    IFieldDecommissioningService GetDecommissioningService();
    
    // Cross-phase queries for current field
    Task<List<object>> GetFieldWellsAsync(); // All wells across all phases
    Task<FieldStatistics> GetFieldStatisticsAsync();
    Task<FieldTimeline> GetFieldTimelineAsync();
}
```

## Field Context Management

### Setting Active Field

```csharp
// In FieldOrchestrator implementation
public async Task<bool> SetActiveFieldAsync(string fieldId)
{
    // Validate field exists
    var metadata = await _metadata.GetTableMetadataAsync("FIELD");
    var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
    _fieldRepository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
        entityType, _connectionName, "FIELD");
        
    var field = await _fieldRepository.GetByIdAsync(_defaults.FormatIdForTable("FIELD", fieldId));
    if (field == null)
        return false;
        
    _currentFieldId = _defaults.FormatIdForTable("FIELD", fieldId);
    
    // Initialize phase services with field context
    _explorationService = new PPDMExplorationService(...);
    _developmentService = new PPDMDevelopmentService(...);
    _productionService = new PPDMProductionService(...);
    _decommissioningService = new PPDMDecommissioningService(...);
    
    return true;
}
```

### Field-Scoped Operations Pattern

**All phase services automatically include FIELD_ID filter:**

```csharp
// In any field-scoped phase service method
public async Task<List<object>> GetEntitiesForFieldAsync(string fieldId, List<AppFilter> additionalFilters = null)
{
    var filters = new List<AppFilter>
    {
        new AppFilter 
        { 
            FieldName = "FIELD_ID", 
            FilterValue = _defaults.FormatIdForTable("TABLE_NAME", fieldId), 
            Operator = "=" 
        }
    };
    
    if (additionalFilters != null)
        filters.AddRange(additionalFilters);
        
    var repo = new PPDMGenericRepository(...);
    return await repo.GetAsync(filters);
}
```

## Phase Services

### Exploration Service

```csharp
// Interface: IFieldExplorationService
public interface IFieldExplorationService
{
    Task<List<object>> GetProspectsForFieldAsync(string fieldId, List<AppFilter> additionalFilters = null);
    Task<object> CreateProspectForFieldAsync(string fieldId, object prospectData, string userId);
    Task<List<object>> GetSeismicSurveysForFieldAsync(string fieldId);
    Task<List<object>> GetExploratoryWellsForFieldAsync(string fieldId);
}

// Implementation
public class PPDMExplorationService : IFieldExplorationService
{
    public async Task<List<object>> GetProspectsForFieldAsync(string fieldId, List<AppFilter> additionalFilters = null)
    {
        var metadata = await _metadata.GetTableMetadataAsync("PROSPECT");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
        var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "PROSPECT");
            
        // Always filter by field ID
        var filters = new List<AppFilter>
        {
            new AppFilter 
            { 
                FieldName = "FIELD_ID", 
                FilterValue = _defaults.FormatIdForTable("PROSPECT", fieldId), 
                Operator = "=" 
            }
        };
        
        if (additionalFilters != null)
            filters.AddRange(additionalFilters);
            
        return (await repo.GetAsync(filters)).ToList();
    }
}
```

### Development Service

```csharp
// Interface: IFieldDevelopmentService
public interface IFieldDevelopmentService
{
    Task<List<object>> GetPoolsForFieldAsync(string fieldId);
    Task<object> CreatePoolForFieldAsync(string fieldId, object poolData, string userId);
    Task<List<object>> GetDevelopmentWellsForFieldAsync(string fieldId);
    Task<List<object>> GetFacilitiesForFieldAsync(string fieldId);
}

// All methods automatically filter by FIELD_ID
```

### Production Service

```csharp
// Interface: IFieldProductionService (enhance existing IProductionService)
public interface IFieldProductionService
{
    Task<List<object>> GetProductionForFieldAsync(string fieldId);
    Task<object> CreateProductionForecastForFieldAsync(string fieldId, object forecastData, string userId);
    Task<List<object>> GetWellTestsForFieldAsync(string fieldId);
    Task<object> RunProductionOptimizationForFieldAsync(string fieldId);
}

// All methods automatically filter by FIELD_ID (via WELL → FIELD relationship)
```

### Decommissioning Service

```csharp
// Interface: IFieldDecommissioningService
public interface IFieldDecommissioningService
{
    Task<List<object>> GetAbandonedWellsForFieldAsync(string fieldId);
    Task<object> AbandonWellForFieldAsync(string fieldId, string wellId, object abandonmentData, string userId);
    Task<List<object>> GetDecommissionedFacilitiesForFieldAsync(string fieldId);
    Task<object> DecommissionFacilityForFieldAsync(string fieldId, string facilityId, object decommissionData, string userId);
    Task<DecommissioningCostEstimate> EstimateCostsForFieldAsync(string fieldId);
}
```

## API Integration

### FieldOrchestratorController

```csharp
// In Beep.OilandGas.ApiService/Controllers/Field/FieldOrchestratorController.cs
[ApiController]
[Route("api/field")]
public class FieldOrchestratorController : ControllerBase
{
    private readonly IFieldOrchestrator _fieldOrchestrator;
    
    [HttpGet("fields")]
    public async Task<ActionResult<List<FieldListItem>>> GetAllFields()
    {
        // List all fields (for field selection UI)
    }
    
    [HttpPost("set-active")]
    public async Task<ActionResult<SetActiveFieldResponse>> SetActiveField(
        [FromBody] SetActiveFieldRequest request)
    {
        var success = await _fieldOrchestrator.SetActiveFieldAsync(request.FieldId);
        return Ok(new SetActiveFieldResponse { Success = success });
    }
    
    [HttpGet("current")]
    public async Task<ActionResult<FieldResponse>> GetCurrentField()
    {
        var field = await _fieldOrchestrator.GetCurrentFieldAsync();
        return Ok(new FieldResponse { Field = field });
    }
    
    [HttpGet("current/summary")]
    public async Task<ActionResult<FieldLifecycleSummary>> GetCurrentFieldSummary()
    {
        var summary = await _fieldOrchestrator.GetFieldLifecycleSummaryAsync();
        return Ok(summary);
    }
    
    // Delegate to phase services (all scoped to current field)
    [HttpGet("current/exploration/prospects")]
    public async Task<ActionResult<List<object>>> GetCurrentFieldProspects()
    {
        var service = _fieldOrchestrator.GetExplorationService();
        // Automatically filters by current field
        return Ok(await service.GetProspectsForFieldAsync(_fieldOrchestrator.CurrentFieldId));
    }
}
```

### Field-Scoped Endpoints

All phase endpoints are field-scoped under `/api/field/current/{phase}/`:

- `GET /api/field/current/exploration/prospects` - List prospects for current field
- `POST /api/field/current/exploration/prospects` - Create prospect for current field
- `GET /api/field/current/development/pools` - Get pools for current field
- `GET /api/field/current/production/production` - Get production data for current field
- `GET /api/field/current/decommissioning/wells-abandoned` - List abandoned wells for current field

## Web UI Integration

### Field Selection Component

```razor
<!-- Beep.OilandGas.Web/Components/FieldSelector.razor -->
<MudSelect T="string" Label="Active Field" @bind-Value="_selectedFieldId">
    @foreach (var field in _fields)
    {
        <MudSelectItem Value="@field.FieldId">@field.FieldName</MudSelectItem>
    }
</MudSelect>

@code {
    @inject DataManagementService DataManagementService
    
    private async Task OnFieldChanged(string fieldId)
    {
        await DataManagementService.SetCurrentFieldAsync(fieldId);
        // Navigation may be required if field switch requires context reset
    }
}
```

### Field-Scoped Pages

```csharp
// In any phase page (Exploration, Development, Production, etc.)
@inject DataManagementService DataManagementService

protected override async Task OnInitializedAsync()
{
    var currentFieldId = await DataManagementService.GetCurrentFieldIdAsync();
    if (string.IsNullOrEmpty(currentFieldId))
    {
        // Redirect to field selection
        NavigationManager.NavigateTo("/ppdm39/field-select");
        return;
    }
    
    // Load data for current field (automatically filtered)
    await LoadPhaseDataForCurrentField();
}
```

## Cross-Phase Queries

### Get All Wells for Field

```csharp
// In FieldOrchestrator
public async Task<List<object>> GetFieldWellsAsync()
{
    // Query all wells across all phases for current field
    var metadata = await _metadata.GetTableMetadataAsync("WELL");
    var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
    var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
        entityType, _connectionName, "WELL");
        
    var filters = new List<AppFilter>
    {
        new AppFilter 
        { 
            FieldName = "FIELD_ID", 
            FilterValue = _currentFieldId, 
            Operator = "=" 
        }
    };
    
    return (await repo.GetAsync(filters)).ToList();
}
```

### Field Lifecycle Summary

```csharp
public async Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync()
{
    var field = await GetCurrentFieldAsync();
    
    // Get exploration data
    var explorationService = GetExplorationService();
    var prospects = await explorationService.GetProspectsForFieldAsync(_currentFieldId);
    
    // Get development data
    var developmentService = GetDevelopmentService();
    var pools = await developmentService.GetPoolsForFieldAsync(_currentFieldId);
    
    // Get production data
    var productionService = GetProductionService();
    var production = await productionService.GetProductionForFieldAsync(_currentFieldId);
    
    // Get decommissioning data
    var decommissioningService = GetDecommissioningService();
    var abandonedWells = await decommissioningService.GetAbandonedWellsForFieldAsync(_currentFieldId);
    
    return new FieldLifecycleSummary
    {
        Field = field,
        ExplorationProspects = prospects.Count,
        DevelopmentPools = pools.Count,
        ProductionRecords = production.Count,
        AbandonedWells = abandonedWells.Count
    };
}
```

## Data Flow

```
User selects field → FieldOrchestrator.SetActiveFieldAsync()
    ↓
Field validated → Current field ID set
    ↓
Phase services initialized (field-scoped)
    ↓
All subsequent operations automatically filter by FIELD_ID
    ↓
Cross-phase queries use current field context
```

## Key Principles

1. **Single Active Field**: Users work with one field at a time
2. **Field-Scoped Operations**: All phase services automatically filter by FIELD_ID
3. **Unified Data Access**: All data access uses PPDMGenericRepository
4. **Cross-Phase Queries**: FieldOrchestrator provides unified queries across phases
5. **Field Switching**: Users can switch fields, triggering context reset
6. **DTO Location**: All DTOs in `Beep.OilandGas.PPDM39/Core/DTOs/`
7. **Single Database**: All data stored in same PPDM39 database

## References

- See `.cursor/plans/oil_field_lifecycle_implementation_expansion_461a75e4.plan.md` for complete implementation plan
- See `ARCHITECTURE.md` for system architecture
- See `data-access-patterns.md` for PPDMGenericRepository usage

