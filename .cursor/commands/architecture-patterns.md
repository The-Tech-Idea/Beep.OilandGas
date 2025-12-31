# Architecture Patterns

## Overview

This document outlines architectural patterns for the Beep.OilandGas system, including service layer patterns, API controller patterns, DTO organization, repository patterns, progress tracking, and workflow orchestration.

## System Architecture

### Three-Layer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Web Layer (Blazor Server)                                  │
│  - Razor Pages (.razor)                                     │
│  - Services: DataManagementService, ApiClient              │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTP REST API / SignalR
┌────────────────────▼────────────────────────────────────────┐
│  API Layer (ASP.NET Core Web API)                           │
│  - Controllers                                               │
│  - Services: PPDM39SetupService, ProgressTrackingService    │
└────────────────────┬────────────────────────────────────────┘
                     │ IDMEEditor Interface
┌────────────────────▼────────────────────────────────────────┐
│  Data Layer (Beep Framework)                               │
│  - IDMEEditor                                                │
│  - IDataSource                                               │
│  - PPDMGenericRepository                                    │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│  Database (PPDM39)                                          │
│  - All tables use string IDs                                │
│  - Standard audit columns                                   │
└─────────────────────────────────────────────────────────────┘
```

## Service Layer Patterns

### Service Interface Pattern

```csharp
// Interface in Beep.OilandGas.PPDM39/Core/Interfaces/
public interface IFieldExplorationService
{
    Task<List<object>> GetProspectsForFieldAsync(string fieldId, List<AppFilter> additionalFilters = null);
    Task<object> CreateProspectForFieldAsync(string fieldId, object prospectData, string userId);
}

// Implementation in Beep.OilandGas.PPDM39.DataManagement/Services/
public class PPDMExplorationService : IFieldExplorationService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    
    public async Task<List<object>> GetProspectsForFieldAsync(string fieldId, List<AppFilter> additionalFilters = null)
    {
        // Use PPDMGenericRepository for data access
        var metadata = await _metadata.GetTableMetadataAsync("PROSPECT");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
        var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, "PROSPECT");
            
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "FIELD_ID", FilterValue = _defaults.FormatIdForTable("PROSPECT", fieldId), Operator = "=" }
        };
        
        if (additionalFilters != null)
            filters.AddRange(additionalFilters);
            
        return (await repo.GetAsync(filters)).ToList();
    }
}
```

### Repository Pattern

**Use PPDMGenericRepository for all data access:**

```csharp
// Get table metadata
var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME");
var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");

// Create repository
var repo = new PPDMGenericRepository(
    _editor, 
    _commonColumnHandler, 
    _defaults, 
    _metadata,
    entityType, 
    _connectionName, 
    "TABLE_NAME");

// CRUD operations
var entities = await repo.GetAsync(filters);
var entity = await repo.GetByIdAsync(id);
await repo.InsertAsync(entity, userId);
await repo.UpdateAsync(entity, userId);
await repo.SoftDeleteAsync(id, userId);
```

## API Controller Patterns

### Standard Controller Pattern

```csharp
// In Beep.OilandGas.ApiService/Controllers/Field/ExplorationController.cs
[ApiController]
[Route("api/field/current/exploration")]
public class ExplorationController : ControllerBase
{
    private readonly IFieldOrchestrator _fieldOrchestrator;
    
    [HttpGet("prospects")]
    public async Task<ActionResult<List<object>>> GetProspects()
    {
        var service = _fieldOrchestrator.GetExplorationService();
        var prospects = await service.GetProspectsForFieldAsync(_fieldOrchestrator.CurrentFieldId);
        return Ok(prospects);
    }
    
