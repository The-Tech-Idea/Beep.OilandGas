using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Beep.OilandGas.Client.App.Services.AccessControl;
using Beep.OilandGas.Client.App.Services.Accounting;
using Beep.OilandGas.Client.App.Services.Analysis;
using Beep.OilandGas.Client.App.Services.Calculations;
using Beep.OilandGas.Client.App.Services.Drilling;
using Beep.OilandGas.Client.App.Services.Field;
using Beep.OilandGas.Client.App.Services.Lease;
using Beep.OilandGas.Client.App.Services.LifeCycle;
using Beep.OilandGas.Client.App.Services.Operations;
using Beep.OilandGas.Client.App.Services.Permits;
using Beep.OilandGas.Client.App.Services.Production;
using Beep.OilandGas.Client.App.Services.Properties;
using Beep.OilandGas.Client.App.Services.Pumps;
using Beep.OilandGas.Client.Authentication;
using Beep.OilandGas.Client.Connection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

using IConnectionService = Beep.OilandGas.Client.App.Services.Connection.IConnectionService;
using IWellService = Beep.OilandGas.Client.App.Services.Well.IWellService;
using IDataManagementService = Beep.OilandGas.Client.App.Services.DataManagement.IDataManagementService;

namespace Beep.OilandGas.Client.App
{
    /// <summary>
    /// Main application facade class - THE PRIMARY ENTRY POINT
    /// Handles both remote (HTTP API) and local (direct service) modes
    /// </summary>
    public class BeepOilandGasApp : IBeepOilandGasApp, IDisposable
    {
        private readonly ServiceAccessMode _accessMode;
        private readonly AppOptions _options;
        private readonly ILogger? _logger;
        
        // Remote mode dependencies
        private readonly HttpClient? _httpClient;
        private readonly IAuthenticationProvider? _authProvider;

        // Local mode dependencies
        private readonly IServiceProvider? _serviceProvider;
        private readonly IDMEEditor? _dmeEditor;
        private readonly ConnectionManager _connectionManager;
        private readonly DataSourceManager? _dataSourceManager;

        // Connection state
        private string? _currentConnectionName;
        private readonly object _connectionLock = new object();

        // Core service instances (lazy initialization)
        private IConnectionService? _connectionService;
        private IWellService? _wellService;
        private IDataManagementService? _dataManagementService;

        // Equipment service instances
        private IPumpsService? _pumpsService;
        private IPropertiesService? _propertiesService;

        // Calculation service instances
        private ICalculationsService? _calculationsService;
        private IAnalysisService? _analysisService;

        // Lifecycle service instances
        private IFieldService? _fieldService;
        private ILifeCycleService? _lifeCycleService;

        // Operations service instances
        private IOperationsService? _operationsService;
        private IDrillingService? _drillingService;

        // Production service instances
        private IProductionService? _productionService;
        private IAccountingService? _accountingService;

        // Administrative service instances
        private IAccessControlService? _accessControlService;
        private IPermitsService? _permitsService;
        private ILeaseService? _leaseService;

        private bool _disposed = false;

        /// <summary>
        /// Create AppClass in remote mode
        /// </summary>
        public BeepOilandGasApp(
            HttpClient httpClient,
            AppOptions options,
            IAuthenticationProvider? authProvider = null,
            ILogger<BeepOilandGasApp>? logger = null)
        {
            _accessMode = ServiceAccessMode.Remote;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authProvider = authProvider;
            _logger = logger;
            _currentConnectionName = options.DefaultConnectionName;

            if (!string.IsNullOrEmpty(options.ApiBaseUrl))
                _httpClient.BaseAddress = new Uri(options.ApiBaseUrl);
            _httpClient.Timeout = options.Timeout;
            
            _connectionManager = new ConnectionManager(new List<ConnectionInfo>());
        }

