using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CostComparisonResult : ModelEntityBase
    {
        private double LowestCostValue;

        public double LowestCost

        {

            get { return this.LowestCostValue; }

            set { SetProperty(ref LowestCostValue, value); }

        }
        private double HighestCostValue;

        public double HighestCost

        {

            get { return this.HighestCostValue; }

            set { SetProperty(ref HighestCostValue, value); }

        }
        private double AverageCostValue;

        public double AverageCost

        {

            get { return this.AverageCostValue; }

            set { SetProperty(ref AverageCostValue, value); }

        }
    }
}
