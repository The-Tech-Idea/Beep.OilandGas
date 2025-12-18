using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.DevelopmentPlanning.Services
{
    /// <summary>
    /// Service for managing development plans.
    /// Uses UnitOfWork directly for data access.
    /// </summary>
    public class DevelopmentPlanService : IDevelopmentPlanService
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;

        public DevelopmentPlanService(IDMEEditor editor, string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _connectionName = connectionName;
        }

        private List<T> ConvertToList<T>(object units) where T : class
        {
            var result = new List<T>();
            if (units == null) return result;
            
            if (units is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is T entity)
                    {
                        result.Add(entity);
                    }
                }
            }
            return result;
        }

        private IUnitOfWorkWrapper GetApplicationUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(APPLICATION), _editor, _connectionName, "APPLICATION", "APPLICATION_ID");
        }

        private IUnitOfWorkWrapper GetFieldUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(FIELD), _editor, _connectionName, "FIELD", "FIELD_ID");
        }

        private IUnitOfWorkWrapper GetWellUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(WELL), _editor, _connectionName, "WELL", "UWI");
        }

        private IUnitOfWorkWrapper GetFacilityUnitOfWork()
        {
            return UnitOfWorkFactory.CreateUnitOfWork(typeof(FACILITY), _editor, _connectionName, "FACILITY", "FACILITY_ID");
        }

        public async Task<List<DevelopmentPlanDto>> GetDevelopmentPlansAsync(string? fieldId = null)
        {
            var applicationUow = GetApplicationUnitOfWork();
            List<APPLICATION> applications;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "APPLICATION_TYPE", FilterValue = "DEVELOPMENT_PLAN", Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                // Note: APPLICATION might not have direct FIELD_ID, may need to join through AREA_ID
                filters.Add(new AppFilter { FieldName = "AREA_ID", FilterValue = fieldId, Operator = "=" });
            }

            var units = await applicationUow.Get(filters);
            applications = ConvertToList<APPLICATION>(units);

            var plans = new List<DevelopmentPlanDto>();
            foreach (var app in applications)
            {
                var plan = await MapToDevelopmentPlanDtoAsync(app);
                plans.Add(plan);
            }

            return plans;
        }

        public async Task<DevelopmentPlanDto?> GetDevelopmentPlanAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return null;

            var applicationUow = GetApplicationUnitOfWork();
            var application = applicationUow.Read(planId) as APPLICATION;
            if (application == null || application.ACTIVE_IND != "Y")
                return null;

            return await MapToDevelopmentPlanDtoAsync(application);
        }

        public async Task<DevelopmentPlanDto> CreateDevelopmentPlanAsync(CreateDevelopmentPlanDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var applicationUow = GetApplicationUnitOfWork();
            var application = new APPLICATION
            {
                APPLICATION_ID = Guid.NewGuid().ToString(),
                APPLICATION_TYPE = "DEVELOPMENT_PLAN",
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = createDto.TargetStartDate ?? DateTime.UtcNow,
                EXPIRY_DATE = (DateTime)createDto.TargetCompletionDate,
                CURRENT_STATUS = "Draft",
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_DATE = DateTime.UtcNow
            };

            var result = await applicationUow.InsertDoc(application);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to create development plan: {result.Message}");

            await applicationUow.Commit();

            return await MapToDevelopmentPlanDtoAsync(application);
        }

        public async Task<DevelopmentPlanDto> UpdateDevelopmentPlanAsync(string planId, UpdateDevelopmentPlanDto updateDto)
        {
            if (string.IsNullOrWhiteSpace(planId))
                throw new ArgumentException("Plan ID cannot be null or empty.", nameof(planId));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var applicationUow = GetApplicationUnitOfWork();
            var application = applicationUow.Read(planId) as APPLICATION;
            if (application == null)
                throw new KeyNotFoundException($"Development plan with ID {planId} not found.");

            if (!string.IsNullOrWhiteSpace(updateDto.Status))
                application.CURRENT_STATUS = updateDto.Status;

            if (updateDto.TargetStartDate.HasValue)
                application.EFFECTIVE_DATE = updateDto.TargetStartDate.Value;

            if (updateDto.TargetCompletionDate.HasValue)
                application.EXPIRY_DATE = updateDto.TargetCompletionDate.Value;

            application.ROW_CHANGED_DATE = DateTime.UtcNow;

            var result = await applicationUow.UpdateDoc(application);
            if (result.Flag != Errors.Ok)
                throw new InvalidOperationException($"Failed to update development plan: {result.Message}");

            await applicationUow.Commit();

            return await MapToDevelopmentPlanDtoAsync(application);
        }

        public async Task<DevelopmentPlanDto> ApproveDevelopmentPlanAsync(string planId, string approvedBy)
        {
            if (string.IsNullOrWhiteSpace(planId))
                throw new ArgumentException("Plan ID cannot be null or empty.", nameof(planId));

            var plan = await GetDevelopmentPlanAsync(planId);
            if (plan == null)
                throw new KeyNotFoundException($"Development plan with ID {planId} not found.");

            plan.Status = "Approved";
            plan.ApprovedBy = approvedBy;
            plan.ApprovalDate = DateTime.UtcNow;

            return await UpdateDevelopmentPlanAsync(planId, new UpdateDevelopmentPlanDto
            {
                Status = "Approved"
            });
        }

        public async Task<List<WellPlanDto>> GetWellPlansAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<WellPlanDto>();

            var wellUow = GetWellUnitOfWork();
            // Note: WELL might be linked to plan through AREA_ID or APPLICATION_ID
            // For now, querying all active wells - in production, would need proper relationship
            var units = await wellUow.Get();
            List<WELL> allWells = ConvertToList<WELL>(units);
            var wells = allWells.Where(w => w.ACTIVE_IND == "Y").ToList();

            return wells.Select(w => new WellPlanDto
            {
                WellPlanId = w.UWI ?? string.Empty,
                PlanId = planId,
                WellUWI = w.UWI,
                WellName = w.WELL_NAME ?? string.Empty,
                WellType = w.PROFILE_TYPE,
                TargetDepth = w.BASE_DEPTH,
                Status = w.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<FacilityPlanDto>> GetFacilityPlansAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<FacilityPlanDto>();

            var facilityUow = GetFacilityUnitOfWork();
            // Note: FACILITY might be linked to plan through AREA_ID or APPLICATION_ID
            var units = await facilityUow.Get();
            List<FACILITY> allFacilities = ConvertToList<FACILITY>(units);
            var facilities = allFacilities.Where(f => f.ACTIVE_IND == "Y").ToList();

            return facilities.Select(f => new FacilityPlanDto
            {   
                FacilityPlanId = f.FACILITY_ID ?? string.Empty,
                PlanId = planId,
                FacilityName = f.FACILITY_SHORT_NAME ?? string.Empty,
                FacilityType = f.FACILITY_TYPE ?? string.Empty,
                Status = f.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<PermitApplicationDto>> GetPermitApplicationsAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<PermitApplicationDto>();

            var applicationUow = GetApplicationUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "APPLICATION_TYPE", FilterValue = "PERMIT", Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
            };
            var units = await applicationUow.Get(filters);
            List<APPLICATION> allApplications = ConvertToList<APPLICATION>(units);

            return allApplications.Select(app => new PermitApplicationDto
            {
                ApplicationId = app.APPLICATION_ID ?? string.Empty,
                ApplicationType = app.APPLICATION_TYPE ?? string.Empty,
                Status = app.CURRENT_STATUS ?? string.Empty,
                SubmittedDate = app.EFFECTIVE_DATE,
                DecisionDate = app.DECISION_DATE,
                Decision = app.DECISION,
                ExpiryDate = app.EXPIRY_DATE
            }).ToList();
        }

        private async Task<DevelopmentPlanDto> MapToDevelopmentPlanDtoAsync(APPLICATION application)
        {
            var plan = new DevelopmentPlanDto
            {
                PlanId = application.APPLICATION_ID ?? string.Empty,
                PlanName = application.APPLICATION_TYPE ?? "Development Plan",
                PlanDate = application.EFFECTIVE_DATE,
                TargetStartDate = application.EFFECTIVE_DATE,
                TargetCompletionDate = application.EXPIRY_DATE,
                Status = application.CURRENT_STATUS ?? "Draft"
            };

            // Load related entities
            plan.WellPlans = await GetWellPlansAsync(plan.PlanId);
            plan.FacilityPlans = await GetFacilityPlansAsync(plan.PlanId);
            plan.PermitApplications = await GetPermitApplicationsAsync(plan.PlanId);

            return plan;
        }
    }
}

