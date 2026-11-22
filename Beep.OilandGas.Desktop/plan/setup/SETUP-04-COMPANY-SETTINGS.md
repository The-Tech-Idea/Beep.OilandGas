# SETUP-04 — Company settings record

**Document ID:** SETUP-04
**Status:** Built
**Actor:** SystemAdmin
**Level:** Company

---

## 1. Purpose

Record what the company is, once, in the company database, so every user of that database works with the same
company name, calendar, currency and oil-and-gas defaults. The record is one JSON row, readable by a DBA and
changeable only by the SystemAdmin.

## 2. Actor

SystemAdmin. The setup run writes the first value; SETUP-10 changes it later.

## 3. Preconditions

- SETUP-03 ran far enough that `BEEPSETTINGS` exists.
- The country chosen has an environment preset (SETUP-07 uses the same choice).

## 4. Steps

1. Collect: company name, company code, country, interface language, time zone, base currency (ISO 4217), fiscal
   year start month, company type, unit system, default field or asset.
2. Validate: name ≥ 2 characters; code 2–20 letters/digits/`-`/`_`; currency exactly three upper-case letters;
   every list has a chosen member.
3. Read `BEEPSETTINGS/COMPANY` to obtain the revision (empty when the row does not exist).
4. Write with that revision. A revision that no longer matches is refused (`settings-record-changed`) instead of
   overwriting another administrator's change.
5. Read back and show the stored values.

## 5. Records written

`BEEPSETTINGS`, `SETTINGS_ID = 'COMPANY'`:

| Column | Content |
|---|---|
| `SETTINGS_JSON` | `{CompanyName, CompanyCode, CountryCode, CultureName, TimeZoneId, BaseCurrencyCode, FiscalYearStartMonth, CompanyTypeCode, UnitSystemCode, DefaultFieldName, RecordedBy, RecordedAtUtc}` |
| `REVISION` | SHA-256 over `settingsId ␟ expectedRevision ␟ intentId ␟ json` |
| `PREVIOUS_REVISION` | the revision this write replaced |
| `INTENT_ID` | 32-hex guid; a retry with the same intent is the same change |
| `UPDATED_BY`, `UPDATED_UTC` | who and when |
| `ACTIVE_IND`, `ROW_*` | the standard audit columns |

The table is created by the first write, from the entity class through the engine's schema provider — no statement
text in the desktop.

## 6. Failure states

| Code | Meaning | Sentence / next action |
|---|---|---|
| `settings-table-missing` | the database has no `BEEPSETTINGS` yet | "no company has been created in it" → run SETUP-03 |
| `settings-database-unreachable` | the connection is not open | check SETUP-02 |
| `settings-record-invalid` | the document fails the format checks (size, identifier, revision shape, intent) | refused before writing |
| `settings-record-changed` | revision mismatch | "The company settings changed since they were read. Reload them before saving." |
| `settings-write-failed` | the provider refused the write | reference only |
| `settings-not-recorded` | the row is absent or the document is blank/unreadable | shown as "not recorded", never as a default |

## 7. Security requirements

- **Read after authentication only.** `OgApplication.LoadSettingsAsync` refuses to read in company mode unless a
  user is signed in; the settings are that person's to read under their role.
- **One writer.** Only `SystemAdmin` may write (`Permissions.IsSystemAdmin`); a non-admin sees the values and the
  reason it cannot change them.
- **Revision check** prevents a lost update between two administrators (OWASP: authorization at the boundary; the
  write is authorised *and* conditional).
- **No secrets in the document.** It holds no password, token or connection string.
- **Unreadable means unreadable.** A document this build cannot parse is reported and treated as "not recorded",
  never adopted as a company with blank fields.

## 8. UX requirements

- The Settings page shows the values, where they come from ("company database 'X' · read at …"), and who may change
  them.
- The edit form validates before it submits and states the rule it broke.
- A save reports what changed and where it will be read next ("every user reads it at their next start").

## 9. Acceptance criteria

| # | Criterion | Evidence |
|---|---|---|
| AC-1 | The first run creates the row and the values round-trip | `SharedSetupIntegrationTests.The_company_record_is_created_by_the_first_run_and_a_stale_write_is_refused` |
| AC-2 | A stale writer is refused and the stored company is unchanged | same test |
| AC-3 | An unreadable or blank document is "not recorded" and the refusal is recorded | `SharedSetupIntegrationTests.An_installation_document_that_cannot_be_read_is_refused_rather_than_half_applied` |
| AC-4 | Only the SystemAdmin may change it | `SourceGuardTests.Company_settings_are_one_database_table_and_only_the_system_administrator_prepares_them` |
| AC-5 | A change is adopted by the running application without a restart | `OgApplication.AdoptCompany` (session, culture) |
| AC-6 | The record is read only after sign-in | `OgApplication.LoadSettingsAsync` guard; `App.OnStartup` order |

## 10. What exists today and what is missing

Exists: `Data/Settings/OgSharedSetup.cs` (entity binding, revision, `BEEPSETTINGS` creation), `OgSharedSetupService`,
`Views/OgCompanySettingsWindow.cs`, `Views/SettingsView.cs`, `Bootstrapping/OgApplication.cs`
(`LoadSettingsAsync`, `AdoptCompany`).

Not built, and not required by an acceptance criterion here: a per-field audit trail (the audit row records the
document, the actor and the revisions, not which field changed) and a "compare with the last revision" view. They
would be a useful addition to SETUP-10, and the stored `PREVIOUS_REVISION` is what such a view would read.
