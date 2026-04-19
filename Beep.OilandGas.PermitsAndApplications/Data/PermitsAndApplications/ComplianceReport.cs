using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class ComplianceReport : ModelEntityBase
    {
        private DateTime GeneratedOnValue = DateTime.UtcNow;
        public DateTime GeneratedOn
        {
            get { return this.GeneratedOnValue; }
            set { SetProperty(ref GeneratedOnValue, value); }
        }

        private DateTime AsOfDateValue = DateTime.UtcNow;
        public DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private int ExpiringWithinDaysValue;
        public int ExpiringWithinDays
        {
            get { return this.ExpiringWithinDaysValue; }
            set { SetProperty(ref ExpiringWithinDaysValue, value); }
        }

        private List<ExpiringPermitRecord> ExpiringPermitsValue = new();
        public List<ExpiringPermitRecord> ExpiringPermits
        {
            get { return this.ExpiringPermitsValue; }
            set { SetProperty(ref ExpiringPermitsValue, value); }
        }

        private List<ComplianceStatusCount> StatusCountsValue = new();
        public List<ComplianceStatusCount> StatusCounts
        {
            get { return this.StatusCountsValue; }
            set { SetProperty(ref StatusCountsValue, value); }
        }

        private List<string> WarningsValue = new();
        public List<string> Warnings
        {
            get { return this.WarningsValue; }
            set { SetProperty(ref WarningsValue, value); }
        }
    }
}
