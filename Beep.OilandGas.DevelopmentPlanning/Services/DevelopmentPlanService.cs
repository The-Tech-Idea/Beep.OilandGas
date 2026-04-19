using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.DataMapping;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep;
using Microsoft.Extensions.Logging;

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

        private async Task<PPDMGenericRepository> GetApplicationRepositoryAsync()
        {
            var meta = await _metadata.GetTableMetadataAsync("APPLICATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "APPLICATION");
        }

        private async Task<PPDMGenericRepository> GetWellRepositoryAsync()
        {
            var meta = await _metadata.GetTableMetadataAsync("WELL");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "WELL");
        }

        private async Task<PPDMGenericRepository> GetFacilityRepositoryAsync()
        {
            var meta = await _metadata.GetTableMetadataAsync("FACILITY");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "FACILITY");
        }

        public async Task<List<DevelopmentPlan>> GetDevelopmentPlansAsync(string? fieldId = null)
        {
            var repo = await GetApplicationRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "APPLICATION_TYPE", Operator = "=", FilterValue = "DEVELOPMENT_PLAN" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrWhiteSpace(fieldId))
                filters.Add(new AppFilter { FieldName = "AREA_ID", Operator = "=", FilterValue = fieldId });

            var entities = await repo.GetAsync(filters);
            var plans = new List<DevelopmentPlan>();
            foreach (var app in entities.OfType<APPLICATION>())
                plans.Add(await MapToDevelopmentPlanAsync(app));

            return plans;
        }

        public async Task<DevelopmentPlan?> GetDevelopmentPlanAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return null;

            var repo = await GetApplicationRepositoryAsync();
            var entity = await repo.GetByIdAsync(planId);
            if (entity is not APPLICATION application || application.ACTIVE_IND != "Y")
                return null;

            return await MapToDevelopmentPlanAsync(application);
        }

        public async Task<DevelopmentPlan> CreateDevelopmentPlanAsync(CreateDevelopmentPlan createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var repo = await GetApplicationRepositoryAsync();
            var application = new APPLICATION
            {
                APPLICATION_ID = Guid.NewGuid().ToString(),
                APPLICATION_TYPE = "DEVELOPMENT_PLAN",
                ACTIVE_IND = "Y",
                EFFECTIVE_DATE = createDto.TargetStartDate ?? DateTime.UtcNow,
                EXPIRY_DATE = createDto.TargetCompletionDate ?? DateTime.UtcNow.AddYears(5),
                CURRENT_STATUS = "Draft"
            };

            await repo.InsertAsync(application, "system");

            return await MapToDevelopmentPlanAsync(application);
        }

        public async Task<DevelopmentPlan> UpdateDevelopmentPlanAsync(string planId, UpdateDevelopmentPlan updateDto)
        {
            if (string.IsNullOrWhiteSpace(planId))
                throw new ArgumentException("Plan ID cannot be null or empty.", nameof(planId));
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var repo = await GetApplicationRepositoryAsync();
            var entity = await repo.GetByIdAsync(planId);
            if (entity is not APPLICATION application)
                throw new KeyNotFoundException($"Development plan with ID {planId} not found.");

            if (!string.IsNullOrWhiteSpace(updateDto.Status))
                application.CURRENT_STATUS = updateDto.Status;

            if (updateDto.TargetStartDate.HasValue)
                application.EFFECTIVE_DATE = updateDto.TargetStartDate.Value;

            if (updateDto.TargetCompletionDate.HasValue)
                application.EXPIRY_DATE = updateDto.TargetCompletionDate.Value;

            await repo.UpdateAsync(application, "system");

            return await MapToDevelopmentPlanAsync(application);
        }

        public async Task<DevelopmentPlan> ApproveDevelopmentPlanAsync(string planId, string approvedBy)
        {
            if (string.IsNullOrWhiteSpace(planId))
                throw new ArgumentException("Plan ID cannot be null or empty.", nameof(planId));

            return await UpdateDevelopmentPlanAsync(planId, new UpdateDevelopmentPlan { Status = "Approved" });
        }

        public async Task<List<WellPlan>> GetWellPlansAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<WellPlan>();

            var repo = await GetWellRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await repo.GetAsync(filters);

            return entities.OfType<WELL>().Select(w => new WellPlan
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

        public async Task<List<FacilityPlan>> GetFacilityPlansAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<FacilityPlan>();

            var repo = await GetFacilityRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await repo.GetAsync(filters);

            return entities.OfType<FACILITY>().Select(f => new FacilityPlan
            {
                FacilityPlanId = f.FACILITY_ID ?? string.Empty,
                PlanId = planId,
                FacilityName = f.FACILITY_SHORT_NAME ?? string.Empty,
                FacilityType = f.FACILITY_TYPE ?? string.Empty,
                Status = f.ACTIVE_IND == "Y" ? "Active" : "Inactive"
            }).ToList();
        }

        public async Task<List<PERMIT_APPLICATION>> GetPermitApplicationsAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
                return new List<PERMIT_APPLICATION>();

            var repo = await GetApplicationRepositoryAsync();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "APPLICATION_TYPE", Operator = "=", FilterValue = "PERMIT" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var entities = await repo.GetAsync(filters);
            var mapper = new ApplicationMapper();
            return entities.OfType<APPLICATION>().Select(app => mapper.MapToDomain(app)).ToList();
        }

        private async Task<DevelopmentPlan> MapToDevelopmentPlanAsync(APPLICATION application)
        {
            var plan = new DevelopmentPlan
            {
                PlanId = application.APPLICATION_ID ?? string.Empty,
                PlanName = application.APPLICATION_TYPE ?? "Development Plan",
                PlanDate = application.EFFECTIVE_DATE,
                TargetStartDate = application.EFFECTIVE_DATE,
                TargetCompletionDate = application.EXPIRY_DATE,
                Status = application.CURRENT_STATUS ?? "Draft"
            };

            plan.WellPlans = await GetWellPlansAsync(plan.PlanId);
            plan.FacilityPlans = await GetFacilityPlansAsync(plan.PlanId);
            plan.PermitApplications = await GetPermitApplicationsAsync(plan.PlanId);

            return plan;
        }


    }
}
