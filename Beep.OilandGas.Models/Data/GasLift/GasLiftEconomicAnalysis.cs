using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Economic analysis result for gas lift project
    /// </summary>
    public class GasLiftEconomicAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the analysis date
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the analysis
        /// </summary>
        public string AnalyzedByUser { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the gas injection cost per Mscf
        /// </summary>
        public decimal GasInjectionCostPerMscf { get; set; }

        /// <summary>
        /// Gets or sets the oil price per barrel
        /// </summary>
        public decimal OilPricePerBarrel { get; set; }

        /// <summary>
        /// Gets or sets the list of economic analysis points
        /// </summary>
        public List<GasLiftEconomicPoint> EconomicPoints { get; set; } = new();

        /// <summary>
        /// Gets or sets the optimal gas injection rate
        /// </summary>
        public decimal OptimalGasInjectionRate { get; set; }

        /// <summary>
        /// Gets or sets the optimal production rate at economic optimum
        /// </summary>
        public decimal OptimalProductionRate { get; set; }

        /// <summary>
        /// Gets or sets the maximum annual net revenue
        /// </summary>
        public decimal MaximumAnnualNetRevenue { get; set; }

        /// <summary>
        /// Gets or sets whether the project is economically viable
        /// </summary>
        public bool IsEconomicallyViable { get; set; }
    }

    /// <summary>
    /// Economic point data for individual gas injection scenario
    /// </summary>
    public class GasLiftEconomicPoint : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the gas injection rate for this scenario
        /// </summary>
        public decimal GasInjectionRate { get; set; }

        /// <summary>
        /// Gets or sets the production rate for this scenario
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Gets or sets the daily revenue
        /// </summary>
        public decimal DailyRevenue { get; set; }

        /// <summary>
        /// Gets or sets the daily cost
        /// </summary>
        public decimal DailyCost { get; set; }

        /// <summary>
        /// Gets or sets the net daily margin
        /// </summary>
        public decimal NetDailyMargin { get; set; }

        /// <summary>
        /// Gets or sets the annual net revenue
        /// </summary>
        public decimal AnnualNetRevenue { get; set; }
    }
}
