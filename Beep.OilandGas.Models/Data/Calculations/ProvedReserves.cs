using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProvedReserves : ModelEntityBase
    {
        /// <summary>
        /// Proved oil reserves in barrels
        /// </summary>
        private decimal ProvedOilReservesValue;

        public decimal ProvedOilReserves

        {

            get { return this.ProvedOilReservesValue; }

            set { SetProperty(ref ProvedOilReservesValue, value); }

        }

        /// <summary>
        /// Proved gas reserves in Mcf
        /// </summary>
        private decimal ProvedGasReservesValue;

        public decimal ProvedGasReserves

        {

            get { return this.ProvedGasReservesValue; }

            set { SetProperty(ref ProvedGasReservesValue, value); }

        }

        /// <summary>
        /// Proved reserves as of date
        /// </summary>
        private DateTime AsOfDateValue;

        public DateTime AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }

        /// <summary>
        /// Reserve estimation method (volumetric, performance, or analog)
        /// </summary>
        private string EstimationMethodValue = "Volumetric";

        public string EstimationMethod

        {

            get { return this.EstimationMethodValue; }

            set { SetProperty(ref EstimationMethodValue, value); }

        }

        /// <summary>
        /// Confidence level (1P, 2P, 3P)
        /// </summary>
        private string ConfidenceLevelValue = "1P";

        public string ConfidenceLevel

        {

            get { return this.ConfidenceLevelValue; }

            set { SetProperty(ref ConfidenceLevelValue, value); }

        }
    }
}
