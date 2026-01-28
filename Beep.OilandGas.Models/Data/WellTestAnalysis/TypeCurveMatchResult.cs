using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class TypeCurveMatchResult : ModelEntityBase
    {
        private string MatchIdValue = string.Empty;

        public string MatchId

        {

            get { return this.MatchIdValue; }

            set { SetProperty(ref MatchIdValue, value); }

        }
        private string TypeCurveNameValue = string.Empty;

        public string TypeCurveName

        {

            get { return this.TypeCurveNameValue; }

            set { SetProperty(ref TypeCurveNameValue, value); }

        }
        private ReservoirModel ReservoirModelValue = new();

        public ReservoirModel ReservoirModel

        {

            get { return this.ReservoirModelValue; }

            set { SetProperty(ref ReservoirModelValue, value); }

        }
        private double MatchQualityValue;

        public double MatchQuality

        {

            get { return this.MatchQualityValue; }

            set { SetProperty(ref MatchQualityValue, value); }

        }
        private double PermeabilityValue;

        public double Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private double SkinFactorValue;

        public double SkinFactor

        {

            get { return this.SkinFactorValue; }

            set { SetProperty(ref SkinFactorValue, value); }

        }
        private double InitialPressureValue;

        public double InitialPressure

        {

            get { return this.InitialPressureValue; }

            set { SetProperty(ref InitialPressureValue, value); }

        }
        private List<string> ConfidenceIndicatorsValue = new();

        public List<string> ConfidenceIndicators

        {

            get { return this.ConfidenceIndicatorsValue; }

            set { SetProperty(ref ConfidenceIndicatorsValue, value); }

        }
    }
}
