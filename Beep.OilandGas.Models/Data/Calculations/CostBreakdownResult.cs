using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CostBreakdownResult : ModelEntityBase
    {
        private double DrillingCostValue;

        public double DrillingCost

        {

            get { return this.DrillingCostValue; }

            set { SetProperty(ref DrillingCostValue, value); }

        }
        private double CompletionCostValue;

        public double CompletionCost

        {

            get { return this.CompletionCostValue; }

            set { SetProperty(ref CompletionCostValue, value); }

        }
        private double EhsPermitsValue;

        public double EhsPermits

        {

            get { return this.EhsPermitsValue; }

            set { SetProperty(ref EhsPermitsValue, value); }

        }
        private double FacilityCostValue;

        public double FacilityCost

        {

            get { return this.FacilityCostValue; }

            set { SetProperty(ref FacilityCostValue, value); }

        }
        private double ContingencyAllowanceValue;

        public double ContingencyAllowance

        {

            get { return this.ContingencyAllowanceValue; }

            set { SetProperty(ref ContingencyAllowanceValue, value); }

        }
    }
}
