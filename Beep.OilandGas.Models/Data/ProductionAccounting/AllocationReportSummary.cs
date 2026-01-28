using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AllocationReportSummary : ModelEntityBase
    {
        private int WellsAllocatedValue;

        public int WellsAllocated

        {

            get { return this.WellsAllocatedValue; }

            set { SetProperty(ref WellsAllocatedValue, value); }

        }
        private int LeasesAllocatedValue;

        public int LeasesAllocated

        {

            get { return this.LeasesAllocatedValue; }

            set { SetProperty(ref LeasesAllocatedValue, value); }

        }
        private decimal TotalAllocatedVolumeValue;

        public decimal TotalAllocatedVolume

        {

            get { return this.TotalAllocatedVolumeValue; }

            set { SetProperty(ref TotalAllocatedVolumeValue, value); }

        }
    }
}
