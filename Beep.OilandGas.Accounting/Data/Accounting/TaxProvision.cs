using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class TaxProvision : ModelEntityBase
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
}
