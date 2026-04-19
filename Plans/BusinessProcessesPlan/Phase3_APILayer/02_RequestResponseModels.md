# Phase 3 — Request & Response Models
## Typed API Models with Validation

> All models live in `Beep.OilandGas.Models/Data/Process/`  
> Validation uses `System.ComponentModel.DataAnnotations`  
> No separate DTO namespace — per CLAUDE.md convention

---

## Request Models

### StartProcessRequest

```csharp
/// <summary>Request body for POST /api/field/current/process/start</summary>
public class StartProcessRequest
{
    /// <summary>Stable process definition ID. Example: "GATE-3-FID", "WO-TURNAROUND".</summary>
    [Required]
    [StringLength(50)]
    public string ProcessId { get; set; } = string.Empty;

    /// <summary>ID of the entity this process operates on (WellId, FacilityId, etc.).</summary>
    [Required]
    [StringLength(100)]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>PPDM table the entity belongs to. Example: "WELL", "FACILITY", "POOL".</summary>
    [Required]
    [StringLength(50)]
    public string EntityTableName { get; set; } = string.Empty;

    /// <summary>Jurisdiction tag for this instance. Examples: "USA", "CANADA", "UK_NORTH_SEA".</summary>
    [Required]
    [StringLength(30)]
    public string Jurisdiction { get; set; } = string.Empty;

    /// <summary>Optional human-readable label. Auto-generated from ProcessName + date if omitted.</summary>
    [StringLength(200)]
    public string? InstanceName { get; set; }
}
```

### ApplyTransitionRequest

```csharp
/// <summary>Request body for POST /api/field/current/process/{instanceId}/transition</summary>
public class ApplyTransitionRequest
{
    /// <summary>Trigger name matching _transitionRegistry. Example: "start", "approve", "close".</summary>
    [Required]
    [StringLength(50)]
    public string Trigger { get; set; } = string.Empty;

    /// <summary>Optional reason or comment for this transition. Required by some guards.</summary>
    [StringLength(1000)]
    public string? Reason { get; set; }
}
```

### CancelProcessRequest

```csharp
/// <summary>Request body for DELETE /api/field/current/process/{instanceId}</summary>
public class CancelProcessRequest
{
    /// <summary>Reason for cancellation. Required by all cancellation guards.</summary>
    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}
```

---

## Response Models

### ProcessDefinitionResponse

```csharp
/// <summary>View model for a process definition (catalog entry).</summary>
public class ProcessDefinitionResponse
{
    public string ProcessId              { get; set; } = string.Empty;
    public string ProcessName            { get; set; } = string.Empty;
    public string ProcessType            { get; set; } = string.Empty;
    public string ProcessCategory        { get; set; } = string.Empty;
    public string JurisdictionTags       { get; set; } = string.Empty;
    public int MinApproverCount          { get; set; }
    public bool RequiresDocuments        { get; set; }
    public List<string> RequiredDocumentTypes { get; set; } = new();
    public List<ProcessStepSummary> Steps { get; set; } = new();

    public static ProcessDefinitionResponse From(ProcessDefinition d) => new()
    {
        ProcessId          = d.ProcessId,
        ProcessName        = d.ProcessName,
        ProcessType        = d.ProcessType,
        ProcessCategory    = d.ProcessCategory,
        JurisdictionTags   = d.JurisdictionTags,
        MinApproverCount   = d.MinApproverCount,
        RequiresDocuments  = d.RequiresDocuments,
        RequiredDocumentTypes = d.RequiredDocumentTypes,
        Steps = d.Steps.Select(s => new ProcessStepSummary
        {
            StepId     = s.StepId,
            StepName   = s.StepName,
            Sequence   = s.Sequence,
            InboundTrigger = s.InboundTrigger,
            PrimaryPPDMTable = s.PrimaryPPDMTable
        }).ToList()
    };
}

public class ProcessStepSummary
{
    public string StepId           { get; set; } = string.Empty;
    public string StepName         { get; set; } = string.Empty;
    public int    Sequence         { get; set; }
    public string InboundTrigger   { get; set; } = string.Empty;
    public string PrimaryPPDMTable { get; set; } = string.Empty;
}
```

### ProcessInstanceResponse

