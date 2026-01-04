using System;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;

namespace Beep.OilandGas.Models.DTOs.Accounting.Financial
{
    /// <summary>
    /// Request DTO for impairment
    /// </summary>
    public class ImpairmentRequest
    {
        public string PropertyId { get; set; } = string.Empty;
        public decimal ImpairmentAmount { get; set; }
    }

    /// <summary>
    /// Request DTO for full cost exploration
    /// </summary>
    public class FullCostExplorationRequest
    {
        public string CostCenterId { get; set; } = string.Empty;
        public ExplorationCostsDto Costs { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for full cost development
    /// </summary>
    public class FullCostDevelopmentRequest
    {
        public string CostCenterId { get; set; } = string.Empty;
        public DevelopmentCostsDto Costs { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for full cost acquisition
    /// </summary>
    public class FullCostAcquisitionRequest
    {
        public string CostCenterId { get; set; } = string.Empty;
        public UnprovedPropertyDto Property { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for ceiling test
    /// </summary>
    public class CeilingTestRequest
    {
        public string CostCenterId { get; set; } = string.Empty;
        public ProvedReservesDto Reserves { get; set; } = new();
        public ProductionDataDto Production { get; set; } = new();
    }
}
