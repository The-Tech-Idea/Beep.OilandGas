# Lifecycle Stage-Gate Criteria

Purpose: define exit criteria per lifecycle phase so Field/Reservoir/Well/Facility transitions are consistent and auditable.

## Global Rules (Apply to All Phases)

- Each phase must store evidence in PPDM tables with audit fields.
- All DTOs and data classes must be PPDM-bound and inherit from `Beep.OilandGas.Models/Data/ModelEntityBase.cs`.
- Permits and regulatory approvals are mandatory gate checks.
- Financial approvals (AFE, budget) required for phase progression.

## Exploration → Development

Required Deliverables:
- Prospect definition and risked volumes (PROSPECT + related tables).
- Seismic interpretation and prospect evaluation.
- Lease status confirmed and valid.
- Initial economic screening (NPV/EMV thresholds).

Gate Checks:
- Exploration permits approved and active.
- Decision record approved by exploration lead and finance.

## Development → Production

Required Deliverables:
- Approved development plan and drilling program.
- Wells drilled/completed per plan.
- Facilities and pipelines commissioned.
- Updated reserves and production forecast.

Gate Checks:
- Construction completion certificates recorded.
- Operational readiness review complete.
- Regulatory production approvals obtained.

## Production → Decommissioning

Required Deliverables:
- Decline and economic limit analysis documented.
- Abandonment plan and cost estimate approved.
- Asset integrity assessments and closure scope.

Gate Checks:
- Decommissioning permits approved.
- Financial closure authorization (abandonment funding).

## Decommissioning → Closure

Required Deliverables:
- Well plug and abandonment executed with verification.
- Facilities removed and site remediated.
- Environmental restoration evidence submitted.

Gate Checks:
- Final regulatory sign-off.
- Asset records archived and status set to CLOSED.

## Implementation Checklists (Service + Process)

### Exploration → Development (Checklist)

Service Touchpoints:
- `Services/Exploration/PPDMExplorationService.cs` (prospects, seismic, leases).
- `Services/FieldLifecycle/FieldLifecycleService.cs` (field phase transitions).
- `Services/ReservoirLifecycle/ReservoirLifecycleService.cs` (reservoir status).
- `Services/Permits/PermitManagementService.cs` (permits).
- `Services/Processes/ProcessIntegrationHelper.cs` (gate enforcement).

Process Definition Checklist:
- Define/verify exploration gate processes in `Services/Processes/ProcessDefinitionInitializer.cs`.
- Add stage-gate approvals and evidence requirements in `Services/Processes/ProcessValidator.cs`.

PPDM Evidence:
- SEIS_ACQTN_SURVEY; SEIS_LINE; WELL; LEASE; PROSPECT and PROSPECT_* extensions.

Checklist:
- [ ] Prospect created with risked volumes and ranking. | DTOs: ProspectRequest, ProspectEvaluation, PROSPECT_RISK_ASSESSMENT, PROSPECT_VOLUME_ESTIMATE, PROSPECT_RANKING | PPDM: PROSPECT.PROSPECT_ID, PROSPECT_NAME, PRIMARY_FIELD_ID, PROSPECT_STATUS; PROSPECT_RISK_ASSESSMENT.ASSESSMENT_ID, RISK_MODEL_TYPE; PROSPECT_VOLUME_ESTIMATE.ESTIMATE_ID, OIL_VOLUME_P50, GAS_VOLUME_P50; PROSPECT_RANKING.RANKING_ID, RANKING_SCORE
- [ ] Seismic interpretation stored and linked to prospect. | DTOs: SeismicSurveyRequest | PPDM: SEIS_ACQTN_SURVEY.SEIS_ACQTN_SURVEY_ID, ACQTN_SURVEY_NAME, AREA_ID, COMPLETED_DATE; SEIS_LINE.SEIS_LINE_ID, SEIS_SET_SUBTYPE, EFFECTIVE_DATE
- [ ] Lease status active and valid. | DTOs: CreateLease, UpdateLease | PPDM: LEASE.LEASE_ID, LEASE_NAME, ACTIVE_IND, EXPIRY_DATE
- [ ] Initial economic screening stored (NPV/EMV). | DTOs: EconomicAnalysisRequest, EconomicAnalysisResult | PPDM: ECONOMIC_ANALYSIS.ECONOMIC_ANALYSIS_ID, STATUS, CALCULATION_DATE
- [ ] Exploration permits approved and attached. | DTOs: PERMIT_APPLICATION, APPLICATION_ATTACHMENT, REQUIRED_FORM | PPDM: APPLICATION.APPLICATION_ID, APPLICATION_TYPE, APPLICATION_STATUS; PERMIT_STATUS_HISTORY.STATUS_DATE
- [ ] Field phase transitioned via FieldLifecycleService. | DTOs: FieldPhaseStatus, ValidationResult | PPDM: FIELD_PHASE.FIELD_ID, PHASE, EFFECTIVE_DATE

