using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for FieldOrchestrator - manages complete lifecycle of a single active field
    /// </summary>
    public interface IFieldOrchestrator
    {
        /// <summary>
        /// Current active field ID
        /// </summary>
        string? CurrentFieldId { get; }

        /// <summary>
        /// Set the active field for the orchestrator
        /// </summary>
        Task<bool> SetActiveFieldAsync(string fieldId);

        /// <summary>
        /// Get the current active field
        /// </summary>
        Task<object?> GetCurrentFieldAsync();

        /// <summary>
        /// Get lifecycle summary for the current field across all phases
        /// </summary>
        Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync();

        /// <summary>
        /// Get all wells for the current field across all phases
        /// </summary>
        Task<List<WELL>> GetFieldWellsAsync();

        /// <summary>
        /// Get statistical aggregates for the current field
        /// </summary>
        Task<FieldStatistics> GetFieldStatisticsAsync();

        /// <summary>
        /// Get timeline of events for the current field across all phases
        /// </summary>
        Task<FieldTimeline> GetFieldTimelineAsync();

        /// <summary>
        /// Get dashboard with performance metrics for the current field
        /// </summary>
        Task<FieldDashboard> GetFieldDashboardAsync();

        /// <summary>
        /// Get exploration service scoped to current field
        /// </summary>
        IFieldExplorationService GetExplorationService();

        /// <summary>
        /// Get development service scoped to current field
        /// </summary>
        IFieldDevelopmentService GetDevelopmentService();

        /// <summary>
        /// Get production service scoped to current field
        /// </summary>
        IFieldProductionService GetProductionService();

        /// <summary>
        /// Get decommissioning service scoped to current field
        /// </summary>
        IFieldDecommissioningService GetDecommissioningService();
    }
}




