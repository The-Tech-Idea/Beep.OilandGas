using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CreateRoyaltyPaymentRequest : ModelEntityBase
    {
        private string RoyaltyOwnerIdValue = string.Empty;

        [Required]
        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private decimal RoyaltyAmountValue;

        [Required]
        public decimal RoyaltyAmount

        {

            get { return this.RoyaltyAmountValue; }

            set { SetProperty(ref RoyaltyAmountValue, value); }

        }
        private decimal? TaxWithholdingsValue;

        public decimal? TaxWithholdings

        {

            get { return this.TaxWithholdingsValue; }

            set { SetProperty(ref TaxWithholdingsValue, value); }

        }
        private DateTime PaymentDateValue;

        [Required]
        public DateTime PaymentDate

        {

            get { return this.PaymentDateValue; }

            set { SetProperty(ref PaymentDateValue, value); }

        }
        private string PaymentMethodValue = string.Empty;

        [Required]
        public string PaymentMethod

        {

            get { return this.PaymentMethodValue; }

            set { SetProperty(ref PaymentMethodValue, value); }

        }
    }
}
