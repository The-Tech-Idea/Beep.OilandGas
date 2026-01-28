using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProductionAvails : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the avails identifier.
        /// </summary>
        private string AvailsIdValue = string.Empty;

        public string AvailsId

        {

            get { return this.AvailsIdValue; }

            set { SetProperty(ref AvailsIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the period start date.
        /// </summary>
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }

        /// <summary>
        /// Gets or sets the period end date.
        /// </summary>
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }

        /// <summary>
        /// Gets or sets the estimated production in barrels.
        /// </summary>
        private decimal EstimatedProductionValue;

        public decimal EstimatedProduction

        {

            get { return this.EstimatedProductionValue; }

            set { SetProperty(ref EstimatedProductionValue, value); }

        }

        /// <summary>
        /// Gets or sets the available for delivery in barrels.
        /// </summary>
        private decimal AvailableForDeliveryValue;

        public decimal AvailableForDelivery

        {

            get { return this.AvailableForDeliveryValue; }

            set { SetProperty(ref AvailableForDeliveryValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocations.
        /// </summary>
        private List<AvailsAllocation> AllocationsValue = new();

        public List<AvailsAllocation> Allocations

        {

            get { return this.AllocationsValue; }

            set { SetProperty(ref AllocationsValue, value); }

        }
    }
}