### Development → Production (Checklist)

Service Touchpoints:
- `Services/Development/PPDMDevelopmentService.cs` (well, facility, pipeline, drilling).
- `Services/FacilityManagement/FacilityManagementService.cs` (commissioning).
- `Services/WellLifecycle/WellLifecycleService.cs` (completion state).
- `Services/Processes/ProcessIntegrationHelper.cs` (gate enforcement).

Process Definition Checklist:
- Define/verify development gate processes in `Services/Processes/ProcessDefinitionInitializer.cs`.
- Enforce construction completion and readiness checks in `Services/Processes/ProcessValidator.cs`.

PPDM Evidence:
- WELL; WELL_XREF; DRILLING_OPERATION; FACILITY; PIPELINE; WELL_EQUIPMENT; FACILITY_EQUIPMENT; POOL.

Checklist:
- [ ] Development plan approved and stored. | DTOs: CreateDevelopmentPlan, DevelopmentPlan | PPDM: DevelopmentPlan.PlanId, FieldId, PlanDate
- [ ] Drilling program executed; wells completed and status updated. | DTOs: DevelopmentWellRequest, CreateDrillingOperation | PPDM: WELL.UWI, WELL_STATUS.UWI, WELL_STATUS.STATUS; DRILLING_OPERATION.DRILLING_OPERATION_ID
- [ ] Facilities and pipelines commissioned with equipment history. | DTOs: FacilityRequest, PipelineRequest, FacilityEquipmentRequest | PPDM: FACILITY.FACILITY_ID, FACILITY_TYPE, PRIMARY_FIELD_ID; PIPELINE.PIPELINE_ID, FIELD_ID; FACILITY_EQUIPMENT.FACILITY_ID, EQUIPMENT_ID, INSTALL_OBS_NO
- [ ] Production forecast updated in PRODUCTION_FORECAST. | DTOs: ProductionForecastRequest | PPDM: PRODUCTION_FORECAST.PRODUCTION_FORECAST_ID, FIELD_ID, FORECAST_DATE
- [ ] Regulatory production approvals recorded. | DTOs: PERMIT_APPLICATION | PPDM: APPLICATION.APPLICATION_ID, APPLICATION_TYPE, APPLICATION_STATUS
- [ ] Field/Well lifecycle states advanced. | DTOs: FieldPhaseStatus, ValidationResult | PPDM: FIELD_PHASE.FIELD_ID, PHASE; WELL_STATUS.UWI, STATUS

### Production → Decommissioning (Checklist)

Service Touchpoints:
- `Services/Production/PPDMProductionService.cs` (production data, forecasts).
- `Services/Calculations/PPDMCalculationService.cs` (economic limit analyses).
- `Services/Decommissioning/PPDMDecommissioningService.cs` (abandonment plan).
- `Services/Processes/ProcessIntegrationHelper.cs` (gate enforcement).

Process Definition Checklist:
- Define/verify production exit gate processes in `Services/Processes/ProcessDefinitionInitializer.cs`.
- Validate economic limit and integrity evidence in `Services/Processes/ProcessValidator.cs`.

