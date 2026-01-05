using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.Calculations
{
    /// <summary>
    /// Unified service for all Calculation operations
    /// Handles Flash, Nodal Analysis, and Economic Analysis
    /// </summary>
    internal partial class CalculationsService : ServiceBase, ICalculationsService
    {
        public CalculationsService(BeepOilandGasApp app, ILogger<CalculationsService>? logger = null)
            : base(app, logger)
        {
        }
    }
}

