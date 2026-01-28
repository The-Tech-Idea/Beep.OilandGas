using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class GasPropertyMatrix : ModelEntityBase
    {
        private string MatrixIdValue = string.Empty;

        public string MatrixId

        {

            get { return this.MatrixIdValue; }

            set { SetProperty(ref MatrixIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime GenerationDateValue;

        public DateTime GenerationDate

        {

            get { return this.GenerationDateValue; }

            set { SetProperty(ref GenerationDateValue, value); }

        }
        private decimal MinPressureValue;

        public decimal MinPressure

        {

            get { return this.MinPressureValue; }

            set { SetProperty(ref MinPressureValue, value); }

        }
        private decimal MaxPressureValue;

        public decimal MaxPressure

        {

            get { return this.MaxPressureValue; }

            set { SetProperty(ref MaxPressureValue, value); }

        }
        private decimal MinTemperatureValue;

        public decimal MinTemperature

        {

            get { return this.MinTemperatureValue; }

            set { SetProperty(ref MinTemperatureValue, value); }

        }
        private decimal MaxTemperatureValue;

        public decimal MaxTemperature

        {

            get { return this.MaxTemperatureValue; }

            set { SetProperty(ref MaxTemperatureValue, value); }

        }
        private List<PropertyValue> PropertyValuesValue = new();

        public List<PropertyValue> PropertyValues

        {

            get { return this.PropertyValuesValue; }

            set { SetProperty(ref PropertyValuesValue, value); }

        }
    }
}
