# Comprehensive List of Values (LOV) Population Plan

## Current Status

**Issue**: Only minimal placeholder data was created in seed data templates. The system needs comprehensive LOV data from:
- PPDM 3.9 Standard (861 RA_* reference tables)
- IHS Standards
- Other Industry Standards (API, ISO, etc.)

## Discovery

From PPDM39 scripts analysis:
- **861 RA_* reference tables** exist in PPDM standard
- These tables define standard values for:
  - Well Status (Life_Cycle, Role, Business_Interest facets)
  - Field Status
  - Cost Types
  - Accounting Methods
  - Units of Measure
  - And many more...

## Required Actions

### Phase 1: Identify All RA_* Tables

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

### Phase 2: Source Standard Values

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

### Phase 3: Create Data Import Service

1. **PPDMStandardValueImporter.cs**
   - Parse PPDM standard documentation
   - Extract values from PPDM Reference Lists
   - Map to RA_* table structures

2. **IHSStandardValueImporter.cs**
   - Import IHS reference codes
   - Map to PPDM RA_* tables where applicable

3. **StandardValueMapper.cs**
   - Map values from different standards to PPDM structure
   - Handle value conflicts and mappings

### Phase 4: Populate Seed Data Templates

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

### Phase 5: Well Status Standard (Priority)

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

