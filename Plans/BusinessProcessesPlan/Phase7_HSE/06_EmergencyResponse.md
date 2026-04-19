# Phase 7 — Emergency Response
## HSE-EMERGENCY-RESP Process, OPA 90 / CEPA Notification Obligations

---

## Process Definition

```
ProcessId: HSE-EMERGENCY-RESP
Category:  HSE

States:
  ACTIVATION → FIELD_RESPONSE_ACTIVE → AGENCY_NOTIFIED
    → CONTAINMENT → REMEDIATION → RESTORATION → CLOSED

Triggers per transition:
  activate         → from ACTIVATION
  deploy_resources → field_response_active
  notify_agencies  → agency_notified
  contain          → containment
  begin_remediation→ remediation
  restore          → restoration
  close            → closed
```

---

## Regulatory Notification Obligations Created on Activation

When the emergency response process transitions out of `ACTIVATION`, the system auto-creates `OBLIGATION` rows for all applicable jurisdictions:

### USA (OPA 90 — Oil Pollution Act 1990)

| Condition | Obligation Created | Deadline |
|---|---|---|
| Oil spill ≥ 1 barrel to navigable water | `OBLIG_TYPE='NRC_REPORT'` | Immediately (§1006) |
| > 10,000 gallons | `OBLIG_TYPE='USCG_NOTIFY'` | Within 24 hours |
| Discharge from offshore facility | `OBLIG_TYPE='BSEE_REPORT'` | Within 24 hours (30 CFR §250.188) |

```csharp
// Auto-created in EmergencyResponseService.ActivateAsync()
if (jurisdiction == "USA" && spillVolume >= 1)
{
    await _obligationService.CreateAsync(new CreateObligationRequest(
        FieldId: fieldId,
        ObligType: "NRC_REPORT",
        DueDate: DateTime.UtcNow,        // Immediate
        Description: "NRC spill notification — OPA 90 §1006",
        JurisdictionCode: "USA"), userId);
}
```

### Canada (CEPA / AER Directive 070)

| Condition | Obligation Created | Deadline |
|---|---|---|
| Any reportable release | `OBLIG_TYPE='ECCC_REPORTABLE_RELEASE'` | Within 2 hours |
| Alberta: spill from well or pipeline | `OBLIG_TYPE='AER_SPILL_REPORT'` | Immediately (AER Directive 070) |
| ECCC: release of listed toxic substance | `OBLIG_TYPE='CEPA_S95_REPORT'` | Within 24 hours |

### International (OSPAR / MARPOL)

| Condition | Obligation Created | Deadline |
|---|---|---|
| Offshore: hydrocarbon release > OSPAR threshold | `OBLIG_TYPE='OSPAR_OFFSHORE_REPORT'` | Within 24 hours |
| Vessel: oil discharge > MARPOL threshold | `OBLIG_TYPE='IMO_MARPOL_REPORT'` | Immediately (MARPOL Annex I) |

---

## IEmergencyResponseService Interface

```csharp
public interface IEmergencyResponseService
{
    Task<string> ActivateAsync(ActivateERRequest request, string userId);

    Task<List<ObligationSummary>> GetNotificationStatusAsync(string processInstanceId);

    Task RecordAgencyContactAsync(
        string instanceId, string obligId, AgencyContactRecord contact, string userId);

    Task<ERStatus> GetCurrentStatusAsync(string instanceId);
}

public record ActivateERRequest(
    string FieldId, string EmergencyType, double? SpillVolumeBarrels,
    string Jurisdiction, string Description);

public record AgencyContactRecord(
    DateTime ContactTime, string AgencyName,
    string ContactPerson, string ContactMethod, string Summary);
```

---

## Emergency Contact Responsibilities

| Role | Activation Responsibility |
|---|---|
| `SafetyOfficer` | Can activate; receives all notifications |
| `Manager` | Notified on activation; approves closeout |
| `ComplianceOfficer` | Owns agency notification obligations |
| `Approver` (senior) | Required to approve remediation → restoration transition |

---

## Emergency Response Status Blazor Page

```razor
@page "/ppdm39/hse/emergency/{InstanceId}"

<MudAlert Severity="Severity.Error" Dense="true">ACTIVE EMERGENCY RESPONSE</MudAlert>

<!-- Timeline of transitions with timestamps -->
<MudTimeline TimelineAlign="TimelineAlign.Start">
    @foreach (var t in _transitions)
    {
        <MudTimelineItem Color="@TimelineColor(t.State)">
            <ItemContent><MudText><b>@t.State</b></MudText></ItemContent>
            <ItemDot><MudIcon Icon="@Icons.Material.Filled.Warning" /></ItemDot>
        </MudTimelineItem>
    }
</MudTimeline>

<!-- Notification obligation status table -->
<MudTable Items="_obligations">
    <!-- Agency | Type | Due | Status | Contact Logged -->
</MudTable>
```
