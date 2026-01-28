using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CalculateVolumeRequest : ModelEntityBase
    {
        private decimal? GrossVolumeValue;

        public decimal? GrossVolume

        {

            get { return this.GrossVolumeValue; }

            set { SetProperty(ref GrossVolumeValue, value); }

        }
        private decimal? NetVolumeValue;

        public decimal? NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal BswPercentageValue;

        public decimal BswPercentage

        {

            get { return this.BswPercentageValue; }

            set { SetProperty(ref BswPercentageValue, value); }

        }
    }
}
