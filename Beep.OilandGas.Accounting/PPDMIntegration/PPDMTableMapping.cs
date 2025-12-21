using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Accounting.PPDMIntegration;

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
        // Financial tables
        public const string Finance = "FINANCE";
        public const string Obligation = "OBLIGATION";
        public const string ObligationComponent = "OBLIGATION_COMPONENT";
        public const string ObligPayment = "OBLIG_PAYMENT";

        // Contract & Legal tables
        public const string Contract = "CONTRACT";
        public const string ContractComponent = "CONTRACT_COMPONENT";

        // Interest & Ownership tables
        public const string InterestSet = "INTEREST_SET";
        public const string IntSetPartner = "INT_SET_PARTNER";
        public const string IntSetPartnerCont = "INT_SET_PARTNER_CONT";

        // Land Rights tables
        public const string LandRight = "LAND_RIGHT";
        public const string LandRightComponent = "LAND_RIGHT_COMPONENT";
        public const string LandAgreement = "LAND_AGREEMENT";

        // Business Associate
        public const string BusinessAssociate = "BUSINESS_ASSOCIATE";
        public const string BaOrganization = "BA_ORGANIZATION";

        // Production tables
        public const string Pden = "PDEN";
        public const string PdenVolSummary = "PDEN_VOL_SUMMARY";

        // New accounting tables
        public const string AccountingMethod = "ACCOUNTING_METHOD";
        public const string AccountingCost = "ACCOUNTING_COST";
        public const string AccountingAmortization = "ACCOUNTING_AMORTIZATION";
        public const string AssetRetirementObligation = "ASSET_RETIREMENT_OBLIGATION";
        public const string DepletionCalculation = "DEPLETION_CALCULATION";
        public const string ProductionAllocation = "PRODUCTION_ALLOCATION";
        public const string RevenueTransaction = "REVENUE_TRANSACTION";
        public const string RevenueDeduction = "REVENUE_DEDUCTION";
        public const string RevenueDistribution = "REVENUE_DISTRIBUTION";
        public const string RoyaltyCalculation = "ROYALTY_CALCULATION";
        public const string JointInterestBill = "JOINT_INTEREST_BILL";
        public const string JibCostAllocation = "JIB_COST_ALLOCATION";
    }
}

