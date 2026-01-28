using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Export
{
    public class ScheduleExportRequest : ModelEntityBase
    {
        private string ExportTypeValue;

        public string ExportType

        {

            get { return this.ExportTypeValue; }

            set { SetProperty(ref ExportTypeValue, value); }

        }
        private string ExportFormatValue;

        public string ExportFormat

        {

            get { return this.ExportFormatValue; }

            set { SetProperty(ref ExportFormatValue, value); }

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
        private string OutputPathValue;

        public string OutputPath

        {

            get { return this.OutputPathValue; }

            set { SetProperty(ref OutputPathValue, value); }

        }
        public Dictionary<string, object>? ExportParameters { get; set; }
    }
}
