using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class GenerateGovernmentalReportRequest : ModelEntityBase
    {
        private string ReportTypeValue = string.Empty;

        [Required]
        public string ReportType

        {

            get { return this.ReportTypeValue; }

            set { SetProperty(ref ReportTypeValue, value); }

        }
        private string JurisdictionValue = string.Empty;

        [Required]
        public string Jurisdiction

        {

            get { return this.JurisdictionValue; }

            set { SetProperty(ref JurisdictionValue, value); }

        }
        private DateTime StartDateValue;

        [Required]
        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        [Required]
        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }
}
