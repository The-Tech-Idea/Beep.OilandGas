using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProductionReportSummary : ModelEntityBase
    {
        private decimal TotalProductionValue;

        public decimal TotalProduction

        {

            get { return this.TotalProductionValue; }

            set { SetProperty(ref TotalProductionValue, value); }

        }
        private int ProducingDaysValue;

        public int ProducingDays

        {

            get { return this.ProducingDaysValue; }

            set { SetProperty(ref ProducingDaysValue, value); }

        }
        private decimal AverageDailyProductionValue;

        public decimal AverageDailyProduction

        {

            get { return this.AverageDailyProductionValue; }

            set { SetProperty(ref AverageDailyProductionValue, value); }

        }
    }
}
