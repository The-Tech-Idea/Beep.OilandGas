using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeTemperatureEffectAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal BaselineTemperatureValue;

        public decimal BaselineTemperature

        {

            get { return this.BaselineTemperatureValue; }

            set { SetProperty(ref BaselineTemperatureValue, value); }

        }
        private decimal BaselineFlowRateValue;

        public decimal BaselineFlowRate

        {

            get { return this.BaselineFlowRateValue; }

            set { SetProperty(ref BaselineFlowRateValue, value); }

        }
        private decimal TemperatureChangeRangeValue;

        public decimal TemperatureChangeRange

        {

            get { return this.TemperatureChangeRangeValue; }

            set { SetProperty(ref TemperatureChangeRangeValue, value); }

        } // °R
        private List<TemperatureFlowPoint> TemperatureEffectCurveValue = new();

        public List<TemperatureFlowPoint> TemperatureEffectCurve

        {

            get { return this.TemperatureEffectCurveValue; }

            set { SetProperty(ref TemperatureEffectCurveValue, value); }

        }
        private decimal FlowSensitivityValue;

        public decimal FlowSensitivity

        {

            get { return this.FlowSensitivityValue; }

            set { SetProperty(ref FlowSensitivityValue, value); }

        } // %change/°R
        private decimal PressureDropSensitivityValue;

        public decimal PressureDropSensitivity

        {

            get { return this.PressureDropSensitivityValue; }

            set { SetProperty(ref PressureDropSensitivityValue, value); }

        } // psi/°R
        private decimal DischargeCoefficientTemperatureCoeffValue;

        public decimal DischargeCoefficientTemperatureCoeff

        {

            get { return this.DischargeCoefficientTemperatureCoeffValue; }

            set { SetProperty(ref DischargeCoefficientTemperatureCoeffValue, value); }

        } // 1/°R
        private string TemperatureControlRecommendationValue = string.Empty;

        public string TemperatureControlRecommendation

        {

            get { return this.TemperatureControlRecommendationValue; }

            set { SetProperty(ref TemperatureControlRecommendationValue, value); }

        }
    }
}
