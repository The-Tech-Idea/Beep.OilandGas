# Phase 7 — Barrier Management
## Bow-Tie Model, API RP 754 Tier Influence, and IBarrierService

---

## Bow-Tie Model Overview

The Bow-Tie model describes how barriers (preventative and mitigative) relate to a hazard top event. In PPDM, the Bow-Tie is stored as:

```
Top Event ← HSE_INCIDENT (INCIDENT_TYPE='PSE_TIER1' or 'PSE_TIER2')

LEFT SIDE (Threats → Top Event):
  HSE_INCIDENT_CAUSE rows with CAUSE_TYPE='CONTRIBUTING' or 'IMMEDIATE'
  Each cause row optionally links to a preventing barrier.

RIGHT SIDE (Top Event → Consequences):
  Modeled as PROJECT_STEP rows (STEP_TYPE='CONSEQUENCE') in the CA project.

BARRIERS:
  Stored in EQUIPMENT table (EQUIP_TYPE='SAFETY_BARRIER')
    linked via HSE_INCIDENT_COMPONENT (COMPONENT_TYPE='BARRIER_FAILED')
    or  HSE_INCIDENT_COMPONENT (COMPONENT_TYPE='BARRIER_EFFECTIVE')
```

---

## IBarrierService Interface

```csharp
public interface IBarrierManagementService
{
    Task<List<BarrierRecord>> GetBarriersForIncidentAsync(string incidentId);

    Task AddBarrierAsync(string incidentId, AddBarrierRequest request, string userId);

    Task SetBarrierStatusAsync(
        string incidentId, string equipId, string status, string userId);
        // status: 'EFFECTIVE', 'DEGRADED', 'FAILED', 'NOT_APPLICABLE'

    Task<BarrierSummary> GetBarrierSummaryAsync(string incidentId);
}

public record BarrierRecord(
    string EquipId, string BarrierName, string BarrierType,
    string BarrierSide,     // 'LEFT'=preventive / 'RIGHT'=mitigative
    string Status,
    string? FailureDesc);

public record BarrierSummary(
    int TotalBarriers, int FailedBarriers, int DegradedBarriers,
    int EffectiveBarriers, int API754TierInfluence);

public record AddBarrierRequest(
    string EquipId,
    string BarrierSide,     // LEFT or RIGHT
    string BarrierType,     // PHYSICAL, PROCEDURAL, DETECTION, MITIGATION
    string? FailureDesc);
```

---

## API RP 754 Tier Classification

A Tier classification is influenced by whether prevention barriers failed:

| Scenario | API RP 754 Tier |
|---|---|
| Loss of containment > threshold; all barriers effective → consequence minor | Tier 3 / Tier 4 (lagging) |
| Loss of containment > threshold; 1 prevention barrier failed | Tier 2 Process Safety Event |
| Loss of containment > threshold; ≥ 2 prevention barriers failed | Tier 1 Process Safety Event |
| Near-miss; no loss of containment but all barriers were required | Tier 3 |
| Near-miss; at least one barrier was degraded | Tier 2 per RP 754 Definition 2.3 |

The system auto-suggests a Tier upgrade when `GetBarrierSummaryAsync` returns `FailedBarriers >= 2`:

```csharp
if (summary.FailedBarriers >= 2 && incident.Tier > 1)
{
    _notifications.Warn($"Barrier analysis suggests Tier 1 classification [API RP 754 §2.3]. Current: Tier {incident.Tier}");
}
```

---

## PPDM Mapping for Barrier Data

| PPDM Column | Table | Barrier Use |
|---|---|---|
| `EQUIP_ID` | `EQUIPMENT` | Safety barrier equipment ID |
| `EQUIP_TYPE` | `EQUIPMENT` | `'SAFETY_BARRIER'` |
| `EQUIP_NAME` | `EQUIPMENT` | Barrier description |
| `INCIDENT_ID` | `HSE_INCIDENT_COMPONENT` | Links barrier to incident |
| `EQUIP_ID` | `HSE_INCIDENT_COMPONENT` | FK to EQUIPMENT |
| `COMPONENT_TYPE` | `HSE_INCIDENT_COMPONENT` | `BARRIER_FAILED`, `BARRIER_EFFECTIVE`, `BARRIER_DEGRADED` |
| `COMPONENT_NOTES` | `HSE_INCIDENT_COMPONENT` | Failure description |

---

## Bow-Tie Visualization (Blazor)

```razor
<!-- BowtiePanel.razor is a read-only diagram; full edit via forms above -->
<MudGrid>
    <MudItem xs="4">
        <!-- Left side: threats and prevention barriers -->
        @foreach (var cause in _causes.Where(c => c.CauseType != "ROOT"))
        {
            <MudPaper Class="pa-1 mb-1" Outlined="true">
                <MudText Typo="Typo.caption">@cause.Description</MudText>
            </MudPaper>
        }
    </MudItem>
    <MudItem xs="4">
        <!-- Center: Top Event box -->
        <MudPaper Class="pa-3 mud-background-gray">
            <MudText Align="Align.Center"><b>TOP EVENT</b></MudText>
            <MudText Align="Align.Center" Typo="Typo.caption">@_incident?.IncidentType · Tier @_incident?.Tier</MudText>
        </MudPaper>
    </MudItem>
    <MudItem xs="4">
        <!-- Right side: consequences and mitigation barriers -->
        @foreach (var barrier in _barriers.Where(b => b.BarrierSide == "RIGHT"))
        {
            <MudChip T="string" Icon="@BarrierIcon(barrier.Status)" Color="@BarrierColor(barrier.Status)">
                @barrier.BarrierName
            </MudChip>
        }
    </MudItem>
</MudGrid>
```
