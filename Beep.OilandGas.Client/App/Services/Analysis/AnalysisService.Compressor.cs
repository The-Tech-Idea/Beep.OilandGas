using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.Models.Data.CompressorAnalysis;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region Compressor

        public async Task<CompressorPowerResult> AnalyzeCompressorAsync(CompressorOperatingConditions request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<CompressorOperatingConditions, CompressorPowerResult>("/api/compressor/analyze", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<COMPRESSOR_POWER_RESULT> DesignCentrifugalCompressorAsync(CENTRIFUGAL_COMPRESSOR_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<CENTRIFUGAL_COMPRESSOR_PROPERTIES, COMPRESSOR_POWER_RESULT>("/api/compressor/design/centrifugal", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<COMPRESSOR_POWER_RESULT> DesignReciprocatingCompressorAsync(RECIPROCATING_COMPRESSOR_PROPERTIES request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<RECIPROCATING_COMPRESSOR_PROPERTIES, COMPRESSOR_POWER_RESULT>("/api/compressor/design/reciprocating", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<COMPRESSOR_OPERATING_CONDITIONS> GetCompressorPerformanceAsync(string compressorId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(compressorId)) throw new ArgumentNullException(nameof(compressorId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<COMPRESSOR_OPERATING_CONDITIONS>($"/api/compressor/{compressorId}/performance", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<CompressorPowerResult> CalculateCompressorPowerAsync(CompressorOperatingConditions request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<CompressorOperatingConditions, CompressorPowerResult>("/api/compressor/power", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
