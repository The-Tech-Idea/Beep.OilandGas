using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeismicDataQuality : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public double? SignalToNoiseRatio { get; init; }
        public string? Notes { get; init; }
    }
}
