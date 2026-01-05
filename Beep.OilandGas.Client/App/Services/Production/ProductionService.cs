using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.Production
{
    /// <summary>
    /// Unified service for Production operations
    /// Handles Accounting, Forecasting, and Operations
    /// </summary>
    internal partial class ProductionService : ServiceBase, IProductionService
    {
        public ProductionService(BeepOilandGasApp app, ILogger<ProductionService>? logger = null)
            : base(app, logger)
        {
        }
    }
}

