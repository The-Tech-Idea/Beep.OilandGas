using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Tax Provision
    /// </summary>
    public partial class TaxProvision : AccountingEntityBase
    {
        private System.DateTime CalculatedDateValue;
        /// <summary>
        /// Gets or sets calculated date.
        /// </summary>
        public System.DateTime CalculatedDate
        {
            get { return this.CalculatedDateValue; }
            set { SetProperty(ref CalculatedDateValue, value); }
        }

        private System.Decimal TaxableIncomeValue;
        /// <summary>
        /// Gets or sets taxable income.
        /// </summary>
        public System.Decimal TaxableIncome
        {
            get { return this.TaxableIncomeValue; }
            set { SetProperty(ref TaxableIncomeValue, value); }
        }

        private System.Decimal EffectiveTaxRateValue;
        /// <summary>
        /// Gets or sets effective tax rate.
        /// </summary>
        public System.Decimal EffectiveTaxRate
        {
            get { return this.EffectiveTaxRateValue; }
            set { SetProperty(ref EffectiveTaxRateValue, value); }
        }

        private List<TaxDeduction> DeductionsValue = new List<TaxDeduction>();
        /// <summary>
        /// Gets or sets deductions.
        /// </summary>
        public List<TaxDeduction> Deductions
        {
            get { return this.DeductionsValue; }
            set { SetProperty(ref DeductionsValue, value); }
        }

        private System.Decimal TotalDeductionsValue;
        /// <summary>
        /// Gets or sets total deductions.
        /// </summary>
        public System.Decimal TotalDeductions
        {
            get { return this.TotalDeductionsValue; }
            set { SetProperty(ref TotalDeductionsValue, value); }
        }

        private System.Decimal AdjustedTaxableIncomeValue;
        /// <summary>
        /// Gets or sets adjusted taxable income.
        /// </summary>
        public System.Decimal AdjustedTaxableIncome
        {
            get { return this.AdjustedTaxableIncomeValue; }
            set { SetProperty(ref AdjustedTaxableIncomeValue, value); }
        }

        private System.Decimal BaseTaxValue;
        /// <summary>
        /// Gets or sets base tax.
        /// </summary>
        public System.Decimal BaseTax
        {
            get { return this.BaseTaxValue; }
            set { SetProperty(ref BaseTaxValue, value); }
        }

        private List<TaxCredit> CreditsValue = new List<TaxCredit>();
        /// <summary>
        /// Gets or sets credits.
        /// </summary>
        public List<TaxCredit> Credits
        {
            get { return this.CreditsValue; }
            set { SetProperty(ref CreditsValue, value); }
        }

        private System.Decimal TotalCreditsValue;
        /// <summary>
        /// Gets or sets total credits.
        /// </summary>
        public System.Decimal TotalCredits
        {
            get { return this.TotalCreditsValue; }
            set { SetProperty(ref TotalCreditsValue, value); }
        }

        private System.Decimal TaxAfterCreditsValue;
        /// <summary>
        /// Gets or sets tax after credits.
        /// </summary>
        public System.Decimal TaxAfterCredits
        {
            get { return this.TaxAfterCreditsValue; }
            set { SetProperty(ref TaxAfterCreditsValue, value); }
        }

        private System.Decimal AlternativeMinimumTaxValue;
        /// <summary>
        /// Gets or sets alternative minimum tax.
        /// </summary>
        public System.Decimal AlternativeMinimumTax
        {
            get { return this.AlternativeMinimumTaxValue; }
            set { SetProperty(ref AlternativeMinimumTaxValue, value); }
        }

        private System.Decimal TaxProvisionAmountValue;
        /// <summary>
        /// Gets or sets tax provision amount.
        /// </summary>
        public System.Decimal TaxProvisionAmount
        {
            get { return this.TaxProvisionAmountValue; }
            set { SetProperty(ref TaxProvisionAmountValue, value); }
        }

        private System.Decimal EffectiveProvisionRateValue;
        /// <summary>
        /// Gets or sets effective provision rate.
        /// </summary>
        public System.Decimal EffectiveProvisionRate
        {
            get { return this.EffectiveProvisionRateValue; }
            set { SetProperty(ref EffectiveProvisionRateValue, value); }
        }
    }

    /// <summary>
    /// Tax Deduction
    /// </summary>
    public partial class TaxDeduction : AccountingEntityBase
    {
        private System.String DescriptionValue = string.Empty;
        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public System.String Description
        {
            get { return this.DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }
    }

    /// <summary>
    /// Tax Credit
    /// </summary>
    public partial class TaxCredit : AccountingEntityBase
    {
        private System.String DescriptionValue = string.Empty;
        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public System.String Description
        {
            get { return this.DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }
    }

    /// <summary>
    /// Quarterly Estimated Tax
    /// </summary>
    public partial class QuarterlyEstimatedTax : AccountingEntityBase
    {
        private System.Int32 TaxYearValue;
        /// <summary>
        /// Gets or sets tax year.
        /// </summary>
        public System.Int32 TaxYear
        {
            get { return this.TaxYearValue; }
            set { SetProperty(ref TaxYearValue, value); }
        }

        private System.DateTime CalculatedDateValue;
        /// <summary>
        /// Gets or sets calculated date.
        /// </summary>
        public System.DateTime CalculatedDate
        {
            get { return this.CalculatedDateValue; }
            set { SetProperty(ref CalculatedDateValue, value); }
        }

        private System.Decimal AnnualTaxableIncomeValue;
        /// <summary>
        /// Gets or sets annual taxable income.
        /// </summary>
        public System.Decimal AnnualTaxableIncome
        {
            get { return this.AnnualTaxableIncomeValue; }
            set { SetProperty(ref AnnualTaxableIncomeValue, value); }
        }

        private System.Decimal EstimatedTaxRateValue;
        /// <summary>
        /// Gets or sets estimated tax rate.
        /// </summary>
        public System.Decimal EstimatedTaxRate
        {
            get { return this.EstimatedTaxRateValue; }
            set { SetProperty(ref EstimatedTaxRateValue, value); }
        }

        private List<QuarterlyPayment> QuartersValue = new List<QuarterlyPayment>();
        /// <summary>
        /// Gets or sets quarters.
        /// </summary>
        public List<QuarterlyPayment> Quarters
        {
            get { return this.QuartersValue; }
            set { SetProperty(ref QuartersValue, value); }
        }

        private List<QuarterlyAdjustment> AdjustmentsValue = new List<QuarterlyAdjustment>();
        /// <summary>
        /// Gets or sets adjustments.
        /// </summary>
        public List<QuarterlyAdjustment> Adjustments
        {
            get { return this.AdjustmentsValue; }
            set { SetProperty(ref AdjustmentsValue, value); }
        }

        private System.Decimal TotalAnnualPaymentValue;
        /// <summary>
        /// Gets or sets total annual payment.
        /// </summary>
        public System.Decimal TotalAnnualPayment
        {
            get { return this.TotalAnnualPaymentValue; }
            set { SetProperty(ref TotalAnnualPaymentValue, value); }
        }

        private System.Decimal TotalAdjustmentsValue;
        /// <summary>
        /// Gets or sets total adjustments.
        /// </summary>
        public System.Decimal TotalAdjustments
        {
            get { return this.TotalAdjustmentsValue; }
            set { SetProperty(ref TotalAdjustmentsValue, value); }
        }
    }

    /// <summary>
    /// Quarterly Payment
    /// </summary>
    public partial class QuarterlyPayment : AccountingEntityBase
    {
        private System.Int32 QuarterValue;
        /// <summary>
        /// Gets or sets quarter.
        /// </summary>
        public System.Int32 Quarter
        {
            get { return this.QuarterValue; }
            set { SetProperty(ref QuarterValue, value); }
        }

        private System.DateTime DueDateValue;
        /// <summary>
        /// Gets or sets due date.
        /// </summary>
        public System.DateTime DueDate
        {
            get { return this.DueDateValue; }
            set { SetProperty(ref DueDateValue, value); }
        }

        private System.Decimal BasePaymentValue;
        /// <summary>
        /// Gets or sets base payment.
        /// </summary>
        public System.Decimal BasePayment
        {
            get { return this.BasePaymentValue; }
            set { SetProperty(ref BasePaymentValue, value); }
        }

        private System.Decimal AdjustmentsValue;
        /// <summary>
        /// Gets or sets adjustments.
        /// </summary>
        public System.Decimal Adjustments
        {
            get { return this.AdjustmentsValue; }
            set { SetProperty(ref AdjustmentsValue, value); }
        }

        private System.Decimal TotalPaymentValue;
        /// <summary>
        /// Gets or sets total payment.
        /// </summary>
        public System.Decimal TotalPayment
        {
            get { return this.TotalPaymentValue; }
            set { SetProperty(ref TotalPaymentValue, value); }
        }
    }

    /// <summary>
    /// Quarterly Adjustment
    /// </summary>
    public partial class QuarterlyAdjustment : AccountingEntityBase
    {
        private System.Int32 QuarterValue;
        /// <summary>
        /// Gets or sets quarter.
        /// </summary>
        public System.Int32 Quarter
        {
            get { return this.QuarterValue; }
            set { SetProperty(ref QuarterValue, value); }
        }

        private System.String DescriptionValue = string.Empty;
        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public System.String Description
        {
            get { return this.DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }
    }

    /// <summary>
    /// Tax Deferral Analysis
    /// </summary>
    public partial class TaxDeferralAnalysis : AccountingEntityBase
    {
        private System.DateTime AnalysisDateValue;
        /// <summary>
        /// Gets or sets analysis date.
        /// </summary>
        public System.DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private System.Decimal CurrentYearIncomeValue;
        /// <summary>
        /// Gets or sets current year income.
        /// </summary>
        public System.Decimal CurrentYearIncome
        {
            get { return this.CurrentYearIncomeValue; }
            set { SetProperty(ref CurrentYearIncomeValue, value); }
        }

        private List<DeferralOpportunity> OpportunitiesValue = new List<DeferralOpportunity>();
        /// <summary>
        /// Gets or sets opportunities.
        /// </summary>
        public List<DeferralOpportunity> Opportunities
        {
            get { return this.OpportunitiesValue; }
            set { SetProperty(ref OpportunitiesValue, value); }
        }

        private System.Decimal TotalDeferralAmountValue;
        /// <summary>
        /// Gets or sets total deferral amount.
        /// </summary>
        public System.Decimal TotalDeferralAmount
        {
            get { return this.TotalDeferralAmountValue; }
            set { SetProperty(ref TotalDeferralAmountValue, value); }
        }

        private System.Decimal TotalTaxSavingsValue;
        /// <summary>
        /// Gets or sets total tax savings.
        /// </summary>
        public System.Decimal TotalTaxSavings
        {
            get { return this.TotalTaxSavingsValue; }
            set { SetProperty(ref TotalTaxSavingsValue, value); }
        }

        private System.Decimal TotalCashFlowImpactValue;
        /// <summary>
        /// Gets or sets total cash flow impact.
        /// </summary>
        public System.Decimal TotalCashFlowImpact
        {
            get { return this.TotalCashFlowImpactValue; }
            set { SetProperty(ref TotalCashFlowImpactValue, value); }
        }
    }

    /// <summary>
    /// Deferral Opportunity
    /// </summary>
    public partial class DeferralOpportunity : AccountingEntityBase
    {
        private System.String OpportunityTypeValue = string.Empty;
        /// <summary>
        /// Gets or sets opportunity type.
        /// </summary>
        public System.String OpportunityType
        {
            get { return this.OpportunityTypeValue; }
            set { SetProperty(ref OpportunityTypeValue, value); }
        }

        private System.String DescriptionValue = string.Empty;
        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public System.String Description
        {
            get { return this.DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }

        private System.Decimal DeferralAmountValue;
        /// <summary>
        /// Gets or sets deferral amount.
        /// </summary>
        public System.Decimal DeferralAmount
        {
            get { return this.DeferralAmountValue; }
            set { SetProperty(ref DeferralAmountValue, value); }
        }

        private System.Decimal TaxSavingsValue;
        /// <summary>
        /// Gets or sets tax savings.
        /// </summary>
        public System.Decimal TaxSavings
        {
            get { return this.TaxSavingsValue; }
            set { SetProperty(ref TaxSavingsValue, value); }
        }

        private System.Decimal CashFlowImpactValue;
        /// <summary>
        /// Gets or sets cash flow impact.
        /// </summary>
        public System.Decimal CashFlowImpact
        {
            get { return this.CashFlowImpactValue; }
            set { SetProperty(ref CashFlowImpactValue, value); }
        }

        private System.DateTime DeadlineValue;
        /// <summary>
        /// Gets or sets deadline.
        /// </summary>
        public System.DateTime Deadline
        {
            get { return this.DeadlineValue; }
            set { SetProperty(ref DeadlineValue, value); }
        }
    }
}

