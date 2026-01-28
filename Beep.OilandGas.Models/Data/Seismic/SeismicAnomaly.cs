using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeismicAnomaly : ModelEntityBase
    {
        public string? Id { get; init; }
        public string? AnomalyId { get; init; }
        public string? AnomalyType { get; init; }
        public string? Location { get; init; }
        public double? Magnitude { get; init; }
        public string? Description { get; init; }
        public double? Confidence { get; init; }
        public string? Notes { get; init; }
    }
}