    [HttpPost("prospects")]
    public async Task<ActionResult<object>> CreateProspect([FromBody] CreateProspectRequest request)
    {
        var service = _fieldOrchestrator.GetExplorationService();
        var prospect = await service.CreateProspectForFieldAsync(
            _fieldOrchestrator.CurrentFieldId, 
            request, 
            User.Identity.Name);
        return CreatedAtAction(nameof(GetProspect), new { id = prospect.GetType().GetProperty("PROSPECT_ID").GetValue(prospect) }, prospect);
    }
}
```

### Field-Scoped Endpoints Pattern

**All phase endpoints are field-scoped under `/api/field/current/{phase}/`:**

- `GET /api/field/current/exploration/prospects` - List prospects for current field
- `POST /api/field/current/exploration/prospects` - Create prospect for current field
- `GET /api/field/current/development/pools` - Get pools for current field
- `GET /api/field/current/production/production` - Get production data for current field

## DTO Location and Structure

### DTO Organization

**All DTOs and interfaces must be in `Beep.OilandGas.PPDM39` project:**

- **DTOs**: `Beep.OilandGas.PPDM39/Core/DTOs/` or `Beep.OilandGas.PPDM39/DTOs/`
- **Interfaces**: `Beep.OilandGas.PPDM39/Core/Interfaces/`
- **Models**: `Beep.OilandGas.PPDM39/Models/` (if needed for non-PPDM-table entities)

### DTO Pattern

```csharp
// Request DTO
public class CreateProspectRequest
{
    public string ProspectName { get; set; }
    public string ProspectType { get; set; }
    public decimal? EstimatedVolume { get; set; }
}

// Response DTO
public class ProspectResponse
{
    public string ProspectId { get; set; }
    public string ProspectName { get; set; }
    public string ProspectType { get; set; }
    public decimal? EstimatedVolume { get; set; }
}

// Convert DTO to Entity
var entity = new PROSPECT
{
    PROSPECT_ID = _defaults.FormatIdForTable("PROSPECT", Guid.NewGuid().ToString()),
    PROSPECT_NAME = request.ProspectName,
    PROSPECT_TYPE = request.ProspectType,
    ESTIMATED_VOLUME = request.EstimatedVolume,
    ACTIVE_IND = "Y",
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow
};

// Convert Entity to DTO
var response = new ProspectResponse
{
    ProspectId = entity.PROSPECT_ID,
    ProspectName = entity.PROSPECT_NAME,
    ProspectType = entity.PROSPECT_TYPE,
    EstimatedVolume = entity.ESTIMATED_VOLUME
};
```

## Progress Tracking (SignalR)

### Progress Tracking Service

```csharp
// In Beep.OilandGas.ApiService/Services/ProgressTrackingService.cs
public class ProgressTrackingService
{
    private readonly IHubContext<ProgressHub> _hubContext;
    
