using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    public class FlashConditions : ModelEntityBase
    {
        /// <summary>
        /// Pressure (in specified units)
        /// </summary>
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        /// <summary>
        /// Temperature (in specified units)
        /// </summary>
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }

        /// <summary>
        /// Feed composition (component mole fractions)
        /// </summary>
        private List<FlashComponent> FeedCompositionValue = new List<FlashComponent>();

        public List<FlashComponent> FeedComposition

        {

            get { return this.FeedCompositionValue; }

            set { SetProperty(ref FeedCompositionValue, value); }

        }
    }
}
