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

        public async Task<WorkflowExecutionResult> StartWorkflowAsync(string workflowType, object request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workflowType)) throw new ArgumentException("Workflow type is required", nameof(workflowType));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                return await PostAsync<object, WorkflowExecutionResult>($"/api/ppdm39workflow/{Uri.EscapeDataString(workflowType)}/start", request, cancellationToken);
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

        public async Task<WorkflowExecutionResult> AdvanceWorkflowAsync(string workflowId, object action, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workflowId)) throw new ArgumentException("Workflow ID is required", nameof(workflowId));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                return await PostAsync<object, WorkflowExecutionResult>($"/api/ppdm39workflow/{Uri.EscapeDataString(workflowId)}/advance", action, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<WorkflowExecutionResult>> GetPendingWorkflowsAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<WorkflowExecutionResult>>("/api/ppdm39workflow/pending", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

