using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProspectRiskAssessment : ModelEntityBase
    {
        public string? ProspectId { get; init; }
        public double? RiskScore { get; init; }
        public List<RiskFactor>? RiskFactors { get; init; }
        public string? Summary { get; init; }
    }
}
