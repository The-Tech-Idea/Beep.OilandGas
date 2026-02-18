using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.FieldOrchestrator
{
    public class FieldProductionSummary : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public decimal DailyOilProduction { get; set; }
        public decimal DailyGasProduction { get; set; }
        public decimal DailyWaterProduction { get; set; }
        public decimal CumulativeOilProduction { get; set; }
        public decimal CumulativeGasProduction { get; set; }
        public decimal CumulativeWaterProduction { get; set; }
        public int ActiveWells { get; set; }
        public int TotalWells { get; set; }
    }
}
