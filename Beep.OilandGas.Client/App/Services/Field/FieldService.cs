using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.FieldOrchestrator;
using Beep.OilandGas.Models.Data.EnhancedRecovery;

namespace Beep.OilandGas.Client.App.Services.Field
{
    /// <summary>
    /// Unified service for Field/LifeCycle operations
    /// </summary>
    internal class FieldService : ServiceBase, IFieldService
    {
        public FieldService(BeepOilandGasApp app, ILogger<FieldService>? logger = null)
            : base(app, logger)
        {
        }

        public async Task SetCurrentFieldAsync(string fieldId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fieldId)) throw new ArgumentException("Field ID is required", nameof(fieldId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                await PostAsync<object, object>($"/api/fieldorchestrator/current/{Uri.EscapeDataString(fieldId)}", null!, cancellationToken);
                return;
            }
            var localService = GetLocalService<IFieldLocalService>();
            if (localService == null) throw new InvalidOperationException("IFieldLocalService not available");
            await localService.SetCurrentFieldAsync(fieldId);
        }

        public async Task<string?> GetCurrentFieldAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<string>("/api/fieldorchestrator/current", cancellationToken);
            var localService = GetLocalService<IFieldLocalService>();
            if (localService == null) throw new InvalidOperationException("IFieldLocalService not available");
            return await localService.GetCurrentFieldAsync();
        }

        public async Task<FieldOrchestratorResponse> GetFieldDetailsAsync(string fieldId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fieldId)) throw new ArgumentException("Field ID is required", nameof(fieldId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<FieldOrchestratorResponse>($"/api/fieldorchestrator/field/{Uri.EscapeDataString(fieldId)}", cancellationToken);
            var localService = GetLocalService<IFieldLocalService>();
            if (localService == null) throw new InvalidOperationException("IFieldLocalService not available");
            return await localService.GetFieldDetailsAsync(fieldId);
        }

        public async Task<List<WELL>> GetFieldWellsAsync(string fieldId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fieldId)) throw new ArgumentException("Field ID is required", nameof(fieldId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<WELL>>($"/api/fieldorchestrator/field/{Uri.EscapeDataString(fieldId)}/wells", cancellationToken);
            var localService = GetLocalService<IFieldLocalService>();
            if (localService == null) throw new InvalidOperationException("IFieldLocalService not available");
            return await localService.GetFieldWellsAsync(fieldId);
        }

        public async Task<FieldProductionSummary> GetFieldProductionSummaryAsync(string fieldId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fieldId)) throw new ArgumentException("Field ID is required", nameof(fieldId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<FieldProductionSummary>($"/api/fieldorchestrator/field/{Uri.EscapeDataString(fieldId)}/production-summary", cancellationToken);
            var localService = GetLocalService<IFieldLocalService>();
            if (localService == null) throw new InvalidOperationException("IFieldLocalService not available");
            return await localService.GetFieldProductionSummaryAsync(fieldId);
        }
    }

    public interface IFieldLocalService
    {
        Task SetCurrentFieldAsync(string fieldId);
        Task<string?> GetCurrentFieldAsync();
        Task<FieldOrchestratorResponse> GetFieldDetailsAsync(string fieldId);
        Task<List<WELL>> GetFieldWellsAsync(string fieldId);
        Task<FieldProductionSummary> GetFieldProductionSummaryAsync(string fieldId);
    }
}

