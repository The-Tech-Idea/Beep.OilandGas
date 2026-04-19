using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PlayStatistics : ModelEntityBase
    {
        private string MetricValue = string.Empty;

        public string Metric

        {

            get { return this.MetricValue; }

            set { SetProperty(ref MetricValue, value); }

        }
        private decimal ValueValue;

        public decimal Value

        {

            get { return this.ValueValue; }

            set { SetProperty(ref ValueValue, value); }

        }
    }
}
