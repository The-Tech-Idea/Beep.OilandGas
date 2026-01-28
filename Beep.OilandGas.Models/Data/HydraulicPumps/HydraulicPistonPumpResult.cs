
namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    public class HydraulicPistonPumpResult : ModelEntityBase
    {
        /// <summary>
        /// Pump displacement in cubic inches per stroke or bbl/day equivalent.
        /// </summary>
        private decimal PumpDisplacementValue;

        public decimal PumpDisplacement

        {

            get { return this.PumpDisplacementValue; }

            set { SetProperty(ref PumpDisplacementValue, value); }

        }

        /// <summary>
        /// Production rate in barrels per day (bbl/day).
        /// </summary>
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }

        /// <summary>
        /// Power fluid consumption in bbl/day.
        /// </summary>
        private decimal PowerFluidConsumptionValue;

        public decimal PowerFluidConsumption

        {

            get { return this.PowerFluidConsumptionValue; }

            set { SetProperty(ref PowerFluidConsumptionValue, value); }

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
        /// Power fluid horsepower required.
        /// </summary>
        private decimal PowerFluidHorsepowerValue;

        public decimal PowerFluidHorsepower

        {

            get { return this.PowerFluidHorsepowerValue; }

            set { SetProperty(ref PowerFluidHorsepowerValue, value); }

        }

        /// <summary>
        /// Hydraulic horsepower output.
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
        /// Inlet pressure in psia.
        /// </summary>
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }

        /// <summary>
        /// Outlet pressure in psia.
        /// </summary>
        private decimal OutletPressureValue;

        public decimal OutletPressure

        {

            get { return this.OutletPressureValue; }

            set { SetProperty(ref OutletPressureValue, value); }

        }

        /// <summary>
        /// Pressure differential (outlet - inlet) in psi.
        /// </summary>
        public decimal PressureDifferential => OutletPressure - InletPressure;

        /// <summary>
        /// Pump speed in revolutions per minute (RPM).
        /// </summary>
        private decimal SpeedValue;

        public decimal Speed

        {

            get { return this.SpeedValue; }

            set { SetProperty(ref SpeedValue, value); }

        }

        /// <summary>
        /// Volumetric efficiency as a fraction (0-1).
        /// </summary>
        private decimal VolumetricEfficiencyValue;

        public decimal VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        }

        /// <summary>
        /// Mechanical efficiency as a fraction (0-1).
        /// </summary>
        private decimal MechanicalEfficiencyValue;

        public decimal MechanicalEfficiency

        {

            get { return this.MechanicalEfficiencyValue; }

            set { SetProperty(ref MechanicalEfficiencyValue, value); }

        }

        /// <summary>
        /// Overall efficiency as a fraction (0-1).
        /// </summary>
        private decimal OverallEfficiencyValue;

        public decimal OverallEfficiency

        {

            get { return this.OverallEfficiencyValue; }

            set { SetProperty(ref OverallEfficiencyValue, value); }

        }

        /// <summary>
        /// Input horsepower required.
        /// </summary>
        private decimal InputHorsepowerValue;

        public decimal InputHorsepower

        {

            get { return this.InputHorsepowerValue; }

            set { SetProperty(ref InputHorsepowerValue, value); }

        }

        /// <summary>
        /// Slippage in barrels per day.
        /// </summary>
        private decimal SlippageValue;

        public decimal Slippage

        {

            get { return this.SlippageValue; }

            set { SetProperty(ref SlippageValue, value); }

        }

        /// <summary>
        /// Pump outlet temperature in degrees Rankine.
        /// </summary>
        private decimal? OutletTemperatureValue;

        public decimal? OutletTemperature

        {

            get { return this.OutletTemperatureValue; }

            set { SetProperty(ref OutletTemperatureValue, value); }

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
