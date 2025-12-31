# Beep.OilandGas.Web Lifecycle Management Documentation

## Overview

The Blazor web application provides comprehensive UI for managing the complete oil and gas field lifecycle, from exploration through decommissioning. All lifecycle operations are performed through the API service, ensuring consistent business logic and data integrity.

## Lifecycle Phases

### 1. Exploration Phase

The exploration phase covers activities related to discovering and evaluating potential oil and gas reserves.

#### Pages

- **`/ppdm39/exploration/prospects`** - Prospect management
- **`/ppdm39/exploration/seismic-surveys`** - Seismic survey management
- **`/ppdm39/exploration/seismic-lines`** - Seismic line management

#### Components

- `ProspectEditDialog.razor` - Create/edit prospects
- `SeismicSurveyEditDialog.razor` - Create/edit seismic surveys

#### API Integration

```razor
@inject ApiClient ApiClient

@code {
    private async Task LoadProspects()
    {
        var prospects = await ApiClient.GetAsync<List<Prospect>>(
            "/api/field/exploration/prospects");
    }
    
    private async Task CreateProspect(Prospect prospect)
    {
        var result = await ApiClient.PostAsync<Prospect, ProspectResponse>(
            "/api/field/exploration/prospects", prospect);
    }
}
```

### 2. Development Phase

The development phase covers activities related to developing discovered reserves, including drilling, facility construction, and infrastructure setup.

#### Pages

- **`/ppdm39/development/facilities`** - Facility management
- **`/ppdm39/development/pools`** - Pool/reservoir management

#### Components

- `FacilityEditDialog.razor` - Create/edit facilities
- `PoolEditDialog.razor` - Create/edit pools

#### API Integration

```razor
@code {
    private async Task LoadFacilities()
    {
        var facilities = await ApiClient.GetAsync<List<Facility>>(
            "/api/field/development/facilities");
    }
    
    private async Task CreateFacility(Facility facility)
    {
        var result = await ApiClient.PostAsync<Facility, FacilityResponse>(
            "/api/field/development/facilities", facility);
    }
}
```

### 3. Production Phase

The production phase covers activities related to producing oil and gas, including well operations, production reporting, and reserves management.

#### Pages

- **`/ppdm39/production/fields`** - Field production overview
- **`/ppdm39/production/pools`** - Pool production data
- **`/ppdm39/production/well-tests`** - Well test management
- **`/ppdm39/production/reserves`** - Reserves management
- **`/ppdm39/production/forecasts`** - Production forecasting
- **`/ppdm39/production/reporting`** - Production reporting

#### Components

- `ProductionForecastDialog.razor` - Production forecast creation

#### API Integration

```razor
@code {
    private async Task LoadProductionData()
    {
        var production = await ApiClient.GetAsync<ProductionData>(
            $"/api/field/production?fieldId={_fieldId}&startDate={startDate}&endDate={endDate}");
    }
    
    private async Task CreateWellTest(WellTestRequest request)
    {
        var result = await ApiClient.PostAsync<WellTestRequest, WellTestResponse>(
            "/api/field/production/well-tests", request);
    }
}
```

### 4. Decommissioning Phase

The decommissioning phase covers activities related to closing down operations, including well abandonment and cost estimation.

#### Pages

- **`/ppdm39/decommissioning/well-abandonment`** - Well abandonment management
- **`/ppdm39/decommissioning/cost-estimation`** - Decommissioning cost estimation

#### Components

- `WellAbandonmentDialog.razor` - Well abandonment workflow

#### API Integration

```razor
@code {
    private async Task LoadAbandonmentPlans()
    {
        var plans = await ApiClient.GetAsync<List<AbandonmentPlan>>(
            "/api/field/decommissioning/abandonment-plans");
    }
    
    private async Task CreateAbandonmentPlan(AbandonmentPlanRequest request)
    {
        var result = await ApiClient.PostAsync<AbandonmentPlanRequest, AbandonmentPlanResponse>(
            "/api/field/decommissioning/abandonment-plans", request);
    }
}
```

## Field Orchestrator Integration

### Field Dashboard

The Field Dashboard (`/ppdm39/field/dashboard`) provides a comprehensive overview of the active field's lifecycle status.

#### Features

