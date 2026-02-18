using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AllocationDetail : ModelEntityBase
    {
        private string DetailIdValue = string.Empty;
        public string DetailId
        {
            get { return this.DetailIdValue; }
            set { SetProperty(ref DetailIdValue, value); }
        }

        private string EntityIdValue = string.Empty;
        public string EntityId
        {
            get { return this.EntityIdValue; }
            set { SetProperty(ref EntityIdValue, value); }
        }

        private decimal AllocatedVolumeValue;
        public decimal AllocatedVolume
        {
            get { return this.AllocatedVolumeValue; }
            set { SetProperty(ref AllocatedVolumeValue, value); }
        }

        private decimal PercentageValue;
        public decimal Percentage
        {
            get { return this.PercentageValue; }
            set { SetProperty(ref PercentageValue, value); }
        }
    }
}
