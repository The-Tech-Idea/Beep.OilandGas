using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProductionSummary : ModelEntityBase
    {
        private decimal GrossOilVolumeValue;

        public decimal GrossOilVolume

        {

            get { return this.GrossOilVolumeValue; }

            set { SetProperty(ref GrossOilVolumeValue, value); }

        }
        private decimal GrossGasVolumeValue;

        public decimal GrossGasVolume

        {

            get { return this.GrossGasVolumeValue; }

            set { SetProperty(ref GrossGasVolumeValue, value); }

        }
        private decimal NetOilVolumeValue;

        public decimal NetOilVolume

        {

            get { return this.NetOilVolumeValue; }

            set { SetProperty(ref NetOilVolumeValue, value); }

        }
        private decimal NetGasVolumeValue;

        public decimal NetGasVolume

        {

            get { return this.NetGasVolumeValue; }

            set { SetProperty(ref NetGasVolumeValue, value); }

        }
    }
}
