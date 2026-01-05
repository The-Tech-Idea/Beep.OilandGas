using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Well
{
    /// <summary>
    /// Service interface for Well operations
    /// </summary>
    public interface IWellService
    {
        Task<object> CompareWellsAsync(object request, CancellationToken cancellationToken = default);
        Task<object> CompareWellsMultiSourceAsync(object request, CancellationToken cancellationToken = default);
        Task<List<object>> GetComparisonFieldsAsync(CancellationToken cancellationToken = default);
    }
}