    public async Task UpdateProgress(string operationId, int percentage, string message)
    {
        await _hubContext.Clients.Group(operationId).SendAsync("ProgressUpdate", new ProgressUpdate
        {
            OperationId = operationId,
            Percentage = percentage,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }
}
```

### Progress Tracking Client

```csharp
// In Beep.OilandGas.Web/Services/ProgressTrackingClient.cs
public class ProgressTrackingClient
{
    private HubConnection _hubConnection;
    
    public async Task ConnectAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("/progressHub")
            .Build();
            
        _hubConnection.On<ProgressUpdate>("ProgressUpdate", (update) =>
        {
            ProgressUpdateReceived?.Invoke(update);
        });
        
        await _hubConnection.StartAsync();
    }
    
    public async Task JoinOperationAsync(string operationId)
    {
        await _hubConnection.SendAsync("JoinOperationGroup", operationId);
    }
    
    public event Action<ProgressUpdate> ProgressUpdateReceived;
}
```

## Workflow Orchestration

### Workflow Definition

```csharp
// Workflow definition
public class WorkflowDefinition
{
    public string WorkflowName { get; set; }
    public List<WorkflowStep> Steps { get; set; }
    public bool StopOnError { get; set; }
}

public class WorkflowStep
{
    public string Name { get; set; }
    public string OperationType { get; set; } // ImportCsv, Validate, QualityCheck, etc.
    public Dictionary<string, object> Parameters { get; set; }
    public List<string> Dependencies { get; set; }
}
```

### Workflow Execution

```csharp
// In Beep.OilandGas.ApiService/Services/PPDM39WorkflowService.cs
public class PPDM39WorkflowService
{
    public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync(WorkflowDefinition workflow)
    {
        var workflowId = Guid.NewGuid().ToString();
        await _progressTracking.StartWorkflow(workflowId, workflow.WorkflowName);
        
        foreach (var step in workflow.Steps)
        {
            // Check dependencies
            if (!await CheckDependencies(step.Dependencies))
                continue;
                
            await _progressTracking.UpdateWorkflowStep(workflowId, step.Name, "Running");
            
            // Execute step
            var result = await ExecuteStep(step);
            
            if (!result.Success && workflow.StopOnError)
            {
                await _progressTracking.CompleteWorkflow(workflowId, false);
                return new WorkflowExecutionResult { Success = false, ErrorMessage = result.ErrorMessage };
            }
            
            await _progressTracking.CompleteWorkflowStep(workflowId, step.Name, result.Success);
        }
        
        await _progressTracking.CompleteWorkflow(workflowId, true);
        return new WorkflowExecutionResult { Success = true };
    }
}
```

## Data Management Service Pattern

### Web Layer Service

```csharp
// In Beep.OilandGas.Web/Services/DataManagementService.cs
public class DataManagementService
{
    private readonly ApiClient _apiClient;
    private string _currentConnectionName;
    private string _currentFieldId;
    
    // Connection management
    public async Task<string> GetCurrentConnectionNameAsync()
    {
        if (string.IsNullOrEmpty(_currentConnectionName))
        {
            var response = await _apiClient.GetAsync<CurrentConnectionResponse>("/api/ppdm39/setup/current-connection");
            _currentConnectionName = response?.ConnectionName;
        }
        return _currentConnectionName;
    }
    
    // Field management
    public async Task<string> GetCurrentFieldIdAsync()
    {
        if (string.IsNullOrEmpty(_currentFieldId))
        {
            var response = await _apiClient.GetAsync<FieldResponse>("/api/field/current");
            _currentFieldId = response?.FieldId;
        }
        return _currentFieldId;
    }
    
    public async Task<bool> SetCurrentFieldAsync(string fieldId)
    {
        var response = await _apiClient.PostAsync<SetActiveFieldRequest, SetActiveFieldResponse>(
            "/api/field/set-active", 
            new SetActiveFieldRequest { FieldId = fieldId });
        
        if (response?.Success == true)
        {
            _currentFieldId = fieldId;
            CurrentFieldChanged?.Invoke(fieldId);
            return true;
        }
        return false;
    }
    
    public event Action<string>? CurrentFieldChanged;
}
```

## Key Principles

1. **Separation of Concerns**: Web layer handles UI, API layer handles business logic, Data layer handles data access
2. **Single Source of Truth**: DataManagementService is the main service for connection/field management in Web app
3. **Repository Pattern**: All data access goes through PPDMGenericRepository
4. **DTO Location**: All DTOs and interfaces in `Beep.OilandGas.PPDM39` project
5. **Progress Tracking**: All long-running operations support real-time progress via SignalR
6. **Workflow Orchestration**: Chain multiple operations with dependency management
7. **Field-Scoped Operations**: All phase services automatically filter by current field ID
8. **Standard Audit Columns**: All entities include standard PPDM audit columns

## References

- See `ARCHITECTURE.md` for complete system architecture
- See `field-lifecycle-management.md` for FieldOrchestrator patterns
- See `data-access-patterns.md` for repository patterns
- See `Beep.OilandGas.Web/ARCHITECTURE.md` for Web layer architecture

