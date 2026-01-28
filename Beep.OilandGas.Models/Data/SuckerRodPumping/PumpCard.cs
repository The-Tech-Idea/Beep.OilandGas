using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public class PumpCard : ModelEntityBase
    {
        /// <summary>
        /// Peak load
        /// </summary>
        private decimal PeakLoadValue;

        public decimal PeakLoad

        {

            get { return this.PeakLoadValue; }

            set { SetProperty(ref PeakLoadValue, value); }

        }

        /// <summary>
        /// Minimum load
        /// </summary>
        private decimal MinimumLoadValue;

        public decimal MinimumLoad

        {

            get { return this.MinimumLoadValue; }

            set { SetProperty(ref MinimumLoadValue, value); }

        }

        /// <summary>
        /// Net area
        /// </summary>
        private decimal NetAreaValue;

        public decimal NetArea

        {

            get { return this.NetAreaValue; }

            set { SetProperty(ref NetAreaValue, value); }

        }

        /// <summary>
        /// Points on the pump card
        /// </summary>
        private List<PumpCardPoint> PointsValue = new();

        public List<PumpCardPoint> Points

        {

            get { return this.PointsValue; }

            set { SetProperty(ref PointsValue, value); }

        }
    }
}
