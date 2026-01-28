using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SaturationPoint : ModelEntityBase
    {
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal RelativeVolumeValue;

        public decimal RelativeVolume

        {

            get { return this.RelativeVolumeValue; }

            set { SetProperty(ref RelativeVolumeValue, value); }

        }
        private decimal CompressibilityValue;

        public decimal Compressibility

        {

            get { return this.CompressibilityValue; }

            set { SetProperty(ref CompressibilityValue, value); }

        }
        private string PhaseValue = string.Empty;

        public string Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        }
    }
}
