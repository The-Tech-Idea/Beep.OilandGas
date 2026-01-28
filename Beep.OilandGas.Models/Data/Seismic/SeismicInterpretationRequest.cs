using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeismicInterpretationRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Payload { get; init; }
        public Dictionary<string, object>? Parameters { get; init; }
    }
}
