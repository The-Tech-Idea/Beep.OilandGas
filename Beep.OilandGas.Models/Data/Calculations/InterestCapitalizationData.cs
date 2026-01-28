using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class InterestCapitalizationData : ModelEntityBase
    {
        /// <summary>
        /// Total construction in progress cost
        /// </summary>
        private decimal ConstructionCostValue;

        public decimal ConstructionCost

        {

            get { return this.ConstructionCostValue; }

            set { SetProperty(ref ConstructionCostValue, value); }

        }

        /// <summary>
        /// Period interest rate
        /// </summary>
        private decimal InterestRateValue;

        public decimal InterestRate

        {

            get { return this.InterestRateValue; }

            set { SetProperty(ref InterestRateValue, value); }

        }

        /// <summary>
        /// Number of periods to capitalize
        /// </summary>
        private int PeriodsValue;

        public int Periods

        {

            get { return this.PeriodsValue; }

            set { SetProperty(ref PeriodsValue, value); }

        }

        /// <summary>
        /// Weighted average cost of capital
        /// </summary>
        private decimal WACCValue;

        public decimal WACC

        {

            get { return this.WACCValue; }

            set { SetProperty(ref WACCValue, value); }

        }
    }
}
