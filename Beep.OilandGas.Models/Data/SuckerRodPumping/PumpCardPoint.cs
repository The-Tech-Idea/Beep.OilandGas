using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public class PumpCardPoint : ModelEntityBase
    {
        /// <summary>
        /// Position (0-1, where 0 = bottom of stroke, 1 = top of stroke)
        /// </summary>
        private decimal PositionValue;

        public decimal Position

        {

            get { return this.PositionValue; }

            set { SetProperty(ref PositionValue, value); }

        }

        /// <summary>
        /// Load in pounds
        /// </summary>
        private decimal LoadValue;

        public decimal Load

        {

            get { return this.LoadValue; }

            set { SetProperty(ref LoadValue, value); }

        }
    }
}
