using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Analytics
{
    public class AnalyticsResult : ModelEntityBase
    {
        private string AnalyticsIdValue;

        public string AnalyticsId

        {

            get { return this.AnalyticsIdValue; }

            set { SetProperty(ref AnalyticsIdValue, value); }

        }
        private string AnalyticsTypeValue;

        public string AnalyticsType

        {

            get { return this.AnalyticsTypeValue; }

            set { SetProperty(ref AnalyticsTypeValue, value); }

        }
        private DateTime CalculationDateValue = DateTime.UtcNow;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private object ResultDataValue;

        public object ResultData

        {

            get { return this.ResultDataValue; }

            set { SetProperty(ref ResultDataValue, value); }

        }
    }
}
