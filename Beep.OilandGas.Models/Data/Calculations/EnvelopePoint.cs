using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EnvelopePoint : ModelEntityBase
    {
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
        private string PhaseRegionValue = string.Empty;

        public string PhaseRegion

        {

            get { return this.PhaseRegionValue; }

            set { SetProperty(ref PhaseRegionValue, value); }

        }
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }
    }
}
