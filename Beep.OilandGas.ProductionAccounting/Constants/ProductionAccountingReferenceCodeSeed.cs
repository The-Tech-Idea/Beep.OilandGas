using System;
using System.Collections.Generic;
using System.Text;
using DataPa = Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// Canonical catalog of <c>R_PRODUCTION_ACCOUNTING_REFERENCE_CODE</c> rows.
    /// Keep in sync with <see cref="AllocationMethods"/>, <see cref="AllocationStatus"/>, <see cref="AfeStatusCodes"/>,
    /// <see cref="FinancialInstrumentTypeCodes"/>, <see cref="FinancialInstrumentStatusCodes"/>, <see cref="FinancialInstrumentMeasurementCodes"/>, and
    /// projection enums under <c>Models.Data.ProductionAccounting</c>; module setup seeds these idempotently.
    /// </summary>
    public static class ProductionAccountingReferenceCodeSeed
    {
        public const string SourceApplication = "APPLICATION";

        /// <summary>
        /// Yields (REFERENCE_SET, REFERENCE_CODE, LONG_NAME) for module seeding.
        /// </summary>
        public static IEnumerable<(string ReferenceSet, string ReferenceCode, string LongName)> GetAllSeedRows()
        {
            foreach (var row in GetRunTicketAndWorkflowRows())
                yield return row;

            foreach (var row in GetConstantsBackedRows())
                yield return row;

            foreach (var row in GetEnumBackedRows())
                yield return row;
        }

        private static IEnumerable<(string, string, string)> GetRunTicketAndWorkflowRows()
        {
            yield return ("RUN_TICKET_STATUS", RunTicketStatusCodes.Open, "Open");
            yield return ("RUN_TICKET_STATUS", RunTicketStatusCodes.Certified, "Certified");
            yield return ("RUN_TICKET_STATUS", RunTicketStatusCodes.Voided, "Voided");
            yield return ("RUN_TICKET_STATUS", RunTicketStatusCodes.Adjusted, "Adjusted");

            yield return ("RUN_TICKET_DISPOSITION", RunTicketDispositionCodes.Sale, "Sale");
            yield return ("RUN_TICKET_DISPOSITION", RunTicketDispositionCodes.Transfer, "Transfer");
            yield return ("RUN_TICKET_DISPOSITION", RunTicketDispositionCodes.Flare, "Flare");
            yield return ("RUN_TICKET_DISPOSITION", RunTicketDispositionCodes.FuelUse, "Fuel use");
            yield return ("RUN_TICKET_DISPOSITION", RunTicketDispositionCodes.Loss, "Loss");

            yield return ("VOLUME_OUOM", VolumeOuomCodes.Bbl, "Barrels");
            yield return ("VOLUME_OUOM", VolumeOuomCodes.Gal, "Gallons");
            yield return ("VOLUME_OUOM", VolumeOuomCodes.Mcf, "Thousand cubic feet");

            yield return ("PERIOD_CLOSE_STATUS", PeriodCloseStatusCodes.Open, "Open");
            yield return ("PERIOD_CLOSE_STATUS", PeriodCloseStatusCodes.Closing, "Closing in progress");
            yield return ("PERIOD_CLOSE_STATUS", PeriodCloseStatusCodes.Closed, "Closed");
            yield return ("PERIOD_CLOSE_STATUS", PeriodCloseStatusCodes.Reopened, "Reopened");

            yield return ("JOURNAL_ENTRY_TYPE", JournalEntryTypeCodes.PeriodClose, "Period close");
            yield return ("JOURNAL_ENTRY_TYPE", JournalEntryTypeCodes.ProductionCycle, "Production cycle");
            yield return ("JOURNAL_ENTRY_TYPE", JournalEntryTypeCodes.Draft, "Draft");
        }

        private static IEnumerable<(string, string, string)> GetConstantsBackedRows()
        {
            foreach (var m in AllocationMethods.AllMethods)
                yield return ("ALLOCATION_METHOD", m, HumanizeIdentifier(m));

            // Revenue-only code; not in <see cref="AllocationMethods.AllMethods"/> (production engine path).
            yield return ("ALLOCATION_METHOD", AllocationMethods.CustomRevenue, "Custom revenue allocation");

            yield return ("ALLOCATION_STATUS", AllocationStatus.Pending, "Allocation pending");
            yield return ("ALLOCATION_STATUS", AllocationStatus.Allocated, "Allocated");
            yield return ("ALLOCATION_STATUS", AllocationStatus.Reconciled, "Reconciled");
            yield return ("ALLOCATION_STATUS", AllocationStatus.Reversed, "Reversed");
            yield return ("ALLOCATION_STATUS", AllocationStatus.Rejected, "Rejected");

            yield return ("ACCOUNTING_METHOD", AccountingMethods.SuccessfulEfforts, "Successful efforts");
            yield return ("ACCOUNTING_METHOD", AccountingMethods.FullCost, "Full cost");

            yield return ("COST_TYPE", CostTypes.Exploration, "Exploration cost");
            yield return ("COST_TYPE", CostTypes.Development, "Development cost");
            yield return ("COST_TYPE", CostTypes.Acquisition, "Acquisition cost");
            yield return ("COST_TYPE", CostTypes.Production, "Production cost");
            yield return ("COST_TYPE", CostTypes.Impairment, "Impairment");
            yield return ("COST_TYPE", CostTypes.ExpiryWriteOff, "Lease expiry write-off");

            yield return ("COST_TYPE", RoyaltyDeductionCostTypeCodes.Transportation, "Transportation (royalty deduction)");
            yield return ("COST_TYPE", RoyaltyDeductionCostTypeCodes.AdValorem, "Ad valorem tax (royalty deduction)");
            yield return ("COST_TYPE", RoyaltyDeductionCostTypeCodes.Severance, "Severance tax (royalty deduction)");

            yield return ("COST_CATEGORY", CostCategories.Drilling, "Drilling");
            yield return ("COST_CATEGORY", CostCategories.Sidetrack, "Sidetrack");
            yield return ("COST_CATEGORY", CostCategories.PlugBack, "Plug back");
            yield return ("COST_CATEGORY", CostCategories.TestWell, "Test well");
            yield return ("COST_CATEGORY", CostCategories.Salvage, "Salvage");
            yield return ("COST_CATEGORY", CostCategories.Completion, "Completion");
            yield return ("COST_CATEGORY", CostCategories.Equipment, "Equipment");
            yield return ("COST_CATEGORY", CostCategories.Lease, "Lease");
            yield return ("COST_CATEGORY", CostCategories.DelayRental, "Delay rental");
            yield return ("COST_CATEGORY", CostCategories.LeaseOption, "Lease option");
            yield return ("COST_CATEGORY", CostCategories.Seismic, "Seismic");
            yield return ("COST_CATEGORY", CostCategories.Administrative, "Administrative");
            yield return ("COST_CATEGORY", CostCategories.Transportation, "Transportation");
            yield return ("COST_CATEGORY", CostCategories.QualityAdjustment, "Quality adjustment");
            yield return ("COST_CATEGORY", CostCategories.Tax, "Tax");
            yield return ("COST_CATEGORY", CostCategories.ExplorationEvaluation, "Exploration and evaluation");
            yield return ("COST_CATEGORY", CostCategories.BorrowingCost, "Borrowing cost");

            yield return ("ROYALTY_TYPE", RoyaltyTypes.Mineral, "Mineral royalty");
            yield return ("ROYALTY_TYPE", RoyaltyTypes.OverridingRoyalty, "Overriding royalty");
            yield return ("ROYALTY_TYPE", RoyaltyTypes.NetProfitInterest, "Net profit interest");
            yield return ("ROYALTY_TYPE", RoyaltyTypes.Reversionary, "Reversionary interest");
            yield return ("ROYALTY_TYPE", RoyaltyTypes.BackIn, "Back-in interest");

            yield return ("ROYALTY_STATUS", RoyaltyStatus.Calculated, "Royalty calculated");
            yield return ("ROYALTY_STATUS", RoyaltyStatus.Accrued, "Royalty accrued");
            yield return ("ROYALTY_STATUS", RoyaltyStatus.Paid, "Royalty paid");
            yield return ("ROYALTY_STATUS", RoyaltyStatus.Reversed, "Royalty reversed");
            yield return ("ROYALTY_STATUS", RoyaltyStatus.Disputed, "Royalty disputed");
            yield return ("ROYALTY_STATUS", RoyaltyStatus.Pending, "Royalty pending");

            yield return ("ROYALTY_PAYMENT_STATUS", RoyaltyPaymentStatusCodes.Pending, "Payment pending");
            yield return ("ROYALTY_PAYMENT_STATUS", RoyaltyPaymentStatusCodes.Approved, "Payment approved");
            yield return ("ROYALTY_PAYMENT_STATUS", RoyaltyPaymentStatusCodes.Paid, "Payment paid");
            yield return ("ROYALTY_PAYMENT_STATUS", RoyaltyPaymentStatusCodes.OnHold, "Payment on hold");
            yield return ("ROYALTY_PAYMENT_STATUS", RoyaltyPaymentStatusCodes.Disputed, "Payment disputed");

            yield return ("REVENUE_RECOGNITION_STATUS", RevenueRecognitionStatusCodes.Deferred, "Deferred revenue");
            yield return ("REVENUE_RECOGNITION_STATUS", RevenueRecognitionStatusCodes.Recognized, "Revenue recognized");
            yield return ("REVENUE_RECOGNITION_STATUS", RevenueRecognitionStatusCodes.Billed, "Revenue billed");
            yield return ("REVENUE_RECOGNITION_STATUS", RevenueRecognitionStatusCodes.Collected, "Revenue collected");

            yield return ("AMORTIZATION_METHOD", AmortizationMethods.UnitOfProduction, "Unit of production");
            yield return ("AMORTIZATION_METHOD", AmortizationMethods.StraightLine, "Straight line");
            yield return ("AMORTIZATION_METHOD", AmortizationMethods.DoubleDeclining, "Double declining");

            yield return ("REVENUE_LINE_PRODUCT", RevenueLineProductCodes.Oil, "Oil");
            yield return ("REVENUE_LINE_PRODUCT", RevenueLineProductCodes.Gas, "Gas");
            yield return ("REVENUE_TYPE", RevenueTypeCodes.Revenue, "Revenue");
            yield return ("REVENUE_TYPE", RevenueTypeCodes.TakeOrPay, "Take or pay adjustment");
            yield return ("FUNCTIONAL_CURRENCY_CODE", AccountingCurrencyCodes.Usd, "US Dollar");

            yield return ("FINANCIAL_INSTRUMENT_TYPE", FinancialInstrumentTypeCodes.Derivative, "Derivative");
            yield return ("FINANCIAL_INSTRUMENT_TYPE", FinancialInstrumentTypeCodes.CommodityContract, "Commodity contract");
            yield return ("FINANCIAL_INSTRUMENT_TYPE", FinancialInstrumentTypeCodes.DebtInstrument, "Debt instrument");

            yield return ("FINANCIAL_INSTRUMENT_STATUS", FinancialInstrumentStatusCodes.Active, "Active");
            yield return ("FINANCIAL_INSTRUMENT_STATUS", FinancialInstrumentStatusCodes.Inactive, "Inactive");

            yield return ("IFRS9_MEASUREMENT_CATEGORY", FinancialInstrumentMeasurementCodes.FairValueThroughProfitOrLoss, "Fair value through profit or loss");
            yield return ("IFRS9_MEASUREMENT_CATEGORY", FinancialInstrumentMeasurementCodes.FairValueThroughOtherComprehensiveIncome, "Fair value through OCI");
            yield return ("IFRS9_MEASUREMENT_CATEGORY", FinancialInstrumentMeasurementCodes.AmortizedCost, "Amortized cost");

            yield return ("MEASUREMENT_METHOD", LegacyMeasurementMethodCodes.Automated, "Automated (legacy)");

            yield return ("REPORT_SERVICE_TYPE", GeneratedReportTypeCodes.Operational, "Operational report");
            yield return ("REPORT_SERVICE_TYPE", GeneratedReportTypeCodes.Financial, "Financial report");
            yield return ("REPORT_SERVICE_TYPE", GeneratedReportTypeCodes.Owner, "Owner financial variant");
            yield return ("REPORT_SERVICE_TYPE", GeneratedReportTypeCodes.Tax, "Tax financial variant");
            yield return ("REPORT_SERVICE_TYPE", GeneratedReportTypeCodes.RoyaltyStatement, "Royalty statement");
            yield return ("REPORT_SERVICE_TYPE", GeneratedReportTypeCodes.JibStatement, "JIB statement");
            yield return ("REPORT_SERVICE_TYPE", GeneratedReportTypeCodes.Lease, "Lease report");
            yield return ("JIB_CHARGE_CATEGORY", JibChargeCategoryCodes.Overhead, "COPAS overhead");
            yield return ("REPORT_SCHEDULE_STATUS", ReportScheduleStatusCodes.Scheduled, "Scheduled");
            yield return ("REPORT_SCHEDULE_FREQUENCY", ReportScheduleFrequencyCodes.Weekly, "Weekly");
            yield return ("REPORT_SCHEDULE_FREQUENCY", ReportScheduleFrequencyCodes.Monthly, "Monthly");
            yield return ("REPORT_SCHEDULE_FREQUENCY", ReportScheduleFrequencyCodes.Daily, "Daily");
            yield return ("JOINT_INTEREST_REPORT_TYPE", JointInterestStatementReportTypeCodes.Jib, "Joint interest billing");
            yield return ("ALLOCATION_DETAIL_ENTITY_TYPE", AllocationEntityTypeCodes.Owner, "Owner / interest entity");

            yield return ("JOURNAL_ENTRY_STATUS", JournalEntryStatusCodes.Posted, "Posted");
            yield return ("DOCUMENT_WORKFLOW_STATUS", DocumentWorkflowStatusCodes.Draft, "Draft");
            yield return ("ROYALTY_STATEMENT_STATUS", RoyaltyStatementStatusCodes.Generated, "Generated");

            yield return ("ACCOUNTING_SOURCE_MODULE", AccountingSourceModuleCodes.PeriodClosing, "Period closing");
            yield return ("ACCOUNTING_SOURCE_MODULE", AccountingSourceModuleCodes.AssetSwap, "Asset swap / farm-in farm-out");
            yield return ("APPROVAL_WORKFLOW_STATUS", ApprovalWorkflowStatusCodes.Pending, "Pending approval");
            yield return ("APPROVAL_WORKFLOW_STATUS", ApprovalWorkflowStatusCodes.Approved, "Approved");

            yield return ("AFE_STATUS", AfeStatusCodes.Draft, "Draft (AFE)");
            yield return ("AFE_STATUS", AfeStatusCodes.Approved, "Approved (AFE)");

            yield return ("INTERNAL_CONTROL_RULE_TYPE", InternalControlRuleTypeCodes.SegregationOfDuties, "Segregation of duties");

            yield return ("ROYALTY_DISPUTE_STATUS", RoyaltyDisputeStatusCodes.Open, "Open");
            yield return ("ROYALTY_DISPUTE_STATUS", RoyaltyDisputeStatusCodes.Resolved, "Resolved");
            yield return ("LEASE_LIFECYCLE_STATUS", LeaseLifecycleStatusCodes.Active, "Active");
            yield return ("DELAY_RENTAL_STATUS", RoyaltyPaymentStatusCodes.Paid, "Paid");
            yield return ("EMISSIONS_OBLIGATION_STATUS", EmissionsObligationStatusCodes.Open, "Open");
            yield return ("EMISSIONS_SETTLEMENT_STATUS", SettlementOutcomeCodes.Settled, "Settled");
            yield return ("CONTRACT_OBLIGATION_STATUS", ContractPerformanceStatusCodes.Open, "Open");
            yield return ("CONTRACT_OBLIGATION_STATUS", ContractPerformanceStatusCodes.Satisfied, "Satisfied");
            yield return ("CONTRACT_OBLIGATION_STATUS", ContractPerformanceStatusCodes.PartiallySatisfied, "Partially satisfied");
            yield return ("ASSET_RETIREMENT_OBLIGATION_STATUS", AssetRetirementObligationStatusCodes.Active, "ARO active");
            yield return ("ASSET_RETIREMENT_OBLIGATION_STATUS", AssetRetirementObligationStatusCodes.Closed, "ARO closed");
            yield return ("IMBALANCE_SETTLEMENT_STATUS", SettlementOutcomeCodes.Settled, "Settled");
            yield return ("IMBALANCE_ADJUSTMENT_TYPE", ImbalanceAdjustmentTypeCodes.Overproduced, "Over-produced");
            yield return ("IMBALANCE_ADJUSTMENT_TYPE", ImbalanceAdjustmentTypeCodes.Underproduced, "Under-produced");

            yield return ("LEASE_EXPIRY_ACTION", LeaseExpiryActionCodes.WriteOff, "Write off");
            yield return ("PRODUCTION_TAX_ASSESSMENT_TYPE", ProductionTaxAssessmentCodes.Severance, "Severance tax");
            yield return ("PRODUCTION_TAX_ASSESSMENT_TYPE", ProductionTaxAssessmentCodes.AdValorem, "Ad valorem tax");
            yield return ("PRODUCTION_TAX_ASSESSMENT_TYPE", ProductionTaxAssessmentCodes.TotalProductionTax, "Total production tax");

            yield return ("TAX_ADJUSTMENT_TYPE", TaxAdjustmentTypeCodes.IdcDeduction, "IDC deduction");
            yield return ("TAX_ADJUSTMENT_TYPE", TaxAdjustmentTypeCodes.TaxDepletion, "Tax depletion");

            yield return ("IMPAIRMENT_RECORD_TYPE", ImpairmentRecordTypeCodes.Ias36, "IAS 36");
            yield return ("IMPAIRMENT_RECORD_TYPE", ImpairmentRecordTypeCodes.CeilingTest, "Ceiling test");
            yield return ("IMPAIRMENT_EVALUATION_REASON", ImpairmentEvaluationReasonCodes.RecoverableBelowCarrying, "Recoverable below carrying");
            yield return ("IMPAIRMENT_EVALUATION_REASON", ImpairmentEvaluationReasonCodes.NoImpairment, "No impairment");

            yield return ("INVENTORY_TRANSACTION_TYPE", InventoryTransactionTypeCodes.Receipt, "Receipt");
            yield return ("INVENTORY_TRANSACTION_TYPE", InventoryTransactionTypeCodes.Purchase, "Purchase");
            yield return ("INVENTORY_TRANSACTION_TYPE", InventoryTransactionTypeCodes.In, "Inbound transfer");
            yield return ("INVENTORY_TRANSACTION_TYPE", InventoryTransactionTypeCodes.Issue, "Issue");
            yield return ("INVENTORY_TRANSACTION_TYPE", InventoryTransactionTypeCodes.Sale, "Sale");
            yield return ("INVENTORY_TRANSACTION_TYPE", InventoryTransactionTypeCodes.Out, "Outbound transfer");

            yield return ("INVENTORY_VALUATION_METHOD", InventoryValuationMethodCodes.WeightedAvg, "Weighted average cost");
            yield return ("INVENTORY_VALUATION_METHOD", InventoryValuationMethodCodes.Lifo, "Last-in first-out");

            yield return ("INVENTORY_ADJUSTMENT_TYPE", InventoryAdjustmentTypeCodes.LcmWritedown, "Lower of cost or market write-down");

            yield return ("PRICE_INDEX_COMMODITY_TYPE", PriceIndexCommodityTypeCodes.Oil, "Crude oil");
            yield return ("PRICE_INDEX_COMMODITY_TYPE", PriceIndexCommodityTypeCodes.Gas, "Natural gas");
            yield return ("PRICE_INDEX_COMMODITY_TYPE", PriceIndexCommodityTypeCodes.Ngl, "Natural gas liquids");

            yield return ("COST_VARIANCE_REPORT_STATUS", CostVarianceReportStatusCodes.ThresholdExceeded, "Threshold exceeded");
            yield return ("COST_VARIANCE_REPORT_STATUS", CostVarianceReportStatusCodes.Ok, "Within threshold");
        }

        private static IEnumerable<(string, string, string)> GetEnumBackedRows()
        {
            foreach (var row in EnumRows<DataPa.DispositionType>("DISPOSITION_TYPE_ENUM"))
                yield return row;
            foreach (var row in EnumRows<DataPa.AllocationMethod>("ALLOCATION_METHOD_ENUM"))
                yield return row;
            foreach (var row in EnumRows<DataPa.PaymentStatus>("PAYMENT_STATUS"))
                yield return row;
            foreach (var row in EnumRows<DataPa.ReportType>("REPORT_TYPE"))
                yield return row;
            foreach (var row in EnumRows<DataPa.PricingMethod>("PRICING_METHOD"))
                yield return row;
            foreach (var row in EnumRows<DataPa.MeasurementMethod>("MEASUREMENT_METHOD"))
                yield return row;
            foreach (var row in EnumRows<DataPa.MeasurementStandard>("MEASUREMENT_STANDARD"))
                yield return row;
            foreach (var row in EnumRows<DataPa.NominationStatus>("NOMINATION_STATUS"))
                yield return row;
            foreach (var row in EnumRows<DataPa.ImbalanceStatus>("IMBALANCE_STATUS"))
                yield return row;
            foreach (var row in EnumRows<DataPa.DivisionOrderStatus>("DIVISION_ORDER_STATUS"))
                yield return row;
            foreach (var row in EnumRows<DataPa.ExchangeCommitmentStatus>("EXCHANGE_COMMITMENT_STATUS"))
                yield return row;
            foreach (var row in EnumRows<DataPa.ExchangeCommitmentType>("EXCHANGE_COMMITMENT_TYPE"))
                yield return row;
            foreach (var row in EnumRows<DataPa.LeaseType>("LEASE_TYPE"))
                yield return row;
            foreach (var row in EnumRows<DataPa.TaxWithholdingType>("TAX_WITHHOLDING_TYPE"))
                yield return row;
            foreach (var row in EnumRows<DataPa.PaymentMethod>("PAYMENT_METHOD"))
                yield return row;
            foreach (var row in EnumRows<DataPa.CrudeOilType>("CRUDE_OIL_TYPE"))
                yield return row;
        }

        private static IEnumerable<(string ReferenceSet, string ReferenceCode, string LongName)> EnumRows<TEnum>(string referenceSet)
            where TEnum : struct, Enum
        {
            foreach (var name in Enum.GetNames<TEnum>())
                yield return (referenceSet, name, HumanizeIdentifier(name));
        }

        private static string HumanizeIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var sb = new StringBuilder(value.Length + 8);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(value[i - 1]))
                    sb.Append(' ');
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
