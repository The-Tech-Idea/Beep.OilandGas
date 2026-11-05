using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class CreateLeaseAcquisition : ModelEntityBase
    {
        public Guid CommandId { get; set; }
        public TheTechIdea.Data.OilGas.LeaseCreationContext? Context { get; set; }

        // Freeze the existing contract before any asynchronous authority/transport work.
        public CreateLeaseAcquisition Snapshot() => new()
        {
            CommandId = CommandId, Context = Context, CalculatedInterest = CalculatedInterest,
            PropertyId = PropertyId, LeaseType = LeaseType, LeaseNumber = LeaseNumber, LeaseName = LeaseName,
            EffectiveDate = EffectiveDate, ExpirationDate = ExpirationDate, PrimaryTermMonths = PrimaryTermMonths,
            WorkingInterest = WorkingInterest, NetRevenueInterest = NetRevenueInterest, RoyaltyRate = RoyaltyRate,
            MineralOwnerBaId = MineralOwnerBaId, SurfaceOwnerBaId = SurfaceOwnerBaId,
            GovernmentAgency = GovernmentAgency, IsFederal = IsFederal, IsIndian = IsIndian
        };
        // Explicit PPDM LAND_RIGHT value; no working-interest/financial interpretation is inferred.
        public decimal? CalculatedInterest { get; set; }
        private string LeaseTypeValue = string.Empty;

        public string LeaseType

        {

            get { return this.LeaseTypeValue; }

            set { SetProperty(ref LeaseTypeValue, value); }

        } // Exact reviewed R_LAND_AGREE_TYPE code.
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string LeaseNumberValue = string.Empty;

        public string LeaseNumber

        {

            get { return this.LeaseNumberValue; }

            set { SetProperty(ref LeaseNumberValue, value); }

        }
        private string LeaseNameValue = string.Empty;

        public string LeaseName

        {

            get { return this.LeaseNameValue; }

            set { SetProperty(ref LeaseNameValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private int? PrimaryTermMonthsValue;

        public int? PrimaryTermMonths

        {

            get { return this.PrimaryTermMonthsValue; }

            set { SetProperty(ref PrimaryTermMonthsValue, value); }

        }
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private decimal RoyaltyRateValue;

        public decimal RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }
        
        // Fee Mineral Lease specific
        private string? MineralOwnerBaIdValue;

        public string? MineralOwnerBaId

        {

            get { return this.MineralOwnerBaIdValue; }

            set { SetProperty(ref MineralOwnerBaIdValue, value); }

        }
        private string? SurfaceOwnerBaIdValue;

        public string? SurfaceOwnerBaId

        {

            get { return this.SurfaceOwnerBaIdValue; }

            set { SetProperty(ref SurfaceOwnerBaIdValue, value); }

        }
        
        // Government Lease specific
        private string? GovernmentAgencyValue;

        public string? GovernmentAgency

        {

            get { return this.GovernmentAgencyValue; }

            set { SetProperty(ref GovernmentAgencyValue, value); }

        }
        private bool? IsFederalValue;

        public bool? IsFederal

        {

            get { return this.IsFederalValue; }

            set { SetProperty(ref IsFederalValue, value); }

        }
        private bool? IsIndianValue;

        public bool? IsIndian

        {

            get { return this.IsIndianValue; }

            set { SetProperty(ref IsIndianValue, value); }

        }
    }
}
