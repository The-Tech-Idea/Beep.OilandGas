using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.LifeCycle
{
    /// <summary>
    /// Unified service for LifeCycle operations
    /// Handles Exploration, Development, Decommissioning, WellManagement, FacilityManagement, WorkOrder
    /// </summary>
    internal partial class LifeCycleService : ServiceBase, ILifeCycleService
    {
        public LifeCycleService(BeepOilandGasApp app, ILogger<LifeCycleService>? logger = null)
            : base(app, logger)
        {
        }
    }
}

