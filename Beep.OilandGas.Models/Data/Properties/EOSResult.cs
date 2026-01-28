using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EOSResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string EquationOfStateValue = string.Empty;

        public string EquationOfState

        {

            get { return this.EquationOfStateValue; }

            set { SetProperty(ref EquationOfStateValue, value); }

        }
        private string MixingRuleValue = string.Empty;

        public string MixingRule

        {

            get { return this.MixingRuleValue; }

            set { SetProperty(ref MixingRuleValue, value); }

        }
        private List<EOSComponent> ComponentsValue = new();

        public List<EOSComponent> Components

        {

            get { return this.ComponentsValue; }

            set { SetProperty(ref ComponentsValue, value); }

        }
        private decimal CriticalPressureValue;

        public decimal CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        }
        private decimal CriticalTemperatureValue;

        public decimal CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        }
        private decimal CriticalVolumeValue;

        public decimal CriticalVolume

        {

            get { return this.CriticalVolumeValue; }

            set { SetProperty(ref CriticalVolumeValue, value); }

        }
        private decimal AcentricFactorValue;

        public decimal AcentricFactor

        {

            get { return this.AcentricFactorValue; }

            set { SetProperty(ref AcentricFactorValue, value); }

        }
        private List<EOSPhase> PhasesValue = new();

        public List<EOSPhase> Phases

        {

            get { return this.PhasesValue; }

            set { SetProperty(ref PhasesValue, value); }

        }
        private decimal BinaryInteractionParameterValue;

        public decimal BinaryInteractionParameter

        {

            get { return this.BinaryInteractionParameterValue; }

            set { SetProperty(ref BinaryInteractionParameterValue, value); }

        }
        private string ConvergenceStatusValue = string.Empty;

        public string ConvergenceStatus

        {

            get { return this.ConvergenceStatusValue; }

            set { SetProperty(ref ConvergenceStatusValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
    }
}
