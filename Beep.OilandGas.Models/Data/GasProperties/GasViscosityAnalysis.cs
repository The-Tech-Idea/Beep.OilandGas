using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class GasViscosityAnalysis : ModelEntityBase
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
        private decimal ViscosityValue;

        public decimal Viscosity

        {

            get { return this.ViscosityValue; }

            set { SetProperty(ref ViscosityValue, value); }

        }
        private decimal ViscosityAtSCValue;

        public decimal ViscosityAtSC

        {

            get { return this.ViscosityAtSCValue; }

            set { SetProperty(ref ViscosityAtSCValue, value); }

        } // Standard conditions
        private decimal PressureCoefficientValue;

        public decimal PressureCoefficient

        {

            get { return this.PressureCoefficientValue; }

            set { SetProperty(ref PressureCoefficientValue, value); }

        }
        private decimal TemperatureCoefficientValue;

        public decimal TemperatureCoefficient

        {

            get { return this.TemperatureCoefficientValue; }

            set { SetProperty(ref TemperatureCoefficientValue, value); }

        }
        private string CorrelationMethodValue = string.Empty;

        public string CorrelationMethod

        {

            get { return this.CorrelationMethodValue; }

            set { SetProperty(ref CorrelationMethodValue, value); }

        }
    }
}
