# Beep.OilandGas Projects - LifeCycle Integration Index

## Overview

This index provides a comprehensive list of all Beep.OilandGas projects and their integration documentation with the LifeCycle services. Each project has a dedicated integration guide explaining how it integrates with and can be used within the LifeCycle project.

---

## Architecture Strategy

### Service Categorization

#### 1. Centralized Calculation Service (`PPDMCalculationService`)
**Purpose:** All pure calculation/analysis projects that perform mathematical computations and analysis without operational data management.

**Projects:**
- ✅ DCA (Decline Curve Analysis) - Already integrated
- ✅ ProductionForecasting - Already integrated
- ⚠️ EconomicAnalysis - To be integrated
- ⚠️ NodalAnalysis - To be integrated
- ⚠️ WellTestAnalysis - To be integrated
- ⚠️ FlashCalculations - To be integrated

**Benefits:**
- Centralized data mapping to PPDM
- Consistent calculation result storage
- Single service for all analysis operations
- Easier maintenance and updates

#### 2. Phase Services (Operational Management)
**Purpose:** Projects that manage operational data and integrate with specific lifecycle phases.

**Services:**
- `PPDMExplorationService` - Exploration phase operations
- `PPDMDevelopmentService` - Development phase operations
- `PPDMProductionService` - Production phase operations
- `PPDMDecommissioningService` - Decommissioning phase operations
- `PPDMAccountingService` - Accounting operations (cross-phase)

**Projects:**
- GasLift → `PPDMDevelopmentService`
- PipelineAnalysis → `PPDMDevelopmentService`
- CompressorAnalysis → `PPDMDevelopmentService`
- ChokeAnalysis → `PPDMProductionService`
- SuckerRodPumping → `PPDMProductionService`

#### 3. Specialized Services
**Purpose:** Projects requiring dedicated services for specific functionality.

**Services:**
- `PermitManagementService` - Permits and applications management
- `PPDMAccountingService` - Accounting and production accounting

### Table Requirements Strategy

**Principle:** Use existing PPDM tables where possible, create new tables only when necessary.

#### Existing PPDM Tables (Use These)
- `WELL_TEST` - Well test data
- `WELL_EQUIPMENT` - Well equipment (chokes, sucker rod systems, gas lift)
- `FACILITY_EQUIPMENT` - Facility equipment (compressors)
- `PIPELINE` - Pipeline data
- `APPLICATION` - Permit applications
- `OBLIGATION` - Obligations and payments
- `FINANCE` - Financial transactions
- `PDEN` - Production entities
- `PDEN_VOL_SUMMARY` - Production volume summaries

#### New Tables Needed (To Be Created)
Following PPDM naming and structure patterns:
- `DCA_CALCULATION` - DCA analysis results (if not exists)
- `ECONOMIC_ANALYSIS` - Economic analysis results
- `NODAL_ANALYSIS` - Nodal analysis results
- `FLASH_CALCULATION` - Flash calculation results
- Additional accounting tables if needed (documented per project)

---

## Integration Documentation Files

### High Priority (Already Used in LifeCycle)

1. **[Beep.OilandGas.DCA](integration_DCA.md)** - Decline Curve Analysis
   - **Status:** ✅ Fully Integrated
   - **Service:** `PPDMCalculationService`
   - **Usage:** Production forecasting and decline analysis

2. **[Beep.OilandGas.ProductionForecasting](integration_ProductionForecasting.md)** - Production Forecasting
   - **Status:** ✅ Fully Integrated
   - **Service:** `PPDMCalculationService`
   - **Usage:** Physics-based production forecasting

3. **[Beep.OilandGas.Accounting](integration_Accounting.md)** - Accounting Calculations
   - **Status:** ✅ Partially Integrated
   - **Service:** `PPDMAccountingService`
   - **Usage:** Successful Efforts and Full Cost accounting

