
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

        private decimal? WorkingInterestValue;

        public decimal? WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        private decimal? NetRevenueInterestValue;

        public decimal? NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        private decimal? MeasuredProductionValue;

        public decimal? MeasuredProduction

        {

            get { return this.MeasuredProductionValue; }

            set { SetProperty(ref MeasuredProductionValue, value); }

        }

        private decimal? EstimatedProductionValue;

        public decimal? EstimatedProduction

        {

            get { return this.EstimatedProductionValue; }

            set { SetProperty(ref EstimatedProductionValue, value); }

        }
    }
}
