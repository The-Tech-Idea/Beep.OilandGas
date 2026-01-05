using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Field
{
    /// <summary>
    /// Service interface for Field/LifeCycle operations
    /// </summary>
    public interface IFieldService
    {
        Task SetCurrentFieldAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<string?> GetCurrentFieldAsync(CancellationToken cancellationToken = default);
        Task<object> GetFieldDetailsAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<object> GetFieldWellsAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<object> GetFieldProductionSummaryAsync(string fieldId, CancellationToken cancellationToken = default);
    }
}

