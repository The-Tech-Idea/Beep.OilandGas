using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.Models.Data.WellTestAnalysis;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region WellTest

        public async Task<WellTestAnalysisResult> AnalyzeBuildUpAsync(WellTestData request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WellTestData, WellTestAnalysisResult>("/api/welltest/buildup", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WellTestAnalysisResult> AnalyzeDrawdownAsync(WellTestData request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WellTestData, WellTestAnalysisResult>("/api/welltest/drawdown", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WELL_TEST_ANALYSIS_RESULT> GetDerivativeAnalysisAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WELL_TEST_DATA, WELL_TEST_ANALYSIS_RESULT>("/api/welltest/derivative", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<WELL_TEST_ANALYSIS_RESULT> InterpretWellTestAsync(WELL_TEST_DATA request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<WELL_TEST_DATA, WELL_TEST_ANALYSIS_RESULT>("/api/welltest/interpret", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<WELL_TEST_DATA>> GetWellTestHistoryAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<WELL_TEST_DATA>>($"/api/welltest/{wellId}/history", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