4. **[Beep.OilandGas.EconomicAnalysis](integration_EconomicAnalysis.md)** - Economic Evaluation
   - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
   - **Service:** `PPDMCalculationService` (planned)
   - **Usage:** NPV, IRR, payback period, economic analysis

### Medium Priority (Should Be Integrated)

5. **[Beep.OilandGas.NodalAnalysis](integration_NodalAnalysis.md)** - IPR/VLP Analysis
   - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
   - **Service:** `PPDMCalculationService` (planned)
   - **Usage:** Well performance analysis, production optimization

6. **[Beep.OilandGas.WellTestAnalysis](integration_WellTestAnalysis.md)** - Well Test Analysis
   - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
   - **Service:** `PPDMCalculationService` (planned)
   - **Usage:** Pressure transient analysis, permeability, skin factor

7. **[Beep.OilandGas.ProductionAccounting](integration_ProductionAccounting.md)** - Production Accounting
   - **Status:** ✅ Partially Integrated
   - **Service:** `PPDMAccountingService`
   - **Usage:** Production allocation, royalty calculations

8. **[Beep.OilandGas.GasLift](integration_GasLift.md)** - Gas Lift Analysis
   - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
   - **Service:** `PPDMDevelopmentService` (planned)
   - **Usage:** Gas lift design, valve spacing, optimization

9. **[Beep.OilandGas.PipelineAnalysis](integration_PipelineAnalysis.md)** - Pipeline Analysis
   - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
   - **Service:** `PPDMDevelopmentService` or `PipelineService` (planned)
   - **Usage:** Pipeline capacity, flow analysis, pressure drop

10. **[Beep.OilandGas.CompressorAnalysis](integration_CompressorAnalysis.md)** - Compressor Analysis
    - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
    - **Service:** `PPDMDevelopmentService` (planned)
    - **Usage:** Compressor power, pressure calculations

11. **[Beep.OilandGas.ChokeAnalysis](integration_ChokeAnalysis.md)** - Choke Flow Analysis
    - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
    - **Service:** `PPDMProductionService` (planned)
    - **Usage:** Choke sizing, flow rate calculations

### Lower Priority (Specialized)

12. **[Beep.OilandGas.SuckerRodPumping](integration_SuckerRodPumping.md)** - Sucker Rod Pumping
    - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
    - **Service:** `PPDMProductionService` (planned)
    - **Usage:** Sucker rod load analysis, power requirements

13. **[Beep.OilandGas.GasProperties](integration_GasProperties.md)** - Gas Properties
    - **Status:** ✅ Used by Other Projects
    - **Service:** Used indirectly by other calculation libraries
    - **Usage:** Z-factor, gas viscosity, pseudo-pressure

14. **[Beep.OilandGas.FlashCalculations](integration_FlashCalculations.md)** - Phase Behavior
    - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
    - **Service:** `PPDMCalculationService` (planned)
    - **Usage:** Phase equilibrium, flash calculations

15. **[Beep.OilandGas.PermitsAndApplications](integration_PermitsAndApplications.md)** - Permits Management
    - **Status:** ⚠️ Not Yet Integrated (Should be integrated)
    - **Service:** New `PermitManagementService` (planned)
    - **Usage:** Permit applications, regulatory compliance

### Additional Projects (To Be Documented)

16. **Beep.OilandGas.PlungerLift** - Plunger lift analysis
17. **Beep.OilandGas.HydraulicPumps** - Hydraulic pump analysis
18. **Beep.OilandGas.PumpPerformance** - Pump performance analysis
19. **Beep.OilandGas.OilProperties** - Oil property calculations
20. **Beep.OilandGas.FieldManagement** - Field management system
21. **Beep.OilandGas.ProspectIdentification** - Prospect identification
22. **Beep.OilandGas.DevelopmentPlanning** - Development planning
23. **Beep.OilandGas.Decommissioning** - Decommissioning operations
24. **Beep.OilandGas.DrillingAndConstruction** - Drilling operations
25. **Beep.OilandGas.LeaseAcquisition** - Lease acquisition
26. **Beep.OilandGas.EnhancedRecovery** - Enhanced recovery
27. **Beep.OilandGas.Drawing** - Visualization
28. **Beep.OilandGas.HeatMap** - Heat map visualization

