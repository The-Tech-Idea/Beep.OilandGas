using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CapitalStructureAnalysisResult : ModelEntityBase
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
        /// Unlevered (all-equity) firm value
        /// </summary>
        private double UnleveredValueValue;

        public double UnleveredValue

        {

            get { return this.UnleveredValueValue; }

            set { SetProperty(ref UnleveredValueValue, value); }

        }

        /// <summary>
        /// Corporate tax rate used in analysis
        /// </summary>
        private double TaxRateValue;

        public double TaxRate

        {

            get { return this.TaxRateValue; }

            set { SetProperty(ref TaxRateValue, value); }

        }

        /// <summary>
        /// Capital structure scenarios analyzed (different debt ratios)
        /// </summary>
        private List<CapitalStructureScenario> ScenariosValue;

        public List<CapitalStructureScenario> Scenarios

        {

            get { return this.ScenariosValue; }

            set { SetProperty(ref ScenariosValue, value); }

        }

        /// <summary>
        /// Optimal debt ratio that minimizes WACC
        /// </summary>
        private double OptimalDebtRatioValue;

        public double OptimalDebtRatio

        {

            get { return this.OptimalDebtRatioValue; }

            set { SetProperty(ref OptimalDebtRatioValue, value); }

        }

        /// <summary>
        /// Weighted Average Cost of Capital at optimal capital structure
        /// </summary>
        private double OptimalWACCValue;

        public double OptimalWACC

        {

            get { return this.OptimalWACCValue; }

            set { SetProperty(ref OptimalWACCValue, value); }

        }

        /// <summary>
        /// Levered firm value at optimal capital structure
        /// </summary>
        private double OptimalLeveredValueValue;

        public double OptimalLeveredValue

        {

            get { return this.OptimalLeveredValueValue; }

            set { SetProperty(ref OptimalLeveredValueValue, value); }

        }

        /// <summary>
        /// Value created by using optimal capital structure
        /// </summary>
        private double ValueCreationValue;

        public double ValueCreation

        {

            get { return this.ValueCreationValue; }

            set { SetProperty(ref ValueCreationValue, value); }

        }
    }
}
