# Beep.OilandGas.Web Components Documentation

## Overview

This document describes the reusable component library available in the Blazor web application. Components are organized by functionality and can be used across different pages.

## Component Organization

### Layout Components

Located in `Components/Layout/`

#### MainLayout

Main application layout with navigation, header, and footer.

**Usage**:
```razor
@inherits LayoutComponentBase
```

#### Role-Based Layouts

- `AccountantLayout.razor` - Layout for accounting users
- `ManagerLayout.razor` - Layout for managers
- `PetroleumEngineerLayout.razor` - Layout for petroleum engineers
- `ReservoirEngineerLayout.razor` - Layout for reservoir engineers
- `ProcessEngineerLayout.razor` - Layout for process engineers

### Data Components

Located in `Components/Shared/`

#### PPDMDataGrid

Generic data grid for displaying PPDM entities.

**Properties**:
- `TableName` (string) - Name of the PPDM table
- `Items` (IEnumerable<T>) - Data items to display
- `OnRowClick` (EventCallback) - Fired when row is clicked
- `OnEdit` (EventCallback) - Fired when edit is requested
- `OnDelete` (EventCallback) - Fired when delete is requested

**Usage**:
```razor
<PPDMDataGrid TableName="FIELD" 
              Items="@_fields"
              OnRowClick="@OnFieldSelected"
              OnEdit="@OnEditField"
              OnDelete="@OnDeleteField" />
```

#### PPDMEntityForm

Generic form for creating/editing PPDM entities.

**Properties**:
- `TableName` (string) - Name of the PPDM table
- `EntityId` (string?) - ID of entity to edit (null for create)
- `OnSave` (EventCallback) - Fired when form is saved
- `OnCancel` (EventCallback) - Fired when form is cancelled

**Usage**:
```razor
<PPDMEntityForm TableName="FIELD"
                EntityId="@_selectedFieldId"
                OnSave="@OnSaveField"
                OnCancel="@OnCancelEdit" />
```

#### PPDMMapView

Map view component for displaying geographic data.

**Properties**:
- `Wells` (List<Well>) - Wells to display on map
- `Fields` (List<Field>) - Fields to display on map
- `OnWellClick` (EventCallback) - Fired when well is clicked
- `OnFieldClick` (EventCallback) - Fired when field is clicked

**Usage**:
```razor
<PPMMapView Wells="@_wells"
            Fields="@_fields"
            OnWellClick="@OnWellSelected"
            OnFieldClick="@OnFieldSelected" />
```

### Dialog Components

Located in `Components/`

#### FacilityEditDialog

Dialog for creating/editing facilities.

**Usage**:
```razor
var parameters = new DialogParameters { ["Facility"] = facility };
var dialog = await DialogService.ShowAsync<FacilityEditDialog>("Edit Facility", parameters);
var result = await dialog.Result;
```

#### PoolEditDialog

Dialog for creating/editing pools.

#### ProspectEditDialog

Dialog for creating/editing prospects.

#### SeismicSurveyEditDialog

Dialog for creating/editing seismic surveys.

#### WellAbandonmentDialog

Dialog for well abandonment workflow.

#### RoyaltyCalculationDialog

Dialog for royalty calculations.

#### ProductionForecastDialog

Dialog for production forecasting.

### Progress Components

Located in `Components/Progress/`

#### ProgressDisplay

Displays progress for long-running operations.

**Properties**:
- `Progress` (ProgressUpdate?) - Current progress update
- `ShowPercentage` (bool) - Whether to show percentage
- `ShowDetails` (bool) - Whether to show detailed progress

**Usage**:
```razor
<ProgressDisplay Progress="@_currentProgress"
                 ShowPercentage="true"
                 ShowDetails="true" />
```

#### MultiOperationProgress

Tracks progress for multiple concurrent operations.

**Properties**:
- `Operations` (List<OperationProgress>) - List of operations
- `OnOperationComplete` (EventCallback) - Fired when operation completes

#### WorkflowProgress

Displays workflow progress with steps.

**Properties**:
- `Steps` (List<WorkflowStep>) - Workflow steps
- `CurrentStep` (int) - Current step index
- `OnStepClick` (EventCallback) - Fired when step is clicked

### Connection Components

Located in `Components/`

#### ConnectionCheck

Checks database connection status.

**Usage**:
```razor
<ConnectionCheck OnConnectionChanged="@OnConnectionChanged" />
```

#### ConnectionSetupDialog

Dialog for setting up database connections.

### Navigation Components

