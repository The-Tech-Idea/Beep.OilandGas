using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.Drilling
{
    /// <summary>
    /// Unified service for Drilling operations
    /// Handles Operations and Enhanced Recovery
    /// </summary>
    internal partial class DrillingService : ServiceBase, IDrillingService
    {
        public DrillingService(BeepOilandGasApp app, ILogger<DrillingService>? logger = null)
            : base(app, logger)
        {
        }
    }
}

