
namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    public class ForecastPoint : ModelEntityBase
    {
        /// <summary>
        /// Time in days from start
        /// </summary>
        private decimal TimeValue;

        public decimal Time

        {

            get { return this.TimeValue; }

            set { SetProperty(ref TimeValue, value); }

        }

        /// <summary>
        /// Production rate in bbl/day or Mscf/day
        /// </summary>
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }

        /// <summary>
        /// Cumulative production in bbl or Mscf
        /// </summary>
        private decimal CumulativeProductionValue;

        public decimal CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }

        /// <summary>
        /// Reservoir pressure in psia
        /// </summary>
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }

        /// <summary>
        /// Bottom hole pressure in psia
        /// </summary>
        private decimal BottomHolePressureValue;

        public decimal BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }

        /// <summary>
        /// Decline exponent (b) for decline curve analysis
        /// Range 0-1: 0=exponential, 0.5=typical hyperbolic, 1=harmonic
        /// </summary>
        private decimal? DeclineExponentValue;

        public decimal? DeclineExponent

        {

            get { return this.DeclineExponentValue; }

            set { SetProperty(ref DeclineExponentValue, value); }

        }

        /// <summary>
        /// Forecast method used (e.g., "Exponential Decline", "Hyperbolic Decline (b=0.5)")
        /// </summary>
        private string ForecastMethodValue;

        public string ForecastMethod

        {

            get { return this.ForecastMethodValue; }

            set { SetProperty(ref ForecastMethodValue, value); }

        }
    }
}
