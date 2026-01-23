using System;
using System.Collections.Generic;
using System.Linq;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Allocation method enumeration.
    /// </summary>


    /// <summary>
    /// Represents an allocation result (DTO for calculations/reporting).
    /// </summary>
    public class AllocationResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the allocation identifier.
        /// </summary>
        private string AllocationIdValue = string.Empty;

        public string AllocationId

        {

            get { return this.AllocationIdValue; }

            set { SetProperty(ref AllocationIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation date.
        /// </summary>
        private DateTime AllocationDateValue;

        public DateTime AllocationDate

        {

            get { return this.AllocationDateValue; }

            set { SetProperty(ref AllocationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation method used.
        /// </summary>
        private AllocationMethod MethodValue;

        public AllocationMethod Method

        {

            get { return this.MethodValue; }

            set { SetProperty(ref MethodValue, value); }

        }

        /// <summary>
        /// Gets or sets the total volume allocated.
        /// </summary>
        private decimal TotalVolumeValue;

        public decimal TotalVolume

        {

            get { return this.TotalVolumeValue; }

            set { SetProperty(ref TotalVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation details.
        /// </summary>
        private List<AllocationDetail> DetailsValue = new();

        public List<AllocationDetail> Details

        {

            get { return this.DetailsValue; }

            set { SetProperty(ref DetailsValue, value); }

        }

        /// <summary>
        /// Gets the sum of all allocated volumes (should equal TotalVolume).
        /// </summary>
        public decimal AllocatedVolume => Details.Sum(d => d.AllocatedVolume);

        /// <summary>
        /// Gets the allocation variance.
        /// </summary>
        public decimal AllocationVariance => TotalVolume - AllocatedVolume;
    }

    /// <summary>
    /// Represents an allocation detail for a specific entity.
    /// </summary>
    public class AllocationDetail : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the entity identifier (well, lease, tract, etc.).
        /// </summary>
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the entity name.
        /// </summary>
        private string EntityNameValue = string.Empty;

        public string EntityName

        {

            get { return this.EntityNameValue; }

            set { SetProperty(ref EntityNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocated volume in barrels.
        /// </summary>
        private decimal AllocatedVolumeValue;

        public decimal AllocatedVolume

        {

            get { return this.AllocatedVolumeValue; }

            set { SetProperty(ref AllocatedVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation percentage (0-100).
        /// </summary>
        private decimal AllocationPercentageValue;

        public decimal AllocationPercentage

        {

            get { return this.AllocationPercentageValue; }

            set { SetProperty(ref AllocationPercentageValue, value); }

        }

        /// <summary>
        /// Gets or sets the basis for allocation (working interest, net revenue interest, etc.).
        /// </summary>
        private decimal AllocationBasisValue;

        public decimal AllocationBasis

        {

            get { return this.AllocationBasisValue; }

            set { SetProperty(ref AllocationBasisValue, value); }

        }
    }

    /// <summary>
    /// Represents well allocation data.
    /// </summary>
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

    /// <summary>
    /// Represents lease allocation data.
    /// </summary>
    public class LeaseAllocationData : ModelEntityBase
    {
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
    }

    /// <summary>
    /// Represents tract allocation data.
    /// </summary>
    public class TractAllocationData : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the tract identifier.
        /// </summary>
        private string TractIdValue = string.Empty;

        public string TractId

        {

            get { return this.TractIdValue; }

            set { SetProperty(ref TractIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        private string UnitIdValue = string.Empty;

        public string UnitId

        {

            get { return this.UnitIdValue; }

            set { SetProperty(ref UnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the tract participation percentage.
        /// </summary>
        private decimal TractParticipationValue;

        public decimal TractParticipation

        {

            get { return this.TractParticipationValue; }

            set { SetProperty(ref TractParticipationValue, value); }

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
    }
}








