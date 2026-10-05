using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using Beep.OilandGas.Models.Data;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.Web.Services
{
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
        Task<List<FieldListItem>> GetAllFieldsAsync(string connectionName = "PPDM39");
        Task<FieldResponse> GetCurrentFieldAsync();
        Task<SetActiveFieldResponse> SetActiveFieldAsync(string fieldId);
        Task<FieldDashboard> GetFieldDashboardAsync();
        Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync();
        Task<List<object>> GetFieldWellsAsync();
        Task<object> GetFieldStatisticsAsync();
        Task<object> GetFieldTimelineAsync();

        // Work Order Operations
        Task<AFEResponse> CreateOrLinkAFEAsync(string workOrderId);
        Task<WorkOrderCostResponse> RecordWorkOrderCostAsync(string workOrderId, WorkOrderCostRequest request);
        Task<AFEResponse> GetAFEForWorkOrderAsync(string workOrderId);
    }

    /// <summary>
    /// Service for lifecycle management operations
    /// </summary>
    public class LifeCycleService : ILifeCycleService
    {
        private readonly ApiClient _apiClient;
        private readonly OilGasCallFailures _calls;
        private readonly IFailureReporter _failures;

        public LifeCycleService(ApiClient apiClient, OilGasCallFailures calls, IFailureReporter failures)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _calls = calls ?? throw new ArgumentNullException(nameof(calls));
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        }

        /// <summary>
        /// Get all fields for selection
        /// </summary>
        public async Task<List<FieldListItem>> GetAllFieldsAsync(string connectionName = "PPDM39")
        {
            var endpoint = "/api/field/fields";
            if (!string.IsNullOrEmpty(connectionName))
            {
                endpoint += $"?connectionName={Uri.EscapeDataString(connectionName)}";
            }
            var fields = await _apiClient.GetAsync<List<FieldListItem>>(endpoint);
            return fields ?? new List<FieldListItem>();
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
            // The endpoint defines 404 as no active field, not a failure: answered as no field, and recorded.
            catch (OilGasApiException noField) when (noField.StatusCode == HttpStatusCode.NotFound)
            {
                _failures.ReportHandled(
                    noField,
                    "reading the active field",
                    consequence: "the API holds no active field for this person; the caller was answered with no field",
                    FailureSeverity.Degraded);
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SetActiveFieldResponse
                {
                    Success = false,
                    ErrorMessage = _calls.Explain(failure, "setting the active field", "The active field was not changed")
                };
            }
        }

        /// <summary>
        /// Get field dashboard with KPIs and summaries
        /// </summary>
        public async Task<FieldDashboard> GetFieldDashboardAsync()
        {
            var dashboard = await _apiClient.GetAsync<FieldDashboard>("/api/field/current/dashboard");
            return dashboard ?? throw new InvalidOperationException("Field dashboard response was empty.");
        }

        /// <summary>
        /// Get field lifecycle summary
        /// </summary>
        public async Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync()
        {
            var summary = await _apiClient.GetAsync<FieldLifecycleSummary>("/api/field/current/summary");
            return summary ?? new FieldLifecycleSummary();
        }

        /// <summary>
        /// Get all wells for current field
        /// </summary>
        public async Task<List<object>> GetFieldWellsAsync()
        {
            var wells = await _apiClient.GetAsync<List<object>>("/api/field/current/wells");
            return wells ?? new List<object>();
        }

        /// <summary>
        /// Get field statistics
        /// </summary>
        public async Task<object> GetFieldStatisticsAsync()
        {
            var statistics = await _apiClient.GetAsync<object>("/api/field/current/statistics");
            return statistics ?? new { };
        }

        /// <summary>
        /// Get field timeline
        /// </summary>
        public async Task<object> GetFieldTimelineAsync()
        {
            var timeline = await _apiClient.GetAsync<object>("/api/field/current/timeline");
            return timeline ?? new { };
        }

        /// <summary>
        /// Create or link AFE for work order
        /// </summary>
        public async Task<AFEResponse> CreateOrLinkAFEAsync(string workOrderId)
        {
            var endpoint = $"/api/lifecycle/workorders/{Uri.EscapeDataString(workOrderId)}/afe";
            var afe = await _apiClient.PostAsync<AFEResponse>(endpoint, (HttpContent?)null);
            return afe ?? new AFEResponse();
        }

        /// <summary>
        /// Record work order cost
        /// </summary>
        public async Task<WorkOrderCostResponse> RecordWorkOrderCostAsync(
            string workOrderId, 
            WorkOrderCostRequest request)
        {
            request.WorkOrderId = workOrderId;
            var endpoint = $"/api/lifecycle/workorders/{Uri.EscapeDataString(workOrderId)}/costs";
            var response = await _apiClient.PostAsync<WorkOrderCostRequest, WorkOrderCostResponse>(
                endpoint, request);
            return response ?? new WorkOrderCostResponse
            {
                WorkOrderId = workOrderId,
                Message = "Failed to record work order cost"
            };
        }

        /// <summary>
        /// Get AFE for work order
        /// </summary>
        public async Task<AFEResponse> GetAFEForWorkOrderAsync(string workOrderId)
        {
            var afe = await _apiClient.GetAsync<AFEResponse>(
                $"/api/lifecycle/workorders/{Uri.EscapeDataString(workOrderId)}/afe");
            return afe ?? new AFEResponse();
        }
    }
}

