using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class CreateLease : ModelEntityBase
    {
        private string LeaseNumberValue = string.Empty;

        public string LeaseNumber

        {

            get { return this.LeaseNumberValue; }

            set { SetProperty(ref LeaseNumberValue, value); }

        }
        private string? LessorIdValue;

        public string? LessorId

        {

            get { return this.LessorIdValue; }

            set { SetProperty(ref LessorIdValue, value); }

        }
        private string? LesseeIdValue;

        public string? LesseeId

        {

            get { return this.LesseeIdValue; }

            set { SetProperty(ref LesseeIdValue, value); }

        }
        private DateTime? LeaseDateValue;

        public DateTime? LeaseDate

        {

            get { return this.LeaseDateValue; }

            set { SetProperty(ref LeaseDateValue, value); }

        }
        private DateTime? EffectiveDateValue;

        public DateTime? EffectiveDate

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
        private decimal LeaseAreaValue;

        public decimal LeaseArea

        {

            get { return this.LeaseAreaValue; }

            set { SetProperty(ref LeaseAreaValue, value); }

        }
        private string? AreaUnitValue;

        public string? AreaUnit

        {

            get { return this.AreaUnitValue; }

            set { SetProperty(ref AreaUnitValue, value); }

        }
        private string? LocationValue;

        public string? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private string? LegalDescriptionValue;

        public string? LegalDescription

        {

            get { return this.LegalDescriptionValue; }

            set { SetProperty(ref LegalDescriptionValue, value); }

        }
        private decimal RoyaltyRateValue;

        public decimal RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }
        private decimal BonusPaymentValue;

        public decimal BonusPayment

        {

            get { return this.BonusPaymentValue; }

            set { SetProperty(ref BonusPaymentValue, value); }

        }
        private decimal AnnualRentalValue;

        public decimal AnnualRental

        {

            get { return this.AnnualRentalValue; }

            set { SetProperty(ref AnnualRentalValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }
}
