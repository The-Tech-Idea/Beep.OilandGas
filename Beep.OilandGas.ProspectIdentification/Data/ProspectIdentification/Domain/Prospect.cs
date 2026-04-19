using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class Prospect : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string ProspectNameValue = string.Empty;

        public string ProspectName

        {

            get { return this.ProspectNameValue; }

            set { SetProperty(ref ProspectNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? LocationValue;

        public string? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private string? CountryValue;

        public string? Country

        {

            get { return this.CountryValue; }

            set { SetProperty(ref CountryValue, value); }

        }
        private string? StateProvinceValue;

        public string? StateProvince

        {

            get { return this.StateProvinceValue; }

            set { SetProperty(ref StateProvinceValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime? CreatedDateValue;

        public DateTime? CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private DateTime? EvaluationDateValue;

        public DateTime? EvaluationDate

        {

            get { return this.EvaluationDateValue; }

            set { SetProperty(ref EvaluationDateValue, value); }

        }
        private string? EvaluatedByValue;

        public string? EvaluatedBy

        {

            get { return this.EvaluatedByValue; }

            set { SetProperty(ref EvaluatedByValue, value); }

        }
        private decimal? EstimatedResourcesValue;

        public decimal? EstimatedResources

        {

            get { return this.EstimatedResourcesValue; }

            set { SetProperty(ref EstimatedResourcesValue, value); }

        }
        private string? ResourceUnitValue;

        public string? ResourceUnit

        {

            get { return this.ResourceUnitValue; }

            set { SetProperty(ref ResourceUnitValue, value); }

        }
        private decimal? RiskScoreValue;

        public decimal? RiskScore

        {

            get { return this.RiskScoreValue; }

            set { SetProperty(ref RiskScoreValue, value); }

        }
        private string? RiskLevelValue;

        public string? RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private List<SeismicSurvey> SeismicSurveysValue = new();

        public List<SeismicSurvey> SeismicSurveys

        {

            get { return this.SeismicSurveysValue; }

            set { SetProperty(ref SeismicSurveysValue, value); }

        }
        private ProspectEvaluation? EvaluationValue;

        public ProspectEvaluation? Evaluation

        {

            get { return this.EvaluationValue; }

            set { SetProperty(ref EvaluationValue, value); }

        }
    }
}
