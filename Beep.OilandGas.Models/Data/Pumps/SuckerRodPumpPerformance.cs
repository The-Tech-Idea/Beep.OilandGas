using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class SuckerRodPumpPerformance : ModelEntityBase
    {
        /// <summary>
        /// Pump identifier
        /// </summary>
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }

        /// <summary>
        /// Performance analysis date
        /// </summary>
        private System.DateTime PerformanceDateValue;

        public System.DateTime PerformanceDate

        {

            get { return this.PerformanceDateValue; }

            set { SetProperty(ref PerformanceDateValue, value); }

        }

        /// <summary>
        /// Flow rate in bbl/day
        /// </summary>
        private decimal FlowRateValue;

        [Range(0, double.MaxValue)]
        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }

        /// <summary>
        /// Pump efficiency (0-1 or percentage)
        /// </summary>
        private decimal EfficiencyValue;

        [Range(0, 1.5)]
        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }

        /// <summary>
        /// Power consumption in horsepower
        /// </summary>
        private decimal PowerConsumptionValue;

        [Range(0, double.MaxValue)]
        public decimal PowerConsumption

        {

            get { return this.PowerConsumptionValue; }

            set { SetProperty(ref PowerConsumptionValue, value); }

        }

        /// <summary>
        /// Rod load percentage (0-100+)
        /// </summary>
        private decimal RodLoadPercentageValue;

        [Range(0, double.MaxValue)]
        public decimal RodLoadPercentage

        {

            get { return this.RodLoadPercentageValue; }

            set { SetProperty(ref RodLoadPercentageValue, value); }

        }

        /// <summary>
        /// Performance status
        /// </summary>
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
