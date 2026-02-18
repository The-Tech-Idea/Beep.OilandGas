using System;
using System.Threading.Tasks;
using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.CompressorAnalysis.Services
{
    public partial class CompressorAnalysisService
    {
        public async Task<CentrifugalCompressorAnalysis> AnalyzeCentrifugalRigorousAsync(
            CentrifugalCompressorAnalysis analysis,
            decimal inletTempRankine,
            decimal gasSpecificGravity)
        {
            if (analysis == null) throw new ArgumentNullException(nameof(analysis));
            
            _logger?.LogInformation("Running rigorous centrifugal analysis for ID: {Id}", analysis.AnalysisId);

            decimal k_iso = 1.28m; // Default natural gas
            decimal z_inlet = 0.95m; // Approximation
            decimal z_discharge = 0.95m; // Approximation

            AdvancedCompressorCalculator.CalculateCentrifugalRigorous(
                analysis, 
                inletTempRankine, 
                gasSpecificGravity, 
                k_iso, 
                z_inlet, 
                z_discharge);
                
            analysis.AnalysisDate = DateTime.UtcNow;
            
            return await Task.FromResult(analysis);
        }

        public async Task<ReciprocationCompressorAnalysis> AnalyzeReciprocatingRigorousAsync(
            ReciprocationCompressorAnalysis analysis,
            decimal gasSpecificGravity)
        {
            if (analysis == null) throw new ArgumentNullException(nameof(analysis));

            _logger?.LogInformation("Running rigorous reciprocating analysis for ID: {Id}", analysis.AnalysisId);

            decimal k_iso = 1.28m;
            decimal z_avg = 0.95m;

            AdvancedCompressorCalculator.CalculateReciprocatingRigorous(
                analysis,
                gasSpecificGravity,
                k_iso,
                z_avg);

            analysis.AnalysisDate = DateTime.UtcNow;

            return await Task.FromResult(analysis);
        }
    }
}
