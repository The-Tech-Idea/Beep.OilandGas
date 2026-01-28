using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class QuarterlyEstimatedTax : ModelEntityBase
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
}