- **KPI Cards**: Key performance indicators for the field
- **Phase Summaries**: Status of each lifecycle phase
- **Recent Activities**: Timeline of recent operations
- **Quick Actions**: Common operations shortcuts

#### API Integration

```razor
@inject ApiClient ApiClient

@code {
    private FieldDashboard? _dashboard;
    
    protected override async Task OnInitializedAsync()
    {
        _dashboard = await ApiClient.GetAsync<FieldDashboard>(
            "/api/field/dashboard");
    }
}
```

### Field Selection

Users can select and switch between different fields using the Field Selector component.

```razor
@inject IDataManagementService DataManagementService

@code {
    private async Task SetActiveField(string fieldId)
    {
        var success = await DataManagementService.SetCurrentFieldAsync(fieldId);
        if (success)
        {
            // Refresh dashboard
            await LoadDashboard();
        }
    }
}
```

## Process Management

### Process Workflows

Lifecycle operations follow defined process workflows that can be visualized and tracked.

#### Process Visualization

```razor
@code {
    private async Task LoadProcessInstances()
    {
        var processes = await ApiClient.GetAsync<List<ProcessInstance>>(
            "/api/field/processes");
    }
    
    private async Task StartProcess(ProcessDefinition definition)
    {
        var result = await ApiClient.PostAsync<StartProcessRequest, ProcessInstanceResponse>(
            "/api/field/processes/start", new StartProcessRequest 
            { 
                ProcessDefinitionId = definition.Id 
            });
    }
}
```

### Workflow Components

- `WorkflowProgress.razor` - Visualize workflow progress
- `MultiOperationProgress.razor` - Track multiple operations

## Work Order Management

### Work Order Operations

Work orders integrate with lifecycle operations and production accounting.

#### API Integration

```razor
@code {
    private async Task CreateWorkOrder(WorkOrderRequest request)
    {
        var result = await ApiClient.PostAsync<WorkOrderRequest, WorkOrderResponse>(
            "/api/lifecycle/workorders", request);
    }
    
    private async Task RecordWorkOrderCost(string workOrderId, WorkOrderCostRequest request)
    {
        var result = await ApiClient.PostAsync<WorkOrderCostRequest, CostTransactionResponse>(
            $"/api/lifecycle/workorders/{workOrderId}/costs", request);
    }
    
    private async Task CreateAFEForWorkOrder(string workOrderId)
    {
        var result = await ApiClient.PostAsync<AFEResponse>(
            $"/api/lifecycle/workorders/{workOrderId}/afe");
    }
}
```

## Phase Transitions

### Transition Workflows

Fields transition between phases through defined workflows.

#### Transition API

```razor
@code {
    private async Task TransitionToDevelopment(string fieldId)
    {
        var result = await ApiClient.PostAsync<PhaseTransitionRequest, PhaseTransitionResponse>(
            "/api/field/transition", new PhaseTransitionRequest
            {
                FieldId = fieldId,
                TargetPhase = "Development",
                TransitionDate = DateTime.UtcNow
            });
    }
}
```

## Data Visualization

### Production Charts

Visualize production data over time:

```razor
@code {
    private async Task LoadProductionChart()
    {
        var data = await ApiClient.GetAsync<ProductionChartData>(
            $"/api/field/production/chart?fieldId={_fieldId}&startDate={startDate}&endDate={endDate}");
    }
}
```

### Well Status Visualization

Display well status across the field:

```razor
@code {
    private async Task LoadWellStatus()
    {
        var wells = await ApiClient.GetAsync<List<WellStatus>>(
            $"/api/field/wells/status?fieldId={_fieldId}");
    }
}
```

## Best Practices

### 1. Always Use Field Context

- Ensure field is selected before operations
- Pass fieldId in API requests
- Update UI when field changes

### 2. Handle Phase-Specific Operations

- Validate phase before allowing operations
- Show appropriate UI for current phase
- Guide users through phase transitions

### 3. Real-Time Updates

- Use SignalR for real-time updates
- Refresh data after operations
- Show progress for long-running operations

### 4. Error Handling

- Handle phase transition errors
- Validate data before submission
- Provide clear error messages

## Related Documentation

- [Architecture](beep-oilgas-web-architecture.md)
- [API Integration](beep-oilgas-web-api-integration.md)
- [Services](beep-oilgas-web-services.md)
- [Pages](beep-oilgas-web-pages.md)

