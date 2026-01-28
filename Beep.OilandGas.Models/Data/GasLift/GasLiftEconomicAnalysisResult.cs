using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public class GasLiftEconomicAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the analysis date
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the user who performed the analysis
        /// </summary>
        private string AnalyzedByUserValue = string.Empty;

        public string AnalyzedByUser

        {

            get { return this.AnalyzedByUserValue; }

            set { SetProperty(ref AnalyzedByUserValue, value); }

        }

        /// <summary>
        /// Gets or sets the gas injection cost per Mscf
        /// </summary>
        private decimal GasInjectionCostPerMscfValue;

        public decimal GasInjectionCostPerMscf

        {

            get { return this.GasInjectionCostPerMscfValue; }

            set { SetProperty(ref GasInjectionCostPerMscfValue, value); }

        }

        /// <summary>
        /// Gets or sets the oil price per barrel
        /// </summary>
        private decimal OilPricePerBarrelValue;

        public decimal OilPricePerBarrel

        {

            get { return this.OilPricePerBarrelValue; }

            set { SetProperty(ref OilPricePerBarrelValue, value); }

        }

        /// <summary>
        /// Gets or sets the list of economic analysis points
        /// </summary>
        private List<GasLiftEconomicPoint> EconomicPointsValue = new();

        public List<GasLiftEconomicPoint> EconomicPoints

        {

            get { return this.EconomicPointsValue; }

            set { SetProperty(ref EconomicPointsValue, value); }

        }

        /// <summary>
        /// Gets or sets the optimal gas injection rate
        /// </summary>
        private decimal OptimalGasInjectionRateValue;

        public decimal OptimalGasInjectionRate

        {

            get { return this.OptimalGasInjectionRateValue; }

            set { SetProperty(ref OptimalGasInjectionRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the optimal production rate at economic optimum
        /// </summary>
        private decimal OptimalProductionRateValue;

        public decimal OptimalProductionRate

        {

            get { return this.OptimalProductionRateValue; }

            set { SetProperty(ref OptimalProductionRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum annual net revenue
        /// </summary>
        private decimal MaximumAnnualNetRevenueValue;

        public decimal MaximumAnnualNetRevenue

        {

            get { return this.MaximumAnnualNetRevenueValue; }

            set { SetProperty(ref MaximumAnnualNetRevenueValue, value); }

        }

        /// <summary>
        /// Gets or sets whether the project is economically viable
        /// </summary>
        private bool IsEconomicallyViableValue;

        public bool IsEconomicallyViable

        {

            get { return this.IsEconomicallyViableValue; }

            set { SetProperty(ref IsEconomicallyViableValue, value); }

        }
    }
}
