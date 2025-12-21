# Shared Components Integration Guide

This document describes the existing components and their integration with the new shared UI components.

## New Shared Components

### 1. PPDMDataGrid.razor
A reusable data grid component that works with PPDMGenericRepository data.

**Location:** `Components/Shared/PPDMDataGrid.razor`

**Features:**
- Auto-detects columns from data objects
- Search/filtering
- Export to CSV
- Customizable action menu
- Cell editing support
- Works with both Dictionary and strongly-typed objects

### 2. PPDMEntityForm.razor
A reusable form component for creating/editing PPDM entities.

**Location:** `Components/Shared/PPDMEntityForm.razor`

**Features:**
- Auto-generates form fields
- Type-aware rendering (text, date, boolean, dropdown)
- Field configuration (labels, placeholders, read-only)
- Default value support
- Primary key handling

### 3. PPDMMapView.razor
A map component for displaying geographic data.

**Location:** `Components/Shared/PPDMMapView.razor`

**Features:**
- Extracts latitude/longitude from entities
- Map controls (zoom, fit bounds)
- Map type selector
- Legend support

## Existing Components

### GenericCrudPage.razor

**Current State:**
- Provides full CRUD operations for PPDM tables
- Uses `MudDataGrid` directly
- Has inline form generation logic

**Recommended Enhancement:**
Refactor to use the new shared components:

```razor
<!-- Replace MudDataGrid with PPDMDataGrid -->
<PPDMDataGrid Items="@_data"
              Title="@GetTableDisplayName()"
              OnEdit="EditRecord"
              OnView="ViewRecord"
              OnDelete="DeleteRecord"
              OnExport="ExportToCsv"
              TableMetadata="@_tableMetadata" />

<!-- Replace inline EditForm with PPDMEntityForm -->
@if (_showEditForm)
{
    <PPDMEntityForm Entity="@_editingRecord"
                    OnValidSubmit="SaveRecord"
                    OnCancel="CancelEdit"
                    TableMetadata="@_tableMetadata"
                    DefaultValues="@GetDefaultValues()" />
}
```

**Benefits:**
- Reduced code duplication
- Consistent UI across pages
- Easier maintenance
- Better reusability

### ImportCsvDialog.razor

**Current State:**
- Simple dialog for CSV import
- Basic validation
- Preview functionality

**Status:** ✅ Good as-is. Can optionally use `PPDMDataGrid` for preview table.

**Suggested Enhancement:**
```razor
<!-- In preview section -->
<PPDMDataGrid Items="@_previewData.Take(5)"
              ShowActions="false"
              ShowExport="false"
              ShowPagination="false" />
```

### ImportCsvWizard.razor

**Current State:**
- Multi-step wizard for CSV import
- Column mapping
- Data validation

**Status:** ✅ Good as-is. Already has comprehensive functionality.

**Optional Enhancement:**
- Could use `PPDMEntityForm` to show a sample form based on mapped columns

## Calculation Libraries Integration

### Beep.OilandGas.DCA

**Current State:**
- Library exists with `DCAManager` class
- Provides DCA calculation methods
- Located in: `Beep.OilandGas.DCA/DCAManager.cs`

**Integration Status:**
- `PPDMCalculationService` currently has placeholder logic
- **Needs:** Actual integration with `DCAManager` for DCA calculations

**Recommended Integration:**

```csharp
// In PPDMCalculationService.PerformDCAAnalysisAsync
public async Task<DCAResult> PerformDCAAnalysisAsync(DCARequest request)
{
    // 1. Fetch production data from PPDM database
    var productionData = await FetchProductionDataAsync(request.WellId, request.StartDate, request.EndDate);
    
    // 2. Extract production rates and dates
    var rates = productionData.Select(p => (double)p.ProductionRate).ToList();
    var dates = productionData.Select(p => p.ProductionDate).ToList();
    
    // 3. Use DCA library
    var dcaManager = new DCAManager();
    var fitResult = dcaManager.AnalyzeWithStatistics(
        rates, 
        dates,
        request.InitialProductionRate ?? 1000,
        request.InitialDeclineRate ?? 0.1);
    
    // 4. Generate forecast points
    var forecastPoints = GenerateForecastPoints(fitResult, request.EndDate);
    
    // 5. Create and save result
    var result = new DCAResult
    {
        CalculationId = Guid.NewGuid().ToString(),
        InitialRate = fitResult.Parameters[0],
        DeclineRate = fitResult.Parameters[1],
        ForecastPoints = forecastPoints,
        EstimatedEUR = CalculateEUR(fitResult)
    };
    
    await SaveDCAResultAsync(result, request.WellId, request.FieldId);
    return result;
}
```

