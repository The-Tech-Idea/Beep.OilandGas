using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ValueRunTicketRequest : ModelEntityBase
    {
        private string RunTicketNumberValue = string.Empty;

        [Required]
        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private PricingMethod PricingMethodValue;

        [Required]
        public PricingMethod PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        }
        private decimal? FixedPriceValue;

        public decimal? FixedPrice

        {

            get { return this.FixedPriceValue; }

            set { SetProperty(ref FixedPriceValue, value); }

        }
        private string? IndexNameValue;

        public string? IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }
        private decimal? DifferentialValue;

        public decimal? Differential

        {

            get { return this.DifferentialValue; }

            set { SetProperty(ref DifferentialValue, value); }

        }
        private DateTime? ValuationDateValue;

        public DateTime? ValuationDate

        {

            get { return this.ValuationDateValue; }

            set { SetProperty(ref ValuationDateValue, value); }

        }
    }
}
