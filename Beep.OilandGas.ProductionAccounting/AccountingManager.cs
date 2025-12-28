using Beep.OilandGas.ProductionAccounting.Financial.SuccessfulEfforts;
using Beep.OilandGas.ProductionAccounting.Financial.FullCost;
using Beep.OilandGas.ProductionAccounting.Financial.Amortization;
using Beep.OilandGas.ProductionAccounting.GeneralLedger;
using Beep.OilandGas.ProductionAccounting.Invoice;
using Beep.OilandGas.ProductionAccounting.PurchaseOrder;
using Beep.OilandGas.ProductionAccounting.AccountsPayable;
using Beep.OilandGas.ProductionAccounting.AccountsReceivable;
using Beep.OilandGas.ProductionAccounting.Inventory;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting
{
    /// <summary>
    /// Main manager class for oil and gas accounting operations.
    /// Provides static methods for Financial Accounting and instance access to Traditional Accounting modules.
    /// </summary>
    public static class AccountingManager
    {
        /// <summary>
        /// Creates a new Successful Efforts accounting instance with database access.
        /// </summary>
        public static SuccessfulEffortsAccounting CreateSuccessfulEffortsAccounting(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            return new SuccessfulEffortsAccounting(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
        }

        /// <summary>
        /// Creates a new Full Cost accounting instance with database access.
        /// </summary>
        public static FullCostAccounting CreateFullCostAccounting(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            return new FullCostAccounting(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
        }

        /// <summary>
        /// Calculates amortization using units-of-production method.
        /// </summary>
        public static decimal CalculateAmortization(
            decimal netCapitalizedCosts,
            decimal totalProvedReservesBOE,
            decimal productionBOE)
        {
            return AmortizationCalculator.CalculateUnitsOfProduction(
                netCapitalizedCosts,
                totalProvedReservesBOE,
                productionBOE);
        }

        /// <summary>
        /// Calculates interest capitalization.
        /// </summary>
        public static decimal CalculateInterestCapitalization(InterestCapitalizationData data)
        {
            return InterestCapitalizationCalculator.CalculateInterestCapitalization(data);
        }

        /// <summary>
        /// Converts production to BOE.
        /// </summary>
        public static decimal ConvertProductionToBOE(ProductionData production)
        {
            return AmortizationCalculator.ConvertProductionToBOE(production);
        }

        /// <summary>
        /// Converts reserves to BOE.
        /// </summary>
        public static decimal ConvertReservesToBOE(ProvedReserves reserves)
        {
            return AmortizationCalculator.ConvertReservesToBOE(reserves);
        }
    }

    /// <summary>
    /// Unified Accounting Manager providing instance access to Traditional Accounting modules:
    /// - General Ledger
    /// - Invoice
    /// - Purchase Order
    /// - Accounts Payable
    /// - Accounts Receivable
    /// - Inventory
    /// </summary>
    public class TraditionalAccountingManager
    {
        // Traditional Accounting
        public GLAccountManager GeneralLedger { get; }
        public JournalEntryManager JournalEntry { get; }
        public InvoiceManager Invoice { get; }
        public PurchaseOrderManager PurchaseOrder { get; }
        public AccountsPayable.APManager AccountsPayable { get; }
        public AccountsReceivable.ARManager AccountsReceivable { get; }
        public InventoryTransactionManager Inventory { get; }

        public TraditionalAccountingManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            GeneralLedger = new GLAccountManager(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
            JournalEntry = new JournalEntryManager(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
            Invoice = new InvoiceManager(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
            PurchaseOrder = new PurchaseOrderManager(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
            AccountsPayable = new AccountsPayable.APManager(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
            AccountsReceivable = new AccountsReceivable.ARManager(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
            Inventory = new InventoryTransactionManager(editor, commonColumnHandler, defaults, metadata, loggerFactory, connectionName);
        }
    }
}

