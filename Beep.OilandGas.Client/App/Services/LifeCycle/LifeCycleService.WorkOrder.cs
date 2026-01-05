using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.LifeCycle
{
    internal partial class LifeCycleService
    {
        #region WorkOrder

        public async Task<WorkOrderEntity> CreateWorkOrderAsync(WorkOrderEntity request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WorkOrderEntity, WorkOrderEntity>("/api/lifecycle/workorder/create", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<WorkOrderEntity>> GetWorkOrdersAsync(string assetId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(assetId)) throw new ArgumentNullException(nameof(assetId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<WorkOrderEntity>>($"/api/lifecycle/workorder/asset/{Uri.EscapeDataString(assetId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WorkOrderEntity> UpdateWorkOrderStatusAsync(string workOrderId, WorkOrderEntity status, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workOrderId)) throw new ArgumentNullException(nameof(workOrderId));
            if (status == null) throw new ArgumentNullException(nameof(status));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<WorkOrderEntity, WorkOrderEntity>($"/api/lifecycle/workorder/{Uri.EscapeDataString(workOrderId)}/status", status, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WorkOrderEntity> GetWorkOrderDetailsAsync(string workOrderId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workOrderId)) throw new ArgumentNullException(nameof(workOrderId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<WorkOrderEntity>($"/api/lifecycle/workorder/{Uri.EscapeDataString(workOrderId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WorkOrderEntity> AssignWorkOrderAsync(string workOrderId, WorkOrderEntity assignment, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(workOrderId)) throw new ArgumentNullException(nameof(workOrderId));
            if (assignment == null) throw new ArgumentNullException(nameof(assignment));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<WorkOrderEntity, WorkOrderEntity>($"/api/lifecycle/workorder/{Uri.EscapeDataString(workOrderId)}/assign", assignment, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
