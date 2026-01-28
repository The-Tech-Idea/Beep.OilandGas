using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class ThermalConductivityAnalysis : ModelEntityBase
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
        private decimal ThermalConductivityValue;

        public decimal ThermalConductivity

        {

            get { return this.ThermalConductivityValue; }

            set { SetProperty(ref ThermalConductivityValue, value); }

        } // BTU/(hr·ft·°R)
        private decimal TemperatureDependenceValue;

        public decimal TemperatureDependence

        {

            get { return this.TemperatureDependenceValue; }

            set { SetProperty(ref TemperatureDependenceValue, value); }

        }
        private decimal PressureDependenceValue;

        public decimal PressureDependence

        {

            get { return this.PressureDependenceValue; }

            set { SetProperty(ref PressureDependenceValue, value); }

        }
        private string CorrelationMethodValue = string.Empty;

        public string CorrelationMethod

        {

            get { return this.CorrelationMethodValue; }

            set { SetProperty(ref CorrelationMethodValue, value); }

        }
    }
}
