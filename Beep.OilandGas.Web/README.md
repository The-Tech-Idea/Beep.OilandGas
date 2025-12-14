# Beep.OilandGas.Web

Blazor Server application for Beep Oil and Gas PPDM39 data management.

## Features

- ✅ **MudBlazor 8.x** - Modern Material Design component library
- ✅ **PPDM39 Module** - Well management, comparison, and data quality
- ✅ **API Integration** - Connects to Beep.OilandGas.ApiService
- ✅ **Dark/Light Mode** - Built-in theme switching

## Setup

1. **Configure API Service URL** in `appsettings.json`:
```json
{
  "ApiService": {
    "BaseUrl": "https://localhost:7001"
  }
}
```

2. **Run the API Service** first:
```powershell
dotnet run --project Beep.OilandGas.ApiService
```

3. **Run the Web App**:
```powershell
dotnet run --project Beep.OilandGas.Web
```

## Pages

### PPDM39 Module
- `/ppdm39/wells` - Well search and management
- `/ppdm39/wells/compare` - Compare multiple wells side-by-side
- `/ppdm39/data-quality` - Data quality dashboard

### Data Management
- `/data/quality-dashboard` - Quality metrics dashboard
- `/data/versioning` - Data versioning
- `/data/audit` - Access audit logs

## Project Structure

```
Beep.OilandGas.Web/
├── Components/
│   └── App.razor          # Main app router
├── Pages/
│   ├── PPDM39/
│   │   ├── Wells.razor
│   │   ├── CompareWells.razor
│   │   └── DataQuality.razor
│   └── Index.razor
├── Shared/
│   ├── MainLayout.razor   # Main layout with MudBlazor
│   └── NavMenu.razor      # Navigation menu
├── Program.cs              # Startup configuration
└── appsettings.json        # Configuration
```

## Dependencies

- Uses `ApiClient` to call `Beep.OilandGas.ApiService`
- All data operations go through the API service
- API service uses `IDMEEditor` and `UnitOfWork` for data access


