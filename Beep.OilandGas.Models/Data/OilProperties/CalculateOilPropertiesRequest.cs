using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class CalculateOilPropertiesRequest : ModelEntityBase
    {
        private OilComposition CompositionValue = null!;

        [Required(ErrorMessage = "Composition is required")]
        public OilComposition Composition

        {

            get { return this.CompositionValue; }

            set { SetProperty(ref CompositionValue, value); }

        }

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
     }
}
