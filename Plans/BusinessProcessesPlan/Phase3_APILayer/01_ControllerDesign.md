# Phase 3 — Controller Design
## All 4 ASP.NET Core Controller Classes

> All controllers go in `Beep.OilandGas.ApiService/Controllers/Process/`  
> Route base: `/api/field/current/process` (instance-scoped) and `/api/process` (definition catalog)  
> All endpoints require `[Authorize]`

---

## Controller 1: ProcessDefinitionController

**File**: `ProcessDefinitionController.cs`  
**Route**: `/api/process`

```csharp
[ApiController]
[Route("api/process")]
[Authorize]
public class ProcessDefinitionController : ControllerBase
{
    private readonly IProcessService _service;
    private readonly ILogger<ProcessDefinitionController> _logger;

    public ProcessDefinitionController(IProcessService service, ILoggerFactory loggerFactory)
    {
        _service = service;
        _logger  = loggerFactory.CreateLogger<ProcessDefinitionController>();
    }
```

### Endpoints

| Method | Route | Description | Request | Success Response |
|---|---|---|---|---|
| `GET` | `/api/process/definitions` | All 96 process definitions | — | `200 List<ProcessDefinitionResponse>` |
| `GET` | `/api/process/definitions/{processId}` | Single definition by ProcessId | Path: `processId` | `200 ProcessDefinitionResponse` / `404` |
| `GET` | `/api/process/definitions/by-category/{category}` | Definitions for one category (1–12) | Path: `category` | `200 List<ProcessDefinitionResponse>` |
| `GET` | `/api/process/definitions/by-type/{processType}` | Definitions sharing a state machine type | Path: `processType` | `200 List<ProcessDefinitionResponse>` |
| `GET` | `/api/process/types` | Distinct state machine type strings | — | `200 List<string>` |

### Action Method Signatures

```csharp
    [HttpGet("definitions")]
    [ProducesResponseType(typeof(List<ProcessDefinitionResponse>), 200)]
    public async Task<ActionResult<List<ProcessDefinitionResponse>>> GetAllDefinitionsAsync()

    [HttpGet("definitions/{processId}")]
    [ProducesResponseType(typeof(ProcessDefinitionResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProcessDefinitionResponse>> GetDefinitionByIdAsync(string processId)

    [HttpGet("definitions/by-category/{category}")]
    [ProducesResponseType(typeof(List<ProcessDefinitionResponse>), 200)]
    public async Task<ActionResult<List<ProcessDefinitionResponse>>> GetDefinitionsByCategoryAsync(string category)

    [HttpGet("definitions/by-type/{processType}")]
    [ProducesResponseType(typeof(List<ProcessDefinitionResponse>), 200)]
    public async Task<ActionResult<List<ProcessDefinitionResponse>>> GetDefinitionsByTypeAsync(string processType)

    [HttpGet("types")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public async Task<ActionResult<List<string>>> GetProcessTypesAsync()
}
```

---

## Controller 2: ProcessInstanceController

**File**: `ProcessInstanceController.cs`  
**Route**: `/api/field/current/process`

All operations automatically scope to `IFieldOrchestrator.CurrentFieldId`.

```csharp
[ApiController]
[Route("api/field/current/process")]
[Authorize]
public class ProcessInstanceController : ControllerBase
{
    private readonly IProcessService _service;
    private readonly IFieldOrchestrator _fieldOrchestrator;
    private readonly ILogger<ProcessInstanceController> _logger;
```

### Endpoints

