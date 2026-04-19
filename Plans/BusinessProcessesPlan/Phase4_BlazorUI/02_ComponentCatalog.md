# Phase 4 — Component Catalog
## 8 Reusable Blazor Components with Props, Events, and Usage

---

## 1. ProcessStateChip.razor

Displays the current state of a process instance as a colored MudChip.

```razor
<!-- Props -->
[Parameter] public string State { get; set; }  // e.g. "DRAFT", "APPROVED"
[Parameter] public string Size { get; set; } = "Small"

<!-- Output -->
<MudChip T="string" Color="@_color" Size="@_size" Label="true">@State</MudChip>

@code {
    private Color _color => State switch
    {
        "DRAFT"           => Color.Default,
        "PLANNED"         => Color.Info,
        "IN_PROGRESS"     => Color.Primary,
        "UNDER_REVIEW"    => Color.Warning,
        "APPROVED"        => Color.Success,
        "COMPLETED"       => Color.Success,
        "ON_HOLD"         => Color.Warning,
        "CANCELLED"       => Color.Error,
        "CLOSED"          => Color.Dark,
        _                 => Color.Default
    };
}
```

**Used by**: `ProcessDashboard`, `ProcessDetail`, all category-filtered pages.

---

## 2. TransitionPanel.razor

Renders available transition trigger buttons for an instance. Fires event callbacks on success or guard failure.

```razor
[Parameter] public List<TransitionOption> Transitions { get; set; }
[Parameter] public string InstanceId { get; set; }
[Parameter] public EventCallback<ProcessInstanceResponse> OnTransitionSuccess { get; set; }
[Parameter] public EventCallback<ProcessGuardProblem> OnGuardFailure { get; set; }

@foreach (var t in Transitions)
{
    <MudButton Variant="Variant.Filled"
               Color="@GetButtonColor(t)"
               Disabled="@_loading"
               OnClick="@(() => ApplyAsync(t.Trigger))">
        @t.Label
    </MudButton>
}
```

On 422 response, invoke `OnGuardFailure` with deserialized `ProcessGuardProblem`. Parent page shows `GuardFailureDialog`.

---

## 3. ProcessTimeline.razor

Renders a vertical MudTimeline of audit records. Supports "recent" (5 items) and "full history" modes.

```razor
[Parameter] public List<TransitionRecord> Records { get; set; }
[Parameter] public bool ShowAll { get; set; } = false

<MudTimeline TimelineAlign="TimelineAlign.Start">
    @foreach (var r in DisplayRecords)
    {
        <MudTimelineItem Color="@GetColor(r.ToState)" Size="Size.Small">
            <ItemContent>
                <MudText Typo="Typo.caption">@r.AppliedAt.ToString("g")</MudText>
                <MudText Typo="Typo.body2">@r.FromState → <b>@r.ToState</b> (@r.Trigger)</MudText>
                <MudText Typo="Typo.caption" Color="Color.Secondary">@r.AppliedBy</MudText>
                @if (!string.IsNullOrEmpty(r.Reason))
                {
                    <MudText Typo="Typo.caption"><em>@r.Reason</em></MudText>
                }
            </ItemContent>
        </MudTimelineItem>
    }
</MudTimeline>
```

---

## 4. ProcessStartStepper.razor

Three-step MudStepper for launching a new process instance.

```razor
[Parameter] public EventCallback<ProcessInstanceResponse> OnStarted { get; set; }

<!-- Step 1: Select Process Definition (MudSelect from catalog) -->
<!-- Step 2: Select Entity + Jurisdiction -->
<!-- Step 3: Review + Confirm (read-only summary) -->
```

Uses `MudStepper` (check MudBlazor docs for latest API — component name and props may differ across versions). On Step 3 confirm, calls `POST /api/field/current/process/start`. On success, fires `OnStarted`.

---

## 5. GuardFailureDialog.razor

Modal dialog shown when a transition returns 422.

```razor
[Parameter] public ProcessGuardProblem Problem { get; set; }
[CascadingParameter] IMudDialogInstance MudDialog { get; set; }

<MudAlert Severity="Severity.Error">
    @Problem.RequiredField
</MudAlert>
<MudText Typo="Typo.body2">@Problem.Detail</MudText>
@if (!string.IsNullOrEmpty(Problem.RegulatoryReference))
{
    <MudText Typo="Typo.caption">Regulatory reference: @Problem.RegulatoryReference</MudText>
}

<MudButton OnClick="@MudDialog.Close">OK</MudButton>
```

---

## 6. ProcessFilterBar.razor

Toolbar with state, category, and jurisdiction filter chips.

```razor
[Parameter] public EventCallback<ProcessFilterBarState> OnFilterChanged { get; set; }

<!-- State filter: MudChipSet, multi-select, values = all state names -->
<!-- Category filter: MudSelect, values = all 12 category names -->
<!-- Jurisdiction filter: MudSelect, values = USA / CANADA / INTERNATIONAL / All -->
<!-- Reset button clears all -->
```

On any change, fires `OnFilterChanged` with current selections. Parent re-calls API with updated query params.

---

## 7. JurisdictionBadge.razor

Small inline badge for jurisdiction label.

```razor
[Parameter] public string Jurisdiction { get; set; }

<MudChip T="string" Color="@_color" Variant="Variant.Outlined" Size="Size.Small">@Jurisdiction</MudChip>

@code {
    private Color _color => Jurisdiction switch
    {
        "USA"           => Color.Primary,
        "CANADA"        => Color.Secondary,
        "INTERNATIONAL" => Color.Tertiary,
        _               => Color.Default
    };
}
```

---

## 8. ProcessCategoryCard.razor

Card on the catalog page grouping a category's definitions.

```razor
[Parameter] public string CategoryName { get; set; }
[Parameter] public List<ProcessDefinitionResponse> Definitions { get; set; }
[Parameter] public EventCallback<ProcessDefinitionResponse> OnDefinitionSelected { get; set; }

<MudCard>
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@CategoryName</MudText>
            <MudText Typo="Typo.caption">@Definitions.Count definitions</MudText>
        </CardHeaderContent>
    </MudCardHeader>
    <MudCardContent>
        @foreach (var def in Definitions)
        {
            <MudListItem T="ProcessDefinitionResponse" OnClick="() => OnDefinitionSelected.InvokeAsync(def)">
                @def.ProcessName
            </MudListItem>
        }
    </MudCardContent>
</MudCard>
```
