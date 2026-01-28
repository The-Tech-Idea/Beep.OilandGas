using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class UpdateSeismicSurvey : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Name { get; init; }
        public string? Description { get; init; }
        public DateTime? SurveyDate { get; init; }
    }
}
