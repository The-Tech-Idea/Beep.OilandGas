using System;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class OWNERSHIP_INTEREST
    {
        public string InterestId
        {
            get => OWNERSHIP_ID;
            set => OWNERSHIP_ID = value;
        }

        public string PropertyOrLeaseId
        {
            get => PROPERTY_OR_LEASE_ID;
            set => PROPERTY_OR_LEASE_ID = value;
        }

        public string OwnerId
        {
            get => OWNER_ID;
            set => OWNER_ID = value;
        }

        public decimal? WorkingInterest
        {
            get => WORKING_INTEREST;
            set => WORKING_INTEREST = value;
        }

        public decimal? NetRevenueInterest
        {
            get => NET_REVENUE_INTEREST;
            set => NET_REVENUE_INTEREST = value;
        }

        public DateTime? EffectiveDate
        {
            get => EFFECTIVE_START_DATE;
            set => EFFECTIVE_START_DATE = value;
        }

        public DateTime? ExpirationDate
        {
            get => EFFECTIVE_END_DATE;
            set => EFFECTIVE_END_DATE = value;
        }
    }

}