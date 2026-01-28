using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class TargetIdentificationRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Criteria { get; init; }
    }
}
