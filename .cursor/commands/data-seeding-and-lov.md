# Data Seeding and List of Values (LOV) Population

## Overview

This document outlines patterns for seeding reference data (LOV) and converting Excel seed data files to CSV format for PPDM39 tables.

## Excel to CSV Conversion

### Prerequisites
- Microsoft Excel must be installed on your system
- PowerShell (included with Windows)

### Usage

Run the PowerShell script to convert all Excel files in the PPDM_DATA folder to CSV:

```powershell
.\ConvertExcelToCsv.ps1
```

Or specify custom folders:

```powershell
.\ConvertExcelToCsv.ps1 -SourceFolder "C:\Path\To\Excel\Files" -OutputFolder "C:\Path\To\CSV\Output"
```

### What it does

1. Scans the source folder for all `.xlsx` files
2. Opens each Excel file using Excel COM automation
3. Converts each worksheet to CSV format
4. Saves CSV files to the output folder
5. Handles multiple worksheets by creating separate CSV files (e.g., `Well_Status_Type_Sheet1.csv`)

### Output

CSV files will be saved to:
- Default: `C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\PPDM_DATA\CSV`
- Or the folder specified by `-OutputFolder` parameter

## Well Status Files

The following Excel files are particularly important for well status seeding:

- `Well_Status_Type_PPDM_20220225.xlsx` → `R_WELL_STATUS` table
- `Business_Intention_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Business Intention")
- `Business_Interest_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Business Interest")
- `Life_Cycle_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Business Life Cycle Phase")
- `Outcome_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Outcome")
- `Role_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Role")
- `Play_Type_PPDM_20220425.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Play Type")
- `Well_Structure_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Well Structure")
- `Fluid_Direction_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Fluid Direction")
- `Trajectory_Type_PPDM_20220421.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Trajectory Type")
- `Wellbore_Status_PPDM_20220610.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Wellbore Status")
- `Well_Status_PPDM_20220225.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Well Status")
- `Fluid_Type_PPDM_20220419.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Fluid Type")
- `Well_Reporting_Class_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Well Reporting Class")
- `Lahee_Class_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Lahee Class")
- `Operatorship_PPDM_20220310.xlsx` → `R_WELL_STATUS` table (STATUS_TYPE = "Operatorship")

## Reference Data (LOV) Population

### Current Status

**Issue**: Only minimal placeholder data was created in seed data templates. The system needs comprehensive LOV data from:
- PPDM 3.9 Standard (861 RA_* reference tables)
- IHS Standards
- Other Industry Standards (API, ISO, etc.)

### Discovery

From PPDM39 scripts analysis:
- **861 RA_* reference tables** exist in PPDM standard
- These tables define standard values for:
  - Well Status (Life_Cycle, Role, Business_Interest facets)
  - Field Status
  - Cost Types
  - Accounting Methods
  - Units of Measure
  - And many more...

### Required Actions

#### Phase 1: Identify All RA_* Tables

1. **Extract all RA_* table names from PPDM scripts**
   - Source: `Beep.OilandGas.PPDM39/Scripts/Sqlserver/TAB.sql`
   - Count: 861 tables
   - Output: Complete list of all RA_* tables

2. **Categorize RA_* tables by domain**
   - Well-related (RA_WELL_*)
   - Field-related (RA_FIELD_*)
   - Cost-related (RA_COST_*)
   - Accounting-related (RA_ACCOUNT_*)
   - Analysis-related (RA_ANL_*)
   - And others...

#### Phase 2: Source Standard Values

1. **PPDM Standard Values**
   - Access PPDM Reference Lists from: https://www.ppdm.org/standards
   - Well Status Standard: http://www.ppdm.org/standards/wellstatus
   - Extract standard values for each RA_* table
   - Include all facets, qualifiers, and values

2. **IHS Standards**
   - IHS Markit reference codes
   - Well classification codes
   - Completion type codes
   - Production status codes

3. **Other Industry Standards**
   - API (American Petroleum Institute) codes
   - ISO standards
   - Regulatory agency codes (state/provincial)

#### Phase 3: Create Data Import Service

```csharp
// PPDMStandardValueImporter.cs
public class PPDMStandardValueImporter
{
    // Parse PPDM standard documentation
    // Extract values from PPDM Reference Lists
    // Map to RA_* table structures
}

// IHSStandardValueImporter.cs
public class IHSStandardValueImporter
{
    // Import IHS reference codes
    // Map to PPDM RA_* tables where applicable
}

// StandardValueMapper.cs
public class StandardValueMapper
{
    // Map values from different standards to PPDM structure
    // Handle value conflicts and mappings
}
```

