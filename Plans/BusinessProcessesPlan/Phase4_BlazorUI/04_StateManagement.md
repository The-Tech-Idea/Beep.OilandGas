# Phase 4 — State Management
## ProcessStateContainer, Cascading Parameters, and Data Refresh Patterns

---

## ProcessStateContainer (Scoped Service)

Injected into process pages. Holds the current user's active instance context and notifies components on change.

```csharp
// Beep.OilandGas.Web/Services/Process/ProcessStateContainer.cs

public class ProcessStateContainer
{
    // ---- Active instance ----
    public ProcessInstanceResponse? SelectedInstance { get; private set; }
    public List<TransitionOption>   AvailableTransitions { get; private set; } = [];

    // ---- Field-level list (loaded on dashboard) ----
    public List<ProcessInstanceResponse> FieldInstances { get; private set; } = [];

    // ---- Raised when SelectedInstance or AvailableTransitions changes ----
    public event Action? OnChange;

    public void SetSelectedInstance(ProcessInstanceResponse instance, List<TransitionOption> transitions)
    {
        SelectedInstance     = instance;
        AvailableTransitions = transitions;
        NotifyStateChanged();
    }

    public void UpdateAfterTransition(ProcessInstanceResponse updated, List<TransitionOption> transitions)
    {
        SelectedInstance     = updated;
        AvailableTransitions = transitions;

        // Reflect new state in FieldInstances list without full reload
        var idx = FieldInstances.FindIndex(i => i.InstanceId == updated.InstanceId);
        if (idx >= 0) FieldInstances[idx] = updated;

        NotifyStateChanged();
    }

    public void SetFieldInstances(List<ProcessInstanceResponse> instances)
    {
        FieldInstances = instances;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
```

**Registration in Program.cs**:
```csharp
builder.Services.AddScoped<ProcessStateContainer>();
```

---

## Using the Container in a Page

```razor
@inject ProcessStateContainer ProcessState
@implements IDisposable

protected override async Task OnInitializedAsync()
{
    ProcessState.OnChange += StateHasChanged;
    var instances = await ApiClient.GetAsync<List<ProcessInstanceResponse>>(
        "/api/field/current/process/instances");
    ProcessState.SetFieldInstances(instances ?? []);
}

public void Dispose() => ProcessState.OnChange -= StateHasChanged;
```

---

## TransitionPanel Callback Pattern

The `TransitionPanel` delegates result handling up to the page:

```razor
<!-- ProcessDetail.razor -->
<TransitionPanel Transitions="@ProcessState.AvailableTransitions"
                 InstanceId="@ProcessState.SelectedInstance!.InstanceId"
                 OnTransitionSuccess="@HandleTransitionSuccess"
                 OnGuardFailure="@HandleGuardFailure" />

@code {
    private async Task HandleTransitionSuccess(ProcessInstanceResponse updated)
    {
        var transitions = await ApiClient.GetAsync<List<TransitionOption>>(
            $"/api/field/current/process/{updated.InstanceId}/transitions");
        ProcessState.UpdateAfterTransition(updated, transitions ?? []);
    }

    private async Task HandleGuardFailure(ProcessGuardProblem problem)
    {
        var parameters = new DialogParameters<GuardFailureDialog>
        {
            { x => x.Problem, problem }
        };
        await DialogService.ShowAsync<GuardFailureDialog>("Transition Blocked", parameters);
    }
}
```

---

## Data Refresh Strategy

| Scenario | Method |
|---|---|
| Initial dashboard load | `OnInitializedAsync` calls API and calls `SetFieldInstances` |
| After transition | `UpdateAfterTransition` mutates single item in list — no full reload |
| After process start | Re-navigate to `ProcessDetail`; container `SetSelectedInstance` on load |
| Field changes | `IFieldOrchestrator.OnFieldChanged` event → flush container + reload |

---

## Cascading Parameter: Current Field

The root app layout provides the current field ID as a cascading parameter so all nested components can read it:

```razor
<!-- MainLayout.razor -->
<CascadingValue Value="@FieldOrchestrator.CurrentFieldId" Name="CurrentFieldId" IsFixed="false">
    @Body
</CascadingValue>
```

Consumer:
```razor
[CascadingParameter(Name = "CurrentFieldId")]
public string? CurrentFieldId { get; set; }
```
