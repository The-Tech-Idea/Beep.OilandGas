using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class GasInjectionAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string GasTypeValue;

        public string GasType

        {

            get { return this.GasTypeValue; }

            set { SetProperty(ref GasTypeValue, value); }

        }
        private double InjectionPressureValue;

        public double InjectionPressure

        {

            get { return this.InjectionPressureValue; }

            set { SetProperty(ref InjectionPressureValue, value); }

        }
        private double MinimumMiscibilityPressureValue;

        public double MinimumMiscibilityPressure

        {

            get { return this.MinimumMiscibilityPressureValue; }

            set { SetProperty(ref MinimumMiscibilityPressureValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private bool IsMiscibleValue;

        public bool IsMiscible

        {

            get { return this.IsMiscibleValue; }

            set { SetProperty(ref IsMiscibleValue, value); }

        }
        private string DisplacementMechanismValue;

        public string DisplacementMechanism

        {

            get { return this.DisplacementMechanismValue; }

            set { SetProperty(ref DisplacementMechanismValue, value); }

        }
        private double DisplacementEfficiencyValue;

        public double DisplacementEfficiency

        {

            get { return this.DisplacementEfficiencyValue; }

            set { SetProperty(ref DisplacementEfficiencyValue, value); }

        }
        private double ResidualOilSaturationValue;

        public double ResidualOilSaturation

        {

            get { return this.ResidualOilSaturationValue; }

            set { SetProperty(ref ResidualOilSaturationValue, value); }

        }
        private double ProductionImprovementValue;

        public double ProductionImprovement

        {

            get { return this.ProductionImprovementValue; }

            set { SetProperty(ref ProductionImprovementValue, value); }

        }
        private double TertiaryRecoveryPotentialValue;

        public double TertiaryRecoveryPotential

        {

            get { return this.TertiaryRecoveryPotentialValue; }

            set { SetProperty(ref TertiaryRecoveryPotentialValue, value); }

        }
        private GasTypeCharacteristics GasTypeCharacteristicsValue;

        public GasTypeCharacteristics GasTypeCharacteristics

        {

            get { return this.GasTypeCharacteristicsValue; }

            set { SetProperty(ref GasTypeCharacteristicsValue, value); }

        }
    }
}
