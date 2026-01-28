using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class VenturiChokeAnalysis : ModelEntityBase
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
        private decimal ThroatDiameterValue;

        public decimal ThroatDiameter

        {

            get { return this.ThroatDiameterValue; }

            set { SetProperty(ref ThroatDiameterValue, value); }

        }
        private decimal UpstreamDiameterValue;

        public decimal UpstreamDiameter

        {

            get { return this.UpstreamDiameterValue; }

            set { SetProperty(ref UpstreamDiameterValue, value); }

        }
        private decimal DownstreamDiameterValue;

        public decimal DownstreamDiameter

        {

            get { return this.DownstreamDiameterValue; }

            set { SetProperty(ref DownstreamDiameterValue, value); }

        }
        private decimal RecoveryLengthValue;

        public decimal RecoveryLength

        {

            get { return this.RecoveryLengthValue; }

            set { SetProperty(ref RecoveryLengthValue, value); }

        }
        private decimal UpstreamPressureValue;

        public decimal UpstreamPressure

        {

            get { return this.UpstreamPressureValue; }

            set { SetProperty(ref UpstreamPressureValue, value); }

        }
        private decimal ThroatPressureValue;

        public decimal ThroatPressure

        {

            get { return this.ThroatPressureValue; }

            set { SetProperty(ref ThroatPressureValue, value); }

        }
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        }
        private decimal RecoveryPressureValue;

        public decimal RecoveryPressure

        {

            get { return this.RecoveryPressureValue; }

            set { SetProperty(ref RecoveryPressureValue, value); }

        }
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        }
        private decimal ThroatVelocityValue;

        public decimal ThroatVelocity

        {

            get { return this.ThroatVelocityValue; }

            set { SetProperty(ref ThroatVelocityValue, value); }

        }
        private decimal RecoveryFractionValue;

        public decimal RecoveryFraction

        {

            get { return this.RecoveryFractionValue; }

            set { SetProperty(ref RecoveryFractionValue, value); }

        }
        private decimal EffectivePressureDropValue;

        public decimal EffectivePressureDrop

        {

            get { return this.EffectivePressureDropValue; }

            set { SetProperty(ref EffectivePressureDropValue, value); }

        }
        private decimal CoefficientOfRecoveryValue;

        public decimal CoefficientOfRecovery

        {

            get { return this.CoefficientOfRecoveryValue; }

            set { SetProperty(ref CoefficientOfRecoveryValue, value); }

        }
        private string AdvantageValue = string.Empty;

        public string Advantage

        {

            get { return this.AdvantageValue; }

            set { SetProperty(ref AdvantageValue, value); }

        } // Lower erosion, Higher recovery, etc.
    }
}
