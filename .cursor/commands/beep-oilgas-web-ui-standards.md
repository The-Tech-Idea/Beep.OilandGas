# Beep.OilandGas.Web UI/UX Standards Documentation

## Overview

This document outlines the UI/UX standards and best practices for the Beep Oil & Gas web application, following industry standards for oil and gas software applications.

## Design Principles

### 1. Clarity and Readability

- Clear visual hierarchy
- Consistent typography
- Adequate spacing and padding
- High contrast for readability

### 2. Efficiency

- Quick access to frequently used functions
- Minimal clicks to complete tasks
- Keyboard shortcuts where appropriate
- Batch operations for bulk actions

### 3. Safety and Compliance

- Clear warnings for critical operations
- Confirmation dialogs for destructive actions
- Audit trail visibility
- Compliance status indicators

### 4. Responsiveness

- Works on desktop, tablet, and mobile
- Adaptive layouts
- Touch-friendly controls
- Responsive data tables

## Dashboard Standards

### KPI Cards

KPI cards display key performance indicators with clear visual hierarchy.

#### Design Guidelines

- **Size**: Consistent card size (typically 3-4 columns on desktop)
- **Color Coding**: Use color to indicate status (green=good, yellow=warning, red=critical)
- **Icons**: Use meaningful icons to represent metrics
- **Trend Indicators**: Show trends (up/down arrows, percentage change)

#### Example

```razor
<MudCard Elevation="2" Class="h-100">
    <MudCardContent>
        <MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween">
            <MudStack>
                <MudText Typo="Typo.body2" Color="Color.Secondary">Production Volume</MudText>
                <MudText Typo="Typo.h4" Color="Color.Primary">@kpi.Value</MudText>
                <MudChip Size="Size.Small" Color="Color.Success">+5.2%</MudChip>
            </MudStack>
            <MudIcon Icon="@Icons.Material.Filled.TrendingUp" Size="Size.Large" Color="Color.Success" />
        </MudStack>
    </MudCardContent>
</MudCard>
```

### Charts and Graphs

#### Production Charts

- **Time Series**: Line charts for production trends
- **Bar Charts**: For comparing wells, fields, or periods
- **Pie Charts**: For allocation percentages
- **Interactive**: Tooltips, zoom, and drill-down capabilities

#### Chart Standards

- Clear axis labels with units
- Legend for multiple series
- Responsive sizing
- Export capability (PNG, PDF)

### Alert Panels

#### Alert Types

- **Safety Alerts**: Red, high priority
- **Compliance Warnings**: Yellow/Orange, medium priority
- **Data Quality Issues**: Blue, informational
- **System Notifications**: Gray, low priority

#### Alert Display

```razor
<MudAlert Severity="Severity.Warning" 
          Dismissible="true"
          Variant="Variant.Filled">
    <MudText Typo="Typo.body2">@alert.Message</MudText>
    <MudButton Size="Size.Small" Variant="Variant.Text" OnClick="@HandleAlert">
        View Details
    </MudButton>
</MudAlert>
```

### Status Indicators

#### Well Status

- **Producing**: Green
- **Shut-in**: Yellow
- **Abandoned**: Red
- **Drilling**: Blue
- **Testing**: Orange

#### Field Status

- **Active**: Green
- **Development**: Blue
- **Decommissioning**: Red
- **Exploration**: Purple

## Data Display Standards

### Tables

#### Requirements

- Sortable columns
- Filterable rows
- Pagination for large datasets
- Export to CSV/Excel
- Row selection
- Responsive design

#### Example

```razor
<MudTable Items="@_wells" 
          Hover="true"
          Striped="true"
          Dense="true"
          SortMode="SortMode.Multiple"
          FilterMode="FilterMode.Simple">
    <HeaderContent>
        <MudTh>Well Name</MudTh>
        <MudTh>Status</MudTh>
        <MudTh>Production</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd DataLabel="Well Name">@context.WellName</MudTd>
        <MudTd DataLabel="Status">
            <MudChip Color="@GetStatusColor(context.Status)" Size="Size.Small">
                @context.Status
            </MudChip>
        </MudTd>
        <MudTd DataLabel="Production">@context.ProductionVolume</MudTd>
    </RowTemplate>
</MudTable>
```

### Forms

#### Form Standards

- Clear field labels
- Required field indicators (*)
- Inline validation feedback
- Help text for complex fields
- Logical field grouping
- Multi-step wizards for complex forms

#### Form Layout

```razor
<MudForm @ref="_form" @bind-IsValid="@_isValid">
    <MudTextField @bind-Value="@_fieldName" 
                  Label="Field Name"
                  Required="true"
                  RequiredError="Field name is required"
                  HelperText="Enter the official field name" />
    
    <MudSelect @bind-Value="@_fieldType"
               Label="Field Type"
               Required="true">
        <MudSelectItem Value="Onshore">Onshore</MudSelectItem>
        <MudSelectItem Value="Offshore">Offshore</MudSelectItem>
    </MudSelect>
</MudForm>
```

### Dialogs

#### Confirmation Dialogs

Always use confirmation dialogs for destructive actions:

