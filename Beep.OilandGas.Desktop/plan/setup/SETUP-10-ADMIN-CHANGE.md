# SETUP-10 — Admin change control

**Document ID:** SETUP-10
**Status:** Built
**Actor:** SystemAdmin
**Level:** Company

---

## 1. Purpose

Let the SystemAdmin change the company's settings, module set, environment, users and roles after the first run —
without a second person overwriting the change, and with a record of who changed what.

## 2. Actor

SystemAdmin only. `Permissions.IsSystemAdmin` is the gate; the database's `Administrator`/`Admin` role grants it.

## 3. Preconditions

- A signed-in SystemAdmin (SETUP-09).
- The settings documents exist (SETUP-03/04) — a change is a change, not a creation.

## 4. Steps

1. **Read** the current document and keep its revision.
2. **Edit** in a dialog that validates the same rules as first run (name, code, currency, list membership).
3. **Write** with the revision just read. A mismatch is refused (`settings-record-changed`) and the person is told
   to reload.
4. **Environment follows the company.** If the country or unit system changed, `TOOLS_ENVIRONMENT` is rewritten in
   the same operation, with its own revision.
5. **Adopt in memory.** `OgApplication.AdoptCompany` rebuilds the signed-in session (company,
   field, culture, currency) and applies the interface language, so the running application matches what was saved.
6. **The save records itself.** `OgSharedSetupService.SaveAsync` writes the audit row — `settings.save`, the document id, the actor, `previous=<revision>`, `revision=<new>` and `ok`/`refused` — in the same operation as the write, so a page that forgets to notify cannot lose the trail.

## 5. Records written

| Store | What |
|---|---|
| `BEEPSETTINGS/COMPANY` | the new document, `REVISION`, `PREVIOUS_REVISION`, `INTENT_ID`, `UPDATED_BY`, `UPDATED_UTC` |
| `BEEPSETTINGS/TOOLS_ENVIRONMENT` | when country or units changed |
| Audit store | the change, the actor and the outcome |
| — | nothing on disk: the record is already in the database |

## 6. Failure states

| Code | Meaning | Sentence / next action |
|---|---|---|
| `settings-record-changed` | someone else saved first | "Reload them before saving." |
| `settings-write-failed` | the provider refused | the reference; nothing changed |
| `settings-database-unreachable` | the server is down | try again later; the running session keeps the old values |
| `not-system-admin` (UI) | a non-administrator tried | the button is disabled and the reason is shown |

## 7. Security requirements

- **Authorised at the boundary.** The check is the signed-in role from the database, not a hidden button.
- **Conditional write.** Revision + intent id: a stale writer cannot clobber a change it did not see (OWASP: enforce
  authorization at a boundary every path crosses; here it is the revision-checked store).
- **Audited.** Who, when, which document, previous revision.
- **No privilege escalation through settings.** A settings change cannot grant a role; RBAC changes are their own
  stage (SETUP-05 AC-9, the `users` page) and is authorised separately there.
- **Environment is data.** A change rewrites a document; it cannot introduce executable content.

## 8. UX requirements

- The Settings page shows the values, their source, when they were read, and who may change them.
- A save says what happened and when it will be seen ("every user reads it at their next start").
- A refusal is a sentence, not a dialog with a stack trace.

## 9. Acceptance criteria

| # | Criterion | Status |
|---|---|---|
| AC-1 | Only the SystemAdmin may change the company settings | **Met** — `SettingsView.CanEditCompany`, guard test |
| AC-2 | A stale write is refused and the stored values are unchanged | **Met** — `SharedSetupIntegrationTests` |
| AC-3 | A country/unit change rewrites the shared environment | **Met** — `SettingsView.SaveCompanyAsync` (`environmentChanged`) |
| AC-4 | A change is applied to the running session without a restart | **Met** — `OgApplication.AdoptCompany` |
| AC-5 | Every change is audited with the previous revision | **Met** — `OgSharedSetupService` writes one `settings.save` row (id, actor, previous revision, new revision, `ok`/`refused`) in the same operation; `settings.delete` for a rollback; `SettingsAuditTests` (4) |
| AC-6 | The SystemAdmin can change users and roles | **Met** — the `users` page: create, reset, enable/disable, grant/revoke, with the last-administrator guard |
| AC-7 | The SystemAdmin can edit free-form environment overrides | **Met** — `OgEnvironmentDialog` + `OgEnvironmentDocument.Compose/Read`: country preset, unit system and a JSON overrides object (an explicit override wins), written through the service with the revision read and applied to the running lifetime; `EnvironmentEditorTests` (5) |

## 10. What exists today and what is missing

Exists: `Views/SettingsView.cs` (company section, save path), `Views/OgCompanySettingsWindow.cs`,
`Data/Settings/OgSharedSetup.cs` (revision store), `Bootstrapping/OgApplication.cs` (`AdoptCompany`).

Missing: nothing this stage asks for. AC-5 is the save's own audit row, AC-6 is the `users` page and AC-7 is the
environment editor; each has its tests. A free-form overrides editor validates the JSON shape but cannot know every
key the engine supports — an unknown key is reported by the engine as a warning at load, which is the honest place to
find it.
