using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
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
