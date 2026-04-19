using System;

namespace Beep.OilandGas.Models.Data.HeatMap
{
    public class GradientPoint : ModelEntityBase
    {
        private decimal _distance;
        public decimal Distance { get { return _distance; } set { SetProperty(ref _distance, value); } }

        private decimal _temperature;
        public decimal Temperature { get { return _temperature; } set { SetProperty(ref _temperature, value); } }

        private decimal _localGradient;
        public decimal LocalGradient { get { return _localGradient; } set { SetProperty(ref _localGradient, value); } }
    }
}
