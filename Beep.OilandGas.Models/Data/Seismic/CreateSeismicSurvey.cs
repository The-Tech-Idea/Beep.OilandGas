using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CreateSeismicSurvey : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Name { get; init; }
        public string? SurveyName { get; init; }
        public string? SurveyType { get; init; }
        public string? Description { get; init; }
        public DateTime? SurveyDate { get; init; }
        public DateTime? AcquisitionDate { get; init; }
        public string? ProspectId { get; init; }
        public string? FieldId { get; init; }
        public string? CreatedBy { get; init; }
    }
}
