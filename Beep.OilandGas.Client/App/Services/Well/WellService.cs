using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.WellComparison;

namespace Beep.OilandGas.Client.App.Services.Well
{
    /// <summary>
    /// Unified service for Well operations
    /// </summary>
    internal class WellService : ServiceBase, IWellService
    {
        public WellService(BeepOilandGasApp app, ILogger<WellService>? logger = null)
            : base(app, logger)
        {
        }

        public async Task<WellComparisonData> CompareWellsAsync(CompareWellsRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<CompareWellsRequest, WellComparisonData>("/api/well/compare", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WellComparisonData> CompareWellsMultiSourceAsync(CompareWellsMultiSourceRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<CompareWellsMultiSourceRequest, WellComparisonData>("/api/well/compare-multi-source", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<ComparisonField>> GetComparisonFieldsAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<ComparisonField>>("/api/well/comparison-fields", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }
    }
}
