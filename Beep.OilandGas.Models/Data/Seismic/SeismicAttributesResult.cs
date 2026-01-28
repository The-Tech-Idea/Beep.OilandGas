using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeismicAttributesResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Attribute { get; init; }
        public object? Values { get; init; }
    }
}
