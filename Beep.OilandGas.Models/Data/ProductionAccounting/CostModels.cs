using System;

using Beep.OilandGas.Models.Data;
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
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the production period.
        /// </summary>
        private DateTime ProductionPeriodValue;

        public DateTime ProductionPeriod

        {

            get { return this.ProductionPeriodValue; }

            set { SetProperty(ref ProductionPeriodValue, value); }

        }

        /// <summary>
        /// Gets or sets the oil production in barrels.
        /// </summary>
        private decimal OilProductionValue;

        public decimal OilProduction

        {

            get { return this.OilProductionValue; }

            set { SetProperty(ref OilProductionValue, value); }

        }

        /// <summary>
        /// Gets or sets the gas production in MCF.
        /// </summary>
        private decimal GasProductionValue;

        public decimal GasProduction

        {

            get { return this.GasProductionValue; }

            set { SetProperty(ref GasProductionValue, value); }

        }

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
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the beginning accumulated expenditures.
        /// </summary>
        private decimal BeginningAccumulatedExpendituresValue;

        public decimal BeginningAccumulatedExpenditures

        {

            get { return this.BeginningAccumulatedExpendituresValue; }

            set { SetProperty(ref BeginningAccumulatedExpendituresValue, value); }

        }

        /// <summary>
        /// Gets or sets the ending accumulated expenditures.
        /// </summary>
        private decimal EndingAccumulatedExpendituresValue;

        public decimal EndingAccumulatedExpenditures

        {

            get { return this.EndingAccumulatedExpendituresValue; }

            set { SetProperty(ref EndingAccumulatedExpendituresValue, value); }

        }

        /// <summary>
        /// Gets the average accumulated expenditures.
        /// </summary>
        public decimal AverageAccumulatedExpenditures =>
            (BeginningAccumulatedExpenditures + EndingAccumulatedExpenditures) / 2.0m;

        /// <summary>
        /// Gets or sets the interest rate (as decimal, e.g., 0.10 for 10%).
        /// </summary>
        private decimal InterestRateValue;

        public decimal InterestRate

        {

            get { return this.InterestRateValue; }

            set { SetProperty(ref InterestRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the capitalization period in months.
        /// </summary>
        private decimal CapitalizationPeriodMonthsValue;

        public decimal CapitalizationPeriodMonths

        {

            get { return this.CapitalizationPeriodMonthsValue; }

            set { SetProperty(ref CapitalizationPeriodMonthsValue, value); }

        }

        /// <summary>
        /// Gets or sets the actual interest costs incurred.
        /// </summary>
        private decimal ActualInterestCostsValue;

        public decimal ActualInterestCosts

        {

            get { return this.ActualInterestCostsValue; }

            set { SetProperty(ref ActualInterestCostsValue, value); }

        }
    }

    /// <summary>
    /// Represents proved oil and gas reserves (DTO for calculations/reporting).
    /// </summary>
    public class ProvedReserves : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved developed oil reserves in barrels.
        /// </summary>
        private decimal ProvedDevelopedOilReservesValue;

        public decimal ProvedDevelopedOilReserves

        {

            get { return this.ProvedDevelopedOilReservesValue; }

            set { SetProperty(ref ProvedDevelopedOilReservesValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved undeveloped oil reserves in barrels.
        /// </summary>
        private decimal ProvedUndevelopedOilReservesValue;

        public decimal ProvedUndevelopedOilReserves

        {

            get { return this.ProvedUndevelopedOilReservesValue; }

            set { SetProperty(ref ProvedUndevelopedOilReservesValue, value); }

        }

        /// <summary>
        /// Gets the total proved oil reserves in barrels.
        /// </summary>
        public decimal TotalProvedOilReserves => ProvedDevelopedOilReserves + ProvedUndevelopedOilReserves;

        /// <summary>
        /// Gets or sets the proved developed gas reserves in MCF.
        /// </summary>
        private decimal ProvedDevelopedGasReservesValue;

        public decimal ProvedDevelopedGasReserves

        {

            get { return this.ProvedDevelopedGasReservesValue; }

            set { SetProperty(ref ProvedDevelopedGasReservesValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved undeveloped gas reserves in MCF.
        /// </summary>
        private decimal ProvedUndevelopedGasReservesValue;

        public decimal ProvedUndevelopedGasReserves

        {

            get { return this.ProvedUndevelopedGasReservesValue; }

            set { SetProperty(ref ProvedUndevelopedGasReservesValue, value); }

        }

        /// <summary>
        /// Gets the total proved gas reserves in MCF.
        /// </summary>
        public decimal TotalProvedGasReserves => ProvedDevelopedGasReserves + ProvedUndevelopedGasReserves;

        /// <summary>
        /// Gets or sets the reserve date.
        /// </summary>
        private DateTime ReserveDateValue;

        public DateTime ReserveDate

        {

            get { return this.ReserveDateValue; }

            set { SetProperty(ref ReserveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the oil price used for reserve valuation ($/barrel).
        /// </summary>
        private decimal OilPriceValue;

        public decimal OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }

        /// <summary>
        /// Gets or sets the gas price used for reserve valuation ($/MCF).
        /// </summary>
        private decimal GasPriceValue;

        public decimal GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private object TotalProvedReservesBOEValue;

        public object TotalProvedReservesBOE

        {

            get { return this.TotalProvedReservesBOEValue; }

            set { SetProperty(ref TotalProvedReservesBOEValue, value); }

        }
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
        private string ExplorationCostIdValue = string.Empty;

        public string ExplorationCostId

        {

            get { return this.ExplorationCostIdValue; }

            set { SetProperty(ref ExplorationCostIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the geological and geophysical (G&G) costs.
        /// </summary>
        private decimal GeologicalGeophysicalCostsValue;

        public decimal GeologicalGeophysicalCosts

        {

            get { return this.GeologicalGeophysicalCostsValue; }

            set { SetProperty(ref GeologicalGeophysicalCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the exploratory drilling costs (intangible drilling costs - IDC).
        /// </summary>
        private decimal ExploratoryDrillingCostsValue;

        public decimal ExploratoryDrillingCosts

        {

            get { return this.ExploratoryDrillingCostsValue; }

            set { SetProperty(ref ExploratoryDrillingCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the exploratory well equipment costs.
        /// </summary>
        private decimal ExploratoryWellEquipmentValue;

        public decimal ExploratoryWellEquipment

        {

            get { return this.ExploratoryWellEquipmentValue; }

            set { SetProperty(ref ExploratoryWellEquipmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the cost date.
        /// </summary>
        private DateTime CostDateValue;

        public DateTime CostDate

        {

            get { return this.CostDateValue; }

            set { SetProperty(ref CostDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the well identifier (if applicable).
        /// </summary>
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is a dry hole.
        /// </summary>
        private bool IsDryHoleValue;

        public bool IsDryHole

        {

            get { return this.IsDryHoleValue; }

            set { SetProperty(ref IsDryHoleValue, value); }

        }

        /// <summary>
        /// Gets or sets whether the well found proved reserves.
        /// </summary>
        private bool FoundProvedReservesValue;

        public bool FoundProvedReserves

        {

            get { return this.FoundProvedReservesValue; }

            set { SetProperty(ref FoundProvedReservesValue, value); }

        }

        /// <summary>
        /// Gets or sets whether classification is deferred.
        /// </summary>
        private bool IsDeferredClassificationValue;

        public bool IsDeferredClassification

        {

            get { return this.IsDeferredClassificationValue; }

            set { SetProperty(ref IsDeferredClassificationValue, value); }

        }

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
        private string DevelopmentCostIdValue = string.Empty;

        public string DevelopmentCostId

        {

            get { return this.DevelopmentCostIdValue; }

            set { SetProperty(ref DevelopmentCostIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the development well drilling costs (IDC).
        /// </summary>
        private decimal DevelopmentWellDrillingCostsValue;

        public decimal DevelopmentWellDrillingCosts

        {

            get { return this.DevelopmentWellDrillingCostsValue; }

            set { SetProperty(ref DevelopmentWellDrillingCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the development well equipment costs.
        /// </summary>
        private decimal DevelopmentWellEquipmentValue;

        public decimal DevelopmentWellEquipment

        {

            get { return this.DevelopmentWellEquipmentValue; }

            set { SetProperty(ref DevelopmentWellEquipmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the support equipment and facilities costs.
        /// </summary>
        private decimal SupportEquipmentAndFacilitiesValue;

        public decimal SupportEquipmentAndFacilities

        {

            get { return this.SupportEquipmentAndFacilitiesValue; }

            set { SetProperty(ref SupportEquipmentAndFacilitiesValue, value); }

        }

        /// <summary>
        /// Gets or sets the service well costs.
        /// </summary>
        private decimal ServiceWellCostsValue;

        public decimal ServiceWellCosts

        {

            get { return this.ServiceWellCostsValue; }

            set { SetProperty(ref ServiceWellCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the cost date.
        /// </summary>
        private DateTime CostDateValue;

        public DateTime CostDate

        {

            get { return this.CostDateValue; }

            set { SetProperty(ref CostDateValue, value); }

        }

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
        private string ProductionCostIdValue = string.Empty;

        public string ProductionCostId

        {

            get { return this.ProductionCostIdValue; }

            set { SetProperty(ref ProductionCostIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the operating costs.
        /// </summary>
        private decimal OperatingCostsValue;

        public decimal OperatingCosts

        {

            get { return this.OperatingCostsValue; }

            set { SetProperty(ref OperatingCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the workover costs.
        /// </summary>
        private decimal WorkoverCostsValue;

        public decimal WorkoverCosts

        {

            get { return this.WorkoverCostsValue; }

            set { SetProperty(ref WorkoverCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the maintenance costs.
        /// </summary>
        private decimal MaintenanceCostsValue;

        public decimal MaintenanceCosts

        {

            get { return this.MaintenanceCostsValue; }

            set { SetProperty(ref MaintenanceCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the cost period.
        /// </summary>
        private DateTime CostPeriodValue;

        public DateTime CostPeriod

        {

            get { return this.CostPeriodValue; }

            set { SetProperty(ref CostPeriodValue, value); }

        }

        /// <summary>
        /// Gets the total production costs.
        /// </summary>
        public decimal TotalProductionCosts =>
            OperatingCosts + WorkoverCosts + MaintenanceCosts;
    }
}








