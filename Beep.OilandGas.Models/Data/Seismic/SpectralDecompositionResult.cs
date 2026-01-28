using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SpectralDecompositionResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public object? Result { get; init; }
    }
}
