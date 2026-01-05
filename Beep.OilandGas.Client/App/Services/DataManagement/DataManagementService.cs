using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    /// <summary>
    /// Unified service for Data Management (PPDM39) operations
    /// Includes CRUD, Validation, Quality, Versioning, Audit, LOV, and Setup
    /// </summary>
    internal partial class DataManagementService : ServiceBase, IDataManagementService
    {
        public DataManagementService(BeepOilandGasApp app, ILogger<DataManagementService>? logger = null)
            : base(app, logger)
        {
        }
    }
}
