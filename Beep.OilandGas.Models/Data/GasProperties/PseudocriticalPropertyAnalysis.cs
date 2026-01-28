using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class PseudocriticalPropertyAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal PseudoCriticalTemperatureValue;

        public decimal PseudoCriticalTemperature

        {

            get { return this.PseudoCriticalTemperatureValue; }

            set { SetProperty(ref PseudoCriticalTemperatureValue, value); }

        }
        private decimal PseudoCriticalPressureValue;

        public decimal PseudoCriticalPressure

        {

            get { return this.PseudoCriticalPressureValue; }

            set { SetProperty(ref PseudoCriticalPressureValue, value); }

        }
        private decimal PseudoReducedTemperatureValue;

        public decimal PseudoReducedTemperature

        {

            get { return this.PseudoReducedTemperatureValue; }

            set { SetProperty(ref PseudoReducedTemperatureValue, value); }

        }
        private decimal PseudoReducedPressureValue;

        public decimal PseudoReducedPressure

        {

            get { return this.PseudoReducedPressureValue; }

            set { SetProperty(ref PseudoReducedPressureValue, value); }

        }
        private decimal AccentricityFactorValue;

        public decimal AccentricityFactor

        {

            get { return this.AccentricityFactorValue; }

            set { SetProperty(ref AccentricityFactorValue, value); }

        }
        private List<ComponentContribution> ComponentContributionsValue = new();

        public List<ComponentContribution> ComponentContributions

        {

            get { return this.ComponentContributionsValue; }

            set { SetProperty(ref ComponentContributionsValue, value); }

        }
    }
}
