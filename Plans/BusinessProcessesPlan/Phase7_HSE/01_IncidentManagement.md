# Phase 7 — Incident Management
## HSE_INCIDENT Table Workflow and Related Tables

---

## Incident Lifecycle States

```
REPORTED → UNDER_INVESTIGATION → PENDING_RCA → PENDING_CORRECTIVE_ACTION
    → PENDING_CLOSE_OUT → CLOSED
    └→ CANCELLED (at any pre-close state)
```

---

## IHSEService Interface

```csharp
public interface IHSEService
{
    Task<HSEIncidentRecord> ReportIncidentAsync(ReportIncidentRequest request, string userId);
    Task AssignInvestigatorAsync(string incidentId, string baId, string userId);
    Task<List<HSEIncidentRecord>> GetFieldIncidentsAsync(string fieldId, DateRangeFilter? range);
    Task<HSEIncidentRecord?> GetByIdAsync(string incidentId);
    Task UpdateTierAsync(string incidentId, int tier, string userId);
    Task<bool> TransitionAsync(string incidentId, string trigger, string? reason, string userId);
}

public record ReportIncidentRequest(
    string FieldId,
    string IncidentType,        // 'SPILL', 'NEAR_MISS', 'INJURY', 'PSE_TIER1', 'PSE_TIER2'
    int Tier,                   // API RP 754 Tier 1–4
    DateTime IncidentDate,
    string Location,
    string Description,
    string Jurisdiction);       // USA / CANADA / INTERNATIONAL
```

---

## HSE_INCIDENT Column Mapping

| PPDM Column | DTO Field | Notes |
|---|---|---|
| `INCIDENT_ID` | `IncidentId` | Formatted: `HSE-{FIELD_ID}-{YYYYMMDD}-{SEQ}` |
| `FIELD_ID` | `FieldId` | FK to `FIELD.FIELD_ID`; partition key |
| `INCIDENT_TYPE` | `IncidentType` | LOV from `R_HSE_INCIDENT_TYPE` |
| `INCIDENT_TIER` | `Tier` | 1–4; API RP 754 |
| `INCIDENT_DATE` | `IncidentDate` | Required |
| `LOCATION_DESC` | `Location` | Free text |
| `INCIDENT_DESC` | `Description` | Long text |
| `JURISDICTION` | `Jurisdiction` | `USA` / `CANADA` / `INTERNATIONAL` |
| `INCIDENT_STATUS` | `CurrentState` | Matches SM state names |
| `INVESTIGATOR_BA_ID` | `InvestigatorId` | FK to `BUSINESS_ASSOCIATE.BA_ID` |
| `ACTIVE_IND` | — | Auto-set to `'Y'` by `CommonColumnHandler` |
| `CREATE_USER` / `ROW_CHANGED_BY` | — | Auto-set by `CommonColumnHandler` |

---

## Related Tables

### HSE_INCIDENT_BA — Involved Parties

| Column | Values | Notes |
|---|---|---|
| `INCIDENT_ID` | FK | Links to parent incident |
| `BA_ID` | FK | Injured/involved person |
| `INVOLVEMENT_TYPE` | `INJURED`, `WITNESS`, `RESPONSIBLE` | LOV |
| `INJURY_TYPE` | `FATALITY`, `LTI`, `RWC`, `MTC`, `FAC`, `NEAR_MISS` | Drives TRIR calc |

### HSE_INCIDENT_CAUSE — Cause Chain

| Column | Values | Notes |
|---|---|---|
| `INCIDENT_ID` | FK | |
| `CAUSE_SEQ` | Integer | Ordered from immediate to root |
| `CAUSE_TYPE` | `IMMEDIATE`, `CONTRIBUTING`, `ROOT` | |
| `CAUSE_DESC` | Free text | |
| `CAUSE_CATEGORY` | `HUMAN`, `EQUIPMENT`, `PROCESS`, `ENVIRONMENT` | |

### HSE_INCIDENT_COMPONENT — Equipment Involved

| Column | Values | Notes |
|---|---|---|
| `INCIDENT_ID` | FK | |
| `EQUIP_ID` | FK to EQUIPMENT | |
| `COMPONENT_TYPE` | `BARRIER_FAILED`, `INITIATING_EVENT`, `DAMAGED` | |

---

## Regulatory Notification Triggers

| Jurisdiction | Condition | Action | Deadline |
|---|---|---|---|
| USA | Tier 1 PSE | Auto-create OBLIGATION row (`OBLIG_TYPE='BSEE_REPORT'`) | Within 24 hours (30 CFR §250.188) |
| USA | Oil spill > 1 barrel | Auto-create `OBLIG_TYPE='NRC_REPORT'` | Immediately (OPA 90) |
| Canada | Incident with injury | Auto-create `OBLIG_TYPE='AER_INCIDENT_REPORT'` | Within 2 hours (AER Directive 070) |
| International | Tier 1/2 per OSPAR | Auto-create `OBLIG_TYPE='OSPAR_REPORT'` | Within 24 hours |

These obligations are created automatically when `TransitionAsync` moves the incident to `UNDER_INVESTIGATION`.
