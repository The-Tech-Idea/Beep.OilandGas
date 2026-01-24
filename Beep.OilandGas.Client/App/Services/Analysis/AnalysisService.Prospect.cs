using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    internal partial class AnalysisService
    {
        #region Prospect

        public async Task<PROSPECT> IdentifyProspectAsync(PROSPECT request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PROSPECT, PROSPECT>("/api/prospect/identify", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PROSPECT_RISK_ASSESSMENT> EvaluateRiskAsync(PROSPECT request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PROSPECT, PROSPECT_RISK_ASSESSMENT>("/api/prospect/risk", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PROSPECT_VOLUME_ESTIMATE> GetVolumetricsAsync(string prospectId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(prospectId)) throw new ArgumentNullException(nameof(prospectId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PROSPECT_VOLUME_ESTIMATE>($"/api/prospect/{prospectId}/volumetrics", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PROSPECT_RANKING> RankProspectsAsync(PROSPECT_PORTFOLIO request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PROSPECT_PORTFOLIO, PROSPECT_RANKING>("/api/prospect/rank", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<PROSPECT>> GetProspectPortfolioAsync(string basinId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(basinId)) throw new ArgumentNullException(nameof(basinId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<PROSPECT>>($"/api/prospect/portfolio/{basinId}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
