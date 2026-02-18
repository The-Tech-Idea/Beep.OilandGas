using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.FieldOrchestrator;
using Beep.OilandGas.Models.Data.EnhancedRecovery;

namespace Beep.OilandGas.Client.App.Services.Field
{
    /// <summary>
    /// Service interface for Field/LifeCycle operations
    /// </summary>
    public interface IFieldService
    {
        Task SetCurrentFieldAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<string?> GetCurrentFieldAsync(CancellationToken cancellationToken = default);
        Task<FieldOrchestratorResponse> GetFieldDetailsAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<List<WELL>> GetFieldWellsAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<FieldProductionSummary> GetFieldProductionSummaryAsync(string fieldId, CancellationToken cancellationToken = default);
    }
}

