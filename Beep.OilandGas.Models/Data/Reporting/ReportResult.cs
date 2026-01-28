using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Reporting
{
    public class ReportResult : ModelEntityBase
    {
        private string ReportIdValue;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private string ReportTypeValue;

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private DateTime GeneratedDateValue = DateTime.UtcNow;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
        private string GeneratedByValue;

        public string GeneratedBy

        {

            get { return this.GeneratedByValue; }

            set { SetProperty(ref GeneratedByValue, value); }

        }
        private object ReportDataValue;

        public object ReportData

        {

            get { return this.ReportDataValue; }

            set { SetProperty(ref ReportDataValue, value); }

        }
        private string FormatValue = "JSON";

        public string Format

        {

            get { return this.FormatValue; }

            set { SetProperty(ref FormatValue, value); }

        }
    }
}
