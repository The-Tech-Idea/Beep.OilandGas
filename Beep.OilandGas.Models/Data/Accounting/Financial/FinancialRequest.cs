using System;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Data.Accounting.Financial
{
    /// <summary>
    /// Request DTO for impairment
    /// </summary>
    public class ImpairmentRequest : ModelEntityBase
    {
        public string PropertyId { get; set; } = string.Empty;
        public decimal ImpairmentAmount { get; set; }
    }

    /// <summary>
    /// Request DTO for full cost exploration
    /// </summary>
    public class FullCostExplorationRequest : ModelEntityBase
    {
        public string CostCenterId { get; set; } = string.Empty;
        public ExplorationCosts Costs { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for full cost development
    /// </summary>
    public class FullCostDevelopmentRequest : ModelEntityBase
    {
        public string CostCenterId { get; set; } = string.Empty;
        public DevelopmentCosts Costs { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for full cost acquisition
    /// </summary>
    public class FullCostAcquisitionRequest : ModelEntityBase
    {
        public string CostCenterId { get; set; } = string.Empty;
        public UnprovedProperty Property { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for ceiling test
    /// </summary>
    public class CeilingTestRequest : ModelEntityBase
    {
        public string CostCenterId { get; set; } = string.Empty;
        public ProvedReserves Reserves { get; set; } = new();
        public ProductionData Production { get; set; } = new();
    }
}




