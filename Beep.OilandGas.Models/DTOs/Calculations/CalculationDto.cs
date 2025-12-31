using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    public class CalculationResultResponse
    {
        public string CalculationId { get; set; }
        public string CalculationType { get; set; }
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;
        public object Result { get; set; }
        public Dictionary<string, object> InputParameters { get; set; } = new();
    }

    public class CalculateDeclineRateRequest
    {
        public decimal InitialRate { get; set; }
        public decimal CurrentRate { get; set; }
        public decimal TimePeriod { get; set; }
        public string DeclineType { get; set; } = "Exponential";
    }

    public class CalculateVolumeRequest
    {
        public decimal? GrossVolume { get; set; }
        public decimal? NetVolume { get; set; }
        public decimal BswPercentage { get; set; }
    }

    public class CalculateApiGravityRequest
    {
        public decimal? SpecificGravity { get; set; }
        public decimal? ApiGravity { get; set; }
    }
}

