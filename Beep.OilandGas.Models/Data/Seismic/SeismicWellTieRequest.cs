using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeismicWellTieRequest : ModelEntityBase
    {
        public string? WellId { get; init; }
        public string? SurveyId { get; init; }
    }
}
