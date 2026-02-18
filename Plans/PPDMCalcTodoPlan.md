PPDM Calculation Service Rewrite Plan

Goals
- Make calculation result DTOs complete and PPDM-aligned.
- Remove ad-hoc dictionary inserts; persist typed entities consistently.
- Standardize mapping across all analysis types in PPDMCalculationService.

Constraints
- Calculation DTOs remain PPDM-bound and inherit from ModelEntityBase.
- No new ad-hoc data classes inside services; use Models project only.
- Preserve existing public APIs in LifeCycle services unless explicitly approved.

Inventory (what to validate)
- DCA: DCAResult, DCAForecastPoint, DCAFitResultEnhanced, DCA_FIT_RESULT.
- Economic: EconomicAnalysisResult, ECONOMIC_ANALYSIS_RESULT, NPV_PROFILE_POINT, ECONOMIC_CASH_FLOW.
- Nodal: NodalAnalysisResult, NODAL_ANALYSIS_RESULT, NODAL_IPR_POINT, NODAL_VLP_POINT, NODAL_OPERATING_POINT.
- Well Test: WellTestAnalysisResult, WELL_TEST_ANALYSIS_RESULT, WELL_TEST_DATA.
- Flash: FlashCalculationResult, FLASH_CALCULATION_RESULT, FLASH_COMPONENT, FLASH_CONDITIONS.
- Choke/Gas Lift/Pump/Sucker Rod/Compressor/Pipeline/Plunger/Hydraulic: verify result entity classes exist and match table names.

Work Plan
1) Schema and metadata audit
   - For each PPDM table used by PPDMCalculationService, list the expected columns.
   - Compare PPDM metadata EntityTypeName and table names with Models project entities.
   - Identify missing or duplicate properties in result DTOs.

2) Result DTO alignment (Models project)
   - Add missing properties to result DTOs based on PPDM column list.
   - Remove or consolidate duplicate properties and inconsistent names.
   - Ensure all result DTOs inherit from ModelEntityBase.
   - Add minimal JSON fields where structured results need persistence (e.g., points).

3) Mapping standards (LifeCycle)
   - Define a mapping matrix: DTO -> PPDM entity -> columns.
   - Standardize ID usage (CALCULATION_ID vs ANALYSIS_ID vs RESULT_ID).
   - Standardize well/facility/equipment references (WELL_ID, FACILITY_ID, EQUIPMENT_ID).
   - Standardize status and error fields across all results.

4) PPDMCalculationService rewrite
   - Replace dictionary inserts with typed entity creation.
   - Centralize mapping methods per analysis type.
   - Use metadata-resolved entity types and validated table names.
   - Add strict validation: fail fast if no entity type found or key columns missing.

5) Cross-project integration
   - Pull calculation inputs from other projects where available (Well Test, Flash, DCA, etc.).
   - Remove placeholder data generation paths.

6) Test and verification
   - Add unit tests for entity mapping (DTO -> entity).
   - Add integration tests for insert/read with PPDMGenericRepository.
   - Validate get-results APIs return correct typed DTOs.

Open Questions
- Which tables should store each analysis result when multiple options exist (e.g., CHOKE_ANALYSIS_RESULT vs CHOKE_FLOW_RESULT)?
- Should calculation detail points be stored in child tables or JSON columns?
- Which PPDM table names are authoritative when metadata differs from code?
