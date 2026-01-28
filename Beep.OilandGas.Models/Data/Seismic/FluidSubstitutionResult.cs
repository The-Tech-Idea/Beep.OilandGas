using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FluidSubstitutionResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public object? Result { get; init; }
    }
}
