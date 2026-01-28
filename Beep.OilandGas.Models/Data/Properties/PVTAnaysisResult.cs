using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PVTAnaysisResult : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private string AnalysisTypeValue = string.Empty;

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        }
        private List<PVTDataPoint> DataPointsValue = new();

        public List<PVTDataPoint> DataPoints

        {

            get { return this.DataPointsValue; }

            set { SetProperty(ref DataPointsValue, value); }

        }
        private PVTParameters ParametersValue = new();

        public PVTParameters Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
        private List<PropertyResult> DerivedPropertiesValue = new();

        public List<PropertyResult> DerivedProperties

        {

            get { return this.DerivedPropertiesValue; }

            set { SetProperty(ref DerivedPropertiesValue, value); }

        }
        private string QualityAssessmentValue = string.Empty;

        public string QualityAssessment

        {

            get { return this.QualityAssessmentValue; }

            set { SetProperty(ref QualityAssessmentValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }
}
