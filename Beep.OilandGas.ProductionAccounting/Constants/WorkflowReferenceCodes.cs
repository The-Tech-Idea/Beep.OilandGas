namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Run ticket, period close, volume UOM, and revenue product codes aligned with
    /// <see cref="ProductionAccountingReferenceCodeSeed"/> REFERENCE_SET rows.
    /// </summary>
    public static class RunTicketStatusCodes
    {
        public const string Open = "OPEN";
        public const string Certified = "CERTIFIED";
        public const string Voided = "VOIDED";
        public const string Adjusted = "ADJUSTED";
    }

    public static class RunTicketDispositionCodes
    {
        public const string Sale = "SALE";
        public const string Transfer = "TRANSFER";
        public const string Flare = "FLARE";
        public const string FuelUse = "FUEL_USE";
        public const string Loss = "LOSS";
    }

    public static class PeriodCloseStatusCodes
    {
        public const string Open = "OPEN";
        public const string Closing = "CLOSING";
        public const string Closed = "CLOSED";
        public const string Reopened = "REOPENED";
    }

    public static class VolumeOuomCodes
    {
        public const string Bbl = "BBL";
        public const string Gal = "GAL";
        public const string Mcf = "MCF";
    }

    public static class RevenueLineProductCodes
    {
        public const string Oil = "OIL";
        public const string Gas = "GAS";
    }

    public static class RevenueTypeCodes
    {
        public const string Revenue = "REVENUE";
        public const string TakeOrPay = "TAKE_OR_PAY";
    }

    /// <summary>Key in <c>AMORTIZATION_RECORD.REMARK</c> for field-scoped rollforward tagging.</summary>
    public static class AmortizationRecordRemarkKeys
    {
        public const string FieldId = "FIELD_ID";
    }

    /// <summary>Keys in <c>OBLIGATION_DESCRIPTION</c> / <c>REMARK</c> for take-or-pay schedule fallback parsing.</summary>
    public static class TakeOrPayObligationParseKeys
    {
        public const string MinVolume = "MIN_VOLUME";
        public const string MinQty = "MIN_QTY";
        public const string Price = "PRICE";
    }

    /// <summary>Fallback <c>PRICE_INDEX.PRICE_VALUE</c> when null or when royalty flows lack an index row.</summary>
    public static class CommodityPricingFallbackDefaults
    {
        public const decimal DefaultUnitPriceWhenIndexMissing = 75.00m;
    }

    /// <summary>ISO 4217 functional / reporting currency defaults.</summary>
    public static class AccountingCurrencyCodes
    {
        public const string Usd = "USD";
    }

    /// <summary>IFRS 9 <c>FINANCIAL_INSTRUMENT.INSTRUMENT_TYPE</c>; seeded as <c>FINANCIAL_INSTRUMENT_TYPE</c>.</summary>
    public static class FinancialInstrumentTypeCodes
    {
        public const string Derivative = "DERIVATIVE";
        public const string CommodityContract = "COMMODITY_CONTRACT";
        public const string DebtInstrument = "DEBT_INSTRUMENT";
    }

    /// <summary>IFRS 9 <c>FINANCIAL_INSTRUMENT.STATUS</c>; seeded as <c>FINANCIAL_INSTRUMENT_STATUS</c>.</summary>
    public static class FinancialInstrumentStatusCodes
    {
        public const string Active = "ACTIVE";
        public const string Inactive = "INACTIVE";
    }

    /// <summary>IFRS 9 measurement categories (FVPL, FVOCI, amortized cost) for disclosure and future persisted classification fields.</summary>
    public static class FinancialInstrumentMeasurementCodes
    {
        public const string FairValueThroughProfitOrLoss = "FVPL";
        public const string FairValueThroughOtherComprehensiveIncome = "FVOCI";
        public const string AmortizedCost = "AMORTIZED_COST";
    }

    /// <summary><c>ROYALTY_DISPUTE.STATUS</c> and related dispute lifecycle.</summary>
    public static class RoyaltyDisputeStatusCodes
    {
        public const string Open = "OPEN";
        public const string Resolved = "RESOLVED";
    }

    /// <summary>User-facing validation text for royalty dispute operations.</summary>
    public static class RoyaltyDisputeServiceExceptionMessages
    {
        public const string RoyaltyStatementIdRequired = "ROYALTY_STATEMENT_ID is required";
    }

    /// <summary>Invariant <c>string.Format</c> when a dispute row is missing (placeholder 0: dispute id).</summary>
    public static class RoyaltyDisputeMessageFormats
    {
        public const string DisputeNotFoundFormat = "Dispute not found: {0}";
    }

    /// <summary>Lease carrying group, lease option, and similar lease-admin <c>STATUS</c> values.</summary>
    public static class LeaseLifecycleStatusCodes
    {
        public const string Active = "ACTIVE";
    }

    /// <summary>Imbalance settlement, emissions settlement, and other volume/cash settlement outcomes.</summary>
    public static class SettlementOutcomeCodes
    {
        public const string Settled = "SETTLED";
    }

    /// <summary><c>IMBALANCE_ADJUSTMENT.ADJUSTMENT_TYPE</c> for lease-level over/under production.</summary>
    public static class ImbalanceAdjustmentTypeCodes
    {
        public const string Overproduced = "OVER-PRODUCED";
        public const string Underproduced = "UNDER-PRODUCED";

        /// <summary>All values emitted to <c>R_PRODUCTION_ACCOUNTING_REFERENCE_CODE</c> for <c>IMBALANCE_ADJUSTMENT_TYPE</c>.</summary>
        public static readonly string[] AllSeeded = { Overproduced, Underproduced };
    }

    /// <summary><c>EMISSIONS_OBLIGATION.STATUS</c> while liability is open.</summary>
    public static class EmissionsObligationStatusCodes
    {
        public const string Open = "OPEN";
    }

    /// <summary>IAS 37 <c>ASSET_RETIREMENT_OBLIGATION.STATUS</c>; seeded as <c>ASSET_RETIREMENT_OBLIGATION_STATUS</c>.</summary>
    public static class AssetRetirementObligationStatusCodes
    {
        public const string Active = "ACTIVE";
        public const string Closed = "CLOSED";
    }

    /// <summary>
    /// ASC 606 contract performance obligation lifecycle (take-or-pay, etc.); seeded as <c>CONTRACT_OBLIGATION_STATUS</c>.
    /// </summary>
    public static class ContractPerformanceStatusCodes
    {
        public const string Open = "OPEN";
        public const string Satisfied = "SATISFIED";
        public const string PartiallySatisfied = "PARTIALLY_SATISFIED";
    }

    /// <summary>Legacy default persisted when ticket omits method; aligns with <c>MEASUREMENT_METHOD</c> seed (alongside enum names).</summary>
    public static class LegacyMeasurementMethodCodes
    {
        public const string Automated = "AUTOMATED";

        /// <summary>Constants-backed codes for <c>MEASUREMENT_METHOD</c> (see also enum-backed rows in <see cref="ProductionAccountingReferenceCodeSeed"/>).</summary>
        public static readonly string[] AllSeeded = { Automated };
    }

    /// <summary>Strings written to <c>OPERATIONAL_REPORT</c> / <c>FINANCIAL_REPORT</c> and reporting service results.</summary>
    public static class GeneratedReportTypeCodes
    {
        public const string Operational = "OPERATIONAL";
        public const string Financial = "FINANCIAL";
        public const string Owner = "OWNER";
        public const string Tax = "TAX";
        public const string RoyaltyStatement = "ROYALTY_STATEMENT";
        public const string JibStatement = "JIB_STATEMENT";

        /// <summary><c>LEASE_REPORT.REPORT_TYPE</c> (aligned with operational report token style).</summary>
        public const string Lease = "LEASE";
    }

    /// <summary><c>JIB_CHARGE.CATEGORY</c> for COPAS and similar overhead lines.</summary>
    public static class JibChargeCategoryCodes
    {
        public const string Overhead = "OVERHEAD";
    }

    /// <summary>Standard narratives written to <c>COPAS_OVERHEAD_AUDIT.CHANGE_REASON</c>.</summary>
    public static class CopasOverheadAuditChangeReasons
    {
        public const string AppliedOverheadSchedule = "Applied overhead schedule";
    }

    /// <summary><c>INTERNAL_CONTROL_RULE.RULE_TYPE</c> for SoD and related checks.</summary>
    public static class InternalControlRuleTypeCodes
    {
        public const string SegregationOfDuties = "SEGREGATION_OF_DUTIES";
    }

    public static class ReportScheduleStatusCodes
    {
        public const string Scheduled = "SCHEDULED";
    }

    public static class ReportScheduleFrequencyCodes
    {
        public const string Weekly = "WEEKLY";
        public const string Monthly = "MONTHLY";
        public const string Daily = "DAILY";
    }

    /// <summary><c>JOINT_INTEREST_STATEMENT.REPORT_TYPE</c> and similar.</summary>
    public static class JointInterestStatementReportTypeCodes
    {
        public const string Jib = "JIB";
    }

    /// <summary><c>ALLOCATION_DETAIL.ENTITY_TYPE</c> for working-interest owner lines.</summary>
    public static class AllocationEntityTypeCodes
    {
        public const string Owner = "OWNER";
    }

    /// <summary><c>LEASE_EXPIRY_EVENT.ACTION_TAKEN</c> and similar disposition codes.</summary>
    public static class LeaseExpiryActionCodes
    {
        public const string WriteOff = "WRITE_OFF";
    }

    /// <summary>Line-level production / severance tax buckets written to tax transactions.</summary>
    public static class ProductionTaxAssessmentCodes
    {
        public const string Severance = "SEVERANCE";
        public const string AdValorem = "AD_VALOREM";
        public const string TotalProductionTax = "TOTAL_PRODUCTION_TAX";
    }

    /// <summary><c>COST_VARIANCE_REPORT.STATUS</c> after budget vs actual comparison.</summary>
    public static class CostVarianceReportStatusCodes
    {
        public const string ThresholdExceeded = "THRESHOLD_EXCEEDED";
        public const string Ok = "OK";
    }

    /// <summary><c>TAX_ADJUSTMENT.ADJUSTMENT_TYPE</c> for IDC and tax-book depletion.</summary>
    public static class TaxAdjustmentTypeCodes
    {
        public const string IdcDeduction = "IDC_DEDUCTION";
        public const string TaxDepletion = "TAX_DEPLETION";
    }

    /// <summary>Keys parsed from <c>PRODUCTION_TAX_DATA.REMARK</c> for rate extraction.</summary>
    public static class ProductionTaxRemarkKeys
    {
        public const string TaxDepletionRate = "TAX_DEPLETION_RATE";
        public const string DeferredTaxRate = "DEFERRED_TAX_RATE";
        public const string PropertyId = "PROPERTY_ID";
        public const string Jurisdiction = "JURISDICTION";
    }

    /// <summary><c>IMPAIRMENT_RECORD.IMPAIRMENT_TYPE</c> framework markers.</summary>
    public static class ImpairmentRecordTypeCodes
    {
        public const string Ias36 = "IAS36";
        public const string CeilingTest = "CEILING_TEST";

        /// <summary>All values emitted to <c>R_PRODUCTION_ACCOUNTING_REFERENCE_CODE</c> for <c>IMPAIRMENT_RECORD_TYPE</c>.</summary>
        public static readonly string[] AllSeeded = { Ias36, CeilingTest };
    }

    /// <summary><c>IMPAIRMENT_RECORD.REASON</c> evaluation outcomes (seed <c>IMPAIRMENT_EVALUATION_REASON</c>).</summary>
    public static class ImpairmentEvaluationReasonCodes
    {
        public const string RecoverableBelowCarrying = "RECOVERABLE_BELOW_CARRYING";
        public const string NoImpairment = "NO_IMPAIRMENT";

        /// <summary>All values emitted for <c>IMPAIRMENT_EVALUATION_REASON</c> in <see cref="ProductionAccountingReferenceCodeSeed"/>.</summary>
        public static readonly string[] AllSeeded = { RecoverableBelowCarrying, NoImpairment };
    }
}