### Beep.OilandGas.ProductionForecasting

**Current State:**
- Library exists with forecasting classes
- Located in: `Beep.OilandGas.ProductionForecasting/`
- Contains: `PseudoSteadyStateForecast`, `TransientForecast`, `GasWellForecast`

**Integration Status:**
- Not yet integrated
- Should be used for physics-based production forecasting (different from DCA)

**Recommended Integration:**

```csharp
// Add to PPDMCalculationService or create PPDMProductionForecastService
public async Task<ProductionForecastResult> GenerateProductionForecastAsync(ProductionForecastRequest request)
{
    // 1. Fetch reservoir properties from PPDM database
    var reservoirProps = await FetchReservoirPropertiesAsync(request.WellId);
    
    // 2. Use ProductionForecasting library
    var forecastProperties = new ReservoirForecastProperties
    {
        InitialPressure = reservoirProps.InitialPressure,
        Permeability = reservoirProps.Permeability,
        Thickness = reservoirProps.Thickness,
        // ... other properties
    };
    
    var forecast = new PseudoSteadyStateForecast();
    var forecastResult = forecast.GenerateSinglePhaseForecast(forecastProperties);
    
    // 3. Save forecast results
    await SaveForecastResultAsync(forecastResult, request.WellId);
    return forecastResult;
}
```

## Integration Recommendations

### Priority 1: Refactor GenericCrudPage
- Use `PPDMDataGrid` instead of direct `MudDataGrid`
- Use `PPDMEntityForm` instead of inline form generation
- Reduces code by ~40%

### Priority 2: Integrate DCA Library
- Update `PPDMCalculationService.PerformDCAAnalysisAsync` to use `DCAManager`
- Fetch actual production data from PPDM database
- Generate real DCA results

### Priority 3: Integrate ProductionForecasting Library
- Create `PPDMProductionForecastService` or extend existing service
- Use for physics-based forecasting (complement to DCA)

### Priority 4: Enhance Import Components (Optional)
- Use `PPDMDataGrid` for preview in ImportCsvDialog
- Minor improvements, not critical

## Usage Examples

### Using PPDMDataGrid in a Page

```razor
@page "/ppdm39/example"
@inject ApiClient ApiClient

<PPDMDataGrid Items="@_data"
              Title="Production Data"
              OnEdit="@(async (item) => await EditItem(item))"
              OnView="@(async (item) => await ViewItem(item))"
              OnDelete="@(async (item) => await DeleteItem(item))"
              Columns="@(new List<string> { "WELL_ID", "PRODUCTION_DATE", "OIL_VOLUME" })"
              ShowSearch="true"
              ShowExport="true" />

@code {
    private List<object>? _data;
    
    protected override async Task OnInitializedAsync()
    {
        _data = await ApiClient.GetAsync<List<object>>("/api/production/data");
    }
}
```

### Using PPDMEntityForm in a Dialog

```razor
<MudDialog>
    <DialogContent>
        <PPDMEntityForm Entity="@_entity"
                        OnValidSubmit="SaveEntity"
                        OnCancel="Cancel"
                        IncludeFields="@(new List<string> { "FIELD_NAME", "FIELD_TYPE", "STATUS" })"
                        FieldLabels="@(new Dictionary<string, string> { { "FIELD_NAME", "Field Name" } })" />
    </DialogContent>
</MudDialog>
```

### Using PPDMMapView

```razor
<PPDMMapView Items="@_wells"
             Title="Well Locations"
             LatitudeField="LATITUDE"
             LongitudeField="LONGITUDE"
             LabelField="WELL_NAME"
             Height="500px" />
```

## Next Steps

1. **Create Integration Branch** - Start refactoring GenericCrudPage
2. **Test Integration** - Ensure shared components work with existing data flows
3. **Update DCA Service** - Integrate actual DCA library calls
4. **Document Patterns** - Create examples for common use cases