---

## Integration Status Summary

### ✅ Fully Integrated (2 projects)
- Beep.OilandGas.DCA
- Beep.OilandGas.ProductionForecasting

### ✅ Partially Integrated (2 projects)
- Beep.OilandGas.Accounting
- Beep.OilandGas.ProductionAccounting

### ✅ Used by Other Projects (1 project)
- Beep.OilandGas.GasProperties

### ⚠️ Not Yet Integrated (10+ projects)
- Beep.OilandGas.EconomicAnalysis
- Beep.OilandGas.NodalAnalysis
- Beep.OilandGas.WellTestAnalysis
- Beep.OilandGas.GasLift
- Beep.OilandGas.PipelineAnalysis
- Beep.OilandGas.CompressorAnalysis
- Beep.OilandGas.ChokeAnalysis
- Beep.OilandGas.SuckerRodPumping
- Beep.OilandGas.FlashCalculations
- Beep.OilandGas.PermitsAndApplications
- And more...

---

## Integration Patterns

### 1. Calculation/Analysis Projects → `PPDMCalculationService`

**Pattern:** All pure calculation and analysis projects are integrated into `PPDMCalculationService` for centralized data mapping and result storage.

**Steps:**
1. Add calculation method to `PPDMCalculationService`
2. Create repository for result table (new or existing PPDM table)
3. Map input data from PPDM entities (WELL, POOL, FIELD)
4. Perform calculation using project library
5. Store results in PPDM table using `PPDMGenericRepository`
6. Return result DTO

**Example Projects:**
- DCA, ProductionForecasting, EconomicAnalysis, NodalAnalysis, WellTestAnalysis, FlashCalculations

### 2. Operational Projects → Phase Services

**Pattern:** Projects that manage operational data integrate into appropriate phase services.

**Steps:**
1. Add operational methods to phase service (Development, Production, etc.)
2. Use existing PPDM tables for equipment/operational data
3. Link to phase entities (WELL, FACILITY, PIPELINE, etc.)
4. Store operational results in appropriate PPDM tables

**Example Projects:**
- GasLift, PipelineAnalysis, CompressorAnalysis → `PPDMDevelopmentService`
- ChokeAnalysis, SuckerRodPumping → `PPDMProductionService`

### 3. Specialized Projects → Dedicated Services

**Pattern:** Projects requiring specialized functionality get dedicated services.

**Steps:**
1. Create dedicated service if needed
2. Use existing PPDM tables where possible
3. Create new tables only when necessary (following PPDM patterns)
4. Follow dependency injection patterns

**Example Projects:**
- PermitsAndApplications → `PermitManagementService`
- Accounting, ProductionAccounting → `PPDMAccountingService`

### Data Storage Pattern

**Principle:** Use existing PPDM tables where possible, create new only when necessary.

**Existing PPDM Tables (Preferred):**
- `WELL_TEST` - Well test data
- `WELL_EQUIPMENT` - Well equipment
- `FACILITY_EQUIPMENT` - Facility equipment
- `PIPELINE` - Pipeline data
- `APPLICATION` - Applications/permits
- `OBLIGATION` - Obligations
- `FINANCE` - Financial transactions
- `PDEN` - Production entities

**New Tables (When Needed):**
- Follow PPDM naming conventions
- Include standard PPDM columns (ROW_ID, ROW_CHANGED_BY, etc.)
- Link to entities via foreign keys (WELL_ID, FIELD_ID, etc.)
- Document table structure in integration docs

---

## Next Steps

