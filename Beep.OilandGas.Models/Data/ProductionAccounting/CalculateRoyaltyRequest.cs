using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CalculateRoyaltyRequest : ModelEntityBase
    {
        private string PropertyOrLeaseIdValue = string.Empty;

        [Required]
        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private string RoyaltyOwnerIdValue = string.Empty;

        [Required]
        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private decimal GrossRevenueValue;

        [Required]
        public decimal GrossRevenue

        {

            get { return this.GrossRevenueValue; }

            set { SetProperty(ref GrossRevenueValue, value); }

        }
        private RoyaltyDeductions? DeductionsValue;

        public RoyaltyDeductions? Deductions

        {

            get { return this.DeductionsValue; }

            set { SetProperty(ref DeductionsValue, value); }

        }
        private decimal RoyaltyInterestValue;

        [Required]
        [Range(0, 1)]
        public decimal RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }
        private DateTime CalculationDateValue;

        [Required]
        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
    }
}