```csharp
/// <summary>Full view model for a process instance with current state and metadata.</summary>
public class ProcessInstanceResponse
{
    public string   InstanceId      { get; set; } = string.Empty;
    public string   ProcessId       { get; set; } = string.Empty;
    public string   ProcessName     { get; set; } = string.Empty;
    public string   ProcessType     { get; set; } = string.Empty;
    public string   InstanceName    { get; set; } = string.Empty;
    public string   CurrentState    { get; set; } = string.Empty;
    public string   EntityId        { get; set; } = string.Empty;
    public string   EntityTableName { get; set; } = string.Empty;
    public string   FieldId         { get; set; } = string.Empty;
    public string   Jurisdiction    { get; set; } = string.Empty;
    public DateTime? StartDate      { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string   InitiatedBy     { get; set; } = string.Empty;
    public string?  Notes           { get; set; }

    /// <summary>Triggers available from the current state.</summary>
    public List<TransitionOption> AvailableTransitions { get; set; } = new();

    public static ProcessInstanceResponse From(ProcessInstance i) => new()
    {
        InstanceId       = i.InstanceId,
        ProcessId        = i.ProcessId,
        InstanceName     = i.InstanceName,
        ProcessType      = /* populated by service */ string.Empty,
        CurrentState     = i.CurrentState,
        EntityId         = i.EntityId,
        EntityTableName  = i.EntityTableName,
        FieldId          = i.FieldId,
        Jurisdiction     = i.Jurisdiction,
        StartDate        = i.StartDate,
        CompletionDate   = i.CompletionDate,
        InitiatedBy      = i.InitiatingUserId,
        Notes            = i.Notes
    };
}
```

### TransitionOption

```csharp
/// <summary>A trigger that is currently available for a process instance.</summary>
public class TransitionOption
{
    /// <summary>Trigger name to pass to ApplyTransitionRequest.Trigger.</summary>
    public string Trigger     { get; set; } = string.Empty;

    /// <summary>UI-friendly label.</summary>
    public string Label       { get; set; } = string.Empty;

    /// <summary>Resulting state if this transition is applied.</summary>
    public string TargetState { get; set; } = string.Empty;

    /// <summary>True if this transition has a guard that may block it.</summary>
    public bool HasGuard      { get; set; }

    /// <summary>Regulatory reference shown in tooltip. Empty if no guard.</summary>
    public string RegulatoryRef { get; set; } = string.Empty;
}
```

### ProcessGuardProblem

Returned as the response body when `ProcessGuardException` is caught by middleware.  
HTTP status: `422 Unprocessable Entity`.

```csharp
/// <summary>
/// Problem details response for a blocked state machine transition.
/// Extends RFC 7807 ProblemDetails with process-specific fields.
/// </summary>
public class ProcessGuardProblem : ProblemDetails
{
    public ProcessGuardProblem(ProcessGuardException ex, string instanceId)
    {
        Status   = 422;
        Title    = "Process transition blocked by guard condition";
        Detail   = ex.Message;
        Instance = $"/api/field/current/process/{instanceId}/transition";

        Extensions["transitionName"]      = ex.TransitionName;
        Extensions["requiredField"]        = ex.RequiredField;
        Extensions["regulatoryReference"]  = ex.RegulatoryReference;
        Extensions["instanceId"]           = instanceId;
    }
}
```

### ProcessAuditSummary

```csharp
/// <summary>Summary view of a process instance's audit history.</summary>
public class ProcessAuditSummary
{
    public string   InstanceId         { get; set; } = string.Empty;
    public int      TotalTransitions   { get; set; }
    public string   InitialState       { get; set; } = string.Empty;
    public string   CurrentState       { get; set; } = string.Empty;
    public DateTime? FirstTransitionAt { get; set; }
    public DateTime? LastTransitionAt  { get; set; }
    public string   LastTransitionBy   { get; set; } = string.Empty;
    public string   LastTrigger        { get; set; } = string.Empty;

    /// <summary>Count of times each state was entered.</summary>
    public Dictionary<string, int> StateVisitCounts { get; set; } = new();
}
```

---

## Model Validation Summary

| Model | Validation Rules | HTTP result if violated |
|---|---|---|
| `StartProcessRequest` | `ProcessId` required, max 50; `EntityId` required, max 100; `Jurisdiction` required | `400 Bad Request` with `ValidationProblemDetails` |
| `ApplyTransitionRequest` | `Trigger` required, max 50 | `400 Bad Request` |
| `CancelProcessRequest` | `Reason` required, max 1000 | `400 Bad Request` |

Validation is performed automatically by ASP.NET Core model binding before the action method is reached.
