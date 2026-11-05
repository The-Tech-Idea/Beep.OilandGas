using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    /// <summary>
    /// Unified service for Data Management (PPDM39) operations
    /// Includes CRUD, Validation, Quality, Versioning, Audit, LOV, and Setup
    /// </summary>
    internal partial class DataManagementService : ServiceBase, IDataManagementService
    {
        private PpdmDataClient? _dataClient;
        private PpdmDataClient DataClient => _dataClient ??= new(_app.GetApiTransport());
        public DataManagementService(BeepOilandGasApp app, ILogger<DataManagementService>? logger = null)
            : base(app, logger)
        {
        }
    }
}
