
namespace Beep.OilandGas.Models.Data.Allocation
{
    public class WellAllocationData : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? WellNameValue;

        public string? WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private decimal? AllocationPercentageValue;

        public decimal? AllocationPercentage

        {

            get { return this.AllocationPercentageValue; }

            set { SetProperty(ref AllocationPercentageValue, value); }

        }
        private decimal? ProductionVolumeValue;

        public decimal? ProductionVolume

        {

            get { return this.ProductionVolumeValue; }

            set { SetProperty(ref ProductionVolumeValue, value); }

        }
        private string? AllocationMethodValue;

        public string? AllocationMethod

        {

            get { return this.AllocationMethodValue; }

            set { SetProperty(ref AllocationMethodValue, value); }

        }
        private DateTime? AllocationDateValue;

        public DateTime? AllocationDate

        {

            get { return this.AllocationDateValue; }

            set { SetProperty(ref AllocationDateValue, value); }

        }
    }
}
