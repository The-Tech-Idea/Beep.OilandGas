using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

namespace Beep.OilandGas.Client.App.Services.Calculations
{
    internal partial class CalculationsService
    {
        #region Economic Analysis

        public async Task<decimal> CalculateNPVAsync(List<CashFlow> request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<List<CashFlow>, decimal>("/api/economicanalysis/npv", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<decimal> CalculateIRRAsync(List<CashFlow> request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<List<CashFlow>, decimal>("/api/economicanalysis/irr", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<EconomicResult> PerformEconomicAnalysisAsync(List<CashFlow> request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<List<CashFlow>, EconomicResult>("/api/economicanalysis/analyze", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<NPV_PROFILE_POINT>> GenerateNPVProfileAsync(List<ECONOMIC_CASH_FLOW> request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<List<ECONOMIC_CASH_FLOW>, List<NPV_PROFILE_POINT>>("/api/economicanalysis/npv-profile", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ECONOMIC_ANALYSIS_RESULT> SaveEconomicResultAsync(ECONOMIC_ANALYSIS_RESULT result, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/economicanalysis/result", queryParams);
                return await PostAsync<ECONOMIC_ANALYSIS_RESULT, ECONOMIC_ANALYSIS_RESULT>(endpoint, result, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ECONOMIC_ANALYSIS_RESULT> GetEconomicResultAsync(string resultId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(resultId)) throw new ArgumentException("Result ID is required", nameof(resultId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<ECONOMIC_ANALYSIS_RESULT>($"/api/economicanalysis/result/{Uri.EscapeDataString(resultId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
