# SETUP-11 — Upgrade and migration

**Document ID:** SETUP-11
**Status:** Built
**Actor:** SystemAdmin
**Level:** Company (and machine)

---

## 1. Purpose

Let a new build of the application run against an installation created by an older one, and refuse safely when it
cannot: an older build must never write a record a newer one left.

## 2. Actor

SystemAdmin, on a workstation that has been updated. The migration runs once per database, from the first account
that signs in after the update (`LoadSettingsAsync` migrates before it reads).

## 3. Preconditions

- A backup exists — SETUP-12 AC-6 refuses an upgrade that has none.
- The database has a version row (every creation writes one). A database without one is version 1, which is what it
  is: an installation written before versioning.

## 4. Steps

1. **Read the version.** `BEEPSETTINGS/SYSTEM` (`schemaVersion`) is the **only** version marker. Nothing else in a
   settings document carries a version: two markers would be two answers to the same question.
2. **Refuse if newer.** A version this build does not understand stops the migration before anything is read.
3. **Re-save the documents the database holds.** The company record, the environment document and the module set are
   written back through `OgSharedSetupService` with the revision that was read, so each one is validated and audited
   as an ordinary save. A document this build cannot read is refused, not overwritten with an empty one.
4. **Stamp the version last**, so a failure in step 3 leaves the database saying it is still the old shape.
5. **Record the run** as one `setup.migrate` row with from/to versions and the documents rewritten, beside the
   `settings.save` rows the writes themselves wrote.

## 5. Records written

| Store | What |
|---|---|
| `BEEPSETTINGS/SYSTEM` | `schemaVersion`, rewritten after a successful migration |
| `BEEPSETTINGS` | the re-saved documents, each with a new revision and `PREVIOUS_REVISION` kept |
| Audit store | one `setup.migrate` row, plus one `settings.save` row per document |

## 6. Failure states

| Code | Meaning | Behaviour |
|---|---|---|
| `upgrade-database-newer` | the database was written by a newer build | refuse before any read; nothing written |
| `settings-record-invalid` | a settings document cannot be read by this build | that document is left as it is; the reference is reported |
| `settings-record-changed` | another administrator saved between the read and the write | the migration stops; the version is not stamped |
| `upgrade-no-backup` | no backup was confirmed before an upgrade | refused (SETUP-12 AC-6) |

## 7. Security requirements

- **Downgrade protection.** A newer schema is never written by an older build.
- **No silent data loss.** A document that cannot be migrated is left in place and reported.
- **The revision discipline applies to migrations** as much as to edits: a migration is a conditional write.
- **Audited.** An upgrade is a change of the installation; it is recorded like a settings change.

## 8. UX requirements

- The person is told what will be migrated.
- A refusal names the version that is too new and what to install.
- The migration runs after sign-in, because the settings are the signed-in person's to read.

## 9. Acceptance criteria

| # | Criterion | Status |
|---|---|---|
| AC-1 | The database carries a schema version that the build reads and refuses when newer | **Met** — `SYSTEM` row + `OgSetupCompatibility`; `SetupVersioningTests` |
| AC-2 | A settings document written by the previous build is converted and readable | **Met** — `OgSetupMigration` re-saves the company record, environment and module set through `OgSharedSetupService` with the revision read, then stamps the version; `SetupMigrationTests` |
| AC-3 | A database written by a newer build stops the older build before it writes | **Met** — refused before any read (`upgrade-database-newer`); nothing rewritten |
| AC-4 | An upgrade confirms a backup before touching a company database | **Met** — `RequireBackupBeforeUpgrade` refuses with `upgrade-no-backup`; `DatabaseBackupTests` |
| AC-5 | The upgrade is audited with from/to versions and per-step outcomes | **Met** — one `setup.migrate` row plus one `settings.save` row per document |
| AC-6 | A test proves the conversion on a real database holding the previous shape | **Met** — `SetupMigrationTests` (2) |

## 10. What exists and what is missing

Exists: `OgSetupMigration` (refuse newer, re-save the three documents, stamp the version last, audit), the
`SYSTEM` row and `OgSetupCompatibility` with the downgrade refusal, and `PREVIOUS_REVISION` on every settings write.

Missing: a conversion for a document whose *shape* changes in a later build. Today "migrate" means "re-save through
the service that validates it"; when a future build adds a field, the conversion for it goes in this file. There is
no known field to convert yet, and inventing one would be guessing.

The database's own schema upgrade is the module runner's plan/apply (SETUP-06), not this file's.
