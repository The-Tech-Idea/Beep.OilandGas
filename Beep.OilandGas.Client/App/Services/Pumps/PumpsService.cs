using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.Pumps
{
    /// <summary>
    /// Unified service for all Pump operations
    /// Handles Hydraulic Pump, Plunger Lift, and Sucker Rod Pumping
    /// </summary>
    internal partial class PumpsService : ServiceBase, IPumpsService
    {
        public PumpsService(BeepOilandGasApp app, ILogger<PumpsService>? logger = null)
            : base(app, logger)
        {
        }
    }
}

