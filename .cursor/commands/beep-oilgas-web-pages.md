# Beep.OilandGas.Web Pages Documentation

## Overview

This document describes the page structure, routing, and organization of the Blazor web application. Pages are organized by functional area and follow consistent patterns.

## Page Organization

### Root Pages

- **`/`** - Index/Home page
- **`/dashboard`** - Main dashboard
- **`/login`** - Login page
- **`/register`** - Registration page

### PPDM39 Module Pages

Located in `Pages/PPDM39/`

#### Field Management

- **`/ppdm39/field/dashboard`** - Field dashboard with KPIs and phase summaries

#### Exploration Phase

- **`/ppdm39/exploration/prospects`** - Prospect management
- **`/ppdm39/exploration/seismic-surveys`** - Seismic survey management
- **`/ppdm39/exploration/seismic-lines`** - Seismic line management

#### Development Phase

- **`/ppdm39/development/facilities`** - Facility management
- **`/ppdm39/development/pools`** - Pool/reservoir management

#### Production Phase

- **`/ppdm39/production/fields`** - Field production overview
- **`/ppdm39/production/pools`** - Pool production data
- **`/ppdm39/production/well-tests`** - Well test management
- **`/ppdm39/production/reserves`** - Reserves management
- **`/ppdm39/production/forecasts`** - Production forecasting
- **`/ppdm39/production/reporting`** - Production reporting

#### Decommissioning Phase

- **`/ppdm39/decommissioning/well-abandonment`** - Well abandonment management
- **`/ppdm39/decommissioning/cost-estimation`** - Decommissioning cost estimation

#### Data Management

- **`/ppdm39/data-management`** - Data management overview
- **`/ppdm39/data-quality`** - Data quality dashboard
- **`/ppdm39/database-management`** - Database management
- **`/ppdm39/create-database-wizard`** - Database creation wizard
- **`/ppdm39/compare-wells`** - Well comparison tool
- **`/ppdm39/wells-index`** - Wells index

#### Well Management

- **`/ppdm39/wells/search`** - Well search
- **`/ppdm39/wells/wellbores`** - Wellbore management
- **`/ppdm39/wells/logs`** - Well logs
- **`/ppdm39/well-details/search`** - Well details search
- **`/ppdm39/well-details/status`** - Well status
- **`/ppdm39/well-details/wellbores`** - Well details wellbores
- **`/ppdm39/well-details/logs`** - Well details logs

#### Accounting

- **`/ppdm39/accounting/accounting-dashboard`** - Accounting dashboard
- **`/ppdm39/accounting/royalties`** - Royalty management
- **`/ppdm39/accounting/cost-allocation`** - Cost allocation
- **`/ppdm39/accounting/volume-reconciliation`** - Volume reconciliation

#### Stratigraphy

- **`/ppdm39/stratigraphy/units`** - Stratigraphic units
- **`/ppdm39/stratigraphy/sections`** - Stratigraphic sections
- **`/ppdm39/stratigraphy/hierarchy`** - Stratigraphic hierarchy
- **`/ppdm39/stratigraphy/columns`** - Stratigraphic columns

#### Seismic

- **`/ppdm39/seismic/surveys`** - Seismic surveys
- **`/ppdm39/seismic/acquisition`** - Seismic acquisition
- **`/ppdm39/seismic/processing`** - Seismic processing

### Data Management Pages

Located in `Pages/Data/`

- **`/data/quality-dashboard`** - Data quality dashboard
- **`/data/validation`** - Data validation
- **`/data/versioning`** - Data versioning
- **`/data/audit`** - Audit trail
- **`/data/database-setup`** - Database setup

### Admin Pages

Located in `Pages/Admin/`

- **`/admin/access-control/asset-access`** - Asset access control
- **`/admin/access-control/hierarchy-config`** - Hierarchy configuration
- **`/admin/access-control/user-roles`** - User roles management

## Page Structure Pattern

### Standard Page Template

```razor
@page "/ppdm39/example"
@using Beep.OilandGas.Models.DTOs
@inject IDataManagementService DataManagementService
@inject ApiClient ApiClient
@inject ISnackbar Snackbar
@inject NavigationManager NavigationManager

<PageTitle>Page Title - PPDM39</PageTitle>

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">Page Title</MudText>
    
    @if (_isLoading)
    {
        <MudProgressLinear Color="Color.Primary" Indeterminate="true" />
    }
    else if (_data != null)
    {
        <!-- Page content -->
    }
    else
    {
        <MudAlert Severity="Severity.Warning">No data available</MudAlert>
    }
</MudContainer>

@code {
    private bool _isLoading = true;
    private List<DataItem>? _data;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    private async Task LoadData()
    {
        _isLoading = true;
        try
        {
            _data = await DataManagementService.GetEntitiesAsync("TABLE_NAME");
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading data: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }
}
```

