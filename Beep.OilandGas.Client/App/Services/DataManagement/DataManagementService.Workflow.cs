using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Workflow

        public async Task<object> StartWorkflowAsync(string workflowType, object request, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workflowType)) throw new ArgumentException("Workflow type is required", nameof(workflowType));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39workflow/{Uri.EscapeDataString(workflowType)}/start", queryParams);
                return await PostAsync<object, object>(endpoint, request, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetWorkflowStatusAsync(string workflowId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workflowId)) throw new ArgumentException("Workflow ID is required", nameof(workflowId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39workflow/{Uri.EscapeDataString(workflowId)}/status", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> AdvanceWorkflowAsync(string workflowId, object action, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workflowId)) throw new ArgumentException("Workflow ID is required", nameof(workflowId));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39workflow/{Uri.EscapeDataString(workflowId)}/advance", queryParams);
                return await PostAsync<object, object>(endpoint, action, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetPendingWorkflowsAsync(string? userId = null, CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/ppdm39workflow/pending", queryParams);
                return await GetAsync<List<object>>(endpoint, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

