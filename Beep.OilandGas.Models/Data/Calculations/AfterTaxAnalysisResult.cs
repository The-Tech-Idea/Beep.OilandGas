using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class AfterTaxAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the analysis
        /// </summary>
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Date and time the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Corporate tax rate applied (as decimal, e.g., 0.35 for 35%)
        /// </summary>
        private double TaxRateValue;

        public double TaxRate

        {

            get { return this.TaxRateValue; }

            set { SetProperty(ref TaxRateValue, value); }

        }

        /// <summary>
        /// NPV before considering tax impacts
        /// </summary>
        private double PreTaxNPVValue;

        public double PreTaxNPV

        {

            get { return this.PreTaxNPVValue; }

            set { SetProperty(ref PreTaxNPVValue, value); }

        }

        /// <summary>
        /// NPV after tax deductions and depreciation benefits
        /// </summary>
        private double AfterTaxNPVValue;

        public double AfterTaxNPV

        {

            get { return this.AfterTaxNPVValue; }

            set { SetProperty(ref AfterTaxNPVValue, value); }

        }

        /// <summary>
        /// Internal Rate of Return after tax adjustments
        /// </summary>
        private double AfterTaxIRRValue;

        public double AfterTaxIRR

        {

            get { return this.AfterTaxIRRValue; }

            set { SetProperty(ref AfterTaxIRRValue, value); }

        }

        /// <summary>
        /// Tax shield value from depreciation and deductions
        /// </summary>
        private double TaxShieldValue;

        public double TaxShield

        {

            get { return this.TaxShieldValue; }

            set { SetProperty(ref TaxShieldValue, value); }

        }

        /// <summary>
        /// Effective tax rate on the project
        /// </summary>
        private double EffectiveTaxRateValue;

        public double EffectiveTaxRate

        {

            get { return this.EffectiveTaxRateValue; }

            set { SetProperty(ref EffectiveTaxRateValue, value); }

        }

        /// <summary>
        /// After-tax cash flows by period
        /// </summary>
        private List<CashFlow> AfterTaxCashFlowsValue;

        public List<CashFlow> AfterTaxCashFlows

        {

            get { return this.AfterTaxCashFlowsValue; }

            set { SetProperty(ref AfterTaxCashFlowsValue, value); }

        }
    }
}
