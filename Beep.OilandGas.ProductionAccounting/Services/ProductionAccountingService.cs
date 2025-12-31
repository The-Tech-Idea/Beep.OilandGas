
using Beep.OilandGas.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Management;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Measurement;
using Beep.OilandGas.ProductionAccounting.Allocation;
using Beep.OilandGas.ProductionAccounting.Pricing;
using Beep.OilandGas.ProductionAccounting.Trading;
using Beep.OilandGas.ProductionAccounting.Ownership;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.ProductionAccounting.Royalty;
using Beep.OilandGas.ProductionAccounting.Reporting;
using Beep.OilandGas.ProductionAccounting.Imbalance;
using Beep.OilandGas.ProductionAccounting.Storage;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Financial.SuccessfulEfforts;
using Beep.OilandGas.ProductionAccounting.Financial.FullCost;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Service layer for ProductionAccounting operations
    /// </summary>
    public class ProductionAccountingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _defaultConnectionName;

        private readonly LeaseManager _leaseManager;
        private readonly ProductionManager _productionManager;
        private readonly PricingManager _pricingManager;
        private readonly TradingManager _tradingManager;
        private readonly OwnershipManager _ownershipManager;
        private readonly RoyaltyManager _royaltyManager;
        private readonly ReportManager _reportManager;
        private readonly ImbalanceManager _imbalanceManager;
        private readonly StorageManager _storageManager;
        private readonly TraditionalAccountingManager _traditionalAccounting;

        public ProductionAccountingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string defaultConnectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _defaultConnectionName = defaultConnectionName;

            // Initialize managers with data access dependencies
            _leaseManager = new LeaseManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
            _productionManager = new ProductionManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
            _pricingManager = new PricingManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
            _tradingManager = new TradingManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
            _ownershipManager = new OwnershipManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
            _royaltyManager = new RoyaltyManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
            _reportManager = new ReportManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
            _imbalanceManager = new ImbalanceManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
            _storageManager = new StorageManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
            _traditionalAccounting = new TraditionalAccountingManager(editor, commonColumnHandler, defaults, metadata, null, defaultConnectionName);
        }

        /// <summary>
        /// Gets a repository instance for the specified entity type
        /// </summary>
        public PPDMGenericRepository GetRepository(Type entityType, string connectionName, string? tableName = null)
        {
            var connName = connectionName ?? _defaultConnectionName;
            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                entityType,
                connName,
                tableName,
                _loggerFactory.CreateLogger<PPDMGenericRepository>());
        }

        /// <summary>
        /// Gets the default connection name
        /// </summary>
        public string DefaultConnectionName => _defaultConnectionName;

        // Lease Management
        public LeaseManager LeaseManager => _leaseManager;
        public ProductionManager ProductionManager => _productionManager;
        public PricingManager PricingManager => _pricingManager;
        public TradingManager TradingManager => _tradingManager;
        public OwnershipManager OwnershipManager => _ownershipManager;
        public RoyaltyManager RoyaltyManager => _royaltyManager;
        public ReportManager ReportManager => _reportManager;
        public ImbalanceManager ImbalanceManager => _imbalanceManager;
        public StorageManager StorageManager => _storageManager;
        public TraditionalAccountingManager TraditionalAccounting => _traditionalAccounting;

        // Financial Accounting (static methods via AccountingManager)
        public SuccessfulEffortsAccounting CreateSuccessfulEffortsAccounting(string? connectionName = null)
        {
            var connName = connectionName ?? _defaultConnectionName;
            return AccountingManager.CreateSuccessfulEffortsAccounting(
                _editor, _commonColumnHandler, _defaults, _metadata, _loggerFactory, connName);
        }

        public FullCostAccounting CreateFullCostAccounting(string? connectionName = null)
        {
            var connName = connectionName ?? _defaultConnectionName;
            return AccountingManager.CreateFullCostAccounting(
                _editor, _commonColumnHandler, _defaults, _metadata, _loggerFactory, connName);
        }
        public static decimal CalculateAmortization(decimal netCapitalizedCosts, decimal totalProvedReservesBOE, decimal productionBOE) 
            => AccountingManager.CalculateAmortization(netCapitalizedCosts, totalProvedReservesBOE, productionBOE);
        public static decimal CalculateInterestCapitalization(InterestCapitalizationData data) 
            => AccountingManager.CalculateInterestCapitalization(data);
        public static decimal ConvertProductionToBOE(ProductionData production) 
            => AccountingManager.ConvertProductionToBOE(production);
        public static decimal ConvertReservesToBOE(ProvedReserves reserves) 
            => AccountingManager.ConvertReservesToBOE(reserves);
    }
}
