using System;
using System.Collections.Generic;

namespace Beep.OilandGas.ProductionAccounting.Models
{
    /// <summary>
    /// Represents exploration costs.
    /// </summary>
    public class ExplorationCosts
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
        /// Gets or sets the total exploration costs.
        /// </summary>
        public decimal TotalExplorationCosts => 
            GeologicalGeophysicalCosts + ExploratoryDrillingCosts + ExploratoryWellEquipment;
    }

    /// <summary>
    /// Represents development costs.
    /// </summary>
    public class DevelopmentCosts
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
        /// Gets or sets the total development costs.
        /// </summary>
        public decimal TotalDevelopmentCosts =>
            DevelopmentWellDrillingCosts + DevelopmentWellEquipment + 
            SupportEquipmentAndFacilities + ServiceWellCosts;
    }

    /// <summary>
    /// Represents production costs (lifting costs).
    /// </summary>
    public class ProductionCosts
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
        /// Gets or sets the total production costs.
        /// </summary>
        public decimal TotalProductionCosts =>
            OperatingCosts + WorkoverCosts + MaintenanceCosts;
    }

    /// <summary>
    /// Represents production data for a period.
    /// </summary>
    public class ProductionData
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
        /// Gets or sets the total production in BOE (barrels of oil equivalent).
        /// </summary>
        public decimal TotalProductionBOE => OilProduction + (GasProduction / 6.0m);
    }

    /// <summary>
    /// Represents interest capitalization data.
    /// </summary>
    public class InterestCapitalizationData
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
        /// Gets or sets the average accumulated expenditures.
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
}

