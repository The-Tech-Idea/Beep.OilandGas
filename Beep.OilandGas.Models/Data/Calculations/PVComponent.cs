using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PVComponent : ModelEntityBase
    {
        /// <summary>
        /// Forecast year number
        /// </summary>
        private int YearValue;

        public int Year

        {

            get { return this.YearValue; }

            set { SetProperty(ref YearValue, value); }

        }

        /// <summary>
        /// Cash flow in this year (before discounting)
        /// </summary>
        private double CashFlowValue;

        public double CashFlow

        {

            get { return this.CashFlowValue; }

            set { SetProperty(ref CashFlowValue, value); }

        }

        /// <summary>
        /// Discount factor applied (1 / (1 + WACC)^year)
        /// </summary>
        private double DiscountFactorValue;

        public double DiscountFactor

        {

            get { return this.DiscountFactorValue; }

            set { SetProperty(ref DiscountFactorValue, value); }

        }

        /// <summary>
        /// Present value of the cash flow
        /// </summary>
        private double PresentValueValue;

        public double PresentValue

        {

            get { return this.PresentValueValue; }

            set { SetProperty(ref PresentValueValue, value); }

        }
    }
}
