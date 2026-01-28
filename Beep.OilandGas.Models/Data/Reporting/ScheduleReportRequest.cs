using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Reporting
{
    public class ScheduleReportRequest : ModelEntityBase
    {
        private string ReportTypeValue;

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private string ScheduleFrequencyValue;

        public string ScheduleFrequency

        {

            get { return this.ScheduleFrequencyValue; }

            set { SetProperty(ref ScheduleFrequencyValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        public Dictionary<string, object> ReportParameters { get; set; } = new();
    }
}
