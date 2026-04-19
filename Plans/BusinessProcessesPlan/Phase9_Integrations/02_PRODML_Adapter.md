# Phase 9 — PRODML Adapter
## PRODML 2.2 → PDEN_VOL_SUMMARY / PDEN_VOL_DISPOSITION Sync

---

## IProdmlAdapter Interface

```csharp
public interface IProdmlAdapter
{
    Task<ProdmlSyncResult> SyncMonthlyVolumesAsync(
        string prodmlEndpoint, string fieldId, int year, int month, string userId);

    Task<ProdmlSyncResult> SyncDailyAllocationsAsync(
        string prodmlEndpoint, string fieldId, DateTime date, string userId);

    Task<List<ProdmlWellSummary>> GetAvailableWellsAsync(string prodmlEndpoint);
}

public record ProdmlSyncResult(
    bool Success, int VolumeRowsWritten, int DispositionRowsWritten,
    DateRange PeriodCovered, string? ErrorMessage);

public record ProdmlWellSummary(string Uid, string WellName, string Status);
```

---

## PRODML 2.2 → PPDM Volume Mapping

PRODML 2.2 uses a hierarchical "FluidReport" structure. Key mapping:

### FluidReport → PDEN_VOL_SUMMARY

| PRODML Path | PPDM Column | Notes |
|---|---|---|
| `fluidReport/wellProduction/{well}/uid` | `PDEN_VOL_SUMMARY.WELL_ID` | Lookup via `WELL.WITSML_UID` |
| `fluidReport/reportingEntity/period/dTimStart` | `PDEN_VOL_SUMMARY.PROD_START` | |
| `fluidReport/reportingEntity/period/dTimEnd` | `PDEN_VOL_SUMMARY.PROD_END` | |
| `fluidReport/production/{product}/productKind` | `PDEN_VOL_SUMMARY.PROD_TYPE` | `'oil'→OIL`, `'gas'→GAS`, `'water'→WATER` |
| `fluidReport/production/{product}/volume` | `PDEN_VOL_SUMMARY.PROD_VOLUME` | |
| `fluidReport/production/{product}/volumeUom` | `PDEN_VOL_SUMMARY.VOL_UOM` | `ft3` → `MCF`, `bbl` → `BBL` |

### ProductFlow → PDEN_VOL_DISPOSITION

| PRODML Path | PPDM Column | Notes |
|---|---|---|
| `productFlowModel/networkPort/productDisposition` | `PDEN_VOL_DISPOSITION.DISP_TYPE` | `'sales'→SALES`, `'flare'→FLARE`, `'fuel'→FUEL` |
| `productFlowModel/networkPort/volume` | `PDEN_VOL_DISPOSITION.DISP_VOL` | |

---

## Upsert Logic

```csharp
// For each summary row received from PRODML:
var filters = new List<AppFilter>
{
    new() { FieldName = "WELL_ID",     Operator = "=", FilterValue = wellId },
    new() { FieldName = "PROD_TYPE",   Operator = "=", FilterValue = prodType },
    new() { FieldName = "PROD_START",  Operator = "=", FilterValue = period.Start.ToString("yyyy-MM-dd") },
    new() { FieldName = "PROD_END",    Operator = "=", FilterValue = period.End.ToString("yyyy-MM-dd") }
};

var existing = (await _volRepo.GetAsync(filters)).FirstOrDefault();
if (existing != null) await _volRepo.UpdateAsync(mapped, userId);
else                  await _volRepo.InsertAsync(mapped, userId);
```

---

## Scheduled Sync

The adapter runs on a configurable schedule in `IHostedService`:

```json
"Integrations": {
    "PRODML": {
        "EndpointUrl": "https://prodml-server.company.com/prodml/v2",
        "SyncScheduleCron": "0 2 * * *",
        "LookbackDays": 3,
        "CircuitBreakerThreshold": 3
    }
}
```

Daily at 02:00 UTC, the sync pulls the last `LookbackDays` days of production data for each active field to handle late arrival of metered data from field historians.

---

## Unit Conversion Constants

| From Unit | To Unit | Factor |
|---|---|---|
| `bbl` | `BBL` | 1.0 (pass-through) |
| `m3` | `BBL` | × 6.2898 |
| `ft3` | `MCF` | ÷ 1000 |
| `m3` gas | `MCF` | × 35.3147 / 1000 |
