using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeismicInversionRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Parameters { get; init; }
    }
}