```razor
private async Task DeleteWell(string wellId)
{
    var parameters = new DialogParameters
    {
        ["Title"] = "Delete Well",
        ["Message"] = "Are you sure you want to delete this well? This action cannot be undone.",
        ["ConfirmButtonText"] = "Delete",
        ["CancelButtonText"] = "Cancel"
    };
    
    var dialog = await DialogService.ShowAsync<ConfirmDialog>("Delete Well", parameters);
    var result = await dialog.Result;
    
    if (!result.Canceled)
    {
        await DeleteWellAsync(wellId);
    }
}
```

## Visualization Standards

### Well Visualization

#### Wellbore Diagrams

- Vertical depth representation
- Completion intervals
- Production zones
- Equipment locations

#### Completion Diagrams

- Casing strings
- Tubing configuration
- Packers and seals
- Perforations

### Field Maps

#### Interactive Maps

- Well locations
- Facility locations
- Pipeline routes
- Field boundaries
- Production zones

#### Map Features

- Zoom and pan
- Layer toggling
- Click for details
- Measurement tools
- Export capability

### Production Charts

#### Time Series Charts

- Daily, monthly, yearly views
- Multiple series (oil, gas, water)
- Cumulative production
- Forecast overlays

#### Chart Components

```razor
<MudChart ChartType="ChartType.Line" 
          ChartOptions="@_chartOptions"
          ChartData="@_chartData" />
```

## Compliance and Safety

### Regulatory Compliance

#### Compliance Status Indicators

- **Compliant**: Green checkmark
- **Pending Review**: Yellow warning
- **Non-Compliant**: Red alert
- **Expired**: Gray

#### Compliance Display

```razor
<MudCard>
    <MudCardContent>
        <MudStack Row="true" AlignItems="AlignItems.Center" Justify="Justify.SpaceBetween">
            <MudText Typo="Typo.h6">Environmental Permit</MudText>
            <MudChip Color="@GetComplianceColor(permit.Status)" Size="Size.Small">
                @permit.Status
            </MudChip>
        </MudStack>
        <MudText Typo="Typo.body2" Color="Color.Secondary">
            Expires: @permit.ExpiryDate.ToString("yyyy-MM-dd")
        </MudText>
    </MudCardContent>
</MudCard>
```

### Safety Indicators

#### Safety Alerts

- High visibility (red background, white text)
- Cannot be dismissed without acknowledgment
- Link to safety procedures
- Escalation for critical alerts

### Audit Trails

#### Audit Display

- Chronological timeline
- User identification
- Action descriptions
- Before/after values
- Filtering and search

## Color Palette

### Primary Colors

- **Primary**: Oil & Gas brand color (typically blue or green)
- **Secondary**: Complementary color
- **Accent**: Highlight color for important elements

### Status Colors

- **Success/Good**: Green (#4caf50)
- **Warning**: Yellow/Orange (#ff9800)
- **Error/Critical**: Red (#f44336)
- **Info**: Blue (#2196f3)

### Data Colors

- **Oil**: Dark brown/black
- **Gas**: Light blue
- **Water**: Blue
- **Mixed**: Gradient

## Typography

### Font Hierarchy

- **H1**: Page titles (24px)
- **H2**: Section titles (20px)
- **H3**: Subsection titles (18px)
- **H4**: Card titles (16px)
- **Body**: Regular text (14px)
- **Caption**: Small text (12px)

### Font Weights

- **Bold**: Headings, important text
- **Regular**: Body text
- **Light**: Secondary text

## Spacing and Layout

### Grid System

- Use MudBlazor's 12-column grid
- Responsive breakpoints (xs, sm, md, lg, xl)
- Consistent spacing (4px, 8px, 16px, 24px, 32px)

### Padding and Margins

- **Card Padding**: 16px (pa-4)
- **Section Spacing**: 24px (mt-6)
- **Element Spacing**: 8px (gap-2)

## Accessibility

### Requirements

- Keyboard navigation
- Screen reader support
- High contrast mode
- Focus indicators
- ARIA labels

### Implementation

```razor
<MudButton Variant="Variant.Filled"
           Color="Color.Primary"
           OnClick="@Save"
           AriaLabel="Save field data"
           Title="Save (Ctrl+S)">
    Save
</MudButton>
```

## Responsive Design

### Breakpoints

- **xs**: < 600px (Mobile)
- **sm**: 600px - 960px (Tablet)
- **md**: 960px - 1280px (Small Desktop)
- **lg**: 1280px - 1920px (Desktop)
- **xl**: > 1920px (Large Desktop)

### Responsive Components

```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="4" lg="3">
        <!-- Content -->
    </MudItem>
</MudGrid>
```

## Best Practices

### 1. Consistency

- Use consistent components across the application
- Follow established patterns
- Maintain visual hierarchy

### 2. Feedback

- Provide immediate feedback for user actions
- Show loading states
- Display success/error messages

### 3. Performance

- Lazy load heavy components
- Virtualize long lists
- Optimize images and assets

### 4. User Experience

- Minimize cognitive load
- Provide clear navigation
- Offer undo/redo where possible

## Related Documentation

- [Components](beep-oilgas-web-components.md)
- [Pages](beep-oilgas-web-pages.md)
- [Theming](beep-oilgas-web-theming.md)

