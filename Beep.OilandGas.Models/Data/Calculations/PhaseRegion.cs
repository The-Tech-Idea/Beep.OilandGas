using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PhaseRegion : ModelEntityBase
    {
        private string RegionNameValue = string.Empty;

        public string RegionName

        {

            get { return this.RegionNameValue; }

            set { SetProperty(ref RegionNameValue, value); }

        } // Single Phase, Two-Phase, Three-Phase
        private List<decimal> PressuresValue = new();

        public List<decimal> Pressures

        {

            get { return this.PressuresValue; }

            set { SetProperty(ref PressuresValue, value); }

        }
        private List<decimal> TemperaturesValue = new();

        public List<decimal> Temperatures

        {

            get { return this.TemperaturesValue; }

            set { SetProperty(ref TemperaturesValue, value); }

        }
    }
}
