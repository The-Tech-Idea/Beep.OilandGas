# Integration Matrix (Project → Service → PPDM)

Purpose: map each solution project to the LifeCycle service owner, PPDM storage tables, and lifecycle phase.

## Core Calculations (Centralized in PPDMCalculationService)

| Project | Phase | LifeCycle Service | PPDM Tables (Exact) | Custom/Extension Tables | Status |
| --- | --- | --- | --- | --- | --- |
| Beep.OilandGas.DCA | Production | PPDMCalculationService | DCA_CALCULATION | N/A | Integrated |
| Beep.OilandGas.ProductionForecasting | Production | PPDMCalculationService | PRODUCTION_FORECAST | N/A | Integrated |
| Beep.OilandGas.EconomicAnalysis | Cross-phase | PPDMCalculationService | ECONOMIC_ANALYSIS (new) | N/A | Planned |
| Beep.OilandGas.NodalAnalysis | Production | PPDMCalculationService | NODAL_ANALYSIS (new) | N/A | Planned |
| Beep.OilandGas.WellTestAnalysis | Production | PPDMCalculationService | WELL_TEST; ANL_ANALYSIS_REPORT (fallback) | WELL_TEST_ANALYSIS (new) | Planned |
| Beep.OilandGas.FlashCalculations | Cross-phase | PPDMCalculationService | FLASH_CALCULATION (new) | N/A | Planned |
| Beep.OilandGas.GasLift | Production | PPDMCalculationService + PPDMProductionService | GAS_LIFT_ANALYSIS_RESULT; ANL_ANALYSIS_REPORT (fallback); WELL_EQUIPMENT (context) | N/A | Planned |
| Beep.OilandGas.ChokeAnalysis | Production | PPDMCalculationService + PPDMProductionService | CHOKE_ANALYSIS_RESULT; ANL_ANALYSIS_REPORT (fallback); WELL_EQUIPMENT (context) | N/A | Planned |
| Beep.OilandGas.PipelineAnalysis | Development/Production | PPDMCalculationService + PipelineManagementService | PIPELINE_ANALYSIS_RESULT; ANL_ANALYSIS_REPORT (fallback); PIPELINE | N/A | Planned |
| Beep.OilandGas.CompressorAnalysis | Development/Production | PPDMCalculationService + PPDMDevelopmentService | COMPRESSOR_ANALYSIS_RESULT; ANL_ANALYSIS_REPORT (fallback); FACILITY_EQUIPMENT | N/A | Planned |
| Beep.OilandGas.SuckerRodPumping | Production | PPDMCalculationService + PPDMProductionService | SUCKER_ROD_ANALYSIS_RESULT; ANL_ANALYSIS_REPORT (fallback); WELL_EQUIPMENT | N/A | Planned |
| Beep.OilandGas.PlungerLift | Production | PPDMCalculationService + PPDMProductionService | PLUNGER_LIFT_ANALYSIS_RESULT; ANL_ANALYSIS_REPORT (fallback); WELL_EQUIPMENT | N/A | Planned |
| Beep.OilandGas.HydraulicPumps | Production | PPDMCalculationService + PPDMProductionService | HYDRAULIC_PUMP_ANALYSIS_RESULT; ANL_ANALYSIS_REPORT (fallback); WELL_EQUIPMENT | N/A | Planned |
| Beep.OilandGas.PumpPerformance | Production | PPDMCalculationService + PPDMProductionService | PUMP_ANALYSIS_RESULT; ANL_ANALYSIS_REPORT (fallback); WELL_EQUIPMENT | N/A | Planned |

## Phase Services (Operational Management)