## Routing

### Route Definition

Pages use the `@page` directive to define routes:

```razor
@page "/ppdm39/field/dashboard"
@page "/ppdm39/field/dashboard/{fieldId}"
```

### Route Parameters

Access route parameters:

```razor
@page "/ppdm39/field/dashboard/{fieldId}"

@code {
    [Parameter] public string? FieldId { get; set; }
    
    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(FieldId))
        {
            await LoadFieldData(FieldId);
        }
    }
}
```

### Navigation

Navigate between pages:

```razor
NavigationManager.NavigateTo("/ppdm39/field/dashboard");
NavigationManager.NavigateTo($"/ppdm39/field/dashboard/{fieldId}");
```

## Authorization

### Page-Level Authorization

```razor
@page "/admin/users"
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize(Roles = "Administrator")]
```

### Component-Level Authorization

```razor
@inject IAuthorizationService AuthorizationService

@code {
    private bool _canEdit = false;
    
    protected override async Task OnInitializedAsync()
    {
        var authResult = await AuthorizationService.AuthorizeAsync(
            user, "EditPolicy");
        _canEdit = authResult.Succeeded;
    }
}
```

## Page Patterns

### List Page Pattern

```razor
@page "/ppdm39/wells"
@inject IDataManagementService DataManagementService

<MudContainer>
    <MudDataGrid Items="@_wells" 
                 OnRowClick="@OnWellSelected"
                 OnEdit="@OnEditWell"
                 OnDelete="@OnDeleteWell" />
</MudContainer>

@code {
    private List<Well> _wells = new();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadWells();
    }
    
    private async Task LoadWells()
    {
        var response = await DataManagementService.GetEntitiesAsync("WELL");
        _wells = response.Entities?.Cast<Well>().ToList() ?? new();
    }
}
```

### Detail Page Pattern

```razor
@page "/ppdm39/wells/{wellId}"
@inject IDataManagementService DataManagementService

<MudContainer>
    @if (_well != null)
    {
        <MudCard>
            <MudCardContent>
                <!-- Well details -->
            </MudCardContent>
        </MudCard>
    }
</MudContainer>

@code {
    [Parameter] public string? WellId { get; set; }
    private Well? _well;
    
    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(WellId))
        {
            await LoadWell(WellId);
        }
    }
}
```

### Dashboard Page Pattern

```razor
@page "/ppdm39/field/dashboard"
@inject ApiClient ApiClient

<MudContainer>
    <!-- KPI Cards -->
    <MudGrid>
        @foreach (var kpi in _dashboard.KPIs)
        {
            <MudItem xs="12" sm="6" md="3">
                <KpiCard Kpi="@kpi" />
            </MudItem>
        }
    </MudGrid>
    
    <!-- Charts -->
    <MudGrid Class="mt-4">
        <MudItem xs="12" md="6">
            <ProductionChart Data="@_productionData" />
        </MudItem>
    </MudGrid>
</MudContainer>
```

## Best Practices

### 1. Loading States

Always show loading indicators:

```razor
@if (_isLoading)
{
    <MudProgressLinear Indeterminate="true" />
}
```

### 2. Error Handling

Handle errors gracefully:

```razor
try
{
    await LoadData();
}
catch (Exception ex)
{
    Snackbar.Add($"Error: {ex.Message}", Severity.Error);
    _logger.LogError(ex, "Error loading data");
}
```

### 3. Data Validation

Validate route parameters:

```razor
protected override async Task OnParametersSetAsync()
{
    if (string.IsNullOrEmpty(FieldId))
    {
        NavigationManager.NavigateTo("/ppdm39");
        return;
    }
    await LoadData();
}
```

### 4. Responsive Design

Use MudBlazor grid for responsive layouts:

```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="4" lg="3">
        <!-- Content -->
    </MudItem>
</MudGrid>
```

## Related Documentation

- [Components](beep-oilgas-web-components.md)
- [Services](beep-oilgas-web-services.md)
- [Authentication](beep-oilgas-web-authentication.md)

