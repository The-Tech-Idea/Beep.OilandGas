using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.PPDMIntegration;

/// <summary>
/// Helper class for mapping accounting operations to PPDM tables
/// Provides table name mappings and helper methods for using PPDMGenericRepository
/// </summary>
public static class PPDMTableMapping
{
    /// <summary>
    /// Get PPDMGenericRepository for a PPDM table
    /// Use this to access existing PPDM tables (FINANCE, OBLIGATION, CONTRACT, etc.)
    /// </summary>
    public static PPDMGenericRepository GetPPDMRepository(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        Type entityType,
        string tableName,
        string connectionName = "PPDM39")
    {
        return new PPDMGenericRepository(
            editor,
            commonColumnHandler,
            defaults,
            metadata,
            entityType,
            connectionName,
            tableName,
            null);
    }

    /// <summary>
    /// PPDM table names used by accounting
    /// </summary>
    public static class TableNames
    {
        // Existing PPDM tables
        public const string Finance = "FINANCE";
        public const string Obligation = "OBLIGATION";
        public const string ObligationComponent = "OBLIGATION_COMPONENT";
        public const string ObligPayment = "OBLIG_PAYMENT";
        public const string Contract = "CONTRACT";
        public const string ContractComponent = "CONTRACT_COMPONENT";
        public const string InterestSet = "INTEREST_SET";
        public const string IntSetPartner = "INT_SET_PARTNER";
        public const string IntSetPartnerCont = "INT_SET_PARTNER_CONT";
        public const string LandRight = "LAND_RIGHT";
        public const string LandRightComponent = "LAND_RIGHT_COMPONENT";
        public const string LandAgreement = "LAND_AGREEMENT";
        public const string BusinessAssociate = "BUSINESS_ASSOCIATE";
        public const string BaOrganization = "BA_ORGANIZATION";
        public const string Pden = "PDEN";
        public const string PdenVolSummary = "PDEN_VOL_SUMMARY";
        public const string Equipment = "EQUIPMENT";
        public const string CatEquipment = "CAT_EQUIPMENT";

        // Revenue Accounting tables
        public const string RevenueTransaction = "REVENUE_TRANSACTION";
        public const string SalesContract = "SALES_CONTRACT";
        public const string PriceIndex = "PRICE_INDEX";
        public const string RevenueAllocation = "REVENUE_ALLOCATION";

        // Cost Accounting tables
        public const string CostTransaction = "COST_TRANSACTION";
        public const string CostAllocation = "COST_ALLOCATION";
        public const string AFE = "AFE";
        public const string AFELineItem = "AFE_LINE_ITEM";
        public const string CostCenter = "COST_CENTER";

        // Financial Accounting tables
        public const string AccountingMethod = "ACCOUNTING_METHOD";
        public const string AccountingCost = "ACCOUNTING_COST";
        public const string AccountingAmortization = "ACCOUNTING_AMORTIZATION";
        public const string AmortizationRecord = "AMORTIZATION_RECORD";
        public const string ImpairmentRecord = "IMPAIRMENT_RECORD";
        public const string CeilingTestCalculation = "CEILING_TEST_CALCULATION";

        // Traditional Accounting tables
        public const string GLAccount = "GL_ACCOUNT";
        public const string GLEntry = "GL_ENTRY";
        public const string JournalEntry = "JOURNAL_ENTRY";
        public const string JournalEntryLine = "JOURNAL_ENTRY_LINE";
        public const string Invoice = "INVOICE";
        public const string InvoiceLineItem = "INVOICE_LINE_ITEM";
        public const string InvoicePayment = "INVOICE_PAYMENT";
        public const string PurchaseOrder = "PURCHASE_ORDER";
        public const string POLineItem = "PO_LINE_ITEM";
        public const string POReceipt = "PO_RECEIPT";
        public const string APInvoice = "AP_INVOICE";
        public const string APPayment = "AP_PAYMENT";
        public const string APCreditMemo = "AP_CREDIT_MEMO";
        public const string ARInvoice = "AR_INVOICE";
        public const string ARPayment = "AR_PAYMENT";
        public const string ARCreditMemo = "AR_CREDIT_MEMO";
        public const string InventoryItem = "INVENTORY_ITEM";
        public const string InventoryTransaction = "INVENTORY_TRANSACTION";
        public const string InventoryAdjustment = "INVENTORY_ADJUSTMENT";
        public const string InventoryValuation = "INVENTORY_VALUATION";

        // Joint Venture tables
        public const string JointOperatingAgreement = "JOINT_OPERATING_AGREEMENT";
        public const string JOAInterest = "JOA_INTEREST";
        public const string JointInterestBill = "JOINT_INTEREST_BILL";
        public const string JOIBLineItem = "JOIB_LINE_ITEM";
        public const string JOIBAllocation = "JOIB_ALLOCATION";

        // Royalty tables
        public const string RoyaltyOwner = "ROYALTY_OWNER";
        public const string RoyaltyInterest = "ROYALTY_INTEREST";
        public const string RoyaltyCalculation = "ROYALTY_CALCULATION";
        public const string RoyaltyPayment = "ROYALTY_PAYMENT";
        public const string RoyaltyPaymentDetail = "ROYALTY_PAYMENT_DETAIL";

        // Tax tables
        public const string TaxTransaction = "TAX_TRANSACTION";
        public const string TaxReturn = "TAX_RETURN";
        public const string DepletionCalculation = "DEPLETION_CALCULATION";
    }
}