| Project | Phase | LifeCycle Service | PPDM Tables (Exact) | Custom/Extension Tables | Status |
| --- | --- | --- | --- | --- | --- |
| Beep.OilandGas.ProspectIdentification | Exploration | PPDMExplorationService | SEIS_ACQTN_SURVEY; SEIS_LINE; WELL; LEASE | PROSPECT; PROSPECT_ANALOG; PROSPECT_BA; PROSPECT_DISCOVERY; PROSPECT_ECONOMIC; PROSPECT_FIELD; PROSPECT_HISTORY; PROSPECT_MIGRATION; PROSPECT_PLAY; PROSPECT_PORTFOLIO; PROSPECT_RANKING; PROSPECT_RESERVOIR; PROSPECT_RISK_ASSESSMENT; PROSPECT_SEIS_SURVEY; PROSPECT_SOURCE_ROCK; PROSPECT_TRAP; PROSPECT_VOLUME_ESTIMATE; PROSPECT_WELL; PROSPECT_WORKFLOW_STAGE | Partial |
| Beep.OilandGas.LeaseAcquisition | Exploration | PPDMExplorationService | LEASE | LEASE_ACQUISITION; FEE_MINERAL_LEASE; GOVERNMENT_LEASE; NET_PROFIT_LEASE | Planned |
| Beep.OilandGas.DrillingAndConstruction | Development | PPDMDevelopmentService | WELL; WELL_XREF; DRILLING_OPERATION; WELL_EQUIPMENT | N/A | Planned |
| Beep.OilandGas.DevelopmentPlanning | Development | PPDMDevelopmentService | POOL; WELL; WELL_XREF; FACILITY; PIPELINE; WELL_EQUIPMENT; FACILITY_EQUIPMENT | DevelopmentPlan; WellPlan; FacilityPlan | Partial |
| Beep.OilandGas.ProductionOperations | Production | PPDMProductionService | PDEN_VOL_SUMMARY; RESERVE_ENTITY; WELL_TEST; PRODUCTION_FORECAST; PRODUCTION_FACILITY; WELL; FACILITY; WELL_EQUIPMENT | N/A | Partial |
| Beep.OilandGas.ProductionAccounting | Production | PPDMAccountingService | FINANCE; OBLIGATION | ACCOUNTING_METHOD; ACCOUNTING_AMORTIZATION; WELL_ALLOCATION_DATA; ACTUAL_DELIVERY | Partial |
| Beep.OilandGas.Accounting | Cross-phase | PPDMAccountingService | FINANCE; OBLIGATION | COST (custom) | Partial |
| Beep.OilandGas.Decommissioning | Decommissioning | PPDMDecommissioningService | WELL_ABANDONMENT; FACILITY_DECOMMISSIONING; ENVIRONMENTAL_RESTORATION; DECOMMISSIONING_COST; WELL; FACILITY | N/A | Partial |
| Beep.OilandGas.PermitsAndApplications | Cross-phase | PermitManagementService | APPLICATION; APPLICATION_COMPONENT | PERMIT_APPLICATION; DRILLING_PERMIT_APPLICATION; ENVIRONMENTAL_PERMIT_APPLICATION; INJECTION_PERMIT_APPLICATION; APPLICATION_ATTACHMENT; REQUIRED_FORM; PERMIT_STATUS_HISTORY | Partial |

## Service Implementation Matrix (LifeCycle)

