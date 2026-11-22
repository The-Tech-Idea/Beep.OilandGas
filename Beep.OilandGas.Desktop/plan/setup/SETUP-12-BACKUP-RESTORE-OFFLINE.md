# SETUP-12 — Backup, restore and an unreachable database

**Document ID:** SETUP-12
**Status:** Built
**Actor:** SystemAdmin (with the DBA for a server database)
**Level:** Machine and company

---

## 1. Purpose

Decide what happens when the data path is not there: a missing local file, a server that cannot be reached, a
database that was restored or rolled back, and the backup that makes any of those recoverable.

The product's own behaviour on an unreachable path is built. Backup and restore of a company database are the
DBA's today, and this document says so rather than implying otherwise.

## 2. Actor

SystemAdmin for the application's assessment and repair; the DBA for a server backup/restore.

## 3. Preconditions

- A completed installation (SETUP-03/09).

## 4. Steps

### 4.1 At every start (built)

1. The repair assessment asks: is setup complete, do accounts exist, is the data path present?
2. **Personal:** the local database holds the company and its accounts, so a missing file is lost data, not a
   missing file to recreate. The gate sends that case to **setup**, which creates the database and the company
   again; there is no repair run.
3. **Company:** an unreachable server is a probe failure at start-up, reported with the reference and the
   connection name; the shell does not open on invented data.
4. A failed settings read after sign-in does not stop the shell; it produces a notice with a reference.

### 4.2 Backup (built: the settings page, or the DBA for a server database)

1. Personal: `OgDatabaseBackup` copies `…\Data\Local\BeepOilGas.db` to `<DataRoot>\Backups` with the connection
   closed (SQLite holds the file open, so a copy taken while somebody writes is not a backup).
2. Company: the DBA's own backup of the database, including `BEEPSETTINGS`, the RBAC tables and the module tables
   in one consistent snapshot.
3. The application should be able to *state* the backup it relies on and refuse an upgrade without one
   (SETUP-11 AC-4).

### 4.3 Restore (built for a file database, with the connection closed)

1. Restore the database to a point in time (DBA).
2. Expect the revision discipline to behave: a settings row restored to an older revision means the next save with
   the newer revision is refused (`settings-record-changed`) until the person reloads — a restore is visible, not
   silent.
3. Sessions are re-established by signing in again; the application holds no server session across a restore.

## 5. Records written

| Store | What |
|---|---|
| Audit store | the repair run (`repair`, `local-database`, `ok`/`refused`) |
| Failure store | a `Degraded` record when the data path cannot be read |
| — | there is no settings file to touch |

## 6. Failure states

| Code | Meaning | Behaviour |
|---|---|---|
| `repair-path-invalid` | the local path cannot be used | refuse, state the path |
| `repair-failed` | re-provisioning failed | refuse, keep setup and credentials |
| `connection-host-unreachable` | the server cannot be reached | sign-in says so; the shell does not open a company it cannot read |
| `settings-read-failed` | read failed after sign-in | shell opens, notice with reference |
| `settings-record-changed` after a restore | the restored row is older than the writer's revision | refuse the write, ask to reload |

## 7. Security requirements

- **A restore is not a silent downgrade.** The revision check makes a rolled-back row visible to the next writer.
- **No credential in a backup of the settings.** `BEEPSETTINGS` holds no password; the connection password lives in
  the machine's connection store, and the account hashes live in `USER` (a backup of those is a backup of
  credentials — protect it accordingly).
- **Offline is a state, not a crash.** An unreachable database produces a recorded, actionable answer
  (OWASP Multi-Tenant §1: fail closed on missing context; here "closed" means "no company data shown").
- **Tenant-scoped restore** (OWASP Multi-Tenant §7): a shared backup must not be restored over live data to recover
  one company. With one company per database this is simpler, but the rule is recorded because it stops being
  simple the moment a second company shares a server.

## 8. UX requirements

- A repair window says what was missing, what will be recreated and what will not be recovered.
- A missing company database says which server and which connection, not "an error occurred".
- The settings page shows whether the data path is healthy and the last failure reference.

## 9. Acceptance criteria

| # | Criterion | Status |
|---|---|---|
| AC-1 | A missing local database is a setup run, not a repair | **Met** — `OgStartupGate.Decide`; `LocalRepairTests` proves the repair path refuses |
| AC-2 | A company installation is never sent to "repair the local database file" | **Met** — `OgRepairService.Assess` (deployment-aware) |
| AC-3 | An unreachable company database is reported and does not open invented data | **Met** — `LoadSettingsAsync` failure path |
| AC-4 | A restore is visible to the next settings write | **Met** — `DatabaseBackupTests.A_restore_is_visible_to_the_next_settings_write`: after a restore, a write holding the pre-restore revision is refused and one holding the restored revision succeeds |
| AC-5 | The product can back up and restore the installation's database | **Met** — `OgDatabaseBackup` copies the file database to a dated file under `Backups` with the connection closed, lists copies, restores from one (SQLite header checked, destructive confirmation, audit row), and refuses a server database it cannot see; `DatabaseBackupTests` (5) |
| AC-6 | An upgrade refuses without a confirmed backup | **Met** — `RequireBackupBeforeUpgrade` refuses with `upgrade-no-backup` when no existing backup is named; tested for no path, a missing file, a real file and no upgrade |

## 10. What exists today and what is missing

Exists: `Data/Setup/OgDatabaseBackup.cs` (copy, list, restore, `RequireBackupBeforeUpgrade`), the backup section on
`Views/SettingsView.cs`, `Services/OgRepairService.cs`, `Setup/OgStartupGate.cs`, and the offline paths in
`OgApplication.LoadSettingsAsync` and `App.OnStartup`.

Missing: nothing this stage asks for. A server database's backup and restore remain the DBA's, and the product
says so rather than pretending to copy a file it cannot see.
