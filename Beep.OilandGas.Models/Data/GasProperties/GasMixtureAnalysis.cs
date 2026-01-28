using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class GasMixtureAnalysis : ModelEntityBase
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
        private decimal AverageMolecularWeightValue;

        public decimal AverageMolecularWeight

        {

            get { return this.AverageMolecularWeightValue; }

            set { SetProperty(ref AverageMolecularWeightValue, value); }

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
        private decimal ReducedTemperatureValue;

        public decimal ReducedTemperature

        {

            get { return this.ReducedTemperatureValue; }

            set { SetProperty(ref ReducedTemperatureValue, value); }

        }
        private decimal ReducedPressureValue;

        public decimal ReducedPressure

        {

            get { return this.ReducedPressureValue; }

            set { SetProperty(ref ReducedPressureValue, value); }

        }
        private List<MixtureComponentAnalysis> ComponentAnalysisValue = new();

        public List<MixtureComponentAnalysis> ComponentAnalysis

        {

            get { return this.ComponentAnalysisValue; }

            set { SetProperty(ref ComponentAnalysisValue, value); }

        }
    }
}
