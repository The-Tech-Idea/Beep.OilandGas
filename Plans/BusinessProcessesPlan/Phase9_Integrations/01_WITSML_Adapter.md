# Phase 9 — WITSML Adapter
## WITSML 1.4.1 → PPDM Sync for Wells, Casings, and Logs

---

## IWitsmlAdapter Interface

```csharp
public interface IWitsmlAdapter
{
    Task<WitsmlSyncResult> SyncWellAsync(string witsmlWellUid, string fieldId, string userId);
    Task<WitsmlSyncResult> SyncCasingAsync(string witsmlWellUid, string witsmlWellboreUid, string userId);
    Task<WitsmlSyncResult> SyncLogAsync(string witsmlWellUid, string witsmlWellboreUid, string logUid, string userId);
    Task<List<WitsmlWellSummary>> GetAvailableWellsAsync(string serverUrl);
}

public record WitsmlSyncResult(
    bool Success, string TargetTableName, string TargetId,
    int RecordsWritten, string? ErrorMessage);

public record WitsmlWellSummary(
    string Uid, string Name, string Country, string Operator, DateTime? SpudDate);
```

---

## WITSML 1.4.1 → PPDM Well Mapping

WITSML `well` object fields mapped to PPDM `WELL` table:

| WITSML Path | PPDM Column | Notes |
|---|---|---|
| `well/uid` | `WELL.WITSML_UID` (custom ext col) | Store original UID for dedup |
| `well/name` | `WELL.WELL_NAME` | |
| `well/field` | `WELL.FIELD_ID` | Lookup `FIELD.FIELD_NAME` → `FIELD_ID` |
| `well/country` | `WELL.SURFACE_COUNTRY` | ISO 3166 alpha-2 |
| `well/state` | `WELL.SURFACE_STATE` | |
| `well/operator` | `WELL.OPERATOR_BA_ID` | Lookup by BA name |
| `well/spudDate` | `WELL.SPUD_DATE` | ISO 8601 → DATE |
| `well/statusWell` | `WELL.WELL_STATUS` | WITSML vocab → PPDM LOV |
| `well/dTimLastChange` | `WELL.ROW_CHANGED_DATE` | Auto-updated by adapter |

---

## WITSML → PPDM Casing Mapping

WITSML `casing` (wellbore geometry) → `CASING_PROGRAM` table:

| WITSML Path | PPDM Column |
|---|---|
| `casingSchematic/uid` | `CASING_PROGRAM.CASING_ID` |
| `casingSchematic/description` | `CASING_PROGRAM.CASING_DESCR` |
| `casingSchematic/mdBottom/value` | `CASING_PROGRAM.BASE_DEPTH` |
| `casingSchematic/odSection/value` | `CASING_PROGRAM.OUTSIDE_DIAMETER` |
| `casingSchematic/grade` | `CASING_PROGRAM.MATERIAL_GRADE` |

---

## WITSML → PPDM Log Mapping

WITSML `log` → `LOG` table (wireline / MWD logs):

| WITSML Path | PPDM Column |
|---|---|
| `log/uid` | `LOG.LOG_ID` |
| `log/name` | `LOG.LOG_NAME` |
| `log/wellUid` | `LOG.WELL_ID` |
| `log/wellboreUid` | `LOG.WELLBORE_ID` |
| `log/startIndex` | `LOG.TOP_DEPTH` |
| `log/endIndex` | `LOG.BASE_DEPTH` |
| `log/dataDelimiter` | — | Used for parsing curve data |
| `log/logCurveInfo[*]/mnemonic` | `LOG_PARAMETER.LOG_PARAM_NAME` | One row per curve |

---

## Deduplication Strategy

On each sync:
1. Query `WELL WHERE WITSML_UID = @uid AND ACTIVE_IND = 'Y'`
2. If found → call `PPDMGenericRepository.UpdateAsync`
3. If not found → call `PPDMGenericRepository.InsertAsync`

```csharp
var existing = (await _wellRepo.GetAsync(new List<AppFilter> {
    new() { FieldName = "WITSML_UID", Operator = "=", FilterValue = witsmlWellUid }
})).FirstOrDefault();

if (existing != null)
    await _wellRepo.UpdateAsync(mappedWell, userId);
else
    await _wellRepo.InsertAsync(mappedWell, userId);
```

---

## Configuration (appsettings.json)

```json
"Integrations": {
    "WITSML": {
        "ServerUrl": "https://witsml-server.company.com/witsml/store",
        "Username": "{{env:WITSML_USER}}",
        "Password": "{{env:WITSML_PASS}}",
        "Version": "1.4.1",
        "TimeoutSeconds": 30,
        "CircuitBreakerThreshold": 3
    }
}
```

Credentials loaded from environment variables (not hardcoded); `{{env:...}}` is resolved at startup.
