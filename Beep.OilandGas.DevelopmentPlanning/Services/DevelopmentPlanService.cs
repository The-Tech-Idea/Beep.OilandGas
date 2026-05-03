using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.DevelopmentPlanning.Constants;

namespace Beep.OilandGas.DevelopmentPlanning.Services
{
    /// <summary>
    /// Service for managing development plans.
    /// Uses PPDMGenericRepository with AppFilter pattern for all data access.
    /// </summary>
    public partial class DevelopmentPlanService : IDevelopmentPlanService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;

        public DevelopmentPlanService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<DevelopmentPlanService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
            _logger = logger;
        }

        private async Task<PPDMGenericRepository> GetRepositoryAsync<T>(string tableName)
        {
            var meta = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}") ?? typeof(T);
            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, tableName);
        }

        public async Task<List<DevelopmentPlan>> GetDevelopmentPlansAsync(string? fieldId = null)
        {
            var repo = await GetRepositoryAsync<FIELD_DEVELOPMENT_PLAN>("FIELD_DEVELOPMENT_PLAN");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });

            var entities = await repo.GetAsync(filters);
            var plans = new List<DevelopmentPlan>();
            foreach (var row in entities.OfType<FIELD_DEVELOPMENT_PLAN>())
                plans.Add(await MapToDevelopmentPlanAsync(row));

            return plans;
        }

        public async Task<DevelopmentPlan?> GetDevelopmentPlanAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return null;

            var repo = await GetRepositoryAsync<FIELD_DEVELOPMENT_PLAN>("FIELD_DEVELOPMENT_PLAN");
            var entity = await repo.GetByIdAsync(planId);
            if (entity is not FIELD_DEVELOPMENT_PLAN planEntity || planEntity.ACTIVE_IND != _defaults.GetActiveIndicatorYes())
                return null;

            return await MapToDevelopmentPlanAsync(planEntity);
        }

        public async Task<DevelopmentPlan> CreateDevelopmentPlanAsync(CreateDevelopmentPlan createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var repo = await GetRepositoryAsync<FIELD_DEVELOPMENT_PLAN>("FIELD_DEVELOPMENT_PLAN");
            var fieldPlan = new FIELD_DEVELOPMENT_PLAN
            {
                FDP_ID = Guid.NewGuid().ToString(),
                FIELD_ID = createDto.FieldId ?? string.Empty,
                FDP_NAME = string.IsNullOrWhiteSpace(createDto.PlanName) ? "Development Plan" : createDto.PlanName,
                FDP_VERSION = 1,
                FDP_STATUS = DevelopmentPlanningDefaults.Draft,
                SUBMISSION_DATE = DateTime.UtcNow,
                FIRST_PRODUCTION_DATE = createDto.TargetStartDate,
                FIELD_LIFE_END = createDto.TargetCompletionDate,
                TOTAL_CAPEX_MM = createDto.EstimatedCost,
                CAPEX_CURRENCY = string.IsNullOrWhiteSpace(createDto.Currency) ? "USD" : createDto.Currency,
                DEVELOPMENT_CONCEPT_DESC = createDto.Description ?? string.Empty,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = "system",
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            await repo.InsertAsync(fieldPlan, "system");

            return await MapToDevelopmentPlanAsync(fieldPlan);
        }

        public async Task<DevelopmentPlan> UpdateDevelopmentPlanAsync(string planId, UpdateDevelopmentPlan updateDto)
        {
            if (string.IsNullOrWhiteSpace(planId))
                throw new ArgumentException("Plan ID cannot be null or empty.", nameof(planId));
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var repo = await GetRepositoryAsync<FIELD_DEVELOPMENT_PLAN>("FIELD_DEVELOPMENT_PLAN");
            var entity = await repo.GetByIdAsync(planId);
            if (entity is not FIELD_DEVELOPMENT_PLAN fieldPlan)
                throw new KeyNotFoundException($"Development plan with ID {planId} not found.");

            if (!string.IsNullOrWhiteSpace(updateDto.PlanName))
                fieldPlan.FDP_NAME = updateDto.PlanName;

            if (!string.IsNullOrWhiteSpace(updateDto.Description))
                fieldPlan.DEVELOPMENT_CONCEPT_DESC = updateDto.Description;

            if (!string.IsNullOrWhiteSpace(updateDto.Status))
                fieldPlan.FDP_STATUS = updateDto.Status;

            if (updateDto.TargetStartDate.HasValue)
                fieldPlan.FIRST_PRODUCTION_DATE = updateDto.TargetStartDate.Value;

            if (updateDto.TargetCompletionDate.HasValue)
                fieldPlan.FIELD_LIFE_END = updateDto.TargetCompletionDate.Value;

            if (updateDto.EstimatedCost.HasValue)
                fieldPlan.TOTAL_CAPEX_MM = updateDto.EstimatedCost.Value;

            fieldPlan.ROW_CHANGED_BY = "system";
            fieldPlan.ROW_CHANGED_DATE = DateTime.UtcNow;

            await repo.UpdateAsync(fieldPlan, "system");

            return await MapToDevelopmentPlanAsync(fieldPlan);
        }

        public async Task<DevelopmentPlan> ApproveDevelopmentPlanAsync(string planId, string approvedBy)
        {
            if (string.IsNullOrWhiteSpace(planId))
                throw new ArgumentException("Plan ID cannot be null or empty.", nameof(planId));

            var updated = await UpdateDevelopmentPlanAsync(planId, new UpdateDevelopmentPlan { Status = DevelopmentPlanningDefaults.Approved });
            var repo = await GetRepositoryAsync<FIELD_DEVELOPMENT_PLAN>("FIELD_DEVELOPMENT_PLAN");
            var entity = await repo.GetByIdAsync(planId);
            if (entity is FIELD_DEVELOPMENT_PLAN fieldPlan)
            {
                fieldPlan.APPROVED_BY = approvedBy ?? "system";
                fieldPlan.APPROVAL_DATE = DateTime.UtcNow;
                fieldPlan.ROW_CHANGED_BY = approvedBy ?? "system";
                fieldPlan.ROW_CHANGED_DATE = DateTime.UtcNow;
                await repo.UpdateAsync(fieldPlan, approvedBy ?? "system");
            }
            return updated;
        }

        public async Task<List<WellPlan>> GetWellPlansAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<WellPlan>();

            var repo = await GetRepositoryAsync<DEVELOPMENT_WELL_SCHEDULE>("DEVELOPMENT_WELL_SCHEDULE");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FDP_ID", Operator = "=", FilterValue = planId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };
            var entities = await repo.GetAsync(filters);

            return entities.OfType<DEVELOPMENT_WELL_SCHEDULE>().Select(w => new WellPlan
            {
                WellPlanId = w.SCHEDULE_ID ?? string.Empty,
                PlanId = planId,
                WellUWI = w.PLANNED_WELL_ID,
                WellName = w.WELL_NAME ?? string.Empty,
                WellType = w.WELL_TYPE,
                PlannedSpudDate = w.PLANNED_SPUD_DATE,
                PlannedCompletionDate = w.PLANNED_COMPLETION_DATE,
                EstimatedCost = w.AFE_COST_MM,
                Status = w.SCHEDULE_STATUS
            }).ToList();
        }

        public async Task<List<FacilityPlan>> GetFacilityPlansAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<FacilityPlan>();

            var repo = await GetRepositoryAsync<FACILITY_INVESTMENT>("FACILITY_INVESTMENT");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FDP_ID", Operator = "=", FilterValue = planId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };
            var entities = await repo.GetAsync(filters);

            return entities.OfType<FACILITY_INVESTMENT>().Select(f => new FacilityPlan
            {
                FacilityPlanId = f.FACILITY_INV_ID ?? string.Empty,
                PlanId = planId,
                FacilityName = f.FACILITY_NAME ?? string.Empty,
                FacilityType = f.FACILITY_TYPE ?? string.Empty,
                PlannedStartDate = f.PLANNED_START_DATE,
                PlannedCompletionDate = f.PLANNED_COMPLETION_DATE,
                EstimatedCost = f.CAPEX_PLANNED_MM,
                Capacity = f.CAPACITY_PLANNED_BOPD,
                CapacityUnit = f.CAPACITY_OUOM,
                Status = f.INVESTMENT_PHASE
            }).ToList();
        }

        public async Task<List<PERMIT_APPLICATION>> GetPermitApplicationsAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<PERMIT_APPLICATION>();

            var link = await GetPlanLinkageContextAsync(planId);
            if (link == null)
                return new List<PERMIT_APPLICATION>();

            var permitRepo = await GetRepositoryAsync<PERMIT_APPLICATION>("PERMIT_APPLICATION");
            var permitRows = await permitRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            });

            var permits = permitRows.OfType<PERMIT_APPLICATION>()
                .Where(p =>
                    string.Equals(p.FIELD_ID, link.FieldId, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(p.RELATED_WELL_UWI) && link.WellIds.Contains(p.RELATED_WELL_UWI)) ||
                    (!string.IsNullOrWhiteSpace(p.WELL_ID) && link.WellIds.Contains(p.WELL_ID)) ||
                    (!string.IsNullOrWhiteSpace(p.RELATED_FACILITY_ID) && link.FacilityIds.Contains(p.RELATED_FACILITY_ID)) ||
                    (!string.IsNullOrWhiteSpace(p.FACILITY_ID) && link.FacilityIds.Contains(p.FACILITY_ID)) ||
                    string.Equals(p.APPLICATION_ID, planId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return permits;
        }

        public async Task<List<WELL_ACTIVITY>> GetWellActivitiesAsync(string planId, string? wellUwi = null)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<WELL_ACTIVITY>();

            var repo = await GetRepositoryAsync<WELL_ACTIVITY>("WELL_ACTIVITY");
            var plan = await GetDevelopmentPlanAsync(planId);
            if (plan == null)
                return new List<WELL_ACTIVITY>();

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };
            if (!string.IsNullOrWhiteSpace(wellUwi))
                filters.Add(new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellUwi });

            var rows = await repo.GetAsync(filters);
            return rows?.OfType<WELL_ACTIVITY>().ToList() ?? new List<WELL_ACTIVITY>();
        }

        public async Task<WELL_MAINTENANCE_PLAN> CreateWellMaintenancePlanAsync(CreateWellMaintenancePlan createDto, string userId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));
            if (string.IsNullOrWhiteSpace(createDto.PlanId) || string.IsNullOrWhiteSpace(createDto.WellUwi))
                throw new ArgumentException("PlanId and WellUwi are required.");

            var repo = await GetRepositoryAsync<WELL_MAINTENANCE_PLAN>("WELL_MAINTENANCE_PLAN");
            var entity = new WELL_MAINTENANCE_PLAN
            {
                MAINT_PLAN_ID = Guid.NewGuid().ToString(),
                FDP_ID = createDto.PlanId,
                UWI = createDto.WellUwi,
                MAINTENANCE_TYPE = createDto.MaintenanceType,
                MAINTENANCE_STATUS = DevelopmentPlanningDefaults.Planned,
                PRIORITY = string.IsNullOrWhiteSpace(createDto.Priority) ? DevelopmentPlanningDefaults.Medium : createDto.Priority,
                TRIGGER_BASIS = string.IsNullOrWhiteSpace(createDto.TriggerBasis) ? DevelopmentPlanningDefaults.TimeBased : createDto.TriggerBasis,
                PLANNED_START_DATE = createDto.PlannedStartDate,
                PLANNED_END_DATE = createDto.PlannedEndDate,
                SERVICE_BA_ID = createDto.ServiceBusinessAssociateId ?? string.Empty,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            await repo.InsertAsync(entity, userId);
            return entity;
        }

        public async Task<WELL_SERVICE_JOB> CreateWellServiceJobAsync(CreateWellServiceJob createDto, string userId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));
            if (string.IsNullOrWhiteSpace(createDto.PlanId) || string.IsNullOrWhiteSpace(createDto.WellUwi))
                throw new ArgumentException("PlanId and WellUwi are required.");

            var repo = await GetRepositoryAsync<WELL_SERVICE_JOB>("WELL_SERVICE_JOB");
            var entity = new WELL_SERVICE_JOB
            {
                JOB_ID = Guid.NewGuid().ToString(),
                FDP_ID = createDto.PlanId,
                UWI = createDto.WellUwi,
                JOB_TYPE = createDto.JobType,
                JOB_STATUS = DevelopmentPlanningDefaults.Planned,
                PRIORITY = string.IsNullOrWhiteSpace(createDto.Priority) ? DevelopmentPlanningDefaults.Medium : createDto.Priority,
                SERVICE_BA_ID = createDto.ServiceBusinessAssociateId ?? string.Empty,
                BA_SERVICE_TYPE = createDto.BusinessAssociateServiceType ?? string.Empty,
                PLANNED_START_DATE = createDto.PlannedStartDate,
                PLANNED_END_DATE = createDto.PlannedEndDate,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            await repo.InsertAsync(entity, userId);
            return entity;
        }

        private async Task<DevelopmentPlan> MapToDevelopmentPlanAsync(FIELD_DEVELOPMENT_PLAN row)
        {
            var plan = new DevelopmentPlan
            {
                PlanId = row.FDP_ID ?? string.Empty,
                FieldId = row.FIELD_ID,
                PlanName = row.FDP_NAME ?? "Development Plan",
                Description = row.DEVELOPMENT_CONCEPT_DESC,
                PlanDate = row.SUBMISSION_DATE,
                TargetStartDate = row.FIRST_PRODUCTION_DATE,
                TargetCompletionDate = row.FIELD_LIFE_END,
                Status = row.FDP_STATUS,
                EstimatedCost = row.TOTAL_CAPEX_MM,
                Currency = row.CAPEX_CURRENCY,
                ApprovedBy = row.APPROVED_BY,
                ApprovalDate = row.APPROVAL_DATE,
                CreatedDate = row.ROW_CREATED_DATE ?? DateTime.UtcNow
            };

            plan.WellPlans = await GetWellPlansAsync(plan.PlanId);
            plan.FacilityPlans = await GetFacilityPlansAsync(plan.PlanId);
            plan.PermitApplications = await GetPermitApplicationsAsync(plan.PlanId);

            return plan;
        }

        private async Task<PlanLinkageContext?> GetPlanLinkageContextAsync(string planId)
        {
            var planRepo = await GetRepositoryAsync<FIELD_DEVELOPMENT_PLAN>("FIELD_DEVELOPMENT_PLAN");
            var planEntity = await planRepo.GetByIdAsync(planId) as FIELD_DEVELOPMENT_PLAN;
            if (planEntity == null)
                return null;

            var wellIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var wellRepo = await GetRepositoryAsync<DEVELOPMENT_WELL_SCHEDULE>("DEVELOPMENT_WELL_SCHEDULE");
            var wellRows = await wellRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "FDP_ID", Operator = "=", FilterValue = planId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            });
            foreach (var row in wellRows.OfType<DEVELOPMENT_WELL_SCHEDULE>())
            {
                if (!string.IsNullOrWhiteSpace(row.PLANNED_WELL_ID))
                    wellIds.Add(row.PLANNED_WELL_ID);
            }

            var facilityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var facilityRepo = await GetRepositoryAsync<FACILITY_INVESTMENT>("FACILITY_INVESTMENT");
            var facilityRows = await facilityRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "FDP_ID", Operator = "=", FilterValue = planId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            });
            foreach (var row in facilityRows.OfType<FACILITY_INVESTMENT>())
            {
                if (!string.IsNullOrWhiteSpace(row.FACILITY_ID))
                    facilityIds.Add(row.FACILITY_ID);
            }

            return new PlanLinkageContext(planEntity.FIELD_ID ?? string.Empty, wellIds, facilityIds);
        }

        private sealed record PlanLinkageContext(string FieldId, HashSet<string> WellIds, HashSet<string> FacilityIds);


    }
}