| Service Class | Primary Methods (Existing) | Primary DTOs (PPDM-bound) | PPDM Tables (Exact) |
| --- | --- | --- | --- |
| Services/Calculations/PPDMCalculationService.cs | PerformDCAAnalysisAsync; PerformEconomicAnalysisAsync; PerformNodalAnalysisAsync; PerformWellTestAnalysisAsync; PerformFlashCalculationAsync; PerformChokeAnalysisAsync; PerformGasLiftAnalysisAsync; PerformPumpAnalysisAsync; PerformSuckerRodAnalysisAsync; PerformCompressorAnalysisAsync; PerformPipelineAnalysisAsync; PerformPlungerLiftAnalysisAsync; PerformHydraulicPumpAnalysisAsync; GetCalculationResultAsync; GetCalculationResultsAsync | DCARequest; EconomicAnalysisRequest; NodalAnalysisRequest; WellTestAnalysisCalculationRequest; FlashCalculationRequest; ChokeAnalysisRequest; GasLiftAnalysisRequest; PumpAnalysisRequest; SuckerRodAnalysisRequest; CompressorAnalysisRequest; PipelineAnalysisRequest; PlungerLiftAnalysisRequest; HydraulicPumpAnalysisRequest | DCA_CALCULATION; ECONOMIC_ANALYSIS; NODAL_ANALYSIS; WELL_TEST; FLASH_CALCULATION; CHOKE_ANALYSIS_RESULT; GAS_LIFT_ANALYSIS_RESULT; PUMP_ANALYSIS_RESULT; SUCKER_ROD_ANALYSIS_RESULT; COMPRESSOR_ANALYSIS_RESULT; PIPELINE_ANALYSIS_RESULT; PLUNGER_LIFT_ANALYSIS_RESULT; HYDRAULIC_PUMP_ANALYSIS_RESULT; ANL_ANALYSIS_REPORT |
| Services/Exploration/PPDMExplorationService.cs | GetProspectsForFieldAsync; CreateProspectForFieldAsync; UpdateProspectForFieldAsync; GetProspectForFieldAsync; DeleteProspectForFieldAsync; GetSeismicSurveysForFieldAsync; CreateSeismicSurveyForFieldAsync; GetExploratoryWellsForFieldAsync; GetSeismicLinesForSurveyAsync; IdentifyProspectAsync; EvaluateProspectAsync; AcquireLeaseAsync; ManageLeaseAsync | ProspectRequest; ProspectEvaluation; SeismicSurveyRequest; CreateLease; UpdateLease | PROSPECT; PROSPECT_*; SEIS_ACQTN_SURVEY; SEIS_LINE; WELL; LEASE |
| Services/Development/PPDMDevelopmentService.cs | GetPoolsForFieldAsync; CreatePoolForFieldAsync; UpdatePoolForFieldAsync; GetPoolForFieldAsync; GetDevelopmentWellsForFieldAsync; CreateDevelopmentWellForFieldAsync; GetWellboresForWellAsync; GetFacilitiesForFieldAsync; CreateFacilityForFieldAsync; GetPipelinesForFieldAsync; CreatePipelineForFieldAsync; AnalyzeGasLiftPotentialAsync; DesignGasLiftValvesAsync; CalculateGasLiftValveSpacingAsync; AnalyzePipelineCapacityAsync; AnalyzePipelineFlowAsync; AnalyzeCompressorPowerAsync; AnalyzeCompressorPressureAsync; PlanDevelopmentAsync; DesignWellAsync; ExecuteDrillingAsync; ConstructFacilityAsync | PoolRequest; DevelopmentWellRequest; FacilityRequest; PipelineRequest; CreateDevelopmentPlan; CreateWellPlan; CreateDrillingOperation | POOL; WELL; WELL_XREF; DRILLING_OPERATION; FACILITY; PIPELINE; WELL_EQUIPMENT; FACILITY_EQUIPMENT |
| Services/Production/PPDMProductionService.cs | GetProductionForFieldAsync; CreateProductionForFieldAsync; GetProductionByPoolForFieldAsync; GetReservesForFieldAsync; GetWellTestsForWellAsync; GetProductionForecastsForFieldAsync; CreateProductionForecastForFieldAsync; GetFacilityProductionForFieldAsync; AnalyzeChokeFlowAsync; CalculateChokeSizingAsync; AnalyzeSuckerRodLoadAsync; AnalyzeSuckerRodPowerAsync; ManageProductionOperationsAsync; OptimizeProductionAsync; PlanEnhancedRecoveryAsync; ExecuteEnhancedRecoveryAsync | ProductionRequest; ProductionForecastRequest; CreateEnhancedRecoveryOperation | PDEN_VOL_SUMMARY; RESERVE_ENTITY; WELL_TEST; PRODUCTION_FORECAST; PRODUCTION_FACILITY; WELL; FACILITY; WELL_EQUIPMENT |
| Services/Decommissioning/PPDMDecommissioningService.cs | GetAbandonedWellsForFieldAsync; AbandonWellForFieldAsync; GetWellAbandonmentForFieldAsync; GetDecommissionedFacilitiesForFieldAsync; DecommissionFacilityForFieldAsync; GetFacilityDecommissioningForFieldAsync; GetEnvironmentalRestorationsForFieldAsync; CreateEnvironmentalRestorationForFieldAsync; GetDecommissioningCostsForFieldAsync; EstimateCostsForFieldAsync; PlanDecommissioningAsync; ExecuteDecommissioningAsync; VerifyDecommissioningAsync | WellAbandonmentRequest; FacilityDecommissioningRequest; EnvironmentalRestorationRequest | WELL_ABANDONMENT; FACILITY_DECOMMISSIONING; ENVIRONMENTAL_RESTORATION; DECOMMISSIONING_COST; WELL; FACILITY |
| Services/FacilityManagement/FacilityManagementService.cs | CreateFacilityAsync; GetFacilityAsync; RecordFacilityOperationsAsync; CreateFacilityMaintenanceAsync; CreateFacilityInspectionAsync; AssessFacilityIntegrityAsync; RecordFacilityEquipmentAsync; GetFacilityPerformanceAsync; CreateMaintenanceWorkOrderAsync; CreateRepairWorkOrderAsync; CreateUpgradeWorkOrderAsync; CreateInspectionWorkOrderAsync; AnalyzeCompressorAsync; AnalyzeFacilityPumpAsync | FacilityCreationRequest; FacilityOperationsRequest; FacilityMaintenanceRequest; FacilityInspectionRequest; FacilityIntegrityRequest; FacilityEquipmentRequest; FacilityWorkOrderRequest | FACILITY; EQUIPMENT; FACILITY_EQUIPMENT; WORK_ORDER; WORK_ORDER_XREF |
| Services/WellManagement/WellManagementService.cs | CreateWellAsync; GetWellAsync; CreateWellPlanningAsync; RecordWellOperationsAsync; CreateWellMaintenanceAsync; CreateWellInspectionAsync; RecordWellEquipmentAsync; CreateRigWorkoverAsync; CreateRiglessWorkoverAsync; CreateWirelineWorkAsync; CreateCoiledTubingWorkAsync; CreateSnubbingWorkAsync; CreateWellTestAsync; CreateStimulationAsync; CreateCleanoutAsync; GetWellPerformanceAsync; RunWellNodalAnalysisAsync; RunWellDCAAsync; RunWellTestAnalysisAsync; AnalyzeWellChokeAsync; AnalyzeGasLiftAsync; AnalyzeWellPumpAsync; AnalyzeSuckerRodSystemAsync; AnalyzePlungerLiftAsync; AnalyzeHydraulicPumpAsync | WellCreationRequest; WellPlanningRequest; WellOperationsRequest; WellMaintenanceRequest; WellInspectionRequest; WellEquipmentRequest; RigWorkoverRequest; RiglessWorkoverRequest; WirelineWorkRequest; CoiledTubingWorkRequest; WellTestRequest; StimulationRequest; CleanoutRequest | WELL; WELL_EQUIPMENT; WELL_TEST; WORK_ORDER; WORK_ORDER_XREF |
| Services/FieldManagement/FieldManagementService.cs | CreateFieldAsync; GetFieldAsync; CreateFieldPlanningAsync; RecordFieldOperationsAsync; UpdateFieldConfigurationAsync; GetFieldPerformanceAsync; RunFieldDCAAsync; RunPoolDCAAsync | FieldCreationRequest; FieldPlanningRequest; FieldOperationsRequest; FieldConfigurationRequest; FieldPerformanceRequest | FIELD |
| Services/PipelineManagement/PipelineManagementService.cs | CreatePipelineAsync; GetPipelineAsync; RecordPipelineOperationsAsync; CreatePipelineMaintenanceAsync; CreatePipelineInspectionAsync; AssessPipelineIntegrityAsync; RecordPipelineFlowAsync; GetPipelineCapacityAsync; CreatePipelineMaintenanceWorkOrderAsync; CreatePipelineRepairWorkOrderAsync; CreatePipelineInspectionWorkOrderAsync; CreatePipelineIntegrityTestWorkOrderAsync; CreatePipelineCleaningWorkOrderAsync; CreatePipelineModificationWorkOrderAsync; AnalyzePipelineAsync | PipelineCreationRequest; PipelineOperationsRequest; PipelineMaintenanceRequest; PipelineInspectionRequest; PipelineIntegrityRequest; PipelineFlowRequest | PIPELINE; WORK_ORDER; WORK_ORDER_XREF |
| Services/WorkOrder/WorkOrderManagementService.cs | CreateWorkOrderAsync; UpdateWorkOrderAsync; GetWorkOrderAsync; GetWorkOrdersByEntityAsync | WorkOrderCreationRequest; WorkOrderUpdateRequest | WORK_ORDER; WORK_ORDER_XREF |
| Services/FieldLifecycle/FieldLifecycleService.cs | TransitionFieldPhaseAsync; GetAvailablePhaseTransitionsAsync; CanTransitionPhaseAsync; GetCurrentFieldPhaseAsync; GetFieldPhaseStatusAsync; ValidatePhaseTransitionAsync; ValidatePhaseCompletionAsync | FieldPhaseStatus; ValidationResult | FIELD_PHASE |
| Services/ReservoirLifecycle/ReservoirLifecycleService.cs | TransitionReservoirStateAsync; GetAvailableTransitionsAsync; CanTransitionAsync; GetCurrentReservoirStateAsync; GetReservoirStateHistoryAsync; ValidateStateTransitionAsync; ValidateReservoirStateAsync | ValidationResult | RESERVOIR_STATUS |
| Services/WellLifecycle/WellLifecycleService.cs | TransitionWellStateAsync; GetAvailableTransitionsAsync; CanTransitionAsync; GetCurrentWellStateAsync; GetWellStateHistoryAsync; ValidateStateTransitionAsync; ValidateWellStateAsync | ValidationResult | WELL_STATUS |

