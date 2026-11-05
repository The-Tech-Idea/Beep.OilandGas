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
    /// Service interface for lifecycle operations
    /// </summary>
    public interface ILifeCycleService
    {
        // Field Operations
        Task<FieldResponse> GetCurrentFieldAsync();
        Task<FieldDashboard> GetFieldDashboardAsync();
        Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync();
        Task<List<object>> GetFieldWellsAsync();
        Task<object> GetFieldStatisticsAsync();
        Task<object> GetFieldTimelineAsync();

    }

    /// <summary>
    /// Service for lifecycle management operations
    /// </summary>
    public class LifeCycleService : ILifeCycleService
    {
        private readonly ApiClient _apiClient;
        private readonly OilGasCallFailures _calls;
        private readonly IFailureReporter _failures;
        private readonly TheTechIdea.Data.OilGas.IFieldSelectionContext _fields;

        public LifeCycleService(ApiClient apiClient, OilGasCallFailures calls, IFailureReporter failures, TheTechIdea.Data.OilGas.IFieldSelectionContext fields)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _calls = calls ?? throw new ArgumentNullException(nameof(calls));
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        /// <summary>Read the existing reviewed profile selection through the same Web owner.</summary>
        public Task<FieldResponse> GetCurrentFieldAsync() => _fields.GetCurrentFieldSelectionAsync();

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

    }
}
