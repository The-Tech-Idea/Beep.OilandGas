using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AsphalteneAnalysis : ModelEntityBase
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
        private decimal OnsetPressureValue;

        public decimal OnsetPressure

        {

            get { return this.OnsetPressureValue; }

            set { SetProperty(ref OnsetPressureValue, value); }

        }
        private decimal OnsetTemperatureValue;

        public decimal OnsetTemperature

        {

            get { return this.OnsetTemperatureValue; }

            set { SetProperty(ref OnsetTemperatureValue, value); }

        }
        private decimal AsphalteneContentValue;

        public decimal AsphalteneContent

        {

            get { return this.AsphalteneContentValue; }

            set { SetProperty(ref AsphalteneContentValue, value); }

        }
        private string PrecipitationMechanismValue = string.Empty;

        public string PrecipitationMechanism

        {

            get { return this.PrecipitationMechanismValue; }

            set { SetProperty(ref PrecipitationMechanismValue, value); }

        }
        private List<AsphaltenePoint> PrecipitationPointsValue = new();

        public List<AsphaltenePoint> PrecipitationPoints

        {

            get { return this.PrecipitationPointsValue; }

            set { SetProperty(ref PrecipitationPointsValue, value); }

        }
        private string AnalysisMethodValue = string.Empty;

        public string AnalysisMethod

        {

            get { return this.AnalysisMethodValue; }

            set { SetProperty(ref AnalysisMethodValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }
}