#### Phase 4: Populate Seed Data Templates

1. **Update PPDMReferenceData.json**
   - Add all 861 RA_* tables
   - Populate with standard values from PPDM
   - Include all facets and qualifiers for Well Status

2. **Create IHSReferenceData.json**
   - IHS-specific reference values
   - Map to PPDM tables where applicable

3. **Create IndustryStandardsReferenceData.json**
   - API codes
   - ISO standards
   - Regulatory codes

## Well Status Standard (Priority)

The Well Status standard is complex with multiple facets:
- **Life_Cycle**: Drilling, Completed, Producing, Shut-In, Abandoned, etc.
- **Role**: Producer, Injector, Disposal, Observation, etc.
- **Business_Interest**: Operator, Non-Operator, etc.
- **Fluid_Type**: Oil, Gas, Water, etc.
- And many more facets...

**Required Tables**:
- RA_WELL_STATUS
- RA_WELL_STATUS_TYPE
- RA_WELL_STATUS_QUAL
- RA_WELL_STATUS_QUAL_VALUE
- RA_WELL_STATUS_SYMBOL
- RA_WELL_STATUS_XREF

## PPDM39DefaultsRepository Usage

### Getting Default Values

```csharp
// Get default well status types
var defaults = new PPDM39DefaultsRepository(editor, connectionName);
var wellStatusTypes = await defaults.GetWellStatusTypesAsync();

// Get default values for entity
var defaultValues = await defaults.GetEntityDefaultsAsync("WELL");

// Format ID for table
var formattedId = defaults.FormatIdForTable("WELL", wellId);
```

### Setting Default Values

```csharp
// Set default values when creating entity
var well = new WELL
{
    WELL_ID = defaults.FormatIdForTable("WELL", Guid.NewGuid().ToString()),
    WELL_NAME = "Well-001",
    ACTIVE_IND = defaults.GetDefaultActiveInd(), // "Y"
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow
};
```

## Seed Data Templates

### Structure

```json
{
  "tables": [
    {
      "tableName": "R_WELL_STATUS",
      "data": [
        {
          "STATUS_TYPE": "Life_Cycle",
          "STATUS_VALUE": "Drilling",
          "DESCRIPTION": "Well is currently being drilled",
          "ACTIVE_IND": "Y"
        }
      ]
    }
  ]
}
```

### Usage

1. Review the CSV files to ensure data quality
2. Create SQL INSERT scripts or C# seed data classes from the CSV files
3. Use the seed data to populate `R_WELL_STATUS` and other reference tables
4. Ensure all STATUS_TYPE values match the default well status types defined in `PPDM39DefaultsRepository`

## Implementation Strategy

### Option 1: Manual Population (Initial)
- Review PPDM documentation
- Manually populate high-priority RA_* tables
- Focus on most commonly used tables first

### Option 2: Automated Import (Recommended)
- Create parser for PPDM Reference Lists (if available in structured format)
- Import from CSV/Excel if PPDM provides data files
- Use web scraping/API if PPDM provides online access

### Option 3: Hybrid Approach
- Start with manual population of critical tables
- Build automated import for bulk tables
- Validate against PPDM standard documentation

## Priority Tables (Most Critical)

1. **RA_WELL_STATUS** - Well status with all facets
2. **RA_FIELD_STATUS** - Field status values
3. **RA_COST_TYPE** - Cost type classifications
4. **RA_ACCOUNTING_METHOD** - Accounting methods (SE, FC)
5. **RA_UNIT_OF_MEASURE** - Complete UOM list
6. **RA_ROW_QUALITY** - Data quality indicators
7. **RA_SOURCE** - Data source indicators
8. **RA_COMPLETION_TYPE** - Completion types
9. **RA_COMPLETION_STATUS_TYPE** - Completion status
10. **RA_PROPERTY_STATUS** - Property status

## Next Steps

1. Extract complete list of all 861 RA_* tables
2. Identify data sources (PPDM docs, IHS, etc.)
3. Create import services
4. Populate seed data templates systematically
5. Validate against PPDM standard documentation

## Notes

- Excel files may contain multiple worksheets - each will be converted to a separate CSV
- The script preserves the original Excel file structure
- CSV files use UTF-8 encoding
- Date formats are preserved from Excel

## References

- See `Beep.OilandGas.PPDM39.DataManagement.Tools/README_DATA_SEEDING.md` for Excel conversion guide
- See `Beep.OilandGas.PPDM39.DataManagement/SeedData/COMPREHENSIVE_LOV_POPULATION_PLAN.md` for LOV population plan
- See PPDM documentation: https://www.ppdm.org/standards

