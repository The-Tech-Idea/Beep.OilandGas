using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeismicWellTieResult : ModelEntityBase
    {
        public string? WellId { get; init; }
        public string? SurveyId { get; init; }
        public string? ResultSummary { get; init; }
    }
}
