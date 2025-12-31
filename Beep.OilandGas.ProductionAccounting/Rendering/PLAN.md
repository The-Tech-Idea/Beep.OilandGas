# Rendering Module Enhancement Plan

## Current State Analysis

### Existing Files
- `ProductionChartRenderer.cs` - Chart rendering
- `AllocationChartRenderer.cs` - Allocation chart rendering
- `RevenueChartRenderer.cs` - Revenue chart rendering
- `ProductionChartRendererConfiguration.cs` - Chart configuration

### Issues Identified
1. **No Service Interface**: Missing IRenderingService interface
2. **Missing Workflows**: No chart history tracking, no chart export, no chart customization

## Entity/DTO Migration

### Classes Status

**Rendering is typically stateless** - No entity migration needed unless tracking chart history

## Service Class Creation

### New Service: RenderingService (Optional)

**Note**: Rendering is typically stateless. Service class may not be needed unless we want to track chart history or save chart configurations.

**If Service Needed**:
- Location: `Beep.OilandGas.ProductionAccounting/Rendering/RenderingService.cs`
- Interface: `Beep.OilandGas.PPDM39/Core/Interfaces/IRenderingService.cs`
- Use PPDMGenericRepository for CHART_CONFIGURATION if saving configurations

## Database Integration

### Tables (Optional)

**CHART_CONFIGURATION** (if saving configurations):
- CHART_CONFIGURATION_ID (PK)
- CHART_TYPE
- CONFIGURATION_DATA (JSON)
- Standard PPDM audit columns

### Decision

**Recommendation**: Keep rendering as utility classes unless chart configuration persistence is required.

## Missing Workflows

### 1. Chart Configuration Management (Optional)
- Save chart configurations
- Load chart configurations
- Configuration sharing

### 2. Chart Export
- Export charts to images (PNG, JPEG)
- Export charts to PDF
- Export history

## Implementation Steps

### Option 1: Keep as Utility Classes (Recommended)
1. Keep rendering as utility classes
2. No service class needed
3. No database integration needed
4. Rendering is called by other services/UI

### Option 2: Add Service with Configuration Persistence
1. Create entity class for CHART_CONFIGURATION
2. Create service interface and implementation
3. Use PPDMGenericRepository
4. Save/load chart configurations

## Testing Requirements

1. Test chart rendering
2. Test chart export
3. Test chart configuration (if implemented)

## Dependencies

- SkiaSharp (for rendering)
- Beep.OilandGas.Drawing (if used)

