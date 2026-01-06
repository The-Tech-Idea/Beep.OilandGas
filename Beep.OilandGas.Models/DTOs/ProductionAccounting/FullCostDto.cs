using System;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// Request to create a cost center.
    /// </summary>
    public class CreateCostCenterRequest
    {
        public string CostCenterId { get; set; } = string.Empty;
        public string CostCenterName { get; set; } = string.Empty;
        public string CostCenterType { get; set; } = string.Empty; // Country, Region, Field
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cost center rollup summary.
    /// </summary>
    public class CostCenterRollup
    {
        public string CostCenterId { get; set; } = string.Empty;
        public string CostCenterName { get; set; } = string.Empty;
        public decimal AcquisitionCosts { get; set; }
        public decimal ExplorationCosts { get; set; }
        public decimal DevelopmentCosts { get; set; }
        public decimal TotalCapitalizedCosts { get; set; }
        public decimal AccumulatedAmortization { get; set; }
        public decimal NetCapitalizedCosts { get; set; }
        public DateTime? AsOfDate { get; set; }
    }

    /// <summary>
    /// Result of a ceiling test.
    /// </summary>
    public class CeilingTestResult
    {
        public string CostCenterId { get; set; } = string.Empty;
        public decimal NetCapitalizedCosts { get; set; }
        public decimal PresentValueOfFutureNetRevenues { get; set; }
        public decimal Ceiling { get; set; }
        public bool ImpairmentNeeded { get; set; }
        public decimal ImpairmentAmount { get; set; }
        public DateTime TestDate { get; set; } = DateTime.UtcNow;
    }
}




