# PPDM39 Data Access Framework - Overview

## Purpose

The `Beep.OilandGas.PPDM39.DataManagement` project provides a comprehensive data access framework for working with PPDM39 (Petroleum Public Data Model version 3.9) databases. It offers metadata-driven, database-agnostic data access patterns that work with any data source through the Beep Data Management Engine.

## Key Features

### Core Components

1. **PPDMGenericRepository** - Generic repository for CRUD operations on any PPDM entity
2. **CommonColumnHandler** - Automatic management of common PPDM columns (audit, temporal, active indicators)
3. **QueryBuilder** - Database-agnostic query building utilities
4. **PPDMTreeBuilder** - Tree structure building from PPDM metadata

### Data Management Services

1. **Data Validation Service** - Entity validation against business rules
2. **Data Quality Service** - Data quality metrics and scoring
3. **Data Quality Dashboard Service** - Real-time quality dashboards
4. **Data Versioning Service** - Entity version tracking and rollback
5. **Data Access Audit Service** - Access tracking for compliance
6. **Database Creator Service** - Automated database creation from scripts
7. **Script Execution Services** - Script discovery, categorization, and execution
8. **LOV Management Service** - List of Values management
9. **Mapping Service** - DTO to PPDM model conversion

### Metadata Management

- PPDM39 metadata loading and caching
- Table metadata access
- Subject area and module organization
- Relationship discovery

### Seed Data Management

- CSV data seeding
- Reference data seeding
- Seed data validation
- Seed orchestration

## Architecture

### Design Principles

1. **Metadata-Driven**: All operations use PPDM metadata to automatically configure behavior
2. **Database-Agnostic**: Works with any data source through `IDMEEditor` abstraction
3. **AppFilter-Based**: All queries use `AppFilter` for data source independence
4. **Type-Safe**: Uses strongly-typed entities, no Dictionary-based operations
5. **Audit-Ready**: Built-in support for audit trails via common columns

### Core Dependencies

- `IDMEEditor` - Data access engine abstraction
- `ICommonColumnHandler` - Common column management
- `IPPDM39DefaultsRepository` - Default value generation
- `IPPDMMetadataRepository` - Metadata access
- `TheTechIdea.Beep.DataManagementEngine` - Core data management framework

### Integration Points

- **PPDM39 Models**: Uses PPDM39 entity models
- **Beep Framework**: Integrates with Beep Data Management Engine
- **LifeCycle Services**: Used by LifeCycle services for data access
- **ProductionAccounting**: Used by ProductionAccounting services

## Project Structure

```
Beep.OilandGas.PPDM39.DataManagement/
├── Core/
│   ├── Common/
│   │   └── CommonColumnHandler.cs
│   ├── Metadata/
│   │   ├── PPDMMetadataRepository.cs
│   │   ├── PPDMMetadataLoader.cs
│   │   └── PPDM39Metadata.json
│   ├── Models/
│   │   └── DatabaseCreation/
│   ├── PPDMGenericRepository.cs
│   ├── QueryBuilder.cs
│   └── Tree/
│       └── PPDMTreeBuilder.cs
├── Services/
│   ├── PPDMDataValidationService.cs
│   ├── PPDMDataQualityService.cs
│   ├── PPDMDataQualityDashboardService.cs
│   ├── PPDMDataVersioningService.cs
│   ├── PPDMDataAccessAuditService.cs
│   ├── PPDMDatabaseCreatorService.cs
│   ├── PPDMScriptExecutionEngine.cs
│   ├── PPDMScriptDiscoveryService.cs
│   ├── LOVManagementService.cs
│   └── PPDMMappingService.cs
├── Repositories/
│   ├── PPDM39DefaultsRepository.cs
│   ├── WELL/
│   │   └── WellRepository.cs
│   └── PPDMModuleRepository.cs
└── SeedData/
    ├── PPDMSeederOrchestrator.cs
    ├── PPDMCSVSeeder.cs
    └── PPDMReferenceDataSeeder.cs
```

## Service Registration

### Basic Service Registration

```csharp
// Register core dependencies
builder.Services.AddScoped<ICommonColumnHandler>(sp =>
    new CommonColumnHandler());

builder.Services.AddScoped<IPPDM39DefaultsRepository>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new PPDM39DefaultsRepository(editor, "PPDM39");
});

builder.Services.AddScoped<IPPDMMetadataRepository>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    return new PPDMMetadataRepository(editor);
});

// Register data management services
builder.Services.AddScoped<IPPDMDataValidationService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new PPDMDataValidationService(editor, commonColumnHandler, defaults, metadata);
});

builder.Services.AddScoped<IPPDMDataQualityService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new PPDMDataQualityService(editor, commonColumnHandler, defaults, metadata);
});
```

## Key Concepts

### Generic Repository Pattern

The `PPDMGenericRepository` provides CRUD operations for any PPDM entity without requiring specific repository classes:

```csharp
var repository = new PPDMGenericRepository(
    editor, commonColumnHandler, defaults, metadata,
    typeof(WELL), connectionName: "PPDM39", tableName: "WELL");

// Insert
var well = new WELL { WELL_ID = "WELL-001", ... };
await repository.InsertAsync(well, userId);

// Get with filters
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = "FIELD-001" }
};
var wells = await repository.GetAsync(filters);

// Update
well.WELL_NAME = "Updated Name";
await repository.UpdateAsync(well, userId);

// Delete
await repository.DeleteAsync(well.WELL_ID, userId);
```

### Common Columns

All PPDM entities have common columns managed automatically:

- `ROW_CREATED_BY` / `ROW_CREATED_DATE` - Creation audit
- `ROW_CHANGED_BY` / `ROW_CHANGED_DATE` - Change audit
- `ACTIVE_IND` - Active indicator (Y/N)
- `ROW_EFFECTIVE_DATE` / `ROW_EXPIRY_DATE` - Temporal tracking
- `PPDM_GUID` - Unique identifier

### AppFilter Pattern

All queries use `AppFilter` for database-agnostic filtering:

```csharp
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = "FIELD-001" },
    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
    new AppFilter { FieldName = "CREATED_DATE", Operator = ">=", FilterValue = startDate }
};
```

## Related Documentation

- [Generic Repository](beep-dataaccess-generic-repository.md) - Detailed repository documentation
- [Common Columns](beep-dataaccess-common-columns.md) - Common column management
- [Data Validation](beep-dataaccess-validation.md) - Validation service
- [Data Quality](beep-dataaccess-quality.md) - Quality services
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples
- [Patterns and Best Practices](beep-dataaccess-patterns.md) - Best practices

