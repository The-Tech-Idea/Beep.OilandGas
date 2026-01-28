using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Reporting
{
    public class GenerateOperationalReportRequest : ModelEntityBase
    {
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
        private List<string>? WellIdsValue;

        public List<string>? WellIds

        {

            get { return this.WellIdsValue; }

            set { SetProperty(ref WellIdsValue, value); }

        }
        private List<string>? LeaseIdsValue;

        public List<string>? LeaseIds

        {

            get { return this.LeaseIdsValue; }

            set { SetProperty(ref LeaseIdsValue, value); }

        }
    }
}
