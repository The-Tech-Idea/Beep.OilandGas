using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EORMethodScore : ModelEntityBase
    {
        private string MethodValue;

        public string Method

        {

            get { return this.MethodValue; }

            set { SetProperty(ref MethodValue, value); }

        }
        private double TemperatureSuitabilityValue;

        public double TemperatureSuitability

        {

            get { return this.TemperatureSuitabilityValue; }

            set { SetProperty(ref TemperatureSuitabilityValue, value); }

        }
        private double ViscositySuitabilityValue;

        public double ViscositySuitability

        {

            get { return this.ViscositySuitabilityValue; }

            set { SetProperty(ref ViscositySuitabilityValue, value); }

        }
        private double SaturationSuitabilityValue;

        public double SaturationSuitability

        {

            get { return this.SaturationSuitabilityValue; }

            set { SetProperty(ref SaturationSuitabilityValue, value); }

        }
        private double OverallScoreValue;

        public double OverallScore

        {

            get { return this.OverallScoreValue; }

            set { SetProperty(ref OverallScoreValue, value); }

        }
    }
}
