# Phase 9 — OSDU Integration
## OSDU R3 Well Delivery → PPDM WELL / POOL / SEIS_SURVEY Import

---

## IOsduAdapter Interface

```csharp
public interface IOsduAdapter
{
    Task<OsduSyncResult> SyncWellAsync(
        string osduWellId, string fieldId, string userId);

    Task<OsduSyncResult> SyncPoolAsync(
        string osduPoolId, string fieldId, string userId);

    Task<OsduSyncResult> SyncSeismicSurveyAsync(
        string osduSurveyId, string fieldId, string userId);

    Task<List<OsduEntitySummary>> SearchWellsAsync(
        string fieldName, int maxResults = 100);
}

public record OsduSyncResult(
    bool Success, string EntityType, string TargetId,
    string SyncMode,      // 'INSERT' or 'UPDATE'
    string? ErrorMessage);

public record OsduEntitySummary(
    string OsduId, string Name, string Kind, string Status);
```

---

## OSDU R3 Well → PPDM WELL Mapping

OSDU uses the JSON `osdu:wks:master-data--Well:1.0.0` schema:

| OSDU JSON Path | PPDM Column | Notes |
|---|---|---|
| `data.WellID` | `WELL.OSDU_WELL_ID` (ext col) | Used for dedup |
| `data.FacilityName` | `WELL.WELL_NAME` | |
| `data.FacilityState` | `WELL.WELL_STATUS` | Map OSDU vocab to PPDM LOV |
| `data.OperatorOrganisationID` | `WELL.OPERATOR_BA_ID` | Lookup by OSDU org ID |
| `data.CountryID` | `WELL.SURFACE_COUNTRY` | OSDU uses Alpha-2 country codes |
| `data.SpatialLocation.coordinates[0]` | `WELL.SURFACE_LONGITUDE` | GeoJSON [lon, lat] |
| `data.SpatialLocation.coordinates[1]` | `WELL.SURFACE_LATITUDE` | |
| `data.SpudDate` | `WELL.SPUD_DATE` | ISO 8601 |
| `data.WellType` | `WELL.WELL_TYPE` | Map: `EXPLORATION_WELL` → `EXP` |

---

## OSDU Reservoir → PPDM POOL Mapping

OSDU `osdu:wks:master-data--Reservoir:1.0.0`:

| OSDU JSON Path | PPDM Column |
|---|---|
| `data.ReservoirID` | `POOL.POOL_ID` |
| `data.ReservoirName` | `POOL.POOL_NAME` |
| `data.FieldID` | `POOL.FIELD_ID` |
| `data.ReservoirType` | `POOL.POOL_TYPE` |
| `data.FluidPhase` | `POOL.FLUID_TYPE` |

---

## OSDU Seismic Survey → PPDM SEIS_SURVEY Mapping

OSDU `osdu:wks:master-data--SeismicProcessingProject:1.0.0`:

| OSDU JSON Path | PPDM Column |
|---|---|
| `data.ProjectID` | `SEIS_SURVEY.SEIS_SURVEY_ID` |
| `data.ProjectName` | `SEIS_SURVEY.SURVEY_NAME` |
| `data.SurveyType` | `SEIS_SURVEY.SURVEY_TYPE` |
| `data.AcquisitionDate` | `SEIS_SURVEY.ACQ_START_DATE` |
| `data.Area` | `SEIS_SURVEY.SURVEY_AREA` |

---

## OSDU API Authentication

OSDU uses Azure AD MSAL bearer token (client_credentials flow) routed through OSDU's `/api/entitlements` groups:

```json
"Integrations": {
    "OSDU": {
        "BaseUrl": "https://osdu.company.com",
        "TenantId": "{{env:OSDU_TENANT_ID}}",
        "ClientId": "{{env:OSDU_CLIENT_ID}}",
        "ClientSecret": "{{env:OSDU_CLIENT_SECRET}}",
        "DataPartition": "opendes",
        "CircuitBreakerThreshold": 5
    }
}
```

---

## Query Strategy

OSDU Search API (`POST /api/search/v2/query`) filters by `kind` + `data.FieldName`:

```json
{
    "kind": "osdu:wks:master-data--Well:1.0.0",
    "query": "data.FieldName:Permian*",
    "limit": 100
}
```

Results paged with `cursor` token; all pages collected before processing.

---

## On-Demand vs Scheduled Sync

| Mode | Trigger | Use Case |
|---|---|---|
| Scheduled | Daily 04:00 UTC | Routine data refresh from OSDU data lake |
| On-demand | `POST /api/integrations/osdu/sync-well/{osduWellId}` | After new well delivery in OSDU |
| Bulk | `POST /api/integrations/osdu/sync-field/{fieldId}` | Initial field data migration |
