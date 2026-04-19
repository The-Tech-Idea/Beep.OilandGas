using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    /// <summary>
    /// Request for generating a heat map from data points and configuration.
    /// </summary>
    public class GenerateHeatMapRequest : ModelEntityBase
    {
        public List<HEAT_MAP_DATA_POINT> DataPoints { get; set; } = new();
        public HeatMapConfigurationRecord? Configuration { get; set; }
    }

    /// <summary>
    /// Request for generating a production-based heat map.
    /// </summary>
    public class GenerateProductionHeatMapRequest : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