| Method | Route | Description | Request Body | Success Response |
|---|---|---|---|---|
| `POST` | `/api/field/current/process/start` | Start a new process instance | `StartProcessRequest` | `201 ProcessInstanceResponse` |
| `GET` | `/api/field/current/process` | All instances for current field | Query: `?processType=&state=&category=` | `200 List<ProcessSummaryDto>` |
| `GET` | `/api/field/current/process/{instanceId}` | Single instance detail | Path: `instanceId` | `200 ProcessInstanceResponse` / `404` |
| `GET` | `/api/field/current/process/by-entity/{entityId}` | Instances for an entity (well, facility) | Path: `entityId` | `200 List<ProcessSummaryDto>` |
| `GET` | `/api/field/current/process/active` | All non-terminal instances | — | `200 List<ProcessSummaryDto>` |
| `DELETE` | `/api/field/current/process/{instanceId}` | Cancel a non-terminal instance | Body: `CancelProcessRequest` | `200 ProcessInstanceResponse` / `409 if terminal` |

### Action Method Signatures

```csharp
    [HttpPost("start")]
    [ProducesResponseType(typeof(ProcessInstanceResponse), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<ActionResult<ProcessInstanceResponse>> StartAsync([FromBody] StartProcessRequest request)
    {
        var userId  = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        Log.Information("Starting process {ProcessId} for entity {EntityId} in field {FieldId}",
            request.ProcessId, request.EntityId, fieldId);
        try
        {
            var instance = await _service.StartProcessAsync(
                request.ProcessId, request.EntityId, request.EntityTableName,
                fieldId, request.Jurisdiction, userId, request.InstanceName);
            return CreatedAtAction(nameof(GetByIdAsync), new { instanceId = instance.InstanceId },
                ProcessInstanceResponse.From(instance));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProcessSummaryDto>), 200)]
    public async Task<ActionResult<List<ProcessSummaryDto>>> GetForFieldAsync(
        [FromQuery] string? processType = null,
        [FromQuery] string? state       = null,
        [FromQuery] string? category    = null)

    [HttpGet("{instanceId}")]
    [ProducesResponseType(typeof(ProcessInstanceResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProcessInstanceResponse>> GetByIdAsync(string instanceId)

    [HttpGet("by-entity/{entityId}")]
    [ProducesResponseType(typeof(List<ProcessSummaryDto>), 200)]
    public async Task<ActionResult<List<ProcessSummaryDto>>> GetByEntityAsync(string entityId)

    [HttpGet("active")]
    [ProducesResponseType(typeof(List<ProcessSummaryDto>), 200)]
    public async Task<ActionResult<List<ProcessSummaryDto>>> GetActiveAsync()

    [HttpDelete("{instanceId}")]
    [ProducesResponseType(typeof(ProcessInstanceResponse), 200)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ProcessInstanceResponse>> CancelAsync(
        string instanceId, [FromBody] CancelProcessRequest request)
}
```

---

## Controller 3: ProcessTransitionController

**File**: `ProcessTransitionController.cs`  
**Route**: `/api/field/current/process/{instanceId}/transition`

This controller is dedicated to applying state machine transitions.  
`ProcessGuardException` is mapped to `422 Unprocessable Entity` by middleware.

```csharp
[ApiController]
[Route("api/field/current/process/{instanceId}/transition")]
[Authorize]
public class ProcessTransitionController : ControllerBase
{
    private readonly IProcessService _service;
    private readonly ILogger<ProcessTransitionController> _logger;
```

### Endpoints

| Method | Route | Description | Request Body | Success Response |
|---|---|---|---|---|
| `POST` | `/api/field/current/process/{instanceId}/transition` | Apply a named transition | `ApplyTransitionRequest` | `200 ProcessInstanceResponse` |
| `GET` | `/api/field/current/process/{instanceId}/transition/available` | List available triggers | Path: `instanceId` | `200 List<TransitionOption>` |

### Action Method Signatures