1. **Review Integration Documentation**
   - Review all integration guides
   - Identify priority integrations
   - Plan implementation sequence

2. **Implement High-Priority Integrations**
   - EconomicAnalysis integration
   - NodalAnalysis integration
   - WellTestAnalysis integration

3. **Implement Medium-Priority Integrations**
   - GasLift integration
   - PipelineAnalysis integration
   - CompressorAnalysis integration
   - ChokeAnalysis integration

4. **Complete Documentation**
   - Document remaining projects
   - Update integration guides as implementations progress
   - Add usage examples and best practices

---

## Documentation Structure

Each integration document follows this structure:
1. **Overview** - Project purpose and capabilities
2. **Key Classes and Interfaces** - Main classes and methods
3. **Integration with LifeCycle Services** - Current/planned integration
4. **Usage Examples** - Code examples in LifeCycle context
5. **Integration Patterns** - How to add to services
6. **Data Storage** - PPDM tables and relationships
7. **Best Practices** - Usage recommendations
8. **Future Enhancements** - Planned improvements

---

## Table Requirements Summary

### New Tables Required (To Be Created)

Following PPDM naming and structure patterns, the following new tables need to be created:

#### Calculation/Analysis Tables (PPDMCalculationService)
1. **`DCA_CALCULATION`** - DCA and production forecasting results
   - Used by: DCA, ProductionForecasting
   - Links to: WELL, POOL, FIELD

2. **`ECONOMIC_ANALYSIS`** - Economic analysis results
   - Used by: EconomicAnalysis
   - Links to: PROSPECT, WELL, POOL, FIELD

3. **`NODAL_ANALYSIS`** - Nodal analysis results
   - Used by: NodalAnalysis
   - Links to: WELL

4. **`FLASH_CALCULATION`** - Flash calculation results
   - Used by: FlashCalculations
   - Links to: WELL, FACILITY

#### Accounting Tables (PPDMAccountingService)
5. **`ACCOUNTING_METHOD`** - Accounting method configuration
   - Used by: Accounting
   - Links to: FIELD

6. **`ACCOUNTING_COST`** - Accounting cost records
   - Used by: Accounting
   - Links to: Property, WELL, FIELD

7. **`ACCOUNTING_AMORTIZATION`** - Amortization records
   - Used by: Accounting
   - Links to: Property, WELL, POOL

8. **`ACCOUNTING_IMPAIRMENT`** - Impairment records
   - Used by: Accounting
   - Links to: Property

9. **`PRODUCTION_ALLOCATION`** - Production allocation records
   - Used by: ProductionAccounting
   - Links to: PDEN, FIELD

10. **`ROYALTY_CALCULATION`** - Royalty calculation records
    - Used by: ProductionAccounting
    - Links to: OBLIGATION, FIELD, WELL

#### Permit Management Tables
11. **`PERMIT_STATUS_HISTORY`** - Permit status change history
    - Used by: PermitsAndApplications
    - Links to: APPLICATION

### Existing Tables (Use These)

The following existing PPDM tables are used by various projects:

- **`WELL_TEST`** - Well test data (WellTestAnalysis)
- **`WELL_EQUIPMENT`** - Well equipment (GasLift, ChokeAnalysis, SuckerRodPumping)
- **`FACILITY_EQUIPMENT`** - Facility equipment (CompressorAnalysis)
- **`PIPELINE`** - Pipeline data (PipelineAnalysis)
- **`APPLICATION`** - Applications/permits (PermitsAndApplications)
- **`OBLIGATION`** - Obligations (Accounting, ProductionAccounting)
- **`FINANCE`** - Financial transactions (Accounting)
- **`PDEN`** - Production entities (ProductionAccounting)
- **`PDEN_VOL_SUMMARY`** - Production volume summaries (ProductionAccounting)

---

**Last Updated:** 2024  
**Total Projects Documented:** 15  
**Total Projects:** 28+

