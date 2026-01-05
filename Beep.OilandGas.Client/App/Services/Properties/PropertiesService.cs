using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.Properties
{
    /// <summary>
    /// Unified service for all Properties operations
    /// Handles Oil Properties, Gas Properties, and Heat Map
    /// </summary>
    internal partial class PropertiesService : ServiceBase, IPropertiesService
    {
        public PropertiesService(BeepOilandGasApp app, ILogger<PropertiesService>? logger = null)
            : base(app, logger)
        {
        }
    }
}