## Method → DTO → PPDM Column Map (Minimum)

All DTOs listed here are PPDM-bound and must inherit from `Beep.OilandGas.Models/Data/ModelEntityBase.cs`.

### Services/Calculations/PPDMCalculationService.cs

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| PerformDCAAnalysisAsync | DCARequest, DCAResult | DCA_CALCULATION | DCA_CALCULATION_ID, WELL_ID, FIELD_ID, CALCULATION_DATE, CALCULATION_TYPE, STATUS |
| PerformEconomicAnalysisAsync | EconomicAnalysisRequest, EconomicAnalysisResult | ECONOMIC_ANALYSIS | ECONOMIC_ANALYSIS_ID, FIELD_ID, CALCULATION_DATE, STATUS |
| PerformNodalAnalysisAsync | NodalAnalysisRequest, NodalAnalysisResult | NODAL_ANALYSIS | NODAL_ANALYSIS_ID, WELL_ID, CALCULATION_DATE, STATUS |
| PerformWellTestAnalysisAsync | WellTestAnalysisCalculationRequest, WellTestAnalysisResult | WELL_TEST / WELL_TEST_ANALYSIS (new) | WELL_TEST_ID, WELL_ID, TEST_DATE, STATUS |
| PerformFlashCalculationAsync | FlashCalculationRequest, FlashCalculationResult | FLASH_CALCULATION | FLASH_CALCULATION_ID, WELL_ID, FACILITY_ID, CALCULATION_DATE, STATUS |
| PerformChokeAnalysisAsync | ChokeAnalysisRequest, ChokeAnalysisResult | CHOKE_ANALYSIS_RESULT | CHOKE_ANALYSIS_RESULT_ID, WELL_ID, CALCULATION_DATE, STATUS |
| PerformGasLiftAnalysisAsync | GasLiftAnalysisRequest, GasLiftAnalysisResult | GAS_LIFT_ANALYSIS_RESULT | GAS_LIFT_ANALYSIS_RESULT_ID, WELL_ID, CALCULATION_DATE, STATUS |
| PerformPumpAnalysisAsync | PumpAnalysisRequest, PumpAnalysisResult | PUMP_ANALYSIS_RESULT | PUMP_ANALYSIS_RESULT_ID, WELL_ID, CALCULATION_DATE, STATUS |
| PerformSuckerRodAnalysisAsync | SuckerRodAnalysisRequest, SuckerRodAnalysisResult | SUCKER_ROD_ANALYSIS_RESULT | SUCKER_ROD_ANALYSIS_RESULT_ID, WELL_ID, CALCULATION_DATE, STATUS |
| PerformCompressorAnalysisAsync | CompressorAnalysisRequest, CompressorAnalysisResult | COMPRESSOR_ANALYSIS_RESULT | COMPRESSOR_ANALYSIS_RESULT_ID, FACILITY_ID, CALCULATION_DATE, STATUS |
| PerformPipelineAnalysisAsync | PipelineAnalysisRequest, PipelineAnalysisResult | PIPELINE_ANALYSIS_RESULT | PIPELINE_ANALYSIS_RESULT_ID, PIPELINE_ID, CALCULATION_DATE, STATUS |
| PerformPlungerLiftAnalysisAsync | PlungerLiftAnalysisRequest, PlungerLiftAnalysisResult | PLUNGER_LIFT_ANALYSIS_RESULT | PLUNGER_LIFT_ANALYSIS_RESULT_ID, WELL_ID, CALCULATION_DATE, STATUS |
| PerformHydraulicPumpAnalysisAsync | HydraulicPumpAnalysisRequest, HydraulicPumpAnalysisResult | HYDRAULIC_PUMP_ANALYSIS_RESULT | HYDRAULIC_PUMP_ANALYSIS_RESULT_ID, WELL_ID, CALCULATION_DATE, STATUS |

