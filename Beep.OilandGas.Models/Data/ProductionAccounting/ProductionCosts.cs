using System;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProductionCosts : ModelEntityBase
    {
        public string CostId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string WellId { get; set; } = string.Empty;
        public string CostCategory { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime? CostDate { get; set; }
        public string CostType { get; set; } = string.Empty;
        public string PropertyId { get; set; } = string.Empty;
        public decimal TotalProductionCosts { get; set; }
    }
}
