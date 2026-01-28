using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProspectReportRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string ReportTypeValue = "Comprehensive";

        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
    }
}
