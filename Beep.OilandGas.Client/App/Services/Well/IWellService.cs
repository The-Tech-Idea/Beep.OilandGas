using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.WellComparison;

namespace Beep.OilandGas.Client.App.Services.Well
{
    /// <summary>
    /// Service interface for Well operations
    /// </summary>
    public interface IWellService
    {
        Task<WellComparisonData> CompareWellsAsync(CompareWellsRequest request, CancellationToken cancellationToken = default);
        Task<WellComparisonData> CompareWellsMultiSourceAsync(CompareWellsMultiSourceRequest request, CancellationToken cancellationToken = default);
        Task<List<ComparisonField>> GetComparisonFieldsAsync(CancellationToken cancellationToken = default);
    }
}
