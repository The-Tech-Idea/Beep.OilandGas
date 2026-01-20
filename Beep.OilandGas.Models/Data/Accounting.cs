using System;
using System.Collections.Generic;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    #region Volume Reconciliation DTOs

    /// <summary>
    /// Result of volume reconciliation
    /// </summary>
    public class VolumeReconciliationResult : ModelEntityBase
    {
        public string ReconciliationId { get; set; } = Guid.NewGuid().ToString();
        public string? FieldId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ReconciliationDate { get; set; } = DateTime.UtcNow;

        // Volume comparisons
        public decimal? FieldProductionVolume { get; set; } // Total production from field
        public decimal? AllocatedVolume { get; set; } // Total allocated to wells/pools
        public decimal? Discrepancy { get; set; } // Difference between field and allocated
        public decimal? DiscrepancyPercentage { get; set; } // Percentage discrepancy

        // Volume breakdowns by product
        public VolumeBreakdown? OilVolume { get; set; }
        public VolumeBreakdown? GasVolume { get; set; }
        public VolumeBreakdown? WaterVolume { get; set; }
        public VolumeBreakdown? CondensateVolume { get; set; }

        // Reconciliation status
        public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Pending;
        public List<ReconciliationIssue> Issues { get; set; } = new List<ReconciliationIssue>();

        // Additional metadata
        public string? UserId { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    /// <summary>
    /// Volume breakdown for reconciliation
    /// </summary>
    public class VolumeBreakdown : ModelEntityBase
    {
        public decimal? FieldVolume { get; set; }
        public decimal? AllocatedVolume { get; set; }
        public decimal? Discrepancy { get; set; }
        public decimal? DiscrepancyPercentage { get; set; }
        public List<AllocationDetail>? AllocationDetails { get; set; }
    }

    /// <summary>
    /// Allocation detail for a specific entity (well, pool, facility)
    /// </summary>
    public class AllocationDetail : ModelEntityBase
    {
        public string? EntityId { get; set; } // Well ID, Pool ID, or Facility ID
        public string? EntityType { get; set; } // WELL, POOL, FACILITY
        public string? EntityName { get; set; }
        public decimal? AllocatedVolume { get; set; }
        public decimal? AllocationPercentage { get; set; }
    }

    /// <summary>
    /// Reconciliation issue/problem
    /// </summary>
    public class ReconciliationIssue : ModelEntityBase
    {
        public string IssueType { get; set; } = string.Empty; // DISCREPANCY, MISSING_DATA, INVALID_ALLOCATION
        public string Severity { get; set; } = string.Empty; // ERROR, WARNING, INFO
        public string Description { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string? EntityType { get; set; }
    }

    public enum ReconciliationStatus
    {
        Pending,
        InProgress,
        Reconciled,
        Discrepancies,
        Failed
    }

    #endregion

    #region Royalty Calculation DTOs

    /// <summary>
    /// Result of royalty calculation
    /// </summary>
    public class RoyaltyCalculationResult : ModelEntityBase
    {
        public string CalculationId { get; set; } = Guid.NewGuid().ToString();
        public string? FieldId { get; set; }
        public string? PoolId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;

        // Production volumes
        public decimal? GrossOilVolume { get; set; }
        public decimal? GrossGasVolume { get; set; }
        public decimal? GrossWaterVolume { get; set; }

        // Royalty rates
        public decimal? OilRoyaltyRate { get; set; } // As percentage (e.g., 12.5)
        public decimal? GasRoyaltyRate { get; set; } // As percentage

        // Royalty volumes
        public decimal? RoyaltyOilVolume { get; set; }
        public decimal? RoyaltyGasVolume { get; set; }

        // Net production (gross minus royalty)
        public decimal? NetOilVolume { get; set; }
        public decimal? NetGasVolume { get; set; }

        // Pricing (for royalty value calculation)
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }

        // Royalty values
        public decimal? RoyaltyOilValue { get; set; }
        public decimal? RoyaltyGasValue { get; set; }
        public decimal? TotalRoyaltyValue { get; set; }

        // Additional metadata
        public string? RoyaltyOwnerId { get; set; }
        public string? RoyaltyType { get; set; } // OVERRIDING, WORKING_INTEREST, etc.
        public string? UserId { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    #endregion

    #region Cost Allocation DTOs

    /// <summary>
    /// Result of cost allocation
    /// </summary>
    public class CostAllocationResult : ModelEntityBase
    {
        public string AllocationId { get; set; } = Guid.NewGuid().ToString();
        public string? FieldId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AllocationDate { get; set; } = DateTime.UtcNow;
        public CostAllocationMethod AllocationMethod { get; set; }

        // Total costs to allocate
        public decimal? TotalOperatingCosts { get; set; }
        public decimal? TotalCapitalCosts { get; set; }
        public decimal? TotalCosts { get; set; }

        // Allocation details by entity
        public List<CostAllocationDetail> AllocationDetails { get; set; } = new List<CostAllocationDetail>();

        // Additional metadata
        public string? UserId { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    /// <summary>
    /// Cost allocation detail for a specific entity
    /// </summary>
    public class CostAllocationDetail : ModelEntityBase
    {
        public string? EntityId { get; set; } // Well ID, Pool ID, or Facility ID
        public string? EntityType { get; set; } // WELL, POOL, FACILITY
        public string? EntityName { get; set; }

        // Allocated costs
        public decimal? AllocatedOperatingCost { get; set; }
        public decimal? AllocatedCapitalCost { get; set; }
        public decimal? TotalAllocatedCost { get; set; }

        // Allocation basis (for reference)
        public decimal? AllocationBasisValue { get; set; } // Volume, revenue, etc. used for allocation
        public string? AllocationBasisType { get; set; } // VOLUME, REVENUE, FIXED, etc.
        public decimal? AllocationPercentage { get; set; } // Percentage of total
    }

    /// <summary>
    /// Cost allocation methods
    /// </summary>
    public enum CostAllocationMethod
    {
        VolumeBased,      // Allocate based on production volumes
        RevenueBased,     // Allocate based on revenue
        FixedPercentage,  // Allocate based on fixed percentages
        EqualShare,       // Equal allocation to all entities
        Manual            // Manually specified allocation
    }

    #endregion

    #region Accounting Allocation class DTOs

    /// <summary>
    /// Request to create/update an accounting allocation class
    /// </summary>
    public class AccountingAllocationRequest : ModelEntityBase
    {
        public string? FieldId { get; set; }
        public string? PoolId { get; set; }
        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        public DateTime AllocationDate { get; set; }
        public string? ProductType { get; set; } // OIL, GAS, WATER, CONDENSATE
        public decimal? AllocatedVolume { get; set; }
        public decimal? AllocationPercentage { get; set; }
        public string? AllocationMethod { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    /// <summary>
    /// Request to create/update a royalty calculation class
    /// </summary>
    public class RoyaltyCalculationRequest : ModelEntityBase
    {
        public string? FieldId { get; set; }
        public string? PoolId { get; set; }
        public DateTime CalculationDate { get; set; }
        public decimal? GrossOilVolume { get; set; }
        public decimal? GrossGasVolume { get; set; }
        public decimal? OilRoyaltyRate { get; set; }
        public decimal? GasRoyaltyRate { get; set; }
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }
        public string? RoyaltyOwnerId { get; set; }
        public string? RoyaltyType { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    /// <summary>
    /// Request to create/update a cost allocation class
    /// </summary>
    public class CostAllocationRequest : ModelEntityBase
    {
        public string? FieldId { get; set; }
        public DateTime AllocationDate { get; set; }
        public CostAllocationMethod AllocationMethod { get; set; }
        public decimal? TotalOperatingCosts { get; set; }
        public decimal? TotalCapitalCosts { get; set; }
        public List<CostAllocationDetail>? AllocationDetails { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    #endregion

    #region Accounting class DTOs (for tables that may not exist in PPDM models)

    /// <summary>
    /// Accounting allocation class (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class ACCOUNTING_ALLOCATION : ModelEntityBase
    {
        public string? ACCOUNTING_ALLOCATION_ID { get; set; }
        public string? FIELD_ID { get; set; }
        public string? POOL_ID { get; set; }
        public string? WELL_ID { get; set; }
        public DateTime? ALLOCATION_DATE { get; set; }
        public string? PRODUCT_TYPE { get; set; }
        public decimal? ALLOCATED_VOLUME { get; set; }
        public decimal? ALLOCATION_PERCENTAGE { get; set; }
        public string? ALLOCATION_METHOD { get; set; }
        public string? ACTIVE_IND { get; set; }
        public string? REMARK { get; set; }
        public string? SOURCE { get; set; }
        public string? ROW_QUALITY { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string? CREATE_USER { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
        public string? UPDATE_USER { get; set; }
    }

    /// <summary>
    /// Royalty calculation class (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class ROYALTY_CALCULATION : ModelEntityBase
    {
        public string? ROYALTY_CALCULATION_ID { get; set; }
        public string? FIELD_ID { get; set; }
        public string? POOL_ID { get; set; }
        public DateTime? CALCULATION_DATE { get; set; }
        public decimal? GROSS_OIL_VOLUME { get; set; }
        public decimal? GROSS_GAS_VOLUME { get; set; }
        public decimal? OIL_ROYALTY_RATE { get; set; }
        public decimal? GAS_ROYALTY_RATE { get; set; }
        public decimal? OIL_PRICE { get; set; }
        public decimal? GAS_PRICE { get; set; }
        public string? ROYALTY_OWNER_ID { get; set; }
        public string? ROYALTY_TYPE { get; set; }
        public string? ACTIVE_IND { get; set; }
        public string? REMARK { get; set; }
        public string? SOURCE { get; set; }
        public string? ROW_QUALITY { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string? CREATE_USER { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
        public string? UPDATE_USER { get; set; }
    }

    /// <summary>
    /// Cost allocation class (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class COST_ALLOCATION : ModelEntityBase
    {
        public string? COST_ALLOCATION_ID { get; set; }
        public string? FIELD_ID { get; set; }
        public DateTime? ALLOCATION_DATE { get; set; }
        public string? ALLOCATION_METHOD { get; set; }
        public decimal? TOTAL_OPERATING_COSTS { get; set; }
        public decimal? TOTAL_CAPITAL_COSTS { get; set; }
        public decimal? TOTAL_COSTS { get; set; }
        public string? ACTIVE_IND { get; set; }
        public string? REMARK { get; set; }
        public string? SOURCE { get; set; }
        public string? ROW_QUALITY { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string? CREATE_USER { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
        public string? UPDATE_USER { get; set; }
    }

    /// <summary>
    /// Accounting impairment class (DTO - used when PPDM model class doesn't exist)
    /// </summary>
    public class ACCOUNTING_IMPAIRMENT : ModelEntityBase
    {
        public string? ACCOUNTING_IMPAIRMENT_ID { get; set; }
        public string? PROPERTY_ID { get; set; }
        public DateTime? IMPAIRMENT_DATE { get; set; }
        public decimal? IMPAIRMENT_AMOUNT { get; set; }
        public string? REASON { get; set; }
        public string? ACTIVE_IND { get; set; }
        public string? REMARK { get; set; }
        public string? SOURCE { get; set; }
        public string? ROW_QUALITY { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        public string? CREATE_USER { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
        public string? UPDATE_USER { get; set; }
    }

    #endregion
}






