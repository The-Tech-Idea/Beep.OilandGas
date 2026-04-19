# Phase 7 — Root Cause Analysis
## RCA Methodology, Bow-Tie Summary, and PPDM Column Mapping

---

## RCA Methodology: 5-Why + Cause-and-Effect

Phase 7 implements a simplified 5-Why cause chain stored in `HSE_INCIDENT_CAUSE`. The Bow-Tie model (full barrier mapping) is in Phase 7 — [05_BarrierManagement.md](05_BarrierManagement.md).

### Cause Chain Structure

```
Incident (HSE_INCIDENT)
  │
  ├── CAUSE_SEQ=1, CAUSE_TYPE='IMMEDIATE'     — What directly caused the incident?
  ├── CAUSE_SEQ=2, CAUSE_TYPE='CONTRIBUTING'  — Why did that happen?
  ├── CAUSE_SEQ=3, CAUSE_TYPE='CONTRIBUTING'  — Why did that happen?
  ├── CAUSE_SEQ=4, CAUSE_TYPE='ROOT'          — What is the root systemic cause?
  └── CAUSE_SEQ=5, CAUSE_TYPE='ROOT'          — Is there a deeper root cause?
```

---

## IRCAService Interface

```csharp
public interface IRCAService
{
    Task<List<CauseFinding>> GetCauseChainAsync(string incidentId);

    Task AddCauseAsync(string incidentId, AddCauseRequest request, string userId);

    Task UpdateCauseAsync(string incidentId, int causeSeq, UpdateCauseRequest request, string userId);

    Task<bool> IsRCACompleteAsync(string incidentId);
}

public record AddCauseRequest(
    string CauseType,       // IMMEDIATE / CONTRIBUTING / ROOT
    string CauseDesc,
    string CauseCategory,   // HUMAN / EQUIPMENT / PROCESS / ENVIRONMENT
    int Seq);               // 1-based; 1 = most immediate

public record CauseFinding(
    int Seq, string CauseType, string Category, string Description);
```

---

## RCA Completion Guard

The incident cannot advance from `PENDING_RCA` to `PENDING_CORRECTIVE_ACTION` unless:
1. At least one `ROOT` cause exists in `HSE_INCIDENT_CAUSE`
2. The `CAUSE_DESC` of the ROOT cause is non-empty and ≥ 20 characters (prevents placeholder entries)

```csharp
machine.Configure(PENDING_RCA)
    .Permit("rca_complete", PENDING_CORRECTIVE_ACTION)
    .Guard(() => _rcaService.IsRCACompleteAsync(instanceId).GetAwaiter().GetResult(),
           "Root cause analysis must include at least one ROOT cause before advancing [IOGP 2022e §5.2]");
```

---

## PPDM Column Mapping: HSE_INCIDENT_CAUSE

| PPDM Column | Maps To | Constraint |
|---|---|---|
| `INCIDENT_ID` | `IncidentId` | FK |
| `CAUSE_SEQ` | `AddCauseRequest.Seq` | Integer 1–99 |
| `CAUSE_TYPE` | `AddCauseRequest.CauseType` | LOV: `R_HSE_CAUSE_TYPE` |
| `CAUSE_DESC` | `AddCauseRequest.CauseDesc` | Max 2000 chars |
| `CAUSE_CATEGORY` | `AddCauseRequest.CauseCategory` | LOV: `R_HSE_CAUSE_CATEGORY` |
| `ROW_CREATED_BY` | auto | `CommonColumnHandler` |
| `ACTIVE_IND` | auto `'Y'` | `CommonColumnHandler` |

---

## Fault Tree Node Types (for reference documentation)

Stored as structured `CAUSE_DESC` JSON when `CAUSE_CATEGORY='PROCESS'` and the operator wants formal Fault Tree documentation:

```json
{
  "faultTreeNode": {
    "type": "AND_GATE",
    "eventName": "Gas release from flange",
    "probability": 0.003,
    "children": ["Seal degradation", "Overpressure event"]
  }
}
```

This JSON is stored in `CAUSE_DESC` (CLOB column) as an optional advanced mode. The primary workflow uses plain text; Fault Tree JSON is formatted in the UI if the field contains a valid JSON object.

---

## RCA Form (Blazor)

```razor
<!-- RCAPanel.razor -->
[Parameter] public string IncidentId { get; set; }

@foreach (var cause in _causes.OrderBy(c => c.Seq))
{
    <MudPaper Class="pa-2 mb-2" Elevation="1">
        <MudGrid Spacing="1">
            <MudItem xs="1"><MudText><b>Why @cause.Seq</b></MudText></MudItem>
            <MudItem xs="2">
                <MudChip T="string" Color="@CauseTypeColor(cause.CauseType)" Size="Size.Small">
                    @cause.CauseType
                </MudChip>
            </MudItem>
            <MudItem xs="9"><MudText>@cause.Description</MudText></MudItem>
        </MudGrid>
    </MudPaper>
}
<MudButton OnClick="AddCauseDialog" StartIcon="@Icons.Material.Filled.Add">Add Cause</MudButton>
```
