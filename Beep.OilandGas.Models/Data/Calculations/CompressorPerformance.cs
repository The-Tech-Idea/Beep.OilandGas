using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorPerformance : ModelEntityBase
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
        private string CompressorTypeValue = string.Empty;

        public string CompressorType

        {

            get { return this.CompressorTypeValue; }

            set { SetProperty(ref CompressorTypeValue, value); }

        } // Centrifugal, Reciprocating
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal CompressionRatioValue;

        public decimal CompressionRatio

        {

            get { return this.CompressionRatioValue; }

            set { SetProperty(ref CompressionRatioValue, value); }

        }
        private decimal IsentropicEfficiencyValue;

        public decimal IsentropicEfficiency

        {

            get { return this.IsentropicEfficiencyValue; }

            set { SetProperty(ref IsentropicEfficiencyValue, value); }

        }
        private decimal ActualEfficiencyValue;

        public decimal ActualEfficiency

        {

            get { return this.ActualEfficiencyValue; }

            set { SetProperty(ref ActualEfficiencyValue, value); }

        }
        private decimal PowerRequiredValue;

        public decimal PowerRequired

        {

            get { return this.PowerRequiredValue; }

            set { SetProperty(ref PowerRequiredValue, value); }

        }
        private decimal PolyHeatCapacityRatioValue;

        public decimal PolyHeatCapacityRatio

        {

            get { return this.PolyHeatCapacityRatioValue; }

            set { SetProperty(ref PolyHeatCapacityRatioValue, value); }

        }
    }
}
