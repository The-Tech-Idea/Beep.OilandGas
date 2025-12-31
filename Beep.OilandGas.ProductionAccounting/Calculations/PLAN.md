# Calculations Module Enhancement Plan

## Current State Analysis

### Existing Files
- `ProductionCalculations.cs` - Static calculation methods
- `CrudeOilCalculations.cs` - Static calculation methods

### Issues Identified
1. **No Database Integration**: Static classes don't save calculation results
2. **No Service Class**: Missing ICalculationsService interface
3. **Missing Workflows**: No calculation history tracking, no calculation reporting

## Entity/DTO Migration

### Classes to Create in Beep.OilandGas.Models

**Create in `Beep.OilandGas.Models/Data/Calculations/`:**
- `CALCULATION_RESULT` (entity class with PPDM audit columns, optional)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Calculations/`:**
- `CalculationRequest`
- `CalculationResultResponse`

**Keep in ProductionAccounting:**
- `ProductionCalculations` static methods (calculation logic)
- `CrudeOilCalculations` static methods (calculation logic)

## Service Class Creation

### New Service: CalculationsService (Optional)

**Note**: Calculations are typically stateless operations. Service class may not be needed unless we want to track calculation history.

**If Service Needed**:
- Location: `Beep.OilandGas.ProductionAccounting/Calculations/CalculationsService.cs`
- Interface: `Beep.OilandGas.PPDM39/Core/Interfaces/ICalculationsService.cs`
- Use PPDMGenericRepository for CALCULATION_RESULT if tracking needed

## Database Integration

### Tables (Optional)

**CALCULATION_RESULT** (only if tracking needed):
- CALCULATION_RESULT_ID (PK)
- CALCULATION_TYPE
- CALCULATION_DATE
- INPUT_DATA (JSON)
- RESULT_DATA (JSON)
- Standard PPDM audit columns

### Decision

**Recommendation**: Keep calculations as static utility classes unless calculation history tracking is required.

## Missing Workflows

### 1. Calculation History Tracking (Optional)
- Track calculation results
- Calculation history search
- Calculation audit trail

### 2. Calculation Reporting (Optional)
- Calculation summary reports
- Calculation detail reports

## Implementation Steps

### Option 1: Keep as Static Classes (Recommended)
1. Keep calculations as static utility classes
2. No service class needed
3. No database integration needed
4. Calculations are called by other services

### Option 2: Add Service with History Tracking
1. Create entity class for CALCULATION_RESULT
2. Create service interface and implementation
3. Use PPDMGenericRepository
4. Track calculation history

## Testing Requirements

1. Test calculation methods
2. Test calculation accuracy
3. Test calculation performance

## Dependencies

- None (calculations are utility methods)

