using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting.Reporting
{
    /// <summary>
    /// Request DTO for generating operational report
    /// </summary>
    public class GenerateOperationalReportRequest : ModelEntityBase
    {
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for generating lease report
    /// </summary>
    public class GenerateLeaseReportRequest : ModelEntityBase
    {
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }
}







