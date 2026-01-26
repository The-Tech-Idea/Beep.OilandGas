using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CalculationInputParameter : ModelEntityBase
    {
        public string Name { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? ValueText { get; set; }
        public decimal? ValueNumber { get; set; }
        public DateTime? ValueDate { get; set; }
        public bool? ValueFlag { get; set; }
    }
}