PPDM Evidence:
- PDEN_VOL_SUMMARY; PRODUCTION_FORECAST; WELL_TEST; DECOMMISSIONING_COST.

Checklist:
- [ ] Decline and economic limit analysis stored. | DTOs: DCARequest, EconomicAnalysisRequest | PPDM: DCA_CALCULATION.DCA_CALCULATION_ID, STATUS; ECONOMIC_ANALYSIS.ECONOMIC_ANALYSIS_ID, STATUS
- [ ] Abandonment plan and cost estimate approved. | DTOs: WellAbandonmentRequest, DecommissioningCostResponse | PPDM: WELL_ABANDONMENT.WELL_ABANDONMENT_ID, WELL_ID, STATUS; DECOMMISSIONING_COST.DECOMMISSIONING_COST_ID, FIELD_ID, COST_AMOUNT
- [ ] Integrity and inspection results recorded. | DTOs: WellInspectionRequest, FacilityInspectionRequest, PipelineInspectionRequest | PPDM: WORK_ORDER.WORK_ORDER_ID, WORK_ORDER_TYPE, STATUS; WORK_ORDER_XREF.ENTITY_ID
- [ ] Decommissioning permits approved. | DTOs: PERMIT_APPLICATION | PPDM: APPLICATION.APPLICATION_ID, APPLICATION_TYPE, APPLICATION_STATUS
- [ ] Lifecycle status moved to decommissioning. | DTOs: FieldPhaseStatus, ValidationResult | PPDM: FIELD_PHASE.FIELD_ID, PHASE; WELL_STATUS.UWI, STATUS

### Decommissioning → Closure (Checklist)

Service Touchpoints:
- `Services/Decommissioning/PPDMDecommissioningService.cs` (well/facility closure).
- `Services/WorkOrder/WorkOrderManagementService.cs` (closure work orders).
- `Services/Processes/ProcessIntegrationHelper.cs` (gate enforcement).

Process Definition Checklist:
- Define/verify closure gate processes in `Services/Processes/ProcessDefinitionInitializer.cs`.
- Enforce remediation evidence requirements in `Services/Processes/ProcessValidator.cs`.

PPDM Evidence:
- WELL_ABANDONMENT; FACILITY_DECOMMISSIONING; ENVIRONMENTAL_RESTORATION.

Checklist:
- [ ] Well P&A verified and recorded. | DTOs: WellAbandonmentRequest | PPDM: WELL_ABANDONMENT.WELL_ABANDONMENT_ID, WELL_ID, STATUS, VERIFIED_BY, VERIFIED_DATE
- [ ] Facilities removed and site remediated. | DTOs: FacilityDecommissioningRequest, EnvironmentalRestorationRequest | PPDM: FACILITY_DECOMMISSIONING.FACILITY_DECOMMISSIONING_ID, FACILITY_ID, STATUS; ENVIRONMENTAL_RESTORATION.ENVIRONMENTAL_RESTORATION_ID, FIELD_ID, STATUS
- [ ] Environmental restoration evidence attached. | DTOs: EnvironmentalRestorationRequest | PPDM: ENVIRONMENTAL_RESTORATION.ENVIRONMENTAL_RESTORATION_ID, RESTORATION_STATUS, COMPLETION_DATE
- [ ] Final regulatory sign-off recorded. | DTOs: PERMIT_STATUS_HISTORY | PPDM: PERMIT_STATUS_HISTORY.STATUS_DATE, STATUS_CODE, STATUS_REASON
- [ ] Asset status closed and archived. | DTOs: FieldPhaseStatus, ValidationResult | PPDM: FIELD_PHASE.FIELD_ID, PHASE; WELL_STATUS.UWI, STATUS; FACILITY.FACILITY_ID, ACTIVE_IND

## Entity-Specific Notes

- **Field**: requires consolidated reserves and financial approval at each gate.
- **Reservoir**: requires updated reservoir models and recovery method selection.
- **Well**: requires completion/workover records and integrity status updates.
- **Facility**: requires equipment installation history and inspection status.
