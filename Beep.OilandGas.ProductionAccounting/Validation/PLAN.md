# Validation Module Enhancement Plan

## Current State Analysis

### Existing Files
- `CrudeOilValidator.cs` - Static validation methods
- `LeaseValidator.cs` - Static validation methods
- `EnhancedValidators.cs` - Enhanced validation methods

### Issues Identified
1. **No Database Integration**: Static classes don't save validation results
2. **No Service Class**: Missing IValidationService interface
3. **Missing Workflows**: No validation history tracking, no validation reporting, no validation rules management

## Entity/DTO Migration

### Classes to Create in Beep.OilandGas.Models

**Create in `Beep.OilandGas.Models/Data/Validation/`:**
- `VALIDATION_RESULT` (entity class with PPDM audit columns)
- `VALIDATION_RULE` (entity class, optional)

**Create DTOs in `Beep.OilandGas.Models/DTOs/Validation/`:**
- `ValidationRequest`
- `ValidationResultResponse`
- `ValidationRuleRequest`
- `ValidationRuleResponse`

**Keep in ProductionAccounting:**
- `CrudeOilValidator` static methods (validation logic)
- `LeaseValidator` static methods (validation logic)
- `EnhancedValidators` static methods (validation logic)

## Service Class Creation

### New Service: ValidationService

**Location**: `Beep.OilandGas.ProductionAccounting/Validation/ValidationService.cs`

**Interface**: `Beep.OilandGas.PPDM39/Core/Interfaces/IValidationService.cs`

```csharp
public interface IValidationService
{
    Task<ValidationResult> ValidateEntityAsync<T>(T entity, string entityType, string? connectionName = null);
    Task<ValidationResult> ValidateCrudeOilPropertiesAsync(CrudeOilProperties properties, string? connectionName = null);
    Task<ValidationResult> ValidateLeaseAsync(object lease, string? connectionName = null);
    
    Task<VALIDATION_RESULT> SaveValidationResultAsync(ValidationResult result, string userId, string? connectionName = null);
    Task<List<VALIDATION_RESULT>> GetValidationHistoryAsync(string? entityType, string? entityId, string? connectionName = null);
    
    // Missing workflows
    Task<ValidationRule> CreateValidationRuleAsync(CreateValidationRuleRequest request, string userId, string? connectionName = null);
    Task<List<ValidationRule>> GetValidationRulesAsync(string? entityType, string? connectionName = null);
    Task<ValidationSummary> GetValidationSummaryAsync(string? entityType, DateTime? startDate, DateTime? endDate, string? connectionName = null);
}
```

**Implementation**:
- Constructor takes: IDMEEditor, ICommonColumnHandler, IPPDM39DefaultsRepository, IPPDMMetadataRepository, ILoggerFactory, connectionName
- Uses PPDMGenericRepository for VALIDATION_RESULT table
- Calls static validator methods
- Saves results to database

## Database Integration

### Tables Required

**VALIDATION_RESULT**:
- VALIDATION_RESULT_ID (PK)
- ENTITY_TYPE
- ENTITY_ID
- VALIDATION_DATE
- IS_VALID
- VALIDATION_ERRORS (JSON or structured)
- Standard PPDM audit columns

**VALIDATION_RULE** (optional):
- VALIDATION_RULE_ID (PK)
- ENTITY_TYPE
- RULE_NAME
- RULE_DEFINITION
- Standard PPDM audit columns

### PPDMGenericRepository Usage

```csharp
var metadata = await _metadata.GetTableMetadataAsync("VALIDATION_RESULT");
var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Validation.{metadata.EntityTypeName}");
var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "VALIDATION_RESULT");
```

## Missing Workflows

### 1. Validation History Tracking
- Track all validations
- Validation history search
- Validation trends
- Validation audit trail

### 2. Validation Rules Management
- Create and manage validation rules
- Rule versioning
- Rule activation/deactivation
- Rule reporting

### 3. Validation Reporting
- Validation summary reports
- Validation detail reports
- Validation by entity type reports
- Validation error reports

## Database Scripts

### Scripts to Create

**For VALIDATION_RESULT, VALIDATION_RULE**:
- `{TABLE}_TAB.sql` (6 database types each)
- `{TABLE}_PK.sql`
- `{TABLE}_FK.sql`

## Implementation Steps

### Step 1: Create Entity Classes
1. Create entity classes in `Beep.OilandGas.Models/Data/Validation/`
2. Add standard PPDM audit columns

### Step 2: Create DTOs
1. Create request/response DTOs in `Beep.OilandGas.Models/DTOs/Validation/`

### Step 3: Create Service Interface
1. Create `IValidationService` interface
2. Define all service methods

### Step 4: Create Service Class
1. Create `ValidationService.cs`
2. Implement IValidationService
3. Use PPDMGenericRepository
4. Call static validator methods
5. Save results to database
6. Add missing workflow methods

### Step 5: Create Database Scripts
1. Generate TAB/PK/FK scripts (6 database types)

### Step 6: Implement Missing Workflows
1. Implement validation history tracking
2. Implement validation rules management
3. Enhance validation reporting

## Testing Requirements

1. Test entity validation
2. Test validation result storage
3. Test validation history
4. Test validation rules management

## Dependencies

- Beep.OilandGas.Models (for entity classes)
- Beep.OilandGas.PPDM39 (for PPDMGenericRepository)
- All other modules (for entities to validate)

