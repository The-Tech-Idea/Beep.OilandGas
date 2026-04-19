# Phase 4 — MudBlazor Patterns
## Verified Component Usage for Process Engine UI

> **IMPORTANT**: MudBlazor evolves frequently. Always verify component names and parameter names against the official docs at https://mudblazor.com before implementation. The patterns below reflect MudBlazor 8.x conventions — confirm they apply to your version.

---

## MudDataGrid for Process Dashboard

```razor
<MudDataGrid T="ProcessInstanceResponse"
             Items="@ProcessState.FieldInstances"
             SortMode="SortMode.Multiple"
             Filterable="true"
             Hideable="true"
             Dense="true"
             Striped="true">
    <Columns>
        <PropertyColumn Property="x => x.InstanceName" Title="Name" />
        <TemplateColumn Title="State" SortBy="x => x.CurrentState">
            <CellTemplate>
                <ProcessStateChip State="@context.Item.CurrentState" />
            </CellTemplate>
        </TemplateColumn>
        <PropertyColumn Property="x => x.ProcessId" Title="Process" />
        <TemplateColumn Title="Jurisdiction">
            <CellTemplate>
                <JurisdictionBadge Jurisdiction="@context.Item.Jurisdiction" />
            </CellTemplate>
        </TemplateColumn>
        <PropertyColumn Property="x => x.StartDate" Title="Started" Format="yyyy-MM-dd" />
        <TemplateColumn>
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.OpenInNew"
                               Size="Size.Small"
                               OnClick="() => Nav.NavigateTo($\"/ppdm39/process/{context.Item.InstanceId}\")" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
</MudDataGrid>
```

---

## MudStepper for ProcessStartStepper

```razor
<MudStepper @ref="_stepper" NonLinear="false" LinearStepSelect="true">
    <MudStep Title="Select Process" HasError="@_step1HasError">
        <MudSelect T="ProcessDefinitionResponse"
                   Label="Process Definition"
                   @bind-Value="_selectedDefinition">
            @foreach (var def in _definitions)
            {
                <MudSelectItem Value="def">@def.ProcessName</MudSelectItem>
            }
        </MudSelect>
    </MudStep>

    <MudStep Title="Entity & Jurisdiction">
        <MudTextField @bind-Value="_entityId" Label="Entity ID" Required="true" />
        <MudSelect T="string" Label="Jurisdiction" @bind-Value="_jurisdiction">
            <MudSelectItem Value='"USA"'>United States</MudSelectItem>
            <MudSelectItem Value='"CANADA"'>Canada</MudSelectItem>
            <MudSelectItem Value='"INTERNATIONAL"'>International</MudSelectItem>
        </MudSelect>
    </MudStep>

    <MudStep Title="Review & Confirm">
        <!-- Summary display only -->
        <MudText><b>Process:</b> @_selectedDefinition?.ProcessName</MudText>
        <MudText><b>Entity:</b> @_entityId</MudText>
        <MudText><b>Jurisdiction:</b> @_jurisdiction</MudText>
    </MudStep>
</MudStepper>

<MudButton OnClick="ConfirmAsync" Color="Color.Primary" Variant="Variant.Filled"
           Disabled="_loading || !IsComplete">Start Process</MudButton>
```

**Note**: `MudStepper` API (e.g. `@ref`, `NonLinear`, step navigation methods) — check MudBlazor docs for current API. The component was heavily revised around MudBlazor 7.x.

---

## MudAlert for Guard Failure (inline, no dialog)

Alternative to `GuardFailureDialog` for simple single-field guards:

```razor
@if (_guardProblem != null)
{
    <MudAlert Severity="Severity.Error" CloseIconClicked="@(() => _guardProblem = null)">
        <MudText><b>Transition blocked:</b> @_guardProblem.RequiredField</MudText>
        <MudText Typo="Typo.caption">@_guardProblem.RegulatoryReference</MudText>
    </MudAlert>
}
```

Use `MudAlert` (no dialog) when the guard failure is simple (single required field). Use `GuardFailureDialog` when the failure has multi-line detail or regulatory context that warrants focus.

---

## MudChip / MudChipSet for Filter Bar

```razor
<MudChipSet T="string" MultiSelection="true" @bind-SelectedValues="_selectedStates"
            SelectionChanged="OnFilterChanged">
    @foreach (var state in _allStates)
    {
        <MudChip T="string" Value="@state">@state</MudChip>
    }
</MudChipSet>
```

**Note**: `MudChipSet` `SelectedValues` vs `SelectedChips` naming varies by version. Check docs.

---

## MudTimeline for ProcessTimeline Component

```razor
<MudTimeline TimelineAlign="TimelineAlign.Start" TimelineOrientation="TimelineOrientation.Vertical">
    @foreach (var record in Records.Take(ShowAll ? int.MaxValue : 5))
    {
        <MudTimelineItem Color="@GetColor(record.ToState)"
                         Icon="@Icons.Material.Filled.SwapHoriz"
                         Size="Size.Small">
            <ItemContent>
                <MudText Typo="Typo.body2">
                    <b>@record.FromState</b> → <b>@record.ToState</b>
                </MudText>
                <MudText Typo="Typo.caption">@record.AppliedAt:g · @record.AppliedBy</MudText>
            </ItemContent>
        </MudTimelineItem>
    }
</MudTimeline>
```

---

## MudDialog for GuardFailureDialog

```razor
<!-- GuardFailureDialog.razor -->
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6" Color="Color.Error">Transition Blocked</MudText>
    </TitleContent>
    <DialogContent>
        <MudAlert Severity="Severity.Warning">@Problem.RequiredField</MudAlert>
        <MudText Class="mt-2">@Problem.Detail</MudText>
        @if (!string.IsNullOrEmpty(Problem.RegulatoryReference))
        {
            <MudText Typo="Typo.caption" Class="mt-1">
                Regulatory reference: <i>@Problem.RegulatoryReference</i>
            </MudText>
        }
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="@(() => MudDialog.Close())">Dismiss</MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public ProcessGuardProblem Problem { get; set; } = default!;
}
```

**Note**: `IMudDialogInstance` is the correct interface type in MudBlazor 7+. Prior versions used `MudDialogInstance`. Verify before implementation.
