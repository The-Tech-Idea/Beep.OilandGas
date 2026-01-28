using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class WellAllocationData : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the well identifier.
        /// </summary>
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the lease identifier.
        /// </summary>
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest.
        /// </summary>
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest.
        /// </summary>
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the measured production (if available).
        /// </summary>
        private decimal? MeasuredProductionValue;

        public decimal? MeasuredProduction

        {

            get { return this.MeasuredProductionValue; }

            set { SetProperty(ref MeasuredProductionValue, value); }

        }

        /// <summary>
        /// Gets or sets the estimated production (if measured not available).
        /// </summary>
        private decimal? EstimatedProductionValue;

        public decimal? EstimatedProduction

        {

            get { return this.EstimatedProductionValue; }

            set { SetProperty(ref EstimatedProductionValue, value); }

        }
    }
}
