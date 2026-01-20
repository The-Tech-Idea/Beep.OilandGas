using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Request for amortization calculation
    /// </summary>
    public class AmortizationCalculationRequest : ModelEntityBase
    {
        public string? PropertyId { get; set; }

        [Required(ErrorMessage = "NetCapitalizedCosts is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "NetCapitalizedCosts must be greater than 0")]
        public decimal NetCapitalizedCosts { get; set; }

        [Required(ErrorMessage = "TotalProvedReservesBOE is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "TotalProvedReservesBOE must be greater than 0")]
        public decimal TotalProvedReservesBOE { get; set; }

        [Required(ErrorMessage = "ProductionBOE is required")]
        [Range(0, double.MaxValue, ErrorMessage = "ProductionBOE must be greater than or equal to 0")]
        public decimal ProductionBOE { get; set; }

        public DateTime? CalculationDate { get; set; }
    }

    /// <summary>
    /// Request for impairment calculation
    /// </summary>
    public class ImpairmentRequest : ModelEntityBase
    {
        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId { get; set; } = string.Empty;

        [Required(ErrorMessage = "CarryingValue is required")]
        [Range(0, double.MaxValue, ErrorMessage = "CarryingValue must be greater than or equal to 0")]
        public decimal CarryingValue { get; set; }

        [Required(ErrorMessage = "FairValue is required")]
        [Range(0, double.MaxValue, ErrorMessage = "FairValue must be greater than or equal to 0")]
        public decimal FairValue { get; set; }

        public DateTime? ImpairmentDate { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request for full cost exploration
    /// </summary>
    public class FullCostExplorationRequest : ModelEntityBase
    {
        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId { get; set; } = string.Empty;

        [Required(ErrorMessage = "ExplorationCost is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "ExplorationCost must be greater than 0")]
        public decimal ExplorationCost { get; set; }

        public DateTime? CostDate { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request for full cost development
    /// </summary>
    public class FullCostDevelopmentRequest : ModelEntityBase
    {
        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId { get; set; } = string.Empty;

        [Required(ErrorMessage = "DevelopmentCost is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "DevelopmentCost must be greater than 0")]
        public decimal DevelopmentCost { get; set; }

        public DateTime? CostDate { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request for full cost acquisition
    /// </summary>
    public class FullCostAcquisitionRequest : ModelEntityBase
    {
        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AcquisitionCost is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "AcquisitionCost must be greater than 0")]
        public decimal AcquisitionCost { get; set; }

        public DateTime? AcquisitionDate { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request for ceiling test
    /// </summary>
    public class CeilingTestRequest : ModelEntityBase
    {
        [Required(ErrorMessage = "PropertyId is required")]
        public string PropertyId { get; set; } = string.Empty;

        public DateTime? TestDate { get; set; }
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }
    }
}




