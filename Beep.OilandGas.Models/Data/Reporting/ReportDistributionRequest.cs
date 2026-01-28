using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Reporting
{
    public class ReportDistributionRequest : ModelEntityBase
    {
        private List<string> RecipientEmailsValue = new();

        public List<string> RecipientEmails

        {

            get { return this.RecipientEmailsValue; }

            set { SetProperty(ref RecipientEmailsValue, value); }

        }
        private string DistributionMethodValue = "Email";

        public string DistributionMethod

        {

            get { return this.DistributionMethodValue; }

            set { SetProperty(ref DistributionMethodValue, value); }

        }
        private string FormatValue = "PDF";

        public string Format

        {

            get { return this.FormatValue; }

            set { SetProperty(ref FormatValue, value); }

        }
    }
}
