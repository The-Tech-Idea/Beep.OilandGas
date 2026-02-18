using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AllocationResult : ModelEntityBase
    {
        private string AllocationIdValue = string.Empty;
        public string AllocationId
        {
            get { return this.AllocationIdValue; }
            set { SetProperty(ref AllocationIdValue, value); }
        }

        private DateTime AllocationDateValue;
        public DateTime AllocationDate
        {
            get { return this.AllocationDateValue; }
            set { SetProperty(ref AllocationDateValue, value); }
        }

        private AllocationMethod MethodValue;
        public AllocationMethod Method
        {
            get { return this.MethodValue; }
            set { SetProperty(ref MethodValue, value); }
        }

        private decimal TotalVolumeValue;
        public decimal TotalVolume
        {
            get { return this.TotalVolumeValue; }
            set { SetProperty(ref TotalVolumeValue, value); }
        }

        private List<AllocationDetail> DetailsValue = new();
        public List<AllocationDetail> Details
        {
            get { return this.DetailsValue; }
            set { SetProperty(ref DetailsValue, value); }
        }

        public decimal AllocatedVolume => Details?.Sum(d => d.AllocatedVolume) ?? 0;
        public decimal AllocationVariance => TotalVolume - AllocatedVolume;
    }
}
