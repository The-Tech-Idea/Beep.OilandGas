# Usage Examples

## Overview

This document provides comprehensive usage examples for the PPDM39 Data Access Framework.

## Basic Repository Operations

### Creating a Repository

```csharp
var repository = new PPDMGenericRepository(
    editor,
    commonColumnHandler,
    defaults,
    metadata,
    typeof(WELL),
    connectionName: "PPDM39",
    tableName: "WELL");
```

### Inserting an Entity

```csharp
var well = new WELL
{
    WELL_ID = defaults.GenerateId("WELL"),
    WELL_NAME = "Test Well",
    FIELD_ID = "FIELD-001"
};

var inserted = await repository.InsertAsync(well, userId);
```

### Getting Entities with Filters

```csharp
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = "FIELD-001" },
    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
};

var wells = await repository.GetAsync(filters);
```

### Updating an Entity

```csharp
var well = await repository.GetByIdAsync("WELL-001");
well.WELL_NAME = "Updated Name";
var updated = await repository.UpdateAsync(well, userId);
```

### Deleting an Entity

```csharp
var deleted = await repository.DeleteAsync("WELL-001", userId);
```

## Validation

### Validating Before Insert

```csharp
var validationService = new PPDMDataValidationService(editor, commonColumnHandler, defaults, metadata);
var result = await validationService.ValidateAsync(well, "WELL");

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.FieldName}: {error.ErrorMessage}");
    }
}
else
{
    await repository.InsertAsync(well, userId);
}
```

## Data Quality

### Calculating Quality Score

```csharp
var qualityService = new PPDMDataQualityService(editor, commonColumnHandler, defaults, metadata);
var score = await qualityService.CalculateQualityScoreAsync(well, "WELL");

Console.WriteLine($"Overall Score: {score.OverallScore:P2}");
```

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Patterns and Best Practices](beep-dataaccess-patterns.md) - Best practices

