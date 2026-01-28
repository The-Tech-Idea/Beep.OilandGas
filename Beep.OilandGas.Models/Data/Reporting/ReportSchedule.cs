using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Reporting
{
    public class ReportSchedule : ModelEntityBase
    {
        private string ScheduleIdValue;

        public string ScheduleId

        {

            get { return this.ScheduleIdValue; }

            set { SetProperty(ref ScheduleIdValue, value); }

        }
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
        private DateTime NextRunDateValue;

        public DateTime NextRunDate

        {

            get { return this.NextRunDateValue; }

            set { SetProperty(ref NextRunDateValue, value); }

        }
        private DateTime? LastRunDateValue;

        public DateTime? LastRunDate

        {

            get { return this.LastRunDateValue; }

            set { SetProperty(ref LastRunDateValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
