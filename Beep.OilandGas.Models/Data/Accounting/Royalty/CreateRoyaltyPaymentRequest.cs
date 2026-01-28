using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Royalty
{
    public class CreateRoyaltyPaymentRequest : ModelEntityBase
    {
        private string RoyaltyOwnerIdValue = string.Empty;

        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private decimal RoyaltyAmountValue;

        public decimal RoyaltyAmount

        {

            get { return this.RoyaltyAmountValue; }

            set { SetProperty(ref RoyaltyAmountValue, value); }

        }
        private decimal? RoyaltyInterestValue;

        public decimal? ROYALTY_INTEREST

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }
        private DateTime? PaymentDateValue;

        public DateTime? PaymentDate

        {

            get { return this.PaymentDateValue; }

            set { SetProperty(ref PaymentDateValue, value); }

        }
    }
}
