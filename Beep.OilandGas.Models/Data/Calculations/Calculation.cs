using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CalculationResultResponse : ModelEntityBase
    {
        public string CalculationId { get; set; }
        public string CalculationType { get; set; }
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;
        public object Result { get; set; }
        public Dictionary<string, object> InputParameters { get; set; } = new();
    }

    public class CalculateDeclineRateRequest : ModelEntityBase
    {
        public decimal InitialRate { get; set; }
        public decimal CurrentRate { get; set; }
        public decimal TimePeriod { get; set; }
        public string DeclineType { get; set; } = "Exponential";
    }

    public class CalculateVolumeRequest : ModelEntityBase
    {
        public decimal? GrossVolume { get; set; }
        public decimal? NetVolume { get; set; }
        public decimal BswPercentage { get; set; }
    }

    public class CalculateApiGravityRequest : ModelEntityBase
    {
        public decimal? SpecificGravity { get; set; }
        public decimal? ApiGravity { get; set; }
    }
}





