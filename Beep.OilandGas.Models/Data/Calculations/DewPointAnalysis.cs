using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DewPointAnalysis : ModelEntityBase
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
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal DewPointPressureValue;

        public decimal DewPointPressure

        {

            get { return this.DewPointPressureValue; }

            set { SetProperty(ref DewPointPressureValue, value); }

        }
        public List<FlashComponentFraction> VaporComposition { get; set; } = new();
        public List<FlashComponentKValue> KValues { get; set; } = new();
        private int IterationsValue;

        public int Iterations

        {

            get { return this.IterationsValue; }

            set { SetProperty(ref IterationsValue, value); }

        }
        private bool ConvergedValue;

        public bool Converged

        {

            get { return this.ConvergedValue; }

            set { SetProperty(ref ConvergedValue, value); }

        }
        private decimal ConvergenceErrorValue;

        public decimal ConvergenceError

        {

            get { return this.ConvergenceErrorValue; }

            set { SetProperty(ref ConvergenceErrorValue, value); }

        }
    }
}
