# SETUP-07 — Shared Oil & Gas environment

**Document ID:** SETUP-07
**Status:** Built
**Actor:** SystemAdmin
**Level:** Company

---

## 1. Purpose

Give every user of the company database the same measurement, printing, regional and resource environment. The
authoritative copy is a document in the company database; nothing about it is chosen per workstation.

The environment was previously a per-workstation file (`…\Wellbore\environment.json`). That remains valid for a
personal installation; for a company installation the database document wins.

## 2. Actor

SystemAdmin. Users read it; the application applies it at start.

## 3. Preconditions

- SETUP-04 recorded a country and a unit system (the same two choices describe the environment).
- The country has a built-in preset — the wizard only offers countries the engine ships a preset for.

## 4. Steps

1. The wizard builds the document from the company's country and unit system:
   `{"extends":"country:<CC>","units":{"system":"<oilfield|metric-kpa|metric-bar|metric-mpa>"}}`.
2. It is written to `BEEPSETTINGS/TOOLS_ENVIRONMENT` with the revision discipline of the other documents.
3. At every start, after sign-in, the application loads it with `WellboreEnvironment.FromJson` and publishes the
   resulting services; relative resource paths resolve against the workbook's own folder (none for the built-in
   presets).
4. The Settings page shows the source ("shared company settings"), the findings and the toolbox availability, and
   lets the SystemAdmin re-apply a country preset or reload.

## 5. Records written

| Store | What |
|---|---|
| Company database | `BEEPSETTINGS/TOOLS_ENVIRONMENT` — the document |
| Machine | nothing; the local file is ignored once a company document exists |

## 6. Failure states

| Code | Meaning | Sentence / next action |
|---|---|---|
| `environment-shared-empty` | the document is blank | the previous environment stays active |
| (warnings) | the document names an unknown country or unit system | the engine reports the unknown key; the rest of the document still applies |
| `environment-not-found` | a *file* installation's file is missing | falls back to built-in defaults, and says so |

## 7. Security requirements

- **Company document, not a machine file.** A user cannot change the company's units by editing a local file; the
  file is only consulted for a personal installation.
- **Read after authentication.** The document is loaded by `LoadSettingsAsync`, which refuses in company mode unless
  a user is signed in.
- **Only the SystemAdmin writes it.** A change of country or unit system rewrites the document with the revision it
  read (SETUP-10).
- **No executable content.** The document is data: units, printing, colours, resource paths.

## 8. UX requirements

- The Settings page labels the source honestly (shared company settings / country preset / this workstation's file /
  built-in defaults).
- Findings are listed with severity and source, not swallowed.
- A preset is labelled as a preset, never as a customer configuration.

## 9. Acceptance criteria

| # | Criterion | Evidence |
|---|---|---|
| AC-1 | The document is stored in the company database and round-trips | `SharedSetupIntegrationTests.The_module_set_and_the_environment_document_round_trip` |
| AC-2 | The application loads it instead of the local file in company mode | `OgWellboreEnvironment.LoadSharedDocument`, `OgApplication.LoadSettingsAsync` |
| AC-3 | Every unit system offered is one the engine accepts | `OgSetupCatalog.UnitSystems` — the four names the engine declares, verified by `SetupEnvironmentTests` |
| AC-4 | The Settings page names the source | `OgEnvironmentSnapshot.UsingSharedSettings` → `SettingsView.RefreshEnvironment` |
| AC-5 | A document the engine cannot apply leaves the previous environment active and reports it | `OgWellboreEnvironment.LoadSharedDocument` returns a failed result; the caller publishes a notice |

## 10. What exists today and what is missing

Exists: `WellTools/Environment/OgWellboreEnvironment.cs` (`LoadSharedDocument`), `Services/OgWellboreLifetime.cs`,
`Setup/OgSetupCatalog.cs` (`BuildEnvironmentDocument`), `Data/Settings/OgSharedSetupService.cs`
(`WriteToolsEnvironmentAsync`), `Views/SettingsView.cs`.

The SystemAdmin edits it from the settings page (`Edit the shared environment…`), including free-form overrides:
see [SETUP-10](SETUP-10-ADMIN-CHANGE.md) AC-7.
