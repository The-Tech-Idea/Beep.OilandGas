using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CommodityPriceSensitivityResult : ModelEntityBase
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
        /// Base commodity price used in analysis
        /// </summary>
        private double BasePriceValue;

        public double BasePrice

        {

            get { return this.BasePriceValue; }

            set { SetProperty(ref BasePriceValue, value); }

        }

        /// <summary>
        /// Scenarios showing NPV and IRR at different price points
        /// </summary>
        private List<PriceScenario> PriceScenariosValue;

        public List<PriceScenario> PriceScenarios

        {

            get { return this.PriceScenariosValue; }

            set { SetProperty(ref PriceScenariosValue, value); }

        }

        /// <summary>
        /// Price at which project NPV breaks even (approximately zero)
        /// </summary>
        private double BreakevenPriceValue;

        public double BreakevenPrice

        {

            get { return this.BreakevenPriceValue; }

            set { SetProperty(ref BreakevenPriceValue, value); }

        }

        /// <summary>
        /// NPV at base price point
        /// </summary>
        private double BaseNPVValue;

        public double BaseNPV

        {

            get { return this.BaseNPVValue; }

            set { SetProperty(ref BaseNPVValue, value); }

        }
    }
}
