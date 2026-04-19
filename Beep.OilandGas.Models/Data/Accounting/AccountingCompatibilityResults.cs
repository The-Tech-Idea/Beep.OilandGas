using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public enum ReconciliationStatus
    {
        Matched,
        VarianceDetected,
        Error
    }

    public class VolumeBreakdownResult : ModelEntityBase
    {
        public decimal FieldVolume { get; set; }
        public decimal ALLOCATED_VOLUME { get; set; }
        public decimal Discrepancy { get; set; }
    }

    public class VolumeReconciliationIssue : ModelEntityBase
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }

    public class VolumeReconciliationResult : ModelEntityBase
    {
        public ReconciliationStatus Status { get; set; }
        public decimal FieldProductionVolume { get; set; }
        public decimal ALLOCATED_VOLUME { get; set; }
        public decimal Discrepancy { get; set; }
        public decimal DiscrepancyPercentage { get; set; }
        public VolumeBreakdownResult? OilVolume { get; set; }
        public VolumeBreakdownResult? GasVolume { get; set; }
        public List<VolumeReconciliationIssue> Issues { get; set; } = new();
    }

    public class CostAllocationBreakdown : ModelEntityBase
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public decimal AllocatedOperatingCost { get; set; }
        public decimal AllocatedCapitalCost { get; set; }
        public decimal TotalAllocatedCost { get; set; }
        public decimal AllocationPercentage { get; set; }
    }

    public class CostAllocationComputationResult : ModelEntityBase
    {
        public decimal? TotalOperatingCosts { get; set; }
        public decimal? TotalCapitalCosts { get; set; }
        public List<CostAllocationBreakdown> AllocationDetails { get; set; } = new();
    }

    public class ProductionRoyaltyCalculationResult : ModelEntityBase
    {
        public decimal? GrossOilVolume { get; set; }
        public decimal? GrossGasVolume { get; set; }
        public decimal? RoyaltyOilVolume { get; set; }
        public decimal? RoyaltyGasVolume { get; set; }
        public decimal? OilRoyaltyRate { get; set; }
        public decimal? GasRoyaltyRate { get; set; }
    }
}