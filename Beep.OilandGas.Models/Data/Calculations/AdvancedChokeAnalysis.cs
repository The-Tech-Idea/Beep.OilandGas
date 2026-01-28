using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class AdvancedChokeAnalysis : ModelEntityBase
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
        private string ChokeTypeValue = string.Empty;

        public string ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

        } // Bean, Venturi, Orifice
        private decimal ChokeDiameterValue;

        public decimal ChokeDiameter

        {

            get { return this.ChokeDiameterValue; }

            set { SetProperty(ref ChokeDiameterValue, value); }

        }
        private decimal UpstreamPressureValue;

        public decimal UpstreamPressure

        {

            get { return this.UpstreamPressureValue; }

            set { SetProperty(ref UpstreamPressureValue, value); }

        }
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        }
        private decimal CalculatedFlowRateValue;

        public decimal CalculatedFlowRate

        {

            get { return this.CalculatedFlowRateValue; }

            set { SetProperty(ref CalculatedFlowRateValue, value); }

        }
        private decimal DischargeCoefficientValue;

        public decimal DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
        private decimal PressureRatioValue;

        public decimal PressureRatio

        {

            get { return this.PressureRatioValue; }

            set { SetProperty(ref PressureRatioValue, value); }

        }
        private decimal CriticalPressureRatioValue;

        public decimal CriticalPressureRatio

        {

            get { return this.CriticalPressureRatioValue; }

            set { SetProperty(ref CriticalPressureRatioValue, value); }

        }
        private string FlowRegimeValue = string.Empty;

        public string FlowRegime

        {

            get { return this.FlowRegimeValue; }

            set { SetProperty(ref FlowRegimeValue, value); }

        } // Sonic, Subsonic
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private string ModelUsedValue = string.Empty;

        public string ModelUsed

        {

            get { return this.ModelUsedValue; }

            set { SetProperty(ref ModelUsedValue, value); }

        } // API-43, ISO 6149, etc.
    }
}
