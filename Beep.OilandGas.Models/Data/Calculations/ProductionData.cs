using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProductionData : ModelEntityBase
    {
        /// <summary>
        /// Oil production in barrels
        /// </summary>
        private decimal OilVolumeValue;

        public decimal OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }

        /// <summary>
        /// Gas production in Mcf
        /// </summary>
        private decimal GasVolumeValue;

        public decimal GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }

        /// <summary>
        /// Water production in barrels
        /// </summary>
        private decimal WaterVolumeValue;

        public decimal WaterVolume

        {

            get { return this.WaterVolumeValue; }

            set { SetProperty(ref WaterVolumeValue, value); }

        }

        /// <summary>
        /// Production period date
        /// </summary>
        private DateTime PeriodDateValue;

        public DateTime PeriodDate

        {

            get { return this.PeriodDateValue; }

            set { SetProperty(ref PeriodDateValue, value); }

        }

        /// <summary>
        /// Oil API gravity
        /// </summary>
        private decimal? OilAPIGravityValue;

        public decimal? OilAPIGravity

        {

            get { return this.OilAPIGravityValue; }

            set { SetProperty(ref OilAPIGravityValue, value); }

        }

        /// <summary>
        /// Gas specific gravity
        /// </summary>
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
    }
}
