using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class CalculateZFactorRequest : ModelEntityBase
    {
        private decimal PressureValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        private decimal TemperatureValue;


        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature


        {


            get { return this.TemperatureValue; }


            set { SetProperty(ref TemperatureValue, value); }


        }

        private decimal SpecificGravityValue;


        [Required]
        [Range(0, 2, ErrorMessage = "SpecificGravity must be between 0 and 2")]
        public decimal SpecificGravity


        {


            get { return this.SpecificGravityValue; }


            set { SetProperty(ref SpecificGravityValue, value); }


        }

        private string CorrelationValue = "Standing-Katz";


        public string Correlation


        {


            get { return this.CorrelationValue; }


            set { SetProperty(ref CorrelationValue, value); }


        }
    }
}
