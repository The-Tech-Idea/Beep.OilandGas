using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Reporting
{
    public class ReportDistributionResult : ModelEntityBase
    {
        private string ReportIdValue;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private bool IsDistributedValue;

        public bool IsDistributed

        {

            get { return this.IsDistributedValue; }

            set { SetProperty(ref IsDistributedValue, value); }

        }
        private DateTime DistributionDateValue = DateTime.UtcNow;

        public DateTime DistributionDate

        {

            get { return this.DistributionDateValue; }

            set { SetProperty(ref DistributionDateValue, value); }

        }
        private int RecipientCountValue;

        public int RecipientCount

        {

            get { return this.RecipientCountValue; }

            set { SetProperty(ref RecipientCountValue, value); }

        }
        private List<string> DistributionErrorsValue = new();

        public List<string> DistributionErrors

        {

            get { return this.DistributionErrorsValue; }

            set { SetProperty(ref DistributionErrorsValue, value); }

        }
    }
}
