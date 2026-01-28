using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Ownership
{
    public class CreateOwnershipInterestRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string OwnerBaIdValue;

        public string OwnerBaId

        {

            get { return this.OwnerBaIdValue; }

            set { SetProperty(ref OwnerBaIdValue, value); }

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
        private decimal? RoyaltyInterestValue;

        public decimal? ROYALTY_INTEREST

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }
        private decimal? OverridingRoyaltyInterestValue;

        public decimal? OverridingRoyaltyInterest

        {

            get { return this.OverridingRoyaltyInterestValue; }

            set { SetProperty(ref OverridingRoyaltyInterestValue, value); }

        }
        private DateTime EffectiveStartDateValue;

        public DateTime EffectiveStartDate

        {

            get { return this.EffectiveStartDateValue; }

            set { SetProperty(ref EffectiveStartDateValue, value); }

        }
        private DateTime? EffectiveEndDateValue;

        public DateTime? EffectiveEndDate

        {

            get { return this.EffectiveEndDateValue; }

            set { SetProperty(ref EffectiveEndDateValue, value); }

        }
        private string DivisionOrderIdValue;

        public string DivisionOrderId

        {

            get { return this.DivisionOrderIdValue; }

            set { SetProperty(ref DivisionOrderIdValue, value); }

        }
    }
}
