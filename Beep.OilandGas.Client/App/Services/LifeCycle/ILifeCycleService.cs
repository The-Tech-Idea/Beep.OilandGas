using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Decommissioning;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Client.App.Services.LifeCycle
{
    /// <summary>
    /// Service interface for LifeCycle operations
    /// Includes Exploration, Development, Decommissioning, WellManagement, FacilityManagement, WorkOrder
    /// </summary>
    public interface ILifeCycleService
    {
        #region Exploration

        Task<EXPLORATION_PROGRAM> CreateExplorationProjectAsync(EXPLORATION_PROGRAM request, CancellationToken cancellationToken = default);
        Task<List<PROSPECT>> GetProspectsAsync(string areaId, CancellationToken cancellationToken = default);
        Task<PROSPECT_SEIS_SURVEY> AnalyzeSeismicAsync(PROSPECT_SEIS_SURVEY request, CancellationToken cancellationToken = default);
        Task<EXPLORATION_PROGRAM> GetExplorationStatusAsync(string projectId, CancellationToken cancellationToken = default);
        Task<EXPLORATION_PROGRAM> SaveExplorationResultAsync(EXPLORATION_PROGRAM result, string? userId = null, CancellationToken cancellationToken = default);

        #endregion

        #region Development

        Task<DEVELOPMENT_COSTS> CreateDevelopmentPlanAsync(DEVELOPMENT_COSTS request, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> GetDrillingScheduleAsync(string planId, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> GetCompletionPlanAsync(string wellId, CancellationToken cancellationToken = default);
        Task<DEVELOPMENT_COSTS> UpdateDevelopmentPlanAsync(string planId, DEVELOPMENT_COSTS request, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> GetDevelopmentStatusAsync(string planId, CancellationToken cancellationToken = default);

        #endregion

        #region Decommissioning

        Task<DECOMMISSIONING_STATUS> CreateDecommissioningPlanAsync(DECOMMISSIONING_STATUS request, CancellationToken cancellationToken = default);
        Task<ABANDONMENT_STATUS> GetPluggingStatusAsync(string wellId, CancellationToken cancellationToken = default);
        Task<DECOMMISSIONING_STATUS> GetEnvironmentalAssessmentAsync(string assetId, CancellationToken cancellationToken = default);
        Task<DECOMMISSIONING_STATUS> UpdateDecommissioningStatusAsync(string planId, DECOMMISSIONING_STATUS status, CancellationToken cancellationToken = default);
        Task<DECOMMISSIONING_STATUS> GetDecommissioningCostEstimateAsync(string assetId, CancellationToken cancellationToken = default);

        #endregion

        #region WellManagement

        Task<FIELD_PHASE> GetWellStatusAsync(string wellId, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> UpdateWellStatusAsync(string wellId, FIELD_PHASE status, CancellationToken cancellationToken = default);
        Task<List<FIELD_PHASE>> GetWellHistoryAsync(string wellId, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> GetWellIntegrityAsync(string wellId, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> ScheduleWellInterventionAsync(FIELD_PHASE request, CancellationToken cancellationToken = default);

        #endregion

        #region FacilityManagement

        Task<List<FIELD_PHASE>> GetFacilitiesAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> CreateFacilityAsync(FIELD_PHASE request, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> GetFacilityStatusAsync(string facilityId, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> UpdateFacilityAsync(string facilityId, FIELD_PHASE request, CancellationToken cancellationToken = default);
        Task<FIELD_PHASE> GetFacilityCapacityAsync(string facilityId, CancellationToken cancellationToken = default);

        #endregion

        #region WorkOrder

        Task<WorkOrderEntity> CreateWorkOrderAsync(WorkOrderEntity request, CancellationToken cancellationToken = default);
        Task<List<WorkOrderEntity>> GetWorkOrdersAsync(string assetId, CancellationToken cancellationToken = default);
        Task<WorkOrderEntity> UpdateWorkOrderStatusAsync(string workOrderId, WorkOrderEntity status, CancellationToken cancellationToken = default);
        Task<WorkOrderEntity> GetWorkOrderDetailsAsync(string workOrderId, CancellationToken cancellationToken = default);
        Task<WorkOrderEntity> AssignWorkOrderAsync(string workOrderId, WorkOrderEntity assignment, CancellationToken cancellationToken = default);

        #endregion
    }

    /// <summary>
    /// Work Order entity for lifecycle management
    /// </summary>
    public class WorkOrderEntity
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public System.DateTime? ScheduledDate { get; set; }
        public System.DateTime? CompletedDate { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public System.DateTime? CreatedDate { get; set; }
    }
}
