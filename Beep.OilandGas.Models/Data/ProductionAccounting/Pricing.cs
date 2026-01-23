using System;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    // NOTE: RUN_TICKET_VALUATION, QualityAdjustments, LocationAdjustments, TimeAdjustments, and PriceIndex 
    // are defined in PricingModelsDto.cs
    // This file contains request classes for pricing operations.

    /// <summary>
    /// Request to value a run ticket
    /// </summary>
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

    /// <summary>
    /// Request to add or update price index
    /// </summary>
    public class PriceIndexRequest : ModelEntityBase
    {
        private string IndexNameValue = string.Empty;

        [Required]
        public string IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }
        private DateTime IndexDateValue;

        [Required]
        public DateTime IndexDate

        {

            get { return this.IndexDateValue; }

            set { SetProperty(ref IndexDateValue, value); }

        }
        private decimal PriceValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price

        {

            get { return this.PriceValue; }

            set { SetProperty(ref PriceValue, value); }

        }
        private string? CurrencyValue = "USD";

        public string? Currency

        {

            get { return this.CurrencyValue; }

            set { SetProperty(ref CurrencyValue, value); }

        }
    }
}








