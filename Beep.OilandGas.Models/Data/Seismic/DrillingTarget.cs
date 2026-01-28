using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DrillingTarget : ModelEntityBase
    {
        public string? TargetId { get; init; }
        public string? TargetName { get; init; }
        public string? Name { get; init; }
        public double? TargetDepth { get; init; }
        public double? Depth { get; init; }
        public double? Confidence { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public double? RiskScore { get; init; }
    }
}
