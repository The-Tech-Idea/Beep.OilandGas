using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.ProductionAccounting.Allocation
{
    /// <summary>
    /// Allocation method enumeration for internal calculation use.
    /// </summary>
    public enum AllocationMethod
    {
        /// <summary>
        /// Equal allocation to all parties.
        /// </summary>
        Equal,

        /// <summary>
        /// Pro-rata allocation based on working interest.
        /// </summary>
        ProRataWorkingInterest,

        /// <summary>
        /// Pro-rata allocation based on net revenue interest.
        /// </summary>
        ProRataNetRevenueInterest,

        /// <summary>
        /// Measured allocation based on test data.
        /// </summary>
        Measured,

        /// <summary>
        /// Estimated allocation based on production history.
        /// </summary>
        Estimated
    }

    /// <summary>
    /// Represents an allocation result for internal calculations.
    /// This is the computational model used by AllocationEngine, distinct from the PPDM entity ALLOCATION_RESULT.
    /// </summary>
    public class AllocationResult
    {
        /// <summary>
        /// Gets or sets the allocation identifier.
        /// </summary>
        public string AllocationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the allocation date.
        /// </summary>
        public DateTime AllocationDate { get; set; }

        /// <summary>
        /// Gets or sets the allocation method used.
        /// </summary>
        public AllocationMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the total volume allocated.
        /// </summary>
        public decimal TotalVolume { get; set; }

        /// <summary>
        /// Gets or sets the allocation details.
        /// </summary>
        public List<AllocationDetail> Details { get; set; } = new();

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
    public class AllocationDetail
    {
        /// <summary>
        /// Gets or sets the entity identifier (well, lease, tract, etc.).
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity name.
        /// </summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the allocated volume in barrels.
        /// </summary>
        public decimal AllocatedVolume { get; set; }

        /// <summary>
        /// Gets or sets the allocation percentage (0-100).
        /// </summary>
        public decimal AllocationPercentage { get; set; }

        /// <summary>
        /// Gets or sets the basis for allocation (working interest, net revenue interest, etc.).
        /// </summary>
        public decimal AllocationBasis { get; set; }
    }

    /// <summary>
    /// Represents well allocation data for calculation.
    /// </summary>
    public class WellAllocationData
    {
        /// <summary>
        /// Gets or sets the well identifier.
        /// </summary>
        public string WellId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the lease identifier.
        /// </summary>
        public string LeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working interest.
        /// </summary>
        public decimal WorkingInterest { get; set; }

        /// <summary>
        /// Gets or sets the net revenue interest.
        /// </summary>
        public decimal NetRevenueInterest { get; set; }

        /// <summary>
        /// Gets or sets the measured production (if available).
        /// </summary>
        public decimal? MeasuredProduction { get; set; }

        /// <summary>
        /// Gets or sets the estimated production (if measured not available).
        /// </summary>
        public decimal? EstimatedProduction { get; set; }
    }

    /// <summary>
    /// Represents lease allocation data for calculation.
    /// </summary>
    public class LeaseAllocationData
    {
        /// <summary>
        /// Gets or sets the lease identifier.
        /// </summary>
        public string LeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working interest.
        /// </summary>
        public decimal WorkingInterest { get; set; }

        /// <summary>
        /// Gets or sets the net revenue interest.
        /// </summary>
        public decimal NetRevenueInterest { get; set; }

        /// <summary>
        /// Gets or sets the measured production (if available).
        /// </summary>
        public decimal? MeasuredProduction { get; set; }
    }

    /// <summary>
    /// Represents tract allocation data for calculation.
    /// </summary>
    public class TractAllocationData
    {
        /// <summary>
        /// Gets or sets the tract identifier.
        /// </summary>
        public string TractId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        public string UnitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tract participation percentage.
        /// </summary>
        public decimal TractParticipation { get; set; }

        /// <summary>
        /// Gets or sets the working interest.
        /// </summary>
        public decimal WorkingInterest { get; set; }

        /// <summary>
        /// Gets or sets the net revenue interest.
        /// </summary>
        public decimal NetRevenueInterest { get; set; }
    }
}

