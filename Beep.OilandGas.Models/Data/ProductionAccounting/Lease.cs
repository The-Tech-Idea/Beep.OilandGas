using System;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    // NOTE: LeaseProvisions and LeaseLocation are defined in LeaseModelsDto.cs
    // LeaseType enum is also defined in LeaseModelsDto.cs
    // This file contains DTO and request classes for lease operations.

    /// <summary>
    /// DTO for lease agreement data
    /// </summary>
    public class Lease : ModelEntityBase
    {
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string LeaseNameValue = string.Empty;

        public string LeaseName

        {

            get { return this.LeaseNameValue; }

            set { SetProperty(ref LeaseNameValue, value); }

        }
        private LeaseType LeaseTypeValue;

        public LeaseType LeaseType

        {

            get { return this.LeaseTypeValue; }

            set { SetProperty(ref LeaseTypeValue, value); }

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
        private int PrimaryTermMonthsValue;

        public int PrimaryTermMonths

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
        private LeaseProvisions? ProvisionsValue;

        public LeaseProvisions? Provisions

        {

            get { return this.ProvisionsValue; }

            set { SetProperty(ref ProvisionsValue, value); }

        }
        private LeaseLocation? LocationValue;

        public LeaseLocation? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
    }

    /// <summary>
    /// Request to create or update a lease
    /// </summary>
    public class CreateLeaseRequest : ModelEntityBase
    {
        private string LeaseIdValue = string.Empty;

        [Required]
        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string LeaseNameValue = string.Empty;

        [Required]
        public string LeaseName

        {

            get { return this.LeaseNameValue; }

            set { SetProperty(ref LeaseNameValue, value); }

        }
        private LeaseType LeaseTypeValue;

        [Required]
        public LeaseType LeaseType

        {

            get { return this.LeaseTypeValue; }

            set { SetProperty(ref LeaseTypeValue, value); }

        }
        private DateTime EffectiveDateValue;

        [Required]
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
        private int PrimaryTermMonthsValue;

        [Range(1, int.MaxValue)]
        public int PrimaryTermMonths

        {

            get { return this.PrimaryTermMonthsValue; }

            set { SetProperty(ref PrimaryTermMonthsValue, value); }

        }
        private decimal WorkingInterestValue;

        [Range(0, 1)]
        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        [Range(0, 1)]
        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private decimal RoyaltyRateValue;

        [Range(0, 1)]
        public decimal RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }
        private LeaseProvisions? ProvisionsValue;

        public LeaseProvisions? Provisions

        {

            get { return this.ProvisionsValue; }

            set { SetProperty(ref ProvisionsValue, value); }

        }
        private LeaseLocation? LocationValue;

        public LeaseLocation? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
    }
}








