using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.Analysis
{
    /// <summary>
    /// Unified service for Analysis operations
    /// Handles Choke, Compressor, Pipeline, WellTest, GasLift, PumpPerformance, Prospect
    /// </summary>
    internal partial class AnalysisService : ServiceBase, IAnalysisService
    {
        public AnalysisService(BeepOilandGasApp app, ILogger<AnalysisService>? logger = null)
            : base(app, logger)
        {
        }
    }
}
