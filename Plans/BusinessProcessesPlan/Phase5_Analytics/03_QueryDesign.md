# Phase 5 — Query Design
## AppFilter-Based Query Patterns for Each KPI

> All queries use `PPDMGenericRepository` + `AppFilter`. No raw SQL.

---

## Work Order KPIs

### WO-01: Completion Rate

```csharp
// Get all WOs in date range (denominator)
var allWOFilters = BaseFilters(fieldId, range, "PROJECT");
var allWOs = await repo.GetAsync(allWOFilters);

// Get completed WOs (numerator)
var completedFilters = new List<AppFilter>(allWOFilters)
{
    new() { FieldName = "PROJECT_STATUS", Operator = "=", FilterValue = "COMPLETED" }
};
var completedWOs = await repo.GetAsync(completedFilters);

double completionRate = allWOs.Count == 0 ? 0
    : (completedWOs.Count / (double)allWOs.Count) * 100;
```

### WO-03: Mean Time to Complete (via PPDM_AUDIT_HISTORY)

```csharp
var auditMeta   = await _metadata.GetTableMetadataAsync("PPDM_AUDIT_HISTORY");
var auditRepo   = BuildRepo("PPDM_AUDIT_HISTORY");

var draftFilters = new List<AppFilter>
{
    new() { FieldName = "ENTITY_TYPE",     Operator = "=", FilterValue = "PROJECT"            },
    new() { FieldName = "FIELD_ID",        Operator = "=", FilterValue = fieldId              },
    new() { FieldName = "CHANGE_REASON",   Operator = "=", FilterValue = "draft"              },
    new() { FieldName = "AUDIT_DATE",      Operator = ">=",FilterValue = range.From.ToString("yyyy-MM-dd") }
};
var draftRecords = await auditRepo.GetAsync(draftFilters);

var completeFilters = new List<AppFilter>
{
    new() { FieldName = "ENTITY_TYPE",     Operator = "=", FilterValue = "PROJECT"  },
    new() { FieldName = "FIELD_ID",        Operator = "=", FilterValue = fieldId    },
    new() { FieldName = "CHANGE_REASON",   Operator = "=", FilterValue = "complete" }
};
var completeRecords = await auditRepo.GetAsync(completeFilters);

// Match draft and complete by ENTITY_OID; average delta hours
```

---

## HSE KPIs

### HSE-01: Tier 1 PSE Rate

```csharp
var hseRepo = BuildRepo("HSE_INCIDENT");

var tier1Filters = new List<AppFilter>
{
    new() { FieldName = "FIELD_ID",       Operator = "=", FilterValue = fieldId },
    new() { FieldName = "INCIDENT_TIER",  Operator = "=", FilterValue = "1"     },
    new() { FieldName = "ACTIVE_IND",     Operator = "=", FilterValue = "Y"     },
    new() { FieldName = "INCIDENT_DATE",  Operator = ">=",FilterValue = range.From.ToString("yyyy-MM-dd") },
    new() { FieldName = "INCIDENT_DATE",  Operator = "<=",FilterValue = range.To.ToString("yyyy-MM-dd")   }
};
var tier1Events = await hseRepo.GetAsync(tier1Filters);

// Tier 1 PSE Rate = (count / exposureHours) × 1,000,000
double tier1Rate = exposureHours == 0 ? 0
    : (tier1Events.Count / exposureHours) * 1_000_000;
```

---

## Compliance KPIs

### COMP-01: Obligation On-Time Fulfillment

```csharp
var obligRepo = BuildRepo("OBLIGATION");

// All fulfilled obligations in range
var fulfilledFilters = new List<AppFilter>
{
    new() { FieldName = "FIELD_ID",      Operator = "=",  FilterValue = fieldId      },
    new() { FieldName = "OBLIG_STATUS",  Operator = "=",  FilterValue = "FULFILLED"  },
    new() { FieldName = "ACTIVE_IND",    Operator = "=",  FilterValue = "Y"          },
    new() { FieldName = "DUE_DATE",      Operator = ">=", FilterValue = range.From.ToString("yyyy-MM-dd") },
    new() { FieldName = "DUE_DATE",      Operator = "<=", FilterValue = range.To.ToString("yyyy-MM-dd")   }
};
var fulfilled = await obligRepo.GetAsync(fulfilledFilters);

// Subset: fulfilled ON TIME (FULFILL_DATE <= DUE_DATE)
// Post-filter in C# since AppFilter doesn't support column-to-column comparison
var onTime = fulfilled
    .Select(e => (dynamic)e)
    .Count(e => (DateTime?)e.FULFILL_DATE <= (DateTime?)e.DUE_DATE);

double onTimeRate = fulfilled.Count == 0 ? 0
    : (onTime / (double)fulfilled.Count) * 100;
```

**Note**: Column-to-column comparisons (FULFILL_DATE <= DUE_DATE) are done as a post-filter in C# after fetching fulfilled rows. This is acceptable when the obligation set per field per period is small (< 1,000 rows). If the set grows large, move this to a `RunQuery` call.

---

## Production KPIs

### PROD-01: Monthly Gross Production (BOE)

```csharp
var pdenRepo = BuildRepo("PDEN_VOL_SUMMARY");

var prodFilters = new List<AppFilter>
{
    new() { FieldName = "FIELD_ID",         Operator = "=",  FilterValue = fieldId },
    new() { FieldName = "ACTIVE_IND",       Operator = "=",  FilterValue = "Y"     },
    new() { FieldName = "PROD_PERIOD_DATE", Operator = ">=", FilterValue = range.From.ToString("yyyy-MM-dd") },
    new() { FieldName = "PROD_PERIOD_DATE", Operator = "<=", FilterValue = range.To.ToString("yyyy-MM-dd")   }
};
var volRows = await pdenRepo.GetAsync(prodFilters);

// Group by month; sum PROD_BOE per period
var byMonth = volRows
    .Select(e => (dynamic)e)
    .GroupBy(e => new DateTime(((DateTime)e.PROD_PERIOD_DATE).Year,
                               ((DateTime)e.PROD_PERIOD_DATE).Month, 1))
    .Select(g => new { Period = g.Key, BOE = g.Sum(r => (double?)r.PROD_BOE ?? 0) })
    .OrderBy(x => x.Period)
    .ToList();
```

---

## Helper: BaseFilters and BuildRepo

```csharp
private List<AppFilter> BaseFilters(string fieldId, DateRangeFilter range, string tableName) =>
    new List<AppFilter>
    {
        new() { FieldName = "FIELD_ID",    Operator = "=",  FilterValue = fieldId },
        new() { FieldName = "ACTIVE_IND",  Operator = "=",  FilterValue = "Y"     },
        new() { FieldName = "CREATE_DATE", Operator = ">=", FilterValue = range.From.ToString("yyyy-MM-dd") },
        new() { FieldName = "CREATE_DATE", Operator = "<=", FilterValue = range.To.ToString("yyyy-MM-dd") }
    };

private PPDMGenericRepository BuildRepo(string tableName)
{
    var metadata   = _metadata.GetTableMetadataAsync(tableName).GetAwaiter().GetResult();
    var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
    return new PPDMGenericRepository(
        _editor, _commonColumnHandler, _defaults, _metadata,
        entityType, _connectionName, tableName);
}
```
