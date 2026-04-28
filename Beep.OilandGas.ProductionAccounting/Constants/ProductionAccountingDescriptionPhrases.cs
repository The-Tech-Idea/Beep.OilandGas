namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>Standard <c>JOURNAL_ENTRY_LINE.DESCRIPTION</c> text during period close GL generation.</summary>
    public static class PeriodCloseJournalLineDescriptionPhrases
    {
        public const string CloseRevenue = "Close revenue";
        public const string CloseRoyaltyExpense = "Close royalty expense";
        public const string CloseNetIncomeToRetainedEarnings = "Close net income to retained earnings";
    }

    /// <summary><c>REVENUE_ALLOCATION.DESCRIPTION</c> / <c>REVENUE_TRANSACTION.DESCRIPTION</c> for take-or-pay flows.</summary>
    public static class TakeOrPayDescriptionPhrases
    {
        public const string RevenueAllocation = "Take-or-pay allocation";
        /// <summary>One string placeholder: contract number.</summary>
        public const string DeficiencyForContractFormat = "Take-or-pay deficiency for contract {0}";
    }

    /// <summary><c>REVENUE_ALLOCATION</c> / <c>REVENUE_TRANSACTION.DESCRIPTION</c> for allocation-based revenue.</summary>
    public static class RevenueDescriptionPhrases
    {
        /// <summary>One string placeholder: entity name.</summary>
        public const string AllocationForEntityFormat = "Revenue allocation for {0}";
        /// <summary>One string placeholder: allocation result id.</summary>
        public const string FromAllocationIdFormat = "Revenue from allocation {0}";
    }

    /// <summary><c>INVENTORY_ADJUSTMENT.DESCRIPTION</c>, <c>INVENTORY_VALUATION.DESCRIPTION</c>.</summary>
    public static class InventoryDescriptionPhrases
    {
        /// <summary>Two <c>decimal</c> placeholders: prior unit cost, new unit cost.</summary>
        public const string LcmWritedownFormat = "LCM write-down from {0:0.####} to {1:0.####}";
        /// <summary>Placeholder 0: valuation method (uppercase code); 1: <c>DateTime</c> as yyyy-MM-dd.</summary>
        public const string ValuationAsOfFormat = "{0} valuation as of {1:yyyy-MM-dd}";
    }

    /// <summary><c>ACCOUNTING_COST.DESCRIPTION</c> for drilling scenario cost lines.</summary>
    public static class DrillingScenarioDescriptionPhrases
    {
        /// <summary>One string placeholder: normalized scenario token.</summary>
        public const string ScenarioCostLineFormat = "Drilling scenario: {0}";
        /// <summary>Normalized token when scenario text is missing.</summary>
        public const string UnknownScenarioToken = "UNKNOWN";
    }

    /// <summary>Fallback <c>JIB_CHARGE.DESCRIPTION</c> / <c>JIB_CREDIT.DESCRIPTION</c> when source rows omit text.</summary>
    public static class JointInterestBillingDescriptionPhrases
    {
        public const string DefaultCostChargeDescription = "Cost allocation";
        public const string DefaultRevenueCreditDescription = "Revenue allocation";
    }

    /// <summary><c>JIB_CHARGE.DESCRIPTION</c> for COPAS overhead application.</summary>
    public static class CopasOverheadDescriptionPhrases
    {
        public const string JibChargeCopasOverhead = "COPAS Overhead";
    }

    /// <summary>Fixed <c>ACCOUNTING_COST.DESCRIPTION</c> strings for unproved property workflows.</summary>
    public static class UnprovedPropertyDescriptionPhrases
    {
        public const string UnprovedAcquisition = "Unproved property acquisition";
        public const string LeaseExpiryWriteOff = "Unproved lease expiry write-off";
        public const string LeaseExpiryEventNotes = "Lease expired; unproved costs written off";
        public const string LeaseOptionBonus = "Lease option bonus";
        public const string DelayRentalExpense = "Delay rental expense";
        /// <summary>Invariant-culture format for <c>ACCOUNTING_COST.DESCRIPTION</c> when recording impairment (one <c>decimal</c> placeholder).</summary>
        public const string UnprovedImpairmentPvFormat = "Unproved property impairment (PV {0:0.##})";
        public const string ReclassifiedToProvedCostRemark = "Reclassified to proved property";
    }

    /// <summary>Fixed <c>RESERVE_DISCLOSURE_PACKAGE.DISCLOSURE_NOTES</c> text.</summary>
    public static class ReserveDisclosureNotesPhrases
    {
        public const string IfrsStylePackageFromProvedReserves =
            "IFRS-style reserve disclosure package generated from proved reserves and cashflows.";
    }

    /// <summary><c>IMBALANCE_ADJUSTMENT.REASON</c> when the service synthesizes narrative text.</summary>
    public static class ImbalanceDescriptionPhrases
    {
        /// <summary>One string placeholder: lease or property id.</summary>
        public const string ImbalanceForLeaseFormat = "Imbalance for lease {0}";
    }

    /// <summary><c>IMPAIRMENT_RECORD.REASON</c> for ASC 932 SEC full-cost ceiling outcomes.</summary>
    public static class ImpairmentRecordReasonPhrases
    {
        public const string SecCeilingTestImpairment = "SEC ceiling test impairment";
    }

    /// <summary><c>ALLOCATION_RESULT.DESCRIPTION</c> prefix for pro-rata run-ticket allocations.</summary>
    public static class AllocationDescriptionPhrases
    {
        public const string ProRataRunTicketPrefix = "Pro-rata allocation for run ticket ";
    }

    /// <summary>Standard <c>NOTES</c> on production-tax adjustments and deferred tax balances.</summary>
    public static class ProductionTaxNotesPhrases
    {
        public const string IdcDeductionFromDrillingCompletion = "IDC deduction based on drilling/completion costs";
        /// <summary>One <c>decimal</c> placeholder: tax depletion rate.</summary>
        public const string TaxDepletionAtRateFormat = "Tax depletion at rate {0:0.####}";
        public const string DeferredTaxFromDepletionTiming = "Deferred tax from depletion timing";
    }
}
