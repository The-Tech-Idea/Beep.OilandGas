using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class RoyaltyInterest : ModelEntityBase
    {
        private string RoyaltyInterestIdValue = string.Empty;

        public string RoyaltyInterestId

        {

            get { return this.RoyaltyInterestIdValue; }

            set { SetProperty(ref RoyaltyInterestIdValue, value); }

        }
        private string RoyaltyOwnerIdValue = string.Empty;

        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private string PropertyOrLeaseIdValue = string.Empty;

        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private decimal InterestPercentageValue;

        public decimal InterestPercentage

        {

            get { return this.InterestPercentageValue; }

            set { SetProperty(ref InterestPercentageValue, value); }

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
        private string? DivisionOrderIdValue;

        public string? DivisionOrderId

        {

            get { return this.DivisionOrderIdValue; }

            set { SetProperty(ref DivisionOrderIdValue, value); }

        }
    }
}