Fallback (when result tables not available):
- ANL_ANALYSIS_REPORT: ANL_ANALYSIS_ID, ENTITY_ID, ENTITY_TYPE, ANALYSIS_TYPE, CALCULATION_DATE, STATUS

### Services/Exploration/PPDMExplorationService.cs

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| CreateProspectForFieldAsync | ProspectRequest, PROSPECT | PROSPECT | PROSPECT_ID, PROSPECT_NAME, FIELD_ID (or PRIMARY_FIELD_ID), PROSPECT_TYPE, PROSPECT_STATUS, ACTIVE_IND |
| UpdateProspectForFieldAsync | ProspectRequest, PROSPECT | PROSPECT | PROSPECT_ID, PROSPECT_NAME, PROSPECT_STATUS |
| CreateSeismicSurveyForFieldAsync | SeismicSurveyRequest, SEIS_ACQTN_SURVEY | SEIS_ACQTN_SURVEY | SEIS_ACQTN_SURVEY_ID, ACQTN_SURVEY_NAME, AREA_ID, AREA_TYPE, ACTIVE_IND |
| AcquireLeaseAsync | CreateLease, Lease | LEASE | LEASE_ID, LEASE_NUMBER (or LEASE_NUM), FIELD_ID, EFFECTIVE_DATE, EXPIRY_DATE, ACTIVE_IND |
| ManageLeaseAsync | UpdateLease, Lease | LEASE | LEASE_ID, FIELD_ID, EFFECTIVE_DATE, EXPIRY_DATE, ACTIVE_IND |

