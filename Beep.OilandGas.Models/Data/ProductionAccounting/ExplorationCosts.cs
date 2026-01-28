using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
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
}
