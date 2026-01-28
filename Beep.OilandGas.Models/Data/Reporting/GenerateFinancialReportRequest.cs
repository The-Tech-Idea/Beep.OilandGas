using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Reporting
{
    public class GenerateFinancialReportRequest : ModelEntityBase
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
        private string ReportTypeValue;

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private List<string>? PropertyIdsValue;

        public List<string>? PropertyIds

        {

            get { return this.PropertyIdsValue; }

            set { SetProperty(ref PropertyIdsValue, value); }

        }
    }
}
