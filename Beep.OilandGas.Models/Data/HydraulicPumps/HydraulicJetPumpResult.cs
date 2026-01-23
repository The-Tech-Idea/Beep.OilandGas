namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    /// <summary>
    /// Represents the result of a hydraulic jet pump performance calculation.
    /// </summary>
    public class HydraulicJetPumpResult : ModelEntityBase
    {
        /// <summary>
        /// Production rate in bbl/day.
        /// </summary>
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }

        /// <summary>
        /// Total flow rate (production + power fluid) in bbl/day.
        /// </summary>
        private decimal TotalFlowRateValue;

        public decimal TotalFlowRate

        {

            get { return this.TotalFlowRateValue; }

            set { SetProperty(ref TotalFlowRateValue, value); }

        }

        /// <summary>
        /// Production ratio (production rate / power fluid rate).
        /// </summary>
        private decimal ProductionRatioValue;

        public decimal ProductionRatio

        {

            get { return this.ProductionRatioValue; }

            set { SetProperty(ref ProductionRatioValue, value); }

        }

        /// <summary>
        /// Pump efficiency as a fraction (0-1).
        /// </summary>
        private decimal PumpEfficiencyValue;

        public decimal PumpEfficiency

        {

            get { return this.PumpEfficiencyValue; }

            set { SetProperty(ref PumpEfficiencyValue, value); }

        }

        /// <summary>
        /// Pump intake pressure in psia.
        /// </summary>
        private decimal PumpIntakePressureValue;

        public decimal PumpIntakePressure

        {

            get { return this.PumpIntakePressureValue; }

            set { SetProperty(ref PumpIntakePressureValue, value); }

        }

        /// <summary>
        /// Pump discharge pressure in psia.
        /// </summary>
        private decimal PumpDischargePressureValue;

        public decimal PumpDischargePressure

        {

            get { return this.PumpDischargePressureValue; }

            set { SetProperty(ref PumpDischargePressureValue, value); }

        }

        /// <summary>
        /// Pressure differential (discharge - intake) in psi.
        /// </summary>
        public decimal PressureDifferential => PumpDischargePressure - PumpIntakePressure;

        /// <summary>
        /// Power fluid horsepower.
        /// </summary>
        private decimal PowerFluidHorsepowerValue;

        public decimal PowerFluidHorsepower

        {

            get { return this.PowerFluidHorsepowerValue; }

            set { SetProperty(ref PowerFluidHorsepowerValue, value); }

        }

        /// <summary>
        /// Hydraulic horsepower (produced).
        /// </summary>
        private decimal HydraulicHorsepowerValue;

        public decimal HydraulicHorsepower

        {

            get { return this.HydraulicHorsepowerValue; }

            set { SetProperty(ref HydraulicHorsepowerValue, value); }

        }

        /// <summary>
        /// System efficiency as a fraction (0-1).
        /// </summary>
        private decimal SystemEfficiencyValue;

        public decimal SystemEfficiency

        {

            get { return this.SystemEfficiencyValue; }

            set { SetProperty(ref SystemEfficiencyValue, value); }

        }

        /// <summary>
        /// Pump intake temperature in degrees Rankine.
        /// </summary>
        private decimal? PumpIntakeTemperatureValue;

        public decimal? PumpIntakeTemperature

        {

            get { return this.PumpIntakeTemperatureValue; }

            set { SetProperty(ref PumpIntakeTemperatureValue, value); }

        }

        /// <summary>
        /// Pump discharge temperature in degrees Rankine.
        /// </summary>
        private decimal? PumpDischargeTemperatureValue;

        public decimal? PumpDischargeTemperature

        {

            get { return this.PumpDischargeTemperatureValue; }

            set { SetProperty(ref PumpDischargeTemperatureValue, value); }

        }

        /// <summary>
        /// Calculation timestamp.
        /// </summary>
        private DateTime CalculationTimeValue = DateTime.UtcNow;

        public DateTime CalculationTime

        {

            get { return this.CalculationTimeValue; }

            set { SetProperty(ref CalculationTimeValue, value); }

        }
    }
}


