# Phase 7 — Corrective Actions
## PROJECT_STEP Corrective Action Lifecycle, Assignment, and Deadline Tracking

---

## Corrective Action Model

Each corrective action is stored as a `PROJECT_STEP` row linked to an incident via the `PROJECT.PROJECT_ID` = incident's associated WO/project.

```
HSE_INCIDENT (PENDING_CORRECTIVE_ACTION state)
    │
    └── PROJECT (CA project, PROJECT_TYPE='CORRECTIVE_ACTION')
            │
            ├── PROJECT_STEP (row 1: CA-001 fix valve sealing)
            │       └── PROJECT_STEP_BA (responsible person)
            │           PROJECT_STEP_TIME (due date, completion date)
            │
            ├── PROJECT_STEP (row 2: CA-002 revise procedure)
            │
            └── PROJECT_STEP (row 3: CA-003 conduct retraining)
```

---

## ICorrectiveActionService Interface

```csharp
public interface ICorrectiveActionService
{
    Task<string> CreateCAPlanAsync(string incidentId, string userId);

    Task<string> AddCorrectiveActionAsync(
        string incidentId, AddCARequest request, string userId);

    Task AssignResponsiblePersonAsync(
        string incidentId, int stepSeq, string baId, string userId);

    Task SetDueDateAsync(
        string incidentId, int stepSeq, DateTime dueDate, string userId);

    Task RecordCompletionAsync(
        string incidentId, int stepSeq, string completionNotes, string userId);

    Task<bool> AllCAsMeetDeadlineAsync(string incidentId);

    Task<List<CAStatus>> GetCAStatusAsync(string incidentId);
}

public record AddCARequest(
    string CADescription,
    string CAType,          // ENGINEERING, PROCEDURAL, TRAINING, MANAGEMENT
    DateTime DueDate,
    string ResponsibleBaId);

public record CAStatus(
    int StepSeq, string Description, string CAType,
    string Status, DateTime DueDate, bool IsOverdue,
    string? ResponsiblePerson, DateTime? CompletedDate);
```

---

## Close-Out Guard

The incident cannot advance from `PENDING_CORRECTIVE_ACTION` to `PENDING_CLOSE_OUT` unless all corrective actions are completed or formally deferred:

```csharp
machine.Configure(PENDING_CORRECTIVE_ACTION)
    .Permit("cas_complete", PENDING_CLOSE_OUT)
    .Guard(() => _caService.AllCAsMeetDeadlineAsync(instanceId).GetAwaiter().GetResult(),
           "All corrective actions must be COMPLETED or formally deferred before close-out [IOGP 2022e §6.1]");
```

---

## PPDM Column Mapping

| PPDM Column | Table | CA Meaning |
|---|---|---|
| `PROJECT_ID` | `PROJECT_STEP` | CA plan project ID |
| `STEP_SEQ` | `PROJECT_STEP` | CA sequence within plan |
| `STEP_NAME` | `PROJECT_STEP` | CA description (max 255 chars) |
| `STEP_TYPE` | `PROJECT_STEP` | `'CORRECTIVE_ACTION'` |
| `STEP_STATUS` | `PROJECT_STEP` | `OPEN`, `IN_PROGRESS`, `COMPLETED`, `DEFERRED` |
| `BA_ID` | `PROJECT_STEP_BA` | Responsible person |
| `STEP_BA_TYPE` | `PROJECT_STEP_BA` | `'RESPONSIBLE'` |
| `PLAN_END_DATE` | `PROJECT_STEP_TIME` | Due date |
| `ACT_END_DATE` | `PROJECT_STEP_TIME` | Actual completion date |
| `STEP_NOTES` | `PROJECT_STEP` | Completion notes |

---

## Corrective Actions Blazor Component

```razor
<!-- CorrectiveActionsPanel.razor -->
[Parameter] public string IncidentId { get; set; }

<MudTable Items="_caStatuses" Dense="true">
    <HeaderContent>
        <MudTh>#</MudTh><MudTh>Description</MudTh><MudTh>Type</MudTh>
        <MudTh>Due</MudTh><MudTh>Responsible</MudTh><MudTh>Status</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.StepSeq</MudTd>
        <MudTd>@context.Description</MudTd>
        <MudTd><MudChip T="string" Size="Size.Small">@context.CAType</MudChip></MudTd>
        <MudTd>
            <MudText Color="@(context.IsOverdue ? Color.Error : Color.Default)">
                @context.DueDate.ToString("yyyy-MM-dd")
            </MudText>
        </MudTd>
        <MudTd>@context.ResponsiblePerson</MudTd>
        <MudTd><ProcessStateChip State="@context.Status" /></MudTd>
    </RowTemplate>
</MudTable>

<MudButton OnClick="AddCaDialog" StartIcon="@Icons.Material.Filled.Add">Add Corrective Action</MudButton>
```

---

## Escalation Logic

CA overdue escalation runs as a background check triggered by `IHostedService` daily:

1. Query all `PROJECT_STEP` rows with `STEP_TYPE='CORRECTIVE_ACTION'` and `PLAN_END_DATE < TODAY` and `STEP_STATUS != 'COMPLETED'`
2. For each overdue CA, create a `NOTIFICATION` row (per PPDM NOTIFICATION table) addressed to `PROJECT_STEP_BA` responsible person
3. If 7 days overdue, escalate `NOTIFICATION` to the incident's `INVESTIGATOR_BA_ID`
