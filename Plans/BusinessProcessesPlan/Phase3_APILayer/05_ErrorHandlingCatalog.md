# Phase 3 — Error Handling Catalog
## HTTP Status Codes, ProblemDetails Format, and Middleware

---

## Error Classification

| Error Type | Source | HTTP Status | Format |
|---|---|---|---|
| Model validation failure | ASP.NET model binding | `400 Bad Request` | `ValidationProblemDetails` |
| Unknown process definition | `ArgumentException` from service | `404 Not Found` | `ProblemDetails` |
| Unknown instance | `ArgumentException` from service | `404 Not Found` | `ProblemDetails` |
| Entity not found | `OperationCanceledException` from service | `404 Not Found` | `ProblemDetails` |
| Unauthorized | Missing JWT / role | `401 Unauthorized` | — |
| Forbidden (role check) | Controller role check | `403 Forbidden` | — |
| Guard condition blocked | `ProcessGuardException` from SM | `422 Unprocessable Entity` | `ProcessGuardProblem` |
| Invalid transition (no path exists) | `InvalidOperationException` from SM | `409 Conflict` | `ProblemDetails` |
| Terminal state — no transitions | `InvalidOperationException` from SM | `409 Conflict` | `ProblemDetails` |
| Concurrency conflict | `DbUpdateConcurrencyException` | `409 Conflict` | `ProblemDetails` |
| Unhandled server error | Exception | `500 Internal Server Error` | `ProblemDetails` (no stack trace in prod) |

---

## ProcessGuardExceptionMiddleware

Maps `ProcessGuardException` → `422 Unprocessable Entity` with structured body.

```csharp
// File: Beep.OilandGas.ApiService/Middleware/ProcessGuardExceptionMiddleware.cs

public class ProcessGuardExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProcessGuardExceptionMiddleware> _logger;

    public ProcessGuardExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next   = next;
        _logger = loggerFactory.CreateLogger<ProcessGuardExceptionMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ProcessGuardException ex)
        {
            _logger.LogWarning(
                "Guard blocked '{Trigger}': {RequiredField} [{RegulatoryRef}]",
                ex.TransitionName, ex.RequiredField, ex.RegulatoryReference);

            context.Response.StatusCode  = 422;
            context.Response.ContentType = "application/problem+json";

            var instanceId = context.Request.RouteValues["instanceId"]?.ToString() ?? "unknown";
            var problem    = new ProcessGuardProblem(ex, instanceId);

            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No transition defined") || ex.Message.Contains("terminal"))
        {
            _logger.LogWarning("Invalid or terminal transition attempted: {Message}", ex.Message);

            context.Response.StatusCode  = 409;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 409,
                Title  = "Transition not allowed",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            context.Response.StatusCode  = 404;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 404,
                Title  = "Not found",
                Detail = ex.Message
            });
        }
    }
}
```

### Registration in Program.cs

```csharp
// Must be registered before UseRouting and UseAuthentication
app.UseMiddleware<ProcessGuardExceptionMiddleware>();
```

---

## Example Error Payloads

### 400 — Missing Required Field

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "ProcessId": ["The ProcessId field is required."],
    "Jurisdiction": ["The Jurisdiction field is required."]
  }
}
```

### 404 — Unknown Process Definition

```json
{
  "title": "Not found",
  "status": 404,
  "detail": "Process definition 'NONEXISTENT' not found"
}
```

### 409 — Terminal State Transition Attempted

```json
{
  "title": "Transition not allowed",
  "status": 409,
  "detail": "No transition defined for (WORK_ORDER, COMPLETED, start)"
}
```

### 422 — Guard Condition Blocked

```json
{
  "title": "Process transition blocked by guard condition",
  "status": 422,
  "detail": "Transition 'start' blocked: Contractor assignment (PROJECT_STEP_BA) is required [IOGP S-501 §3.2]",
  "instance": "/api/field/current/process/WO-20240712-01/transition",
  "transitionName": "start",
  "requiredField": "Contractor assignment (PROJECT_STEP_BA)",
  "regulatoryReference": "IOGP S-501 §3.2",
  "instanceId": "WO-20240712-01"
}
```

---

## Logging Standards

All controller actions use structured logging per the Beep pattern:

```csharp
// On entry
Log.Information("Starting process {ProcessId} for entity {EntityId} in field {FieldId}",
    request.ProcessId, request.EntityId, fieldId);

// On success
Log.Information("Process {InstanceId} started successfully", instance.InstanceId);

// On guard failure (logged in middleware, not controller)
Log.Warning("Guard blocked '{Trigger}' ...");

// On unexpected error
Log.Error(ex, "Unexpected error applying transition {Trigger} to {InstanceId}", trigger, instanceId);
```

Never log `userId` or claim values at `Information` level. Use `Debug` if needed.
