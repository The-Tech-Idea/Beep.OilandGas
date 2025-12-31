# Seed Data Management

## Overview

The seed data management system provides comprehensive capabilities for seeding PPDM39 databases with reference data, LOV data, and initial data sets from CSV files and JSON templates.

## Key Components

### PPDMSeederOrchestrator

Orchestrates seeding of all PPDM39 tables from CSV files.

**Key Methods:**
- `SeedAllAsync`: Seeds all CSV files from a folder

**Example:**
```csharp
var orchestrator = new PPDMSeederOrchestrator(editor, commonColumnHandler, defaults, metadata);
var summary = await orchestrator.SeedAllAsync(@"C:\SeedData\CSV", userId);
Console.WriteLine($"Seeded {summary.TotalRecordsSeeded} records from {summary.ProcessedFiles} files");
```

### PPDMCSVSeeder

Seeds data from CSV files.

**Key Methods:**
- `SeedAsync`: Seeds a single CSV file

### PPDMReferenceDataSeeder

Seeds reference data from JSON templates.

**Key Methods:**
- `SeedReferenceDataAsync`: Seeds reference data

### PPDMSeedDataValidator

Validates seed data before seeding.

**Key Methods:**
- `ValidateSeedDataAsync`: Validates seed data

## Seed Data Sources

1. **CSV Files**: Tabular data in CSV format
2. **JSON Templates**: Structured reference data
3. **IHS Standard Values**: Industry standard values
4. **PPDM Standard Values**: PPDM standard values

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [LOV Management](beep-dataaccess-lov-management.md) - LOV management

