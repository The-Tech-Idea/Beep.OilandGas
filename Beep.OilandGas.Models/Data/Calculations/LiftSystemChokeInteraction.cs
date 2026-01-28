using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class LiftSystemChokeInteraction : ModelEntityBase
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
        private string LiftSystemTypeValue = string.Empty;

        public string LiftSystemType

        {

            get { return this.LiftSystemTypeValue; }

            set { SetProperty(ref LiftSystemTypeValue, value); }

        } // ESP, GasLift, SuckerRod, Plunger
        private decimal CurrentChokeSizeValue;

        public decimal CurrentChokeSize

        {

            get { return this.CurrentChokeSizeValue; }

            set { SetProperty(ref CurrentChokeSizeValue, value); }

        }
        private decimal CurrentDischargeValue;

        public decimal CurrentDischarge

        {

            get { return this.CurrentDischargeValue; }

            set { SetProperty(ref CurrentDischargeValue, value); }

        }
        private decimal LiftSystemPowerValue;

        public decimal LiftSystemPower

        {

            get { return this.LiftSystemPowerValue; }

            set { SetProperty(ref LiftSystemPowerValue, value); }

        } // HP or scf/day
        private decimal RequiredHeadOrPressureValue;

        public decimal RequiredHeadOrPressure

        {

            get { return this.RequiredHeadOrPressureValue; }

            set { SetProperty(ref RequiredHeadOrPressureValue, value); }

        }
        private decimal ChokeBackPressureValue;

        public decimal ChokeBackPressure

        {

            get { return this.ChokeBackPressureValue; }

            set { SetProperty(ref ChokeBackPressureValue, value); }

        }
        private decimal SystemEfficiencyValue;

        public decimal SystemEfficiency

        {

            get { return this.SystemEfficiencyValue; }

            set { SetProperty(ref SystemEfficiencyValue, value); }

        }
        private decimal OptimalChokeSizeValue;

        public decimal OptimalChokeSize

        {

            get { return this.OptimalChokeSizeValue; }

            set { SetProperty(ref OptimalChokeSizeValue, value); }

        }
        private decimal EfficiencyGainValue;

        public decimal EfficiencyGain

        {

            get { return this.EfficiencyGainValue; }

            set { SetProperty(ref EfficiencyGainValue, value); }

        }
        private decimal PowerSavingsValue;

        public decimal PowerSavings

        {

            get { return this.PowerSavingsValue; }

            set { SetProperty(ref PowerSavingsValue, value); }

        } // HP or scf/day
        private string RecommendationValue = string.Empty;

        public string Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
        private List<string> OperatingConstraintsValue = new();

        public List<string> OperatingConstraints

        {

            get { return this.OperatingConstraintsValue; }

            set { SetProperty(ref OperatingConstraintsValue, value); }

        }
    }
}
