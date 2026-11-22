# FND-04 — Theme, skin and branding

| Field | Value |
|---|---|
| Stage | STG-00 — Foundation and host shell |
| Area | Foundation |
| Status | Not started |
| Owner projects | none |
| Desktop project | `src/Beep.OilandGas.Desktop/Theme/`, `src/Beep.OilandGas.Desktop.Controls/` |
| External packages | `TheTechIdea.Beep.WPF.Controls` |
| Depends on | FND-03 |
| Blocks | all views |
| Web reference | `Beep.Web/TheTechIdeaWeb.ThemeBranding` (concept only) |

## 1. Purpose

One design system applied consistently, switchable at runtime, persisted per workstation, and
enforced by guard tests so no page invents its own colours.

## 2. Source components

| Source | Reuse |
|---|---|
| `Beep.DoitAll.Erp.Wpf/Theme/ErpSkins.cs` | `OgSkins` — copy almost verbatim: `All`, `Current`, `Initialize`, `Apply`, `Load`, `Save`, class handler for every `Window` |
| `Beep.DoitAll.Erp.Wpf/Theme/ErpTypography.cs` | `OgTypography` |
| `TheTechIdea.Beep.WPF.Controls/Themes/Light.xaml` | base token dictionary, merged first |
| `TheTechIdea.Beep.WPF.Controls` `BeepChrome`, `WpfThemesManager`, `BeepControlStyle` | skin application |
| `Beep.OilandGas.Web/Services/BrandingConfig` (shared concept) | product name, logo, accent, density |

## 3. Screens

- `SettingsView` → Appearance tab: skin picker (`SkinPicker` control from the controls library),
  density (comfortable/compact), sidebar mode, font scale.
- `AboutWindow`: product name, version, environment file path, active skin, data root.

## 4. Data and contracts

```csharp
public sealed record OgSkinOption(BeepControlStyle Style, string DisplayName);
public static class OgSkins
{
    IReadOnlyList<OgSkinOption> All { get; }
    BeepControlStyle Current { get; }
    void Initialize();
    void Apply(BeepControlStyle style, bool persist = true);
    IReadOnlyList<string> SelfCheck();
}
```

Token names used by every view (all `DynamicResource`):

`BeepBackgroundBrush`, `BeepSurfaceBrush`, `BeepAccentBrush`, `BeepOnAccentBrush`,
`BeepTextPrimaryBrush`, `BeepTextSecondaryBrush`, `BeepBorderBrush`, `BeepDangerBrush`,
`BeepWarningBrush`, `BeepSuccessBrush`, `BeepChartBrush1..8`, `BeepMonoFontFamily`,
`BeepBodyFontSize`, `BeepCaptionFontSize`, `BeepSpacingS/M/L`.

## 5. States

| State | Behaviour |
|---|---|
| `skin.json` unreadable | fall back to `Material3`, overwrite on next change |
| Skin fails to apply | keep the previous skin, report a failure with a reference |
| Density change | applies live; layouts must not clip |

## 6. Commands and permissions

| Command | Permission |
|---|---|
| Change skin/density | any user, per workstation |
| Change product branding | Administration, per installation |

## 7. Acceptance criteria

- AC-1 Every `BeepControlStyle` in `All` applies without exception.
- AC-2 The selected skin survives a restart.
- AC-3 New XAML contains no `#RRGGBB`, no `Color="..."`, no hardcoded `FontSize`.
- AC-4 Every window opened after a skin change receives the new skin.
- AC-5 Chart colours come from `BeepChartBrush*` so Skia canvases match the skin.
- AC-6 Layout does not clip at 125 %, 150 % and 200 % display scaling.

## 8. Verification

```
dotnet test test/Beep.OilandGas.Desktop.Tests --filter "FullyQualifiedName~FND-04"
grep -rn "#[0-9A-Fa-f]\{6\}" src/Beep.OilandGas.Desktop.Modules --include=*.xaml   # expect 0
```

Manual: cycle all skins and capture screenshots of the shell, a grid page and a chart page.

## 9. Open questions and risks

- The controls library targets `net9.0-windows`; confirm skin APIs are identical at consumption
  (ADR-003 probe).
- Skia-rendered views must read chart colours from the theme at draw time, not at construction,
  or they will not follow a skin change.
