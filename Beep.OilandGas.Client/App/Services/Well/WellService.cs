using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        public async Task<object> CompareWellsAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/well/compare", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> CompareWellsMultiSourceAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/well/compare-multi-source", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetComparisonFieldsAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>("/api/well/comparison-fields", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }
    }
}
