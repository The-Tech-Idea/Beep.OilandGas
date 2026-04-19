# Phase 9 — Integration Health Monitor
## IIntegrationHealthService, Circuit Breaker Pattern, Failure Notifications

---

## IIntegrationHealthService Interface

```csharp
public interface IIntegrationHealthService
{
    // Returns current health snapshot for all adapters
    Task<List<AdapterHealthStatus>> GetAllStatusAsync();

    // Returns detailed status for one adapter
    Task<AdapterHealthStatus> GetStatusAsync(string adapterName);

    // Records a success call (resets circuit breaker failure count)
    void RecordSuccess(string adapterName);

    // Records a failure; may open the circuit breaker
    void RecordFailure(string adapterName, Exception ex);

    // Resets circuit breaker manually (e.g., from admin UI)
    void ResetCircuitBreaker(string adapterName);
}

public record AdapterHealthStatus(
    string AdapterName, CircuitBreakerState State,
    int ConsecutiveFailures, DateTime? LastSuccessAt,
    DateTime? LastFailureAt, string? LastErrorMessage,
    TimeSpan? HalfOpenCooldown);

public enum CircuitBreakerState { Closed, Open, HalfOpen }
```

---

## Circuit Breaker Behavior

Each adapter has its own `CircuitBreakerPolicy`:

| State | Behavior | Transition |
|---|---|---|
| `Closed` | Normal operation; failures counted | → `Open` when failures ≥ threshold |
| `Open` | All calls rejected immediately; NOTIFICATION created | → `HalfOpen` after cooldown (5 min default) |
| `HalfOpen` | One trial call allowed | → `Closed` on success; → `Open` on failure |

```csharp
// In each adapter implementation
try
{
    if (_health.GetStatusAsync(AdapterName).State == CircuitBreakerState.Open)
        throw new IntegrationUnavailableException($"{AdapterName} circuit breaker is OPEN");

    var result = await CallExternalSystemAsync(...);
    _health.RecordSuccess(AdapterName);
    return result;
}
catch (Exception ex) when (ex is not IntegrationUnavailableException)
{
    _health.RecordFailure(AdapterName, ex);
    throw;
}
```

---

## NOTIFICATION Row Created on Circuit Open

When a circuit breaker transitions to `Open`:

```csharp
// IntegrationHealthService.RecordFailure — when threshold reached
await _notificationService.CreateAsync(new CreateNotificationRequest(
    FieldId: null,          // System-level notification
    RecipientRole: "Manager",
    Message: $"Integration {adapterName} circuit breaker OPEN after {failures} consecutive failures. Last error: {ex.Message}",
    Severity: "HIGH",
    Source: "IntegrationHealthMonitor"), userId: "SYSTEM");
```

---

## Health Check Endpoint

ASP.NET Core HealthChecks registered for each adapter:

```csharp
// Program.cs (after adapter services registered)
builder.Services.AddHealthChecks()
    .AddCheck("WITSML",    () => CheckAdapter("WITSML"),    tags: ["integration"])
    .AddCheck("PRODML",    () => CheckAdapter("PRODML"),    tags: ["integration"])
    .AddCheck("OPC-UA",    () => CheckAdapter("OPC-UA"),    tags: ["integration"])
    .AddCheck("SAP",       () => CheckAdapter("SAP"),       tags: ["integration"])
    .AddCheck("OSDU",      () => CheckAdapter("OSDU"),      tags: ["integration"])
    .AddCheck("SharePoint",() => CheckAdapter("SharePoint"),tags: ["integration"]);

app.MapHealthChecks("/health/integrations", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("integration"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

Returns JSON with status per adapter; consumed by monitoring dashboards (Azure Monitor / Grafana).

---

## Integration Dashboard (Blazor)

```razor
@page "/ppdm39/integrations/health"
[Authorize(Roles = "Manager,Admin")]

<MudText Typo="Typo.h5">Integration Health</MudText>

<MudTable Items="_statuses" Dense="true">
    <HeaderContent>
        <MudTh>Adapter</MudTh><MudTh>State</MudTh>
        <MudTh>Last Success</MudTh><MudTh>Failures</MudTh><MudTh>Actions</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.AdapterName</MudTd>
        <MudTd>
            <MudChip T="string" Color="@CircuitColor(context.State)" Size="Size.Small">
                @context.State
            </MudChip>
        </MudTd>
        <MudTd>@context.LastSuccessAt?.ToString("yyyy-MM-dd HH:mm")</MudTd>
        <MudTd Color="@(context.ConsecutiveFailures > 0 ? Color.Warning : Color.Default)">
            @context.ConsecutiveFailures
        </MudTd>
        <MudTd>
            <MudIconButton Icon="@Icons.Material.Filled.Refresh"
                           OnClick="() => ResetBreaker(context.AdapterName)"
                           Disabled="@(context.State == CircuitBreakerState.Closed)" />
        </MudTd>
    </RowTemplate>
</MudTable>
```
