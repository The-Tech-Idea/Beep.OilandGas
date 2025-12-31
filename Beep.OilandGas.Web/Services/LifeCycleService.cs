using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Field list item model
    /// </summary>
    public class FieldListItem
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }

    /// <summary>
    /// Field response model
    /// </summary>
    public class FieldResponse
    {
        public object? Field { get; set; }
        public string? FieldId { get; set; }
        public string? FieldName { get; set; }
    }

    /// <summary>
    /// Set active field request
    /// </summary>
    public class SetActiveFieldRequest
    {
        public string FieldId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Set active field response
    /// </summary>
    public class SetActiveFieldResponse
    {
        public bool Success { get; set; }
        public string? FieldId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Field dashboard model
    /// </summary>
    public class FieldDashboard
    {
        public Dictionary<string, object>? KPIs { get; set; }
        public object? ExplorationSummary { get; set; }
        public object? DevelopmentSummary { get; set; }
        public object? ProductionSummary { get; set; }
        public object? DecommissioningSummary { get; set; }
        public List<object>? RecentActivities { get; set; }
    }

    /// <summary>
    /// Field lifecycle summary
    /// </summary>
    public class FieldLifecycleSummary
    {
        public string? CurrentPhase { get; set; }
        public Dictionary<string, int>? PhaseCounts { get; set; }
        public Dictionary<string, object>? PhaseSummaries { get; set; }
    }

    /// <summary>
    /// Work order cost request
    /// </summary>
    public class WorkOrderCostRequest
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CostType { get; set; } = string.Empty;
        public string CostCategory { get; set; } = string.Empty;
        public bool IsCapitalized { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Work order cost response
    /// </summary>
    public class WorkOrderCostResponse
    {
        public string CostTransactionId { get; set; } = string.Empty;
        public string WorkOrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// AFE response
    /// </summary>
    public class AFEResponse
    {
        public string AfeId { get; set; } = string.Empty;
        public string AfeNumber { get; set; } = string.Empty;
        public string? AfeName { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public string? Status { get; set; }
        public string? WorkOrderId { get; set; }
    }

    /// <summary>
    /// Service interface for lifecycle operations
    /// </summary>
    public interface ILifeCycleService
    {
        // Field Operations
        Task<List<FieldListItem>> GetAllFieldsAsync(string? connectionName = null);
        Task<FieldResponse> GetCurrentFieldAsync();
        Task<SetActiveFieldResponse> SetActiveFieldAsync(string fieldId);
        Task<FieldDashboard> GetFieldDashboardAsync();
        Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync();
        Task<List<object>> GetFieldWellsAsync();
        Task<object> GetFieldStatisticsAsync();
        Task<object> GetFieldTimelineAsync();

        // Work Order Operations
        Task<AFEResponse> CreateOrLinkAFEAsync(string workOrderId, string? userId = null);
        Task<WorkOrderCostResponse> RecordWorkOrderCostAsync(string workOrderId, WorkOrderCostRequest request, string? userId = null);
        Task<AFEResponse> GetAFEForWorkOrderAsync(string workOrderId);
    }

    /// <summary>
    /// Service for lifecycle management operations
    /// </summary>
    public class LifeCycleService : ILifeCycleService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<LifeCycleService> _logger;

        public LifeCycleService(
            ApiClient apiClient,
            ILogger<LifeCycleService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all fields for selection
        /// </summary>
        public async Task<List<FieldListItem>> GetAllFieldsAsync(string? connectionName = null)
        {
            try
            {
                var endpoint = "/api/field/fields";
                if (!string.IsNullOrEmpty(connectionName))
                {
                    endpoint += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                }
                var fields = await _apiClient.GetAsync<List<FieldListItem>>(endpoint);
                return fields ?? new List<FieldListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all fields");
                return new List<FieldListItem>();
            }
        }

        /// <summary>
        /// Get current active field
        /// </summary>
        public async Task<FieldResponse> GetCurrentFieldAsync()
        {
            try
            {
                var field = await _apiClient.GetAsync<FieldResponse>("/api/field/current");
                return field ?? new FieldResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current field");
                return new FieldResponse();
            }
        }

        /// <summary>
        /// Set active field
        /// </summary>
        public async Task<SetActiveFieldResponse> SetActiveFieldAsync(string fieldId)
        {
            try
            {
                var request = new SetActiveFieldRequest { FieldId = fieldId };
                var response = await _apiClient.PostAsync<SetActiveFieldRequest, SetActiveFieldResponse>(
                    "/api/field/set-active", request);
                return response ?? new SetActiveFieldResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to set active field"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting active field {FieldId}", fieldId);
                return new SetActiveFieldResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Get field dashboard with KPIs and summaries
        /// </summary>
        public async Task<FieldDashboard> GetFieldDashboardAsync()
        {
            try
            {
                var dashboard = await _apiClient.GetAsync<FieldDashboard>("/api/field/current/dashboard");
                return dashboard ?? new FieldDashboard();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field dashboard");
                return new FieldDashboard();
            }
        }

        /// <summary>
        /// Get field lifecycle summary
        /// </summary>
        public async Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync()
        {
            try
            {
                var summary = await _apiClient.GetAsync<FieldLifecycleSummary>("/api/field/current/summary");
                return summary ?? new FieldLifecycleSummary();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field lifecycle summary");
                return new FieldLifecycleSummary();
            }
        }

        /// <summary>
        /// Get all wells for current field
        /// </summary>
        public async Task<List<object>> GetFieldWellsAsync()
        {
            try
            {
                var wells = await _apiClient.GetAsync<List<object>>("/api/field/current/wells");
                return wells ?? new List<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field wells");
                return new List<object>();
            }
        }

        /// <summary>
        /// Get field statistics
        /// </summary>
        public async Task<object> GetFieldStatisticsAsync()
        {
            try
            {
                var statistics = await _apiClient.GetAsync<object>("/api/field/current/statistics");
                return statistics ?? new { };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field statistics");
                return new { };
            }
        }

        /// <summary>
        /// Get field timeline
        /// </summary>
        public async Task<object> GetFieldTimelineAsync()
        {
            try
            {
                var timeline = await _apiClient.GetAsync<object>("/api/field/current/timeline");
                return timeline ?? new { };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field timeline");
                return new { };
            }
        }

        /// <summary>
        /// Create or link AFE for work order
        /// </summary>
        public async Task<AFEResponse> CreateOrLinkAFEAsync(string workOrderId, string? userId = null)
        {
            try
            {
                var endpoint = $"/api/lifecycle/workorders/{Uri.EscapeDataString(workOrderId)}/afe";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                var afe = await _apiClient.PostAsync<AFEResponse>(endpoint, null);
                return afe ?? new AFEResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/linking AFE for work order {WorkOrderId}", workOrderId);
                return new AFEResponse();
            }
        }

        /// <summary>
        /// Record work order cost
        /// </summary>
        public async Task<WorkOrderCostResponse> RecordWorkOrderCostAsync(
            string workOrderId, 
            WorkOrderCostRequest request, 
            string? userId = null)
        {
            try
            {
                request.WorkOrderId = workOrderId;
                var endpoint = $"/api/lifecycle/workorders/{Uri.EscapeDataString(workOrderId)}/costs";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                var response = await _apiClient.PostAsync<WorkOrderCostRequest, WorkOrderCostResponse>(
                    endpoint, request);
                return response ?? new WorkOrderCostResponse
                {
                    WorkOrderId = workOrderId,
                    Message = "Failed to record work order cost"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording work order cost for {WorkOrderId}", workOrderId);
                return new WorkOrderCostResponse
                {
                    WorkOrderId = workOrderId,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get AFE for work order
        /// </summary>
        public async Task<AFEResponse> GetAFEForWorkOrderAsync(string workOrderId)
        {
            try
            {
                var afe = await _apiClient.GetAsync<AFEResponse>(
                    $"/api/lifecycle/workorders/{Uri.EscapeDataString(workOrderId)}/afe");
                return afe ?? new AFEResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AFE for work order {WorkOrderId}", workOrderId);
                return new AFEResponse();
            }
        }
    }
}

