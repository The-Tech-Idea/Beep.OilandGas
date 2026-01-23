using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Pricing
{
    public class ValueRunTicketRequest : ModelEntityBase
    {
        private string RunTicketNumberValue;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private string PricingMethodValue;

        public string PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        } // Fixed, IndexBased, PostedPrice, Regulated
        private decimal? FixedPriceValue;

        public decimal? FixedPrice

        {

            get { return this.FixedPriceValue; }

            set { SetProperty(ref FixedPriceValue, value); }

        }
        private string IndexNameValue;

        public string IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }
        private decimal? DifferentialValue;

        public decimal? Differential

        {

            get { return this.DifferentialValue; }

            set { SetProperty(ref DifferentialValue, value); }

        }
        private string RegulatoryAuthorityValue;

        public string RegulatoryAuthority

        {

            get { return this.RegulatoryAuthorityValue; }

            set { SetProperty(ref RegulatoryAuthorityValue, value); }

        }
    }

    public class CreatePriceIndexRequest : ModelEntityBase
    {
        private string IndexNameValue;

        public string IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }
        private string CommodityTypeValue;

        public string CommodityType

        {

            get { return this.CommodityTypeValue; }

            set { SetProperty(ref CommodityTypeValue, value); }

        }
        private DateTime PriceDateValue;

        public DateTime PriceDate

        {

            get { return this.PriceDateValue; }

            set { SetProperty(ref PriceDateValue, value); }

        }
        private decimal PriceValueValue;

        public decimal PriceValue

        {

            get { return this.PriceValueValue; }

            set { SetProperty(ref PriceValueValue, value); }

        }
        private string CurrencyCodeValue;

        public string CurrencyCode

        {

            get { return this.CurrencyCodeValue; }

            set { SetProperty(ref CurrencyCodeValue, value); }

        }
        private string PricingPointValue;

        public string PricingPoint

        {

            get { return this.PricingPointValue; }

            set { SetProperty(ref PricingPointValue, value); }

        }
        private string UnitValue;

        public string Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }

    }

    public class PricingReconciliationRequest : ModelEntityBase
    {
        private string RunTicketNumberValue;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
    }

    public class PricingReconciliationResult : ModelEntityBase
    {
        private string ReconciliationIdValue;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private string RunTicketNumberValue;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private decimal ExpectedValueValue;

        public decimal ExpectedValue

        {

            get { return this.ExpectedValueValue; }

            set { SetProperty(ref ExpectedValueValue, value); }

        }
        private decimal ActualValueValue;

        public decimal ActualValue

        {

            get { return this.ActualValueValue; }

            set { SetProperty(ref ActualValueValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private DateTime ReconciliationDateValue = DateTime.UtcNow;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
    }

    public class PricingApproval : ModelEntityBase
    {
        private string ValuationIdValue;

        public string ValuationId

        {

            get { return this.ValuationIdValue; }

            set { SetProperty(ref ValuationIdValue, value); }

        }
        private string RunTicketNumberValue;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private decimal TotalValueValue;

        public decimal TotalValue

        {

            get { return this.TotalValueValue; }

            set { SetProperty(ref TotalValueValue, value); }

        }
        private string PricingMethodValue;

        public string PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime ValuationDateValue;

        public DateTime ValuationDate

        {

            get { return this.ValuationDateValue; }

            set { SetProperty(ref ValuationDateValue, value); }

        }
    }

    public class PricingApprovalResult : ModelEntityBase
    {
        private string ValuationIdValue;

        public string ValuationId

        {

            get { return this.ValuationIdValue; }

            set { SetProperty(ref ValuationIdValue, value); }

        }
        private bool IsApprovedValue;

        public bool IsApproved

        {

            get { return this.IsApprovedValue; }

            set { SetProperty(ref IsApprovedValue, value); }

        }
        private string ApproverIdValue;

        public string ApproverId

        {

            get { return this.ApproverIdValue; }

            set { SetProperty(ref ApproverIdValue, value); }

        }
        private DateTime ApprovalDateValue = DateTime.UtcNow;

        public DateTime ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string CommentsValue;

        public string Comments

        {

            get { return this.CommentsValue; }

            set { SetProperty(ref CommentsValue, value); }

        }
    }
}


