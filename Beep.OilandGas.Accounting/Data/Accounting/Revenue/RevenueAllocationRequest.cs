using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Revenue
{
    public class RevenueAllocationRequest : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private decimal TotalRevenueValue;

        public decimal TotalRevenue

        {

            get { return this.TotalRevenueValue; }

            set { SetProperty(ref TotalRevenueValue, value); }

        }
        private DateTime AllocationDateValue;

        public DateTime AllocationDate

        {

            get { return this.AllocationDateValue; }

            set { SetProperty(ref AllocationDateValue, value); }

        }
        private string? RevenueTransactionIdValue;

        public string? RevenueTransactionId

        {

            get { return this.RevenueTransactionIdValue; }

            set { SetProperty(ref RevenueTransactionIdValue, value); }

        }
        private string? AllocationMethodValue;

        public string? AllocationMethod

        {

            get { return this.AllocationMethodValue; }

            set { SetProperty(ref AllocationMethodValue, value); }

        }
        private List<WorkingInterestRequest>? WorkingInterestsValue;

        public List<WorkingInterestRequest>? WorkingInterests

        {

            get { return this.WorkingInterestsValue; }

            set { SetProperty(ref WorkingInterestsValue, value); }

        }
    }
}
