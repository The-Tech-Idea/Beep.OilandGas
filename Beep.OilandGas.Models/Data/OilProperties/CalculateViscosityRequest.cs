using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class CalculateViscosityRequest : ModelEntityBase
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

        private decimal OilGravityValue;


        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity


        {


            get { return this.OilGravityValue; }


            set { SetProperty(ref OilGravityValue, value); }


        }

        private decimal GasOilRatioValue;


        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio


        {


            get { return this.GasOilRatioValue; }


            set { SetProperty(ref GasOilRatioValue, value); }


        }
    }
}
