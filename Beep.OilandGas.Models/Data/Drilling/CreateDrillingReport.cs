using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CreateDrillingReport : ModelEntityBase
    {
        private DateTime ReportDateValue;

        public DateTime ReportDate

        {

            get { return this.ReportDateValue; }

            set { SetProperty(ref ReportDateValue, value); }

        }
        private decimal? DepthValue;

        public decimal? Depth

        {

            get { return this.DepthValue; }

            set { SetProperty(ref DepthValue, value); }

        }
        private string? ActivityValue;

        public string? Activity

        {

            get { return this.ActivityValue; }

            set { SetProperty(ref ActivityValue, value); }

        }
        private decimal? HoursValue;

        public decimal? Hours

        {

            get { return this.HoursValue; }

            set { SetProperty(ref HoursValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }
}
