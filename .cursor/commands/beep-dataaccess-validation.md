# Data Validation Service

## Overview

The `PPDMDataValidationService` validates PPDM entities against business rules and data quality rules. It provides automatic validation rule generation from metadata and supports custom business rule validation.

## Key Features

- **Automatic Rule Generation**: Generates validation rules from PPDM metadata
- **Primary Key Validation**: Validates primary key existence and format
- **Required Field Validation**: Validates required fields
- **Format Validation**: Validates field formats (e.g., UWI format)
- **Custom Business Rules**: Supports custom business rule validation
- **Batch Validation**: Supports batch validation operations

## Service Interface

```csharp
public interface IPPDMDataValidationService
{
    Task<ValidationResult> ValidateAsync(object entity, string tableName);
    Task<List<ValidationRule>> GetValidationRulesAsync(string tableName);
    Task<BatchValidationResult> ValidateBatchAsync(IEnumerable<object> entities, string tableName);
}
```

## Key Methods

### ValidateAsync

Validates an entity against business rules.

```csharp
public async Task<ValidationResult> ValidateAsync(object entity, string tableName)
```

**Example:**
```csharp
var validationService = new PPDMDataValidationService(editor, commonColumnHandler, defaults, metadata);

var well = new WELL { WELL_ID = "WELL-001", WELL_NAME = "Test Well" };
var result = await validationService.ValidateAsync(well, "WELL");

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.FieldName}: {error.ErrorMessage}");
    }
}
```

### GetValidationRulesAsync

Gets validation rules for a table.

```csharp
public async Task<List<ValidationRule>> GetValidationRulesAsync(string tableName)
```

**Example:**
```csharp
var rules = await validationService.GetValidationRulesAsync("WELL");
foreach (var rule in rules)
{
    Console.WriteLine($"{rule.FieldName}: {rule.RuleType} - {rule.Description}");
}
```

### ValidateBatchAsync

Validates multiple entities in a batch.

```csharp
public async Task<BatchValidationResult> ValidateBatchAsync(IEnumerable<object> entities, string tableName)
```

**Example:**
```csharp
var wells = new List<WELL> { well1, well2, well3 };
var batchResult = await validationService.ValidateBatchAsync(wells, "WELL");

Console.WriteLine($"Valid: {batchResult.ValidCount}, Invalid: {batchResult.InvalidCount}");
```

## Validation Rules

### Automatic Rules

The service automatically generates rules from metadata:

1. **Primary Key Validation**: Ensures primary key exists and is valid
2. **Required Field Validation**: Validates required fields are not null
3. **Format Validation**: Validates field formats (e.g., UWI, dates)
4. **Active Indicator Validation**: Validates ACTIVE_IND values ('Y' or 'N')

### Custom Business Rules

You can add custom business rules:

```csharp
// Custom validation example
var customRule = new ValidationRule
{
    FieldName = "WELL_NAME",
    RuleType = "CUSTOM",
    Description = "Well name must be unique",
    IsActive = true
};
```

## Validation Result

```csharp
public class ValidationResult
{
    public object Entity { get; set; }
    public string TableName { get; set; }
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; }
    public List<ValidationWarning> Warnings { get; set; }
}
```

## Best Practices

### 1. Validate Before Insert/Update

```csharp
// ✅ Good - Validate before database operations
var result = await validationService.ValidateAsync(well, "WELL");
if (result.IsValid)
{
    await repository.InsertAsync(well, userId);
}

// ❌ Bad - Insert without validation
await repository.InsertAsync(well, userId);
```

### 2. Handle Validation Errors

```csharp
// ✅ Good - Handle validation errors appropriately
var result = await validationService.ValidateAsync(well, "WELL");
if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        _logger.LogError("Validation error: {Field} - {Message}", error.FieldName, error.ErrorMessage);
    }
    throw new ValidationException("Entity validation failed", result.Errors);
}
```

### 3. Use Batch Validation for Imports

```csharp
// ✅ Good - Use batch validation for data imports
var batchResult = await validationService.ValidateBatchAsync(importedWells, "WELL");
if (batchResult.InvalidCount > 0)
{
    // Log invalid entities
    foreach (var invalid in batchResult.InvalidEntities)
    {
        _logger.LogWarning("Invalid entity: {EntityId}", invalid.EntityId);
    }
}
```

## Integration Points

- **PPDMGenericRepository**: Can be used before repository operations
- **Data Quality Service**: Uses validation for quality scoring
- **Import Services**: Validates imported data
- **API Controllers**: Validates request data

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Data Quality](beep-dataaccess-quality.md) - Quality services
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples
- [Patterns and Best Practices](beep-dataaccess-patterns.md) - Best practices

