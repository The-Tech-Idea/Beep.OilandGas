# Phase 10 — Performance Tuning
## PPDM Query Profiling, AppFilter Index Strategy, Startup Benchmarks

---

## Query Profiling Approach

Enable Beep `IDMEEditor` diagnostic logging in non-production environments:

```json
// appsettings.Development.json
"Beep": {
    "DiagnosticsEnabled": true,
    "SlowQueryThresholdMs": 200,
    "LogParameterValues": false
}
```

For each slow query identified, capture:
1. The `AppFilter` list that triggered it
2. The generated SQL (from Beep diagnostic string)
3. Query plan via `EXPLAIN` / `SET STATISTICS IO ON` (per DB type)

---

## Index Recommendations by Table

### Primary Query Patterns and Missing Index Candidates

| Table | Common Filter Columns | Recommended Index |
|---|---|---|
| `HSE_INCIDENT` | `FIELD_ID`, `INCIDENT_DATE`, `ACTIVE_IND` | Composite: `(FIELD_ID, INCIDENT_DATE, ACTIVE_IND)` |
| `OBLIGATION` | `FIELD_ID`, `OBLIG_STATUS`, `DUE_DATE`, `ACTIVE_IND` | Composite: `(FIELD_ID, OBLIG_STATUS, DUE_DATE)` |
| `PDEN_VOL_SUMMARY` | `FIELD_ID`, `PROD_TYPE`, `PROD_START`, `PROD_END` | Composite: `(FIELD_ID, PROD_TYPE, PROD_START)` |
| `PROJECT` | `FIELD_ID`, `PROJECT_TYPE`, `PROJECT_STATUS` | Composite: `(FIELD_ID, PROJECT_TYPE, PROJECT_STATUS)` |
| `PROJECT_STEP` | `PROJECT_ID`, `STEP_STATUS`, `PLAN_END_DATE` | Composite: `(PROJECT_ID, STEP_STATUS)` |
| `INSTRUMENT` | `INSTRUMENT_ID`, `READING_DATE` | Composite: `(INSTRUMENT_ID, READING_DATE DESC)` |
| `PPDM_AUDIT_HISTORY` | `RECORD_ID`, `TABLE_NAME`, `AUDIT_DATE` | Composite: `(RECORD_ID, TABLE_NAME)` |

---

## Target Performance Benchmarks

| API Endpoint | P50 Target | P95 Target | Current Baseline |
|---|---|---|---|
| `GET /api/field/current/incidents` | < 100 ms | < 250 ms | TBD (measure Sprint 10.1) |
| `GET /api/field/current/obligations?daysAhead=30` | < 150 ms | < 400 ms | TBD |
| `POST /api/compliance/reports/eia914/...` (full report) | < 500 ms | < 1500 ms | TBD |
| `GET /api/analytics/kpis` | < 300 ms | < 800 ms | TBD |
| `GET /health/integrations` | < 50 ms | < 150 ms | TBD |
| App cold start (`dotnet run`) | < 8 s | — | TBD |

---

## Caching Strategy

| Data Type | Cache Level | TTL | Invalidation |
|---|---|---|---|
| Field metadata (`FIELD.FIELD_NAME` etc.) | `IMemoryCache` | 1 hour | On field UPDATE |
| LOV values (`R_*` tables) | `IMemoryCache` | 4 hours | On LOV change |
| KPI summaries (Phase 5/7) | `IMemoryCache` | 15 minutes | On new incident/volume |
| PPDM table metadata | `IMemoryCache` | App lifetime | — |

```csharp
// In service: cache-aside pattern
var cacheKey = $"lov:{valueType}";
if (!_cache.TryGetValue(cacheKey, out List<LOVItem>? lovItems))
{
    lovItems = await FetchFromPPDMAsync(valueType);
    _cache.Set(cacheKey, lovItems, TimeSpan.FromHours(4));
}
return lovItems!;
```

---

## Startup Benchmark Checklist

| Item | Target | Validation Command |
|---|---|---|
| `dotnet build` | < 30 s | `Measure-Command { dotnet build }` |
| API cold start (first request ready) | < 8 s | timed curl to `/health` |
| DI container resolution errors | 0 | Check log for `ObjectDisposedException` / `InvalidOperationException` at startup |
| DB schema validation (PPDM metadata) | < 2 s | Log `IPPDMMetadataRepository.InitializeAsync` timing |

---

## Pagination for Large Resultsets

All endpoints returning collections MUST support `?page=1&pageSize=50` to prevent full-table scans:

```csharp
// AppFilter extensions for pagination
var filtersWithPage = filters
    .Append(new AppFilter { FieldName = "@PAGE_SIZE", FilterValue = pageSize.ToString() })
    .Append(new AppFilter { FieldName = "@PAGE_NUMBER", FilterValue = page.ToString() });
```

`PPDMGenericRepository.GetAsync` passes `@PAGE_SIZE` / `@PAGE_NUMBER` as hints when the underlying Beep data source supports OFFSET/FETCH paging.
