using System;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Represents production data for a period (DTO for calculations/reporting).
    /// </summary>
    public class ProductionData : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public string PropertyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the production period.
        /// </summary>
        public DateTime ProductionPeriod { get; set; }

        /// <summary>
        /// Gets or sets the oil production in barrels.
        /// </summary>
        public decimal OilProduction { get; set; }

        /// <summary>
        /// Gets or sets the gas production in MCF.
        /// </summary>
        public decimal GasProduction { get; set; }

        /// <summary>
        /// Gets the total production in BOE (barrels of oil equivalent).
        /// </summary>
        public decimal TotalProductionBOE => OilProduction + (GasProduction / 6.0m);
    }

    /// <summary>
    /// Represents interest capitalization data (DTO for calculations).
    /// </summary>
    public class InterestCapitalizationData : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public string PropertyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the beginning accumulated expenditures.
        /// </summary>
        public decimal BeginningAccumulatedExpenditures { get; set; }

        /// <summary>
        /// Gets or sets the ending accumulated expenditures.
        /// </summary>
        public decimal EndingAccumulatedExpenditures { get; set; }

        /// <summary>
        /// Gets the average accumulated expenditures.
        /// </summary>
        public decimal AverageAccumulatedExpenditures =>
            (BeginningAccumulatedExpenditures + EndingAccumulatedExpenditures) / 2.0m;

        /// <summary>
        /// Gets or sets the interest rate (as decimal, e.g., 0.10 for 10%).
        /// </summary>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Gets or sets the capitalization period in months.
        /// </summary>
        public decimal CapitalizationPeriodMonths { get; set; }

        /// <summary>
        /// Gets or sets the actual interest costs incurred.
        /// </summary>
        public decimal ActualInterestCosts { get; set; }
    }

    /// <summary>
    /// Represents proved oil and gas reserves (DTO for calculations/reporting).
    /// </summary>
    public class ProvedReserves : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public string PropertyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the proved developed oil reserves in barrels.
        /// </summary>
        public decimal ProvedDevelopedOilReserves { get; set; }

        /// <summary>
        /// Gets or sets the proved undeveloped oil reserves in barrels.
        /// </summary>
        public decimal ProvedUndevelopedOilReserves { get; set; }

        /// <summary>
        /// Gets the total proved oil reserves in barrels.
        /// </summary>
        public decimal TotalProvedOilReserves => ProvedDevelopedOilReserves + ProvedUndevelopedOilReserves;

        /// <summary>
        /// Gets or sets the proved developed gas reserves in MCF.
        /// </summary>
        public decimal ProvedDevelopedGasReserves { get; set; }

        /// <summary>
        /// Gets or sets the proved undeveloped gas reserves in MCF.
        /// </summary>
        public decimal ProvedUndevelopedGasReserves { get; set; }

        /// <summary>
        /// Gets the total proved gas reserves in MCF.
        /// </summary>
        public decimal TotalProvedGasReserves => ProvedDevelopedGasReserves + ProvedUndevelopedGasReserves;

        /// <summary>
        /// Gets or sets the reserve date.
        /// </summary>
        public DateTime ReserveDate { get; set; }

        /// <summary>
        /// Gets or sets the oil price used for reserve valuation ($/barrel).
        /// </summary>
        public decimal OilPrice { get; set; }

        /// <summary>
        /// Gets or sets the gas price used for reserve valuation ($/MCF).
        /// </summary>
        public decimal GasPrice { get; set; }
        public object TotalProvedReservesBOE { get; set; }
    }

    /// <summary>
    /// Represents exploration costs (DTO for calculations/reporting).
    /// Note: For database operations, use EXPLORATION_COSTS entity class.
    /// </summary>
    public class ExplorationCosts : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the exploration cost identifier.
        /// </summary>
        public string ExplorationCostId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public string PropertyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the geological and geophysical (G&G) costs.
        /// </summary>
        public decimal GeologicalGeophysicalCosts { get; set; }

        /// <summary>
        /// Gets or sets the exploratory drilling costs (intangible drilling costs - IDC).
        /// </summary>
        public decimal ExploratoryDrillingCosts { get; set; }

        /// <summary>
        /// Gets or sets the exploratory well equipment costs.
        /// </summary>
        public decimal ExploratoryWellEquipment { get; set; }

        /// <summary>
        /// Gets or sets the cost date.
        /// </summary>
        public DateTime CostDate { get; set; }

        /// <summary>
        /// Gets or sets the well identifier (if applicable).
        /// </summary>
        public string WellId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is a dry hole.
        /// </summary>
        public bool IsDryHole { get; set; }

        /// <summary>
        /// Gets or sets whether the well found proved reserves.
        /// </summary>
        public bool FoundProvedReserves { get; set; }

        /// <summary>
        /// Gets or sets whether classification is deferred.
        /// </summary>
        public bool IsDeferredClassification { get; set; }

        /// <summary>
        /// Gets the total exploration costs.
        /// </summary>
        public decimal TotalExplorationCosts =>
            GeologicalGeophysicalCosts + ExploratoryDrillingCosts + ExploratoryWellEquipment;
    }

    /// <summary>
    /// Represents development costs (DTO for calculations/reporting).
    /// Note: For database operations, use DEVELOPMENT_COSTS entity class.
    /// </summary>
    public class DevelopmentCosts : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the development cost identifier.
        /// </summary>
        public string DevelopmentCostId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public string PropertyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the development well drilling costs (IDC).
        /// </summary>
        public decimal DevelopmentWellDrillingCosts { get; set; }

        /// <summary>
        /// Gets or sets the development well equipment costs.
        /// </summary>
        public decimal DevelopmentWellEquipment { get; set; }

        /// <summary>
        /// Gets or sets the support equipment and facilities costs.
        /// </summary>
        public decimal SupportEquipmentAndFacilities { get; set; }

        /// <summary>
        /// Gets or sets the service well costs.
        /// </summary>
        public decimal ServiceWellCosts { get; set; }

        /// <summary>
        /// Gets or sets the cost date.
        /// </summary>
        public DateTime CostDate { get; set; }

        /// <summary>
        /// Gets the total development costs.
        /// </summary>
        public decimal TotalDevelopmentCosts =>
            DevelopmentWellDrillingCosts + DevelopmentWellEquipment +
            SupportEquipmentAndFacilities + ServiceWellCosts;
    }

    /// <summary>
    /// Represents production costs (lifting costs) (DTO for calculations/reporting).
    /// Note: For database operations, use PRODUCTION_COSTS entity class.
    /// </summary>
    public class ProductionCosts : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the production cost identifier.
        /// </summary>
        public string ProductionCostId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public string PropertyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the operating costs.
        /// </summary>
        public decimal OperatingCosts { get; set; }

        /// <summary>
        /// Gets or sets the workover costs.
        /// </summary>
        public decimal WorkoverCosts { get; set; }

        /// <summary>
        /// Gets or sets the maintenance costs.
        /// </summary>
        public decimal MaintenanceCosts { get; set; }

        /// <summary>
        /// Gets or sets the cost period.
        /// </summary>
        public DateTime CostPeriod { get; set; }

        /// <summary>
        /// Gets the total production costs.
        /// </summary>
        public decimal TotalProductionCosts =>
            OperatingCosts + WorkoverCosts + MaintenanceCosts;
    }
}





