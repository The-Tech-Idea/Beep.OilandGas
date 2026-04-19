# Phase 6 — Inspection Framework
## PROJECT_STEP_CONDITION Checklist Items and BSEE SEMS Compliance

---

## PPDM Tables

| Table | Column | Purpose |
|---|---|---|
| `PROJECT_STEP_CONDITION` | `PROJECT_ID`, `STEP_SEQ`, `COND_SEQ`, `COND_TYPE`, `COND_STATUS`, `RESULT_TEXT`, `INSPECT_DATE` | Inspection items per step |
| `PROJECT_STEP` | `PROJECT_ID`, `STEP_SEQ`, `STEP_NAME`, `STEP_TYPE` | WO steps owning conditions |

---

## Inspection Item Types

| `COND_TYPE` | Meaning | Applies To |
|---|---|---|
| `PRE_START` | Pre-start safety check | All WO types with IN_PROGRESS state |
| `OPERATIONAL` | In-process operational check | Turnaround in MAINTENANCE sub-phase |
| `CLOSE_OUT` | Close-out / post-work verification | All WO types before COMPLETED |
| `REGULATORY` | Regulatory-mandated checklist item | WO-REGULATORY, WO-SAFETY |
| `SCE_VERIFY` | Safety Critical Element verification | WO-SAFETY, WO-TURNAROUND |

---

## BSEE SEMS §250.1920–.1932 Checklist Items

These items must be present as `PROJECT_STEP_CONDITION` rows on every `WO-TURNAROUND` before releasing to IN_PROGRESS (SHUTDOWN sub-phase):

| COND_SEQ | COND_TYPE | Required Text | Regulatory Reference |
|---|---|---|---|
| 1 | `PRE_START` | Process isolation verified (LOTO applied) | SEMS §250.1920(a) |
| 2 | `PRE_START` | Permits to Work issued | SEMS §250.1921 |
| 3 | `PRE_START` | Hydrogen sulfide (H2S) monitor calibrated | SEMS §250.1922 |
| 4 | `PRE_START` | Emergency shutdown system (ESD) tested | SEMS §250.1923 |
| 5 | `PRE_START` | Fire and gas detection armed | SEMS §250.1924 |
| 6 | `PRE_START` | Evacuation routes confirmed clear | SEMS §250.1925 |
| 7 | `SCE_VERIFY` | BOP/PSV test results recorded | SEMS §250.1926 |
| 8 | `SCE_VERIFY` | All SCEs on safe failure inventory checked | SEMS §250.1927–.1928 |
| 9 | `CLOSE_OUT` | Post-work pressure test documented | SEMS §250.1930 |
| 10 | `CLOSE_OUT` | Piping restoration and leak check | SEMS §250.1931 |
| 11 | `CLOSE_OUT` | All permits closed and HSE sign-off complete | SEMS §250.1932 |
| 12 | `REGULATORY` | BSEE notification filed (OCS-G form) | 30 CFR §250.144 |

---

## IInspectionService Interface

```csharp
public interface IInspectionService
{
    /// <summary>
    /// Seed the standard inspection checklist for the WO type.
    /// Called automatically when a WO transitions to PLANNED.
    /// </summary>
    Task SeedChecklistAsync(string instanceId, string woType, string jurisdiction, string userId);

    /// <summary>
    /// Record result for a single inspection item.
    /// </summary>
    Task RecordResultAsync(string instanceId, int condSeq, string result, string? notes, string userId);

    /// <summary>
    /// Returns true only when all required conditions are PASS.
    /// Used as a state machine guard.
    /// </summary>
    Task<bool> AllConditionsPassedAsync(string instanceId, IEnumerable<string> condTypes);

    Task<List<InspectionCondition>> GetChecklistAsync(string instanceId);
}

public record InspectionCondition(
    int CondSeq, string CondType, string RequiredText, string Status,
    string? ResultText, DateTime? InspectDate, string? RegulatoryRef);
```

---

## Guard Wiring for Close-Out

```csharp
machine.Configure(IN_PROGRESS)
    .Permit(COMPLETE_TRIGGER, COMPLETED)
    .Guard(() => _inspectionService
                    .AllConditionsPassedAsync(instanceId, ["PRE_START", "CLOSE_OUT", "SCE_VERIFY"])
                    .GetAwaiter().GetResult(),
           "All inspection checklist items must be PASS before completing the work order [SEMS §250.1932]");
```

---

## Inspection Checklist Blazor Component

```razor
<!-- InspectionChecklist.razor -->
[Parameter] public string InstanceId { get; set; }

<MudTable Items="_items" Dense="true">
    <RowTemplate>
        <MudTd>@context.CondSeq</MudTd>
        <MudTd>
            <MudChip T="string" Color="@CondTypeColor(context.CondType)" Size="Size.Small">
                @context.CondType
            </MudChip>
        </MudTd>
        <MudTd>@context.RequiredText</MudTd>
        <MudTd>
            <ProcessStateChip State="@context.Status" />
        </MudTd>
        <MudTd>
            @if (context.Status != "PASS")
            {
                <MudIconButton Icon="@Icons.Material.Filled.Check"
                               Color="Color.Success"
                               OnClick="() => RecordPassAsync(context.CondSeq)" />
            }
        </MudTd>
    </RowTemplate>
</MudTable>
```
