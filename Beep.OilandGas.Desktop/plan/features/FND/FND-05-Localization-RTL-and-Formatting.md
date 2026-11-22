# FND-05 — Localization, RTL and number/date formatting

| Field | Value |
|---|---|
| Stage | STG-00 — Foundation and host shell |
| Area | Foundation |
| Status | Not started |
| Owner projects | none |
| Desktop project | `src/Beep.OilandGas.Desktop/Localization/` |
| External packages | none (`resx` + `Loc` helper) |
| Depends on | FND-03 |
| Blocks | all views |
| Web reference | `Beep.DoitAll.Erp.Wpf/Localization/Loc.cs`, `LocExtension.cs`, `Strings*.resx` |

## 1. Purpose

Every user-visible string comes from a resource file, every layout mirrors correctly in
right-to-left languages, and every number, date, unit and currency is formatted using the
session culture — never the machine's.

## 2. Source components

| Source | Reuse |
|---|---|
| `Beep.DoitAll.Erp.Wpf/Localization/Loc.cs` | `Initialize`, `Get(key, args)`, `ApplyDirection(window)`, `LanguageChanged`, `ToLanguage` |
| `Beep.DoitAll.Erp.Wpf/Localization/LocExtension.cs` | `{loc:Loc Key}` markup extension for XAML |
| 40 `Strings.<culture>.resx` files | scaffold, English values initially |
| `Beep.DoitAll.Erp.Wpf/Setup/ErpCulture.cs` | culture → `Language` mapping and RTL detection |
| `Beep.OilandGas.Web/Services/...` culture handling | company culture from the environment |

## 3. Screens

- Language selector in `SettingsView` and in the status bar.
- All pages: every label, button, column header, tooltip, validation message and empty-state text.

## 4. Data and contracts

Key naming, matching the ERP convention:

```
Page.<route>            page title used by the navigation catalog
Nav.Group.<group>       group header
Ui.<Noun>               button/short label ("Ui.NewWell")
Msg.<Category>          message or validation text
Field.<Entity>.<Name>   column and form labels
Unit.<Code>             unit display name
```

Number/date rules:

- Numbers and money: `session.Culture` with `NumberFormatInfo`; money always `decimal`.
- Dates: `session.Culture` for display, ISO-8601 with offset for transport.
- Units: `TheTechIdea.OilGas.Wellbore.Data.Units.TryConvert` with the loaded `IUnitCatalog`.
- Direction: `FlowDirection.RightToLeft` on the window root; numeric and unit text stays LTR.

## 5. States

| State | Behaviour |
|---|---|
| Missing key | shows the key in debug, logs a warning in release; a guard test prevents shipping a missing key |
| Unsupported culture | falls back to English with a notice |
| Mixed-direction text (Arabic + code) | isolate with `\u2068…\u2069` |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Change UI language | any user, per workstation |
| Change company culture | Administration, per company |

## 7. Acceptance criteria

- AC-1 No user-visible literal in `Modules/` outside a resource file (guard test).
- AC-2 Every key referenced by `Loc.Get` or `{loc:Loc}` exists in `Strings.resx`.
- AC-3 `ar-SA` mirrors the shell, page frame, grids, dialogs and charts.
- AC-4 Numbers render with the culture's decimal separator; money never loses precision.
- AC-5 Dates with an offset survive a round trip.
- AC-6 A language change applies without restart and preserves the current page.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FND-05"
```

Manual: switch to `ar-SA`, capture the shell, one grid page, one chart page, one dialog.

## 9. Open questions and risks

- Arabic technical terms (PPDM field names, standards) should be transliterated, not invented;
  keep the English term in parentheses on first use.
- RTL for Skia-rendered wellbore views is *not* mirrored — geological depth plots stay LTR by
  convention. Record this decision in VSB-03 rather than silently mirroring.
