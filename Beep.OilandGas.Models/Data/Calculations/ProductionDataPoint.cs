using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProductionDataPoint : ModelEntityBase
    {
        private DateTime MonthValue;

        public DateTime Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }
        private double MonthlyProductionValue;

        public double MonthlyProduction

        {

            get { return this.MonthlyProductionValue; }

            set { SetProperty(ref MonthlyProductionValue, value); }

        }
        private double CumulativeProductionValue;

        public double CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }
    }
}
