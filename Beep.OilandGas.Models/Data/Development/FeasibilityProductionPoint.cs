using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FeasibilityProductionPoint : ModelEntityBase
    {
        private int YearValue;

        public int Year

        {

            get { return this.YearValue; }

            set { SetProperty(ref YearValue, value); }

        }
        private decimal? OilVolumeValue;

        public decimal? OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }
        private decimal? GasVolumeValue;

        public decimal? GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }
        private decimal? WaterVolumeValue;

        public decimal? WaterVolume

        {

            get { return this.WaterVolumeValue; }

            set { SetProperty(ref WaterVolumeValue, value); }

        }
        private string? VolumeOuomValue;

        public string? VolumeOuom

        {

            get { return this.VolumeOuomValue; }

            set { SetProperty(ref VolumeOuomValue, value); }

        }
        private decimal? OperatingCostValue;

        public decimal? OperatingCost

        {

            get { return this.OperatingCostValue; }

            set { SetProperty(ref OperatingCostValue, value); }

        }
    }
}
