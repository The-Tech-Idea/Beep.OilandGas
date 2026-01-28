using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pricing
{
    public class ValueRunTicketRequest : ModelEntityBase
    {
        private string RunTicketNumberValue;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private string PricingMethodValue;

        public string PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        } // Fixed, IndexBased, PostedPrice, Regulated
        private decimal? FixedPriceValue;

        public decimal? FixedPrice

        {

            get { return this.FixedPriceValue; }

            set { SetProperty(ref FixedPriceValue, value); }

        }
        private string IndexNameValue;

        public string IndexName

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
        private string RegulatoryAuthorityValue;

        public string RegulatoryAuthority

        {

            get { return this.RegulatoryAuthorityValue; }

            set { SetProperty(ref RegulatoryAuthorityValue, value); }

        }
    }
}
