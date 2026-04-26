using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class SeismicSurvey : ModelEntityBase
    {
        private string SurveyIdValue = string.Empty;

        public string SurveyId

        {

            get { return this.SurveyIdValue; }

            set { SetProperty(ref SurveyIdValue, value); }

        }
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string SurveyNameValue = string.Empty;

        public string SurveyName

        {

            get { return this.SurveyNameValue; }

            set { SetProperty(ref SurveyNameValue, value); }

        }
        private string? SurveyTypeValue;

        public string? SurveyType

        {

            get { return this.SurveyTypeValue; }

            set { SetProperty(ref SurveyTypeValue, value); }

        }
        private DateTime? SurveyDateValue;

        public DateTime? SurveyDate

        {

            get { return this.SurveyDateValue; }

            set { SetProperty(ref SurveyDateValue, value); }

        }
        private string? ContractorValue;

        public string? Contractor

        {

            get { return this.ContractorValue; }

            set { SetProperty(ref ContractorValue, value); }

        }
        private decimal? AreaCoveredValue;

        public decimal? AreaCovered

        {

            get { return this.AreaCoveredValue; }

            set { SetProperty(ref AreaCoveredValue, value); }

        }
        private string? AreaUnitValue;

        public string? AreaUnit

        {

            get { return this.AreaUnitValue; }

            set { SetProperty(ref AreaUnitValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? InterpretationStatusValue;

        public string? InterpretationStatus

        {

            get { return this.InterpretationStatusValue; }

            set { SetProperty(ref InterpretationStatusValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }
}
