# BeepDM Framework Integration — Quick-Start Guide

**Date:** 2026-06-29
**Status:** Phase 4A & 5A complete (v2.0.96 compatible). Phases 1A-C require v3.0.1 upgrade.

## What's Available Now (v2.0.96)

### 1. Data Import with Quality Rules

Upload CSV files with automatic quality validation, profiling, and error replay.

```bash
# Upload and import a CSV
curl -X POST https://localhost:7001/api/data-import/csv/WELL \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@wells.csv"

# Profile a table before importing
curl https://localhost:7001/api/data-import/profile/WELL?sampleSize=500 \
  -H "Authorization: Bearer $TOKEN"

# Replay failed records
curl -X POST https://localhost:7001/api/data-import/replay/WELL_20260629120000 \
  -H "Authorization: Bearer $TOKEN"

# Cancel an import in progress
curl -X POST https://localhost:7001/api/data-import/cancel/WELL_20260629120000 \
  -H "Authorization: Bearer $TOKEN"
```

**Web UI:** `/ppdm39/data-import` — MudBlazor page with file upload, profiling, and result display.

**Quality rules applied:** NotNull, Unique, Range, Regex, AcceptedValues, ReferentialIntegrity

### 2. Multi-Instance Data Synchronization

Sync PPDM entities between datasource instances with reconciliation and SLO monitoring.

```bash
# Initialize PPDM sync schemas
curl -X POST https://localhost:7001/api/sync/init-schemas \
  -H "Authorization: Bearer $TOKEN" \
  -d 'sourceDataSource=PPDM39_SOURCE&destDataSource=PPDM39'

# Sync a single entity
curl -X POST https://localhost:7001/api/sync/run/ppdm39-well \
  -H "Authorization: Bearer $TOKEN"

# Sync all enabled entities
curl -X POST https://localhost:7001/api/sync/run-all \
  -H "Authorization: Bearer $TOKEN"

# View reconciliation report
curl https://localhost:7001/api/sync/reconciliation \
  -H "Authorization: Bearer $TOKEN"

# List configured schemas
curl https://localhost:7001/api/sync/schemas \
  -H "Authorization: Bearer $TOKEN"
```

**Web UI:** `/ppdm39/sync` — MudBlazor dashboard with schema list, per-entity sync, reconciliation view, and SLO tiers.

**Standard schemas:** WELL, FIELD, FACILITY, PDEN_VOL_SUMMARY (Production)

### 3. Expression-Based Default Values

The `PPDM39DefaultsRepository` now supports BeepDM expression rules:

```csharp
// Resolve with expression rules — falls back to hardcoded constants
var source = defaults.ResolveDefaultWithRules(
    ":IF(:USERNAME, :USERNAME, 'SYSTEM')",  // rule
    "SYSTEM");                                // fallback

// Available resolvers:
//   :NOW          — current UTC timestamp
//   :USERNAME     — current Windows/claims user
//   :NEWGUID      — GUID
//   :LOOKUP(...)  — database entity lookup
//   :IF(a,b,c)    — conditional expression
//   :ADD(a,b)     — arithmetic
```

## What Requires v3.0.1 Upgrade

The following features are code-complete but require `TheTechIdea.Beep.DataManagementEngine >= 3.0.1`:

| Feature | Files | What It Provides |
|---------|-------|-----------------|
| Structured logging | `Program.cs` (commented out) | TelemetryPipeline with enrichment, sampling, and budget enforcement |
| Tamper-evident audit | `Services/BeepAuditAdapter.cs` | HMAC hash-chain audit trails |
| W3C tracing | `Middleware/BeepTracingMiddleware.cs` | TraceContext correlation IDs per request |
| Redaction | `Program.cs` (commented out) | Automatic PII scrubbing (passwords, tokens, emails) |

**To enable:** Upgrade NuGet to v3.0.1 and uncomment the `AddBeepLoggingForWeb` / `AddBeepAuditForWeb` blocks in `Program.cs`.

## Architecture Notes

### Dependency Flow
```
ApiService (v2.0.96 DataManagementEngine)
  ├── DataImportService  →  DataImportManager (Editor.Importing)
  ├── BeepSyncService    →  BeepSyncManager   (Editor.BeepSync)
  └── PPDM39DefaultsRepo →  DefaultsManager   (Editor.Defaults)
```

### Pre-Existing NuGet Conflicts
The ApiService project has pre-existing NU1605 SkiaSharp version conflicts (3.119.4 vs 3.119.2) across transitive dependencies. These are not caused by the BeepDM integration and exist independently. Fix by:
1. Updating all projects to SkiaSharp 3.119.4, OR
2. Adding `<WarningsNotAsErrors>NU1605</WarningsNotAsErrors>` to affected .csproj files

### Key Design Decisions
- **Editor features** (DataImport, BeepSync, Defaults, Migration) are available in v2.0.96
- **Services features** (BeepLog, BeepAudit, redaction, retention) require v3.0.1
- All new services are registered as Scoped in DI
- Zero breaking changes to existing code
- Existing `ILogger<T>` calls continue working with Serilog
