# Phase 7 — HAZOP / HAZID
## Study Process Definition, IEC 61882 Node-Deviation Table, and PPDM Mapping

---

## Process Overview

```
HSE-HAZOP process definition (ProcessId = "HSE-HAZOP")
    State machine: GATE_REVIEW type
    States: DRAFT → SCOPING → NODE_ANALYSIS → REVIEW → APPROVED → CLOSED

A HAZOP Study is a PROJECT entity; each node/deviation row is a PROJECT_STEP_CONDITION.
```

---

## PPDM Mapping

| PPDM Table | Column | HAZOP Meaning |
|---|---|---|
| `PROJECT` | `PROJECT_ID` | Study ID (e.g., `HAZOP-PERMIAN-2025-01`) |
| `PROJECT` | `PROJECT_NAME` | Study title |
| `PROJECT` | `FIELD_ID` | FK to field being studied |
| `PROJECT` | `PROJECT_TYPE` | `'HAZOP'` |
| `PROJECT_STEP` | `STEP_SEQ` | Node sequence number |
| `PROJECT_STEP` | `STEP_NAME` | Node name (e.g., `Node-3: HP Separator`) |
| `PROJECT_STEP_CONDITION` | `COND_SEQ` | Deviation row sequence |
| `PROJECT_STEP_CONDITION` | `COND_TYPE` | Guide word (NONE, MORE, LESS, REVERSE, OTHER) |
| `PROJECT_STEP_CONDITION` | `COND_DESC` | Deviation description |
| `PROJECT_STEP_CONDITION` | `RESULT_TEXT` | Consequence description |
| `PROJECT_STEP_CONDITION` | `COND_STATUS` | `OPEN`, `ACTION_REQUIRED`, `CLOSED` |
| `PROJECT_STEP` | `STEP_NOTES` | Safeguard description |

---

## IEC 61882 Guide Words

| Guide Word | `COND_TYPE` Value | Meaning |
|---|---|---|
| NONE | `NONE` | Complete negation of intent |
| MORE | `MORE` | Quantitative increase |
| LESS | `LESS` | Quantitative decrease |
| AS WELL AS | `AS_WELL_AS` | Qualitative increase |
| PART OF | `PART_OF` | Qualitative decrease |
| REVERSE | `REVERSE` | Logical opposite of intent |
| OTHER THAN | `OTHER_THAN` | Complete substitution |

---

## Example Node-Deviation Table (Node 3: HP Separator)

| Node | Guide Word | Parameters | Deviation | Causes | Consequences | Safeguards | Status |
|---|---|---|---|---|---|---|---|
| Node-3 | MORE | Pressure | Overpressure | Blocked outlet valve; cooling loss | Separator rupture; gas release Tier 1 | PSV PV-301; ESD | OPEN |
| Node-3 | LESS | Level | Low level | Level transmitter fail; drain valve stuck open | Compressor surge; gas carry-under | LIC-301 low alarm; HAH | CLOSED |
| Node-3 | NONE | Flow | No gas flow | Control valve FV-302 fails closed | Equipment vibration; compressor trip | FIC-302 low alarm | ACTION_REQUIRED |

---

## IHAZOPService Interface

```csharp
public interface IHAZOPService
{
    Task<string> CreateStudyAsync(CreateHAZOPStudyRequest request, string userId);
    Task<List<HAZOPNode>> GetNodesAsync(string studyId);
    Task<string> AddNodeAsync(string studyId, AddNodeRequest request, string userId);
    Task<string> AddDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, string userId);
    Task UpdateDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, string userId);
    Task<HAZOPSummary> GetSummaryAsync(string studyId);
}

public record HAZOPSummary(
    string StudyId, string StudyName, int TotalNodes, int TotalDeviations,
    int OpenDeviations, int ActionRequired, int Closed);
```

---

## HAZOP Study Blazor Page

```razor
@page "/ppdm39/hse/hazop/{StudyId}"
@attribute [Authorize]

<!-- Study header -->
<MudText Typo="Typo.h5">@_study?.ProjectName</MudText>
<MudText Typo="Typo.caption">@_summary?.TotalNodes nodes · @_summary?.OpenDeviations open deviations</MudText>

<!-- Node accordion -->
@foreach (var node in _nodes)
{
    <MudExpansionPanel Text="@node.StepName">
        <!-- Deviation table per node -->
        <MudTable Items="node.Deviations" Dense="true">
            <!-- Columns: Guide Word | Deviation | Consequences | Safeguards | Status -->
        </MudTable>
        <MudButton OnClick="AddDeviationDialog">Add Deviation</MudButton>
    </MudExpansionPanel>
}
```
