using Beep.OilandGas.Client.App.Services.AccessControl;
using Beep.OilandGas.Client.App.Services.Accounting;
using Beep.OilandGas.Client.App.Services.Analysis;
using Beep.OilandGas.Client.App.Services.Calculations;
using Beep.OilandGas.Client.App.Services.Connection;
using Beep.OilandGas.Client.App.Services.DataManagement;
using Beep.OilandGas.Client.App.Services.Drilling;
using Beep.OilandGas.Client.App.Services.Field;
using Beep.OilandGas.Client.App.Services.Lease;
using Beep.OilandGas.Client.App.Services.LifeCycle;
using Beep.OilandGas.Client.App.Services.Operations;
using Beep.OilandGas.Client.App.Services.Permits;
using Beep.OilandGas.Client.App.Services.Production;
using Beep.OilandGas.Client.App.Services.Properties;
using Beep.OilandGas.Client.App.Services.Pumps;
using Beep.OilandGas.Client.App.Services.Well;

namespace Beep.OilandGas.Client.App
{
    /// <summary>
    /// Primary public interface for Beep Oil and Gas services
    /// This is the single entry point that all consumers should use
    /// </summary>
    public interface IBeepOilandGasApp
    {
        #region Core Services

        /// <summary>
        /// Connection management - switch connections, test, etc.
        /// </summary>
        IConnectionService Connection { get; }

        /// <summary>
        /// Well operations - comparison, analysis
        /// </summary>
        IWellService Well { get; }

        /// <summary>
        /// Data management operations - CRUD for PPDM39 entities
        /// </summary>
        IDataManagementService DataManagement { get; }

        #endregion

        #region Equipment Services

        /// <summary>
        /// Pumps operations - hydraulic, plunger lift, sucker rod
        /// </summary>
        IPumpsService Pumps { get; }

        /// <summary>
        /// Properties operations - oil, gas, heat map
        /// </summary>
        IPropertiesService Properties { get; }

        #endregion

        #region Calculation Services

        /// <summary>
        /// Calculations operations - flash, nodal analysis, economic
        /// </summary>
        ICalculationsService Calculations { get; }

        /// <summary>
        /// Analysis operations - compressor, pipeline, well test, gas lift, pump performance, prospect
        /// </summary>
        IAnalysisService Analysis { get; }

        #endregion

        #region Lifecycle Services

        /// <summary>
        /// Field operations (legacy)
        /// </summary>
        IFieldService Field { get; }

        /// <summary>
        /// LifeCycle operations - exploration, development, decommissioning, well/facility management, work orders
        /// </summary>
        ILifeCycleService LifeCycle { get; }

        #endregion

        #region Operations Services

        /// <summary>
        /// Operations - drilling, production, enhanced recovery
        /// </summary>
        IOperationsService Operations { get; }

        /// <summary>
        /// Drilling operations - drilling programs, BHA, mud programs, casing, cementing, enhanced recovery
        /// </summary>
        IDrillingService Drilling { get; }

        #endregion

        #region Production Services

        /// <summary>
        /// Production operations - accounting, forecasting, operations
        /// </summary>
        IProductionService Production { get; }

        /// <summary>
        /// Accounting operations - production, cost, revenue, royalty
        /// </summary>
        IAccountingService Accounting { get; }

        #endregion

        #region Administrative Services

        /// <summary>
        /// Access control operations - roles, permissions
        /// </summary>
        IAccessControlService AccessControl { get; }

        /// <summary>
        /// Permits and applications operations
        /// </summary>
        IPermitsService Permits { get; }

        /// <summary>
        /// Lease acquisition operations
        /// </summary>
        ILeaseService Lease { get; }

        #endregion

        /// <summary>
        /// Current access mode (Remote or Local)
        /// </summary>
        ServiceAccessMode AccessMode { get; }
    }
}
