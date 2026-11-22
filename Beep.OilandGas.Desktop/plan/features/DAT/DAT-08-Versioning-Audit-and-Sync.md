# DAT-08 — Versioning, access audit and sync

| Field | Value |
|---|---|
| Stage | STG-01 — Data platform and PPDM core |
| Area | Data |
| Status | Gate met - AC-1..AC-6 with durable storage: versions, read audit and the audit chain live in local SQLite and are read back by a second instance; the workbench and form append a version and a chain entry on every write; a broken chain blocks those writes until a named acknowledgement. Sync is still modelled rather than performed. See [evidence](../../evidence/STG-01-DAT-08/observed-evidence.json) |
| Owner projects | `Beep.OilandGas.PPDM39.DataManagement`, `Beep.OilandGas.Repository` |
| Desktop project | `Modules/Data/Governance/` |
| External packages | none |
| Depends on | DAT-07 |
| Blocks | — |
| Web reference | `Pages/Data/Versioning.razor`, `Pages/Data/Audit.razor`, `Pages/PPDM39/Data/AccessAudit.razor`, `Pages/PPDM39/SyncDashboard.razor` |

## 1. Purpose

Answer three questions an auditor will ask: what changed, who read what, and is this copy of the
database in step with the other one.

## 2. Source components

| Source | Reuse |
|---|---|
| `PPDM39.DataManagement/Services/PPDMDataVersioningService.cs` | record versions |
| `PPDM39.DataManagement/Services/PPDMAccessAuditService.cs` | read/write audit |
| `PPDM39.DataManagement/Services/PPDM39WorkflowService.cs` | sync workflow |
| `Beep.OilandGas.Repository/RepositoryAccessService.cs` | actor and role resolution |
| `Beep.OilandGas.Repository/Models/*` `CompanyMembership*`, `UserAssetAccess` | scope |
| `Beep.OilandGas.UserManagement/Services/AuditChainService.cs` | tamper-evident chain |

## 3. Screens

Three tabs under `Data → Governance`:

- **Versions** — table, key, version, valid-from/to, changed-by, changed-at, diff of changed
  columns (old → new), restore as new version (never destructive).
- **Access audit** — actor, role, table, filter, row count, time, outcome, client; filter by actor,
  table, date; export.
- **Sync** — connection pairs, last run, direction, counts (inserted/updated/skipped/conflict),
  conflict list, run and resume.

## 4. Data and contracts

`OgVersionEntry { Table, Key, Version, ValidFrom, ValidTo, Actor, ChangedAt, Changes[] }`
`OgAccessEntry { Actor, Roles[], Table, Filter, RowCount, At, Outcome, Client }`
`OgSyncRun { Pair, Direction, StartedAt, FinishedAt, Inserted, Updated, Skipped, Conflicts[] }`

## 5. States

| State | Behaviour |
|---|---|
| Never synced | "never run" — distinct from "last run failed" |
| Last run failed | shows failure time and reason; resume offered |
| Conflict | listed with both values; resolution is explicit, never automatic |
| Audit chain broken | a prominent warning; the fact is recorded, not repaired silently |

## 6. Commands and permissions

| Command | Permission | Writes |
|---|---|---|
| View versions | `data.audit.read` | no |
| Restore a version | `data.audit.restore` | yes (new version) |
| View access audit | `data.audit.read` | no |
| Run sync | `data.sync.run` | yes |

## 7. Acceptance criteria

- AC-1 A restore creates a new version and never modifies history.
- AC-2 Access audit records reader, table, filter and row count.
- AC-3 Never-run and last-failed are visually and semantically distinct.
- AC-4 Sync conflicts require explicit resolution.
- AC-5 Audit export matches the on-screen filter exactly.
- AC-6 A broken audit chain is reported and blocks writes to governed tables until acknowledged.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.IntegrationTests --filter "FullyQualifiedName~DAT-08"
```

## 9. Open questions and risks

- Sync between two live databases is the least-verified area of the repository. Treat 1.0 sync as
  one-directional with explicit conflict reporting; document that two-way sync is deferred.
