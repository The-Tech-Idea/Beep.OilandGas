using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeismicReportRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? RequestedBy { get; init; }
    }
}
