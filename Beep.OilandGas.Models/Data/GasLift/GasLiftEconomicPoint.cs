using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public class GasLiftEconomicPoint : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the gas injection rate for this scenario
        /// </summary>
        private decimal GasInjectionRateValue;

        public decimal GasInjectionRate

        {

            get { return this.GasInjectionRateValue; }

            set { SetProperty(ref GasInjectionRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the production rate for this scenario
        /// </summary>
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the daily revenue
        /// </summary>
        private decimal DailyRevenueValue;

        public decimal DailyRevenue

        {

            get { return this.DailyRevenueValue; }

            set { SetProperty(ref DailyRevenueValue, value); }

        }

        /// <summary>
        /// Gets or sets the daily cost
        /// </summary>
        private decimal DailyCostValue;

        public decimal DailyCost

        {

            get { return this.DailyCostValue; }

            set { SetProperty(ref DailyCostValue, value); }

        }

        /// <summary>
        /// Gets or sets the net daily margin
        /// </summary>
        private decimal NetDailyMarginValue;

        public decimal NetDailyMargin

        {

            get { return this.NetDailyMarginValue; }

            set { SetProperty(ref NetDailyMarginValue, value); }

        }

        /// <summary>
        /// Gets or sets the annual net revenue
        /// </summary>
        private decimal AnnualNetRevenueValue;

        public decimal AnnualNetRevenue

        {

            get { return this.AnnualNetRevenueValue; }

            set { SetProperty(ref AnnualNetRevenueValue, value); }

        }
    }
}