### Services/Development/PPDMDevelopmentService.cs

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| CreatePoolForFieldAsync | PoolRequest, POOL | POOL | POOL_ID, POOL_NAME, FIELD_ID |
| CreateDevelopmentWellForFieldAsync | DevelopmentWellRequest, WELL | WELL | UWI, FIELD_ID, WELL_NAME, WELL_TYPE, STATUS |
| CreateFacilityForFieldAsync | FacilityRequest, FACILITY | FACILITY | FACILITY_ID, FACILITY_SHORT_NAME, PRIMARY_FIELD_ID, FACILITY_TYPE, FACILITY_FUNCTION |
| CreatePipelineForFieldAsync | PipelineRequest, PIPELINE | PIPELINE | PIPELINE_ID, PIPELINE_NAME, FIELD_ID, PIPELINE_TYPE, DIAMETER, LENGTH |
| ExecuteDrillingAsync | CreateDrillingOperation, DrillingOperation | DRILLING_OPERATION | DRILLING_OPERATION_ID, WELL_ID, START_DATE, OPERATION_TYPE, STATUS |
| PlanDevelopmentAsync | CreateDevelopmentPlan, DevelopmentPlan | DevelopmentPlan (custom) | PlanId, FieldId, PlanDate, TargetStartDate, TargetCompletionDate |

### Services/Production/PPDMProductionService.cs

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| CreateProductionForFieldAsync | ProductionRequest, ProductionResponse | PDEN_VOL_SUMMARY | PDEN_ID, PERIOD_ID, ACTIVITY_TYPE, OIL_VOL, GAS_VOL, WATER_VOL |
| CreateProductionForecastForFieldAsync | ProductionForecastRequest, ProductionForecastResponse | PRODUCTION_FORECAST | PRODUCTION_FORECAST_ID, FIELD_ID, FORECAST_DATE, OIL_RATE, GAS_RATE, WATER_RATE |
| GetWellTestsForWellAsync | WellTestResponse | WELL_TEST | WELL_TEST_ID, WELL_ID, TEST_DATE, STATUS |