```csharp
    [HttpPost]
    [ProducesResponseType(typeof(ProcessInstanceResponse), 200)]
    [ProducesResponseType(typeof(ProcessGuardProblem), 422)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProcessInstanceResponse>> ApplyTransitionAsync(
        string instanceId,
        [FromBody] ApplyTransitionRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        Log.Information("Applying transition {Trigger} to instance {InstanceId} by {UserId}",
            request.Trigger, instanceId, userId);

        // ProcessGuardException bubbles up to ProcessGuardExceptionMiddleware → 422
        var updated = await _service.ApplyTransitionAsync(instanceId, request.Trigger, userId, request.Reason);
        return Ok(ProcessInstanceResponse.From(updated));
    }

    [HttpGet("available")]
    [ProducesResponseType(typeof(List<TransitionOption>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<TransitionOption>>> GetAvailableTransitionsAsync(string instanceId)
    {
        var transitions = await _service.GetAvailableTransitionsAsync(instanceId);
        return Ok(transitions);
    }
}
```

---

## Controller 4: ProcessAuditController

**File**: `ProcessAuditController.cs`  
**Route**: `/api/field/current/process/{instanceId}/audit`

Read-only endpoint for compliance and audit trail access.

```csharp
[ApiController]
[Route("api/field/current/process/{instanceId}/audit")]
[Authorize]
public class ProcessAuditController : ControllerBase
{
    private readonly IProcessService _service;
    private readonly ILogger<ProcessAuditController> _logger;
```

### Endpoints

| Method | Route | Description | Query Parameters | Response |
|---|---|---|---|---|
| `GET` | `/api/field/current/process/{instanceId}/audit` | Full audit trail | `?from=&to=` (ISO dates) | `200 List<ProcessTransitionRecord>` |
| `GET` | `/api/field/current/process/{instanceId}/audit/summary` | Summary (state counts, last transition) | — | `200 ProcessAuditSummary` |
| `GET` | `/api/field/current/process/audit/by-user/{userId}` | All transitions by a user (field-scoped) | `?from=&to=` | `200 List<ProcessTransitionRecord>` |

### Action Method Signatures

```csharp
    [HttpGet]
    [ProducesResponseType(typeof(List<ProcessTransitionRecord>), 200)]
    public async Task<ActionResult<List<ProcessTransitionRecord>>> GetAuditTrailAsync(
        string instanceId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null)

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ProcessAuditSummary), 200)]
    public async Task<ActionResult<ProcessAuditSummary>> GetAuditSummaryAsync(string instanceId)

    [HttpGet("/api/field/current/process/audit/by-user/{auditUserId}")]
    [ProducesResponseType(typeof(List<ProcessTransitionRecord>), 200)]
    public async Task<ActionResult<List<ProcessTransitionRecord>>> GetByUserAsync(
        string auditUserId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null)
}
```

---

## Route Summary Table

| Controller | Method | Full Route | Auth Role Required |
|---|---|---|---|
| Definitions | GET | `/api/process/definitions` | Any authenticated |
| Definitions | GET | `/api/process/definitions/{processId}` | Any authenticated |
| Definitions | GET | `/api/process/definitions/by-category/{category}` | Any authenticated |
| Definitions | GET | `/api/process/definitions/by-type/{processType}` | Any authenticated |
| Definitions | GET | `/api/process/types` | Any authenticated |
| Instances | POST | `/api/field/current/process/start` | `ProcessOperator` or `Manager` |
| Instances | GET | `/api/field/current/process` | Any authenticated |
| Instances | GET | `/api/field/current/process/{instanceId}` | Any authenticated |
| Instances | GET | `/api/field/current/process/by-entity/{entityId}` | Any authenticated |
| Instances | GET | `/api/field/current/process/active` | Any authenticated |
| Instances | DELETE | `/api/field/current/process/{instanceId}` | `ProcessOperator` or `Manager` |
| Transitions | POST | `/api/field/current/process/{instanceId}/transition` | `ProcessOperator` (role varies per SM) |
| Transitions | GET | `/api/field/current/process/{instanceId}/transition/available` | Any authenticated |
| Audit | GET | `/api/field/current/process/{instanceId}/audit` | `Auditor` or `Manager` |
| Audit | GET | `/api/field/current/process/{instanceId}/audit/summary` | Any authenticated |
| Audit | GET | `/api/field/current/process/audit/by-user/{userId}` | `Auditor` or `Manager` |