        /// <summary>
        /// Create AppClass in local mode
        /// </summary>
        public BeepOilandGasApp(
            IServiceProvider serviceProvider,
            IDMEEditor dmeEditor,
            AppOptions options,
            ILogger<BeepOilandGasApp>? logger = null)
        {
            _accessMode = ServiceAccessMode.Local;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _dmeEditor = dmeEditor ?? throw new ArgumentNullException(nameof(dmeEditor));
            _logger = logger;

            var connections = new List<ConnectionInfo>();
            if (_dmeEditor?.ConfigEditor?.DataConnections != null)
            {
                connections = _dmeEditor.ConfigEditor.DataConnections
                    .Select(cp => new ConnectionInfo
                    {
                        ConnectionName = cp.ConnectionName ?? string.Empty,
                        DatabaseType = cp.DatabaseType.ToString(),
                        Server = cp.Host ?? string.Empty,
                        Database = cp.Database,
                        Port = cp.Port,
                        IsActive = true
                    })
                    .ToList();
            }

            _connectionManager = new ConnectionManager(connections);
            _dataSourceManager = _dmeEditor != null ? new DataSourceManager(_dmeEditor) : null;
            _currentConnectionName = options.DefaultConnectionName;
        }

        #region Public Service Properties

        public ServiceAccessMode AccessMode => _accessMode;

        #region Core Services

        public IConnectionService Connection => 
            _connectionService ??= new Services.Connection.ConnectionService(this);

        public IWellService Well => 
            _wellService ??= new Services.Well.WellService(this);

        public IDataManagementService DataManagement => 
            _dataManagementService ??= new Services.DataManagement.DataManagementService(this);

        #endregion

        #region Equipment Services

        public IPumpsService Pumps => 
            _pumpsService ??= new Services.Pumps.PumpsService(this);

        public IPropertiesService Properties => 
            _propertiesService ??= new Services.Properties.PropertiesService(this);

        #endregion

        #region Calculation Services

        public ICalculationsService Calculations => 
            _calculationsService ??= new Services.Calculations.CalculationsService(this);

        public IAnalysisService Analysis => 
            _analysisService ??= new Services.Analysis.AnalysisService(this);

        #endregion

        #region Lifecycle Services

        public IFieldService Field => 
            _fieldService ??= new Services.Field.FieldService(this);

        public ILifeCycleService LifeCycle => 
            _lifeCycleService ??= new Services.LifeCycle.LifeCycleService(this);

        #endregion

        #region Operations Services

        public IOperationsService Operations => 
            _operationsService ??= new Services.Operations.OperationsService(this);

        public IDrillingService Drilling => 
            _drillingService ??= new Services.Drilling.DrillingService(this);

        #endregion

        #region Production Services

        public IProductionService Production => 
            _productionService ??= new Services.Production.ProductionService(this);

        public IAccountingService Accounting => 
            _accountingService ??= new Services.Accounting.AccountingService(this);

        #endregion

        #region Administrative Services

        public IAccessControlService AccessControl => 
            _accessControlService ??= new Services.AccessControl.AccessControlService(this);

        public IPermitsService Permits => 
            _permitsService ??= new Services.Permits.PermitsService(this);

        public ILeaseService Lease => 
            _leaseService ??= new Services.Lease.LeaseService(this);

        #endregion

        #endregion

        #region Internal Methods for Services

        internal string? GetCurrentConnectionName()
        {
            lock (_connectionLock)
            {
                return _currentConnectionName ?? _options.DefaultConnectionName;
            }
        }

        internal void SetCurrentConnectionInternal(string connectionName)
        {
            lock (_connectionLock)
            {
                _currentConnectionName = connectionName;
            }
        }

        internal ConnectionManager GetConnectionManager() => _connectionManager;
        internal HttpClient? GetHttpClient() => _httpClient;
        internal IAuthenticationProvider? GetAuthProvider() => _authProvider;
        internal AppOptions GetOptions() => _options;
        internal T? GetService<T>() => _accessMode == ServiceAccessMode.Local && _serviceProvider != null 
            ? _serviceProvider.GetService<T>() 
            : default;
        internal IDMEEditor? GetDmeEditor() => _dmeEditor;

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _dataSourceManager?.Dispose();
                _disposed = true;
            }
        }
    }
}
