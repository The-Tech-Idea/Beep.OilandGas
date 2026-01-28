using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class VolumetricAnalysisResult : ModelEntityBase
    {
        public double? STOIIP { get; init; }
        public double? OOIP { get; init; }
        public double? EstimatedRecoverable { get; init; }
    }
}
