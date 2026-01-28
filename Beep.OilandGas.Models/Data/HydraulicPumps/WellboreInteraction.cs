using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellboreInteraction : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private decimal InteractionIntensityValue;

        public decimal InteractionIntensity

        {

            get { return this.InteractionIntensityValue; }

            set { SetProperty(ref InteractionIntensityValue, value); }

        }
        private List<InteractionFactor> FactorsValue = new();

        public List<InteractionFactor> Factors

        {

            get { return this.FactorsValue; }

            set { SetProperty(ref FactorsValue, value); }

        }
        private string OverallAssessmentValue = string.Empty;

        public string OverallAssessment

        {

            get { return this.OverallAssessmentValue; }

            set { SetProperty(ref OverallAssessmentValue, value); }

        }
    }
}
