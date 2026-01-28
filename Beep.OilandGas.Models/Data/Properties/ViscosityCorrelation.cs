using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ViscosityCorrelation : ModelEntityBase
    {
        private string CorrelationIdValue = string.Empty;

        public string CorrelationId

        {

            get { return this.CorrelationIdValue; }

            set { SetProperty(ref CorrelationIdValue, value); }

        }
        private string CorrelationNameValue = string.Empty;

        public string CorrelationName

        {

            get { return this.CorrelationNameValue; }

            set { SetProperty(ref CorrelationNameValue, value); }

        }
        private decimal ViscosityValue;

        public decimal Viscosity

        {

            get { return this.ViscosityValue; }

            set { SetProperty(ref ViscosityValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        public Dictionary<string, decimal> Parameters { get; set; } = new();
        private decimal AccuracyValue;

        public decimal Accuracy

        {

            get { return this.AccuracyValue; }

            set { SetProperty(ref AccuracyValue, value); }

        }
        private string ApplicabilityValue = string.Empty;

        public string Applicability

        {

            get { return this.ApplicabilityValue; }

            set { SetProperty(ref ApplicabilityValue, value); }

        }
        private DateTime CalculatedDateValue;

        public DateTime CalculatedDate

        {

            get { return this.CalculatedDateValue; }

            set { SetProperty(ref CalculatedDateValue, value); }

        }
    }
}
