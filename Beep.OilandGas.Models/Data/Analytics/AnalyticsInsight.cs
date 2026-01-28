using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Analytics
{
    public class AnalyticsInsight : ModelEntityBase
    {
        private string InsightIdValue;

        public string InsightId

        {

            get { return this.InsightIdValue; }

            set { SetProperty(ref InsightIdValue, value); }

        }
        private string InsightTypeValue;

        public string InsightType

        {

            get { return this.InsightTypeValue; }

            set { SetProperty(ref InsightTypeValue, value); }

        }
        private string TitleValue;

        public string Title

        {

            get { return this.TitleValue; }

            set { SetProperty(ref TitleValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string SeverityValue;

        public string Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        }
        private DateTime GeneratedDateValue = DateTime.UtcNow;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
    }
}
