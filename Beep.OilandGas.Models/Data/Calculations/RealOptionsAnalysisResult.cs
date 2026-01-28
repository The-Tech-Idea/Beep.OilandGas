using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class RealOptionsAnalysisResult : ModelEntityBase
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
        /// Initial project NPV without considering options
        /// </summary>
        private double InitialNPVValue;

        public double InitialNPV

        {

            get { return this.InitialNPVValue; }

            set { SetProperty(ref InitialNPVValue, value); }

        }

        /// <summary>
        /// Expected project life in years
        /// </summary>
        private int ProjectLifeValue;

        public int ProjectLife

        {

            get { return this.ProjectLifeValue; }

            set { SetProperty(ref ProjectLifeValue, value); }

        }

        /// <summary>
        /// Cash flow volatility (used for option valuation)
        /// </summary>
        private double VolatilityValue;

        public double Volatility

        {

            get { return this.VolatilityValue; }

            set { SetProperty(ref VolatilityValue, value); }

        }

        /// <summary>
        /// List of option valuations (expansion, abandonment, switching)
        /// </summary>
        private List<OptionValuation> OptionsValue;

        public List<OptionValuation> Options

        {

            get { return this.OptionsValue; }

            set { SetProperty(ref OptionsValue, value); }

        }

        /// <summary>
        /// Total value of all strategic options combined
        /// </summary>
        private double TotalOptionValueValue;

        public double TotalOptionValue

        {

            get { return this.TotalOptionValueValue; }

            set { SetProperty(ref TotalOptionValueValue, value); }

        }

        /// <summary>
        /// Project value including option values
        /// </summary>
        private double ProjectValueWithOptionsValue;

        public double ProjectValueWithOptions

        {

            get { return this.ProjectValueWithOptionsValue; }

            set { SetProperty(ref ProjectValueWithOptionsValue, value); }

        }

        /// <summary>
        /// Flexibility premium as percentage of initial NPV
        /// </summary>
        private double FlexibilityPremiumValue;

        public double FlexibilityPremium

        {

            get { return this.FlexibilityPremiumValue; }

            set { SetProperty(ref FlexibilityPremiumValue, value); }

        }
    }
}
