# Beep.OilandGas.Web Theming Documentation

## Overview

The Blazor web application uses a comprehensive theming system based on MudBlazor themes with custom Oil & Gas industry branding. The theme system supports light/dark modes and can be customized per organization.

## Theme Architecture

### Theme Components

1. **ThemeProvider**: Service for loading and managing themes
2. **Theme JSON**: Theme configuration file
3. **MudTheme**: MudBlazor theme object
4. **BrandingConfig**: Branding configuration (logos, colors, etc.)

## Theme Provider

### IThemeProvider Interface

```csharp
public interface IThemeProvider
{
    MudTheme GetMudTheme();
    BrandingConfig GetBranding();
    bool IsDarkModeDefault { get; }
}
```

### ThemeProvider Implementation

**Location**: `Theme/ThemeProvider.cs`

Loads theme from `Theme/OilGasTheme.json` and provides MudBlazor theme configuration.

**Registration**:
```csharp
builder.Services.AddSingleton<IThemeProvider, ThemeProvider>();
```

## Theme Configuration

### OilGasTheme.json

Theme configuration file located at `Theme/OilGasTheme.json`:

```json
{
  "palette": {
    "primary": "#1976d2",
    "secondary": "#424242",
    "tertiary": "#9c27b0",
    "background": "#f5f5f5",
    "surface": "#ffffff",
    "error": "#d32f2f",
    "warning": "#ed6c02",
    "info": "#0288d1",
    "success": "#2e7d32"
  },
  "typography": {
    "default": {
      "fontFamily": "Roboto, sans-serif",
      "fontSize": "14px"
    }
  },
  "darkMode": false
}
```

## Branding Configuration

### BrandingConfig

```csharp
public class BrandingConfig
{
    public string? AppName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Copyright { get; set; }
    // ... other branding properties
}
```

### Branding Registration

The `BrandingRegistrationService` registers branding with Identity Server on startup so login/register pages show the correct theme.

## Theme Usage

### In MainLayout

```razor
@inject IThemeProvider ThemeProvider

<MudThemeProvider Theme="@_theme" IsDarkMode="@_isDarkMode" />

@code {
    private MudTheme _theme;
    private bool _isDarkMode;
    
    protected override async Task OnInitializedAsync()
    {
        _theme = ThemeProvider.GetMudTheme();
        var defaultDarkMode = ThemeProvider.IsDarkModeDefault;
        await LoadThemePreference(defaultDarkMode);
    }
}
```

### Dark Mode Toggle

```razor
<MudIconButton Icon="@(_isDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode)"
               OnClick="@ToggleDarkMode" />

@code {
    private async Task ToggleDarkMode()
    {
        _isDarkMode = !_isDarkMode;
        await LocalStorage.SetItemAsync("dark-mode", _isDarkMode);
    }
}
```

## Color Palette

### Primary Colors

- **Primary**: Main brand color (typically blue)
- **Secondary**: Secondary brand color
- **Tertiary**: Accent color

### Semantic Colors

- **Success**: Green for positive actions
- **Warning**: Yellow/Orange for warnings
- **Error**: Red for errors
- **Info**: Blue for informational messages

### Oil & Gas Specific Colors

- **Oil**: Dark brown/black (#1a1a1a)
- **Gas**: Light blue (#2196f3)
- **Water**: Blue (#1976d2)
- **Production**: Green (#4caf50)

## Typography

### Font Families

- **Primary**: Roboto (default)
- **Monospace**: 'Courier New', monospace (for code/data)

### Font Sizes

- **H1**: 24px (Page titles)
- **H2**: 20px (Section titles)
- **H3**: 18px (Subsection titles)
- **H4**: 16px (Card titles)
- **Body**: 14px (Regular text)
- **Caption**: 12px (Small text)

## Responsive Design

### Breakpoints

- **xs**: < 600px (Mobile)
- **sm**: 600px - 960px (Tablet)
- **md**: 960px - 1280px (Small Desktop)
- **lg**: 1280px - 1920px (Desktop)
- **xl**: > 1920px (Large Desktop)

### Responsive Theme

Theme adapts to screen size:

```razor
<MudContainer MaxWidth="MaxWidth.ExtraLarge">
    <!-- Content adapts to screen size -->
</MudContainer>
```

## Customization

### Custom Theme

To create a custom theme:

1. Copy `OilGasTheme.json`
2. Modify colors, typography, etc.
3. Update `ThemeProvider` to load custom theme
4. Or use environment-specific configuration

### Organization Branding

Branding can be customized per organization:

1. Update `BrandingConfig` in theme JSON
2. Register with Identity Server
3. Login/register pages show custom branding

## Best Practices

### 1. Use Theme Colors

Always use theme colors instead of hardcoded colors:

```razor
<MudButton Color="Color.Primary">Save</MudButton>
```

### 2. Support Dark Mode

Ensure components work in both light and dark modes:

```razor
<MudPaper Elevation="2" Class="pa-4">
    <!-- Content works in both modes -->
</MudPaper>
```

### 3. Consistent Spacing

Use MudBlazor spacing classes:

```razor
<MudStack Spacing="2">
    <!-- Consistent spacing -->
</MudStack>
```

### 4. Responsive Layouts

Always use responsive layouts:

```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="4">
        <!-- Responsive content -->
    </MudItem>
</MudGrid>
```

## Related Documentation

- [UI Standards](beep-oilgas-web-ui-standards.md)
- [Components](beep-oilgas-web-components.md)
- [Architecture](beep-oilgas-web-architecture.md)

