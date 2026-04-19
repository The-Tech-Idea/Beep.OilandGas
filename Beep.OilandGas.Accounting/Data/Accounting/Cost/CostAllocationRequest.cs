using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Cost
{
    public class CostAllocationRequest : ModelEntityBase
    {
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime AllocationDateValue;

        public DateTime AllocationDate

        {

            get { return this.AllocationDateValue; }

            set { SetProperty(ref AllocationDateValue, value); }

        }
        private string AllocationMethodValue = string.Empty;

        public string AllocationMethod

        {

            get { return this.AllocationMethodValue; }

            set { SetProperty(ref AllocationMethodValue, value); }

        }
        private decimal? TotalOperatingCostsValue;

        public decimal? TotalOperatingCosts

        {

            get { return this.TotalOperatingCostsValue; }

            set { SetProperty(ref TotalOperatingCostsValue, value); }

        }
        private decimal? TotalCapitalCostsValue;

        public decimal? TotalCapitalCosts

        {

            get { return this.TotalCapitalCostsValue; }

            set { SetProperty(ref TotalCapitalCostsValue, value); }

        }
    }
}
