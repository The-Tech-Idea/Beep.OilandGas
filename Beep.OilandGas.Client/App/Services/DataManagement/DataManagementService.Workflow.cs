using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Workflow

        public async Task<WorkflowExecutionResult> StartWorkflowAsync(string workflowType, object request, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workflowType)) throw new ArgumentException("Workflow type is required", nameof(workflowType));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39workflow/{Uri.EscapeDataString(workflowType)}/start", queryParams);
                return await PostAsync<object, WorkflowExecutionResult>(endpoint, request, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WorkflowStatus> GetWorkflowStatusAsync(string workflowId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workflowId)) throw new ArgumentException("Workflow ID is required", nameof(workflowId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<WorkflowStatus>($"/api/ppdm39workflow/{Uri.EscapeDataString(workflowId)}/status", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WorkflowExecutionResult> AdvanceWorkflowAsync(string workflowId, object action, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workflowId)) throw new ArgumentException("Workflow ID is required", nameof(workflowId));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39workflow/{Uri.EscapeDataString(workflowId)}/advance", queryParams);
                return await PostAsync<object, WorkflowExecutionResult>(endpoint, action, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<WorkflowExecutionResult>> GetPendingWorkflowsAsync(string? userId = null, CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/ppdm39workflow/pending", queryParams);
                return await GetAsync<List<WorkflowExecutionResult>>(endpoint, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