Located in `Components/Navigation/`

#### PetroleumEngineerNavMenu

Navigation menu for petroleum engineers.

### Access Control Components

Located in `Components/AccessControl/`

#### AssetAccessTree

Tree view for asset access control.

**Properties**:
- `Assets` (List<Asset>) - Assets to display
- `OnAccessChanged` (EventCallback) - Fired when access is changed

### Import/Export Components

Located in `Components/`

#### ImportCsvDialog

Dialog for importing CSV files.

**Properties**:
- `TableName` (string) - Target table name
- `OnImportComplete` (EventCallback) - Fired when import completes

**Usage**:
```razor
var parameters = new DialogParameters { ["TableName"] = "FIELD" };
var dialog = await DialogService.ShowAsync<ImportCsvDialog>("Import CSV", parameters);
```

#### ImportCsvWizard

Multi-step wizard for CSV import with mapping.

**Properties**:
- `TableName` (string) - Target table name
- `OnImportComplete` (EventCallback) - Fired when import completes

### Field Components

Located in `Components/`

#### FieldSelector

Component for selecting fields.

**Properties**:
- `SelectedFieldId` (string?) - Currently selected field ID
- `OnFieldSelected` (EventCallback) - Fired when field is selected

**Usage**:
```razor
<FieldSelector SelectedFieldId="@_currentFieldId"
               OnFieldSelected="@OnFieldSelected" />
```

### Tree View Components

Located in `Components/`

#### PPDMTreeView

Tree view for PPDM hierarchical data.

**Properties**:
- `RootNode` (TreeNode) - Root node of the tree
- `OnNodeClick` (EventCallback) - Fired when node is clicked
- `OnNodeExpand` (EventCallback) - Fired when node is expanded

### Database Components

Located in `Components/`

#### PPDM39DatabaseWizard

Wizard for creating PPDM39 databases.

**Properties**:
- `OnDatabaseCreated` (EventCallback) - Fired when database is created

## Component Patterns

### Event Callback Pattern

Components use EventCallback for parent-child communication:

```razor
@code {
    [Parameter] public EventCallback<string> OnItemSelected { get; set; }
    
    private async Task SelectItem(string itemId)
    {
        await OnItemSelected.InvokeAsync(itemId);
    }
}
```

### Parameter Pattern

Components accept parameters for configuration:

```razor
@code {
    [Parameter] public string TableName { get; set; } = string.Empty;
    [Parameter] public List<AppFilter>? Filters { get; set; }
    [Parameter] public bool ReadOnly { get; set; } = false;
}
```

### Cascading Parameters

Share data down the component tree:

```razor
@code {
    [CascadingParameter] public string? CurrentFieldId { get; set; }
    [CascadingParameter] public string? CurrentConnectionName { get; set; }
}
```

## Creating New Components

### Component Structure

```razor
@* Component documentation *@
@using Beep.OilandGas.Models.DTOs
@inject IDataManagementService DataManagementService
@inject ISnackbar Snackbar

<MudCard>
    <MudCardContent>
        @* Component content *@
    </MudCardContent>
</MudCard>

@code {
    [Parameter] public string? ItemId { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }
    
    private bool _isLoading = false;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    private async Task LoadData()
    {
        _isLoading = true;
        try
        {
            // Load data
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }
}
```

### Best Practices

1. **Single Responsibility**: Each component should have one clear purpose
2. **Reusability**: Design components to be reusable across pages
3. **Parameters**: Use parameters for configuration, not hard-coded values
4. **Event Callbacks**: Use EventCallback for parent-child communication
5. **Error Handling**: Always handle errors gracefully
6. **Loading States**: Show loading indicators during async operations
7. **Documentation**: Document component purpose and usage

## Component Dependencies

### MudBlazor Components

The application uses MudBlazor components extensively:

- `MudCard`, `MudCardContent` - Cards
- `MudButton` - Buttons
- `MudTextField`, `MudSelect` - Form inputs
- `MudTable` - Tables
- `MudDialog` - Dialogs
- `MudSnackbar` - Notifications
- `MudProgressLinear` - Progress indicators

### Application Services

Components typically inject:

- `IDataManagementService` - Data operations
- `ApiClient` - API communication
- `ISnackbar` - User notifications
- `IDialogService` - Dialog management
- `NavigationManager` - Navigation

## Related Documentation

- [UI Standards](beep-oilgas-web-ui-standards.md)
- [Pages](beep-oilgas-web-pages.md)
- [Services](beep-oilgas-web-services.md)