### Services/Decommissioning/PPDMDecommissioningService.cs

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| AbandonWellForFieldAsync | WellAbandonmentRequest, WellAbandonmentResponse | WELL_ABANDONMENT | WELL_ABANDONMENT_ID, WELL_ID, ABANDONMENT_DATE, STATUS |
| DecommissionFacilityForFieldAsync | FacilityDecommissioningRequest, FacilityDecommissioningResponse | FACILITY_DECOMMISSIONING | FACILITY_DECOMMISSIONING_ID, FACILITY_ID, STATUS |
| CreateEnvironmentalRestorationForFieldAsync | EnvironmentalRestorationRequest, EnvironmentalRestorationResponse | ENVIRONMENTAL_RESTORATION | ENVIRONMENTAL_RESTORATION_ID, FIELD_ID, RESTORATION_STATUS, COMPLETION_DATE |
| EstimateCostsForFieldAsync | DecommissioningCostEstimateResponse | DECOMMISSIONING_COST | DECOMMISSIONING_COST_ID, FIELD_ID, COST_AMOUNT, COST_DATE |

### Services/FacilityManagement/FacilityManagementService.cs

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| CreateFacilityAsync | FacilityCreationRequest, FacilityResponse | FACILITY | FACILITY_ID, FACILITY_SHORT_NAME, PRIMARY_FIELD_ID, FACILITY_TYPE, FACILITY_FUNCTION, ACTIVE_IND |
| RecordFacilityEquipmentAsync | FacilityEquipmentRequest | EQUIPMENT; FACILITY_EQUIPMENT | EQUIPMENT.EQUIPMENT_ID, EQUIPMENT_TYPE, EQUIPMENT_NAME; FACILITY_EQUIPMENT.FACILITY_ID, FACILITY_TYPE, EQUIPMENT_ID, INSTALL_OBS_NO, INSTALL_DATE |

### Services/WellManagement/WellManagementService.cs

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| CreateWellAsync | WellCreationRequest, WellResponse | WELL | UWI, FIELD_ID, WELL_NAME, WELL_TYPE, STATUS |
| RecordWellEquipmentAsync | WellEquipmentRequest | WELL_EQUIPMENT | WELL_ID, EQUIPMENT_TYPE, EQUIPMENT_NAME, MANUFACTURER, MODEL, INSTALLATION_DATE |
| CreateWellTestAsync | WellTestRequest | WORK_ORDER; WORK_ORDER_XREF | WORK_ORDER_ID, WORK_ORDER_TYPE, ENTITY_TYPE, ENTITY_ID, STATUS |

### Services/PipelineManagement/PipelineManagementService.cs

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| CreatePipelineAsync | PipelineCreationRequest, PipelineResponse | PIPELINE | PIPELINE_ID, PIPELINE_NAME, FIELD_ID, PIPELINE_TYPE, DIAMETER, LENGTH, ACTIVE_IND |

### Services/WorkOrder/WorkOrderManagementService.cs

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| CreateWorkOrderAsync | WorkOrderCreationRequest, WorkOrderResponse | WORK_ORDER; WORK_ORDER_XREF | WORK_ORDER_ID, WORK_ORDER_TYPE, STATUS; WORK_ORDER_XREF.ENTITY_TYPE, ENTITY_ID |

### Services/FieldLifecycle/ReservoirLifecycle/WellLifecycle

| Method | DTOs | PPDM Table | Required Columns (minimum) |
| --- | --- | --- | --- |
| TransitionFieldPhaseAsync | FieldPhaseStatus, ValidationResult | FIELD_PHASE | FIELD_ID, PHASE, EFFECTIVE_DATE |
| TransitionReservoirStateAsync | ValidationResult | RESERVOIR_STATUS | RESERVOIR_ID, STATUS, EFFECTIVE_DATE |
| TransitionWellStateAsync | ValidationResult | WELL_STATUS | UWI, STATUS, EFFECTIVE_DATE |

## Cross-Cutting / Platform

| Project | Phase | LifeCycle Service | PPDM Tables (Exact) | Custom/Extension Tables | Status |
| --- | --- | --- | --- | --- | --- |
| Beep.OilandGas.UserManagement | Cross-phase | AccessControlService | N/A | USER; ROLE; PERMISSION; USER_ROLE; ROLE_PERMISSION | Planned |
| Beep.OilandGas.Drawing | Cross-phase | UI/GFX Integration | N/A | N/A | Planned |
| Beep.OilandGas.HeatMap | Cross-phase | UI/GFX Integration | N/A | N/A | Planned |

## Notes

- All DTOs and data models used in integrations are PPDM-bound and must inherit from `Beep.OilandGas.Models/Data/ModelEntityBase.cs`.
- When PPDM tables do not exist, define new tables following PPDM naming conventions and add scripts across supported DBs.
