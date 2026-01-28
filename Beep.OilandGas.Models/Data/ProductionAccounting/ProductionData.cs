using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
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
}
