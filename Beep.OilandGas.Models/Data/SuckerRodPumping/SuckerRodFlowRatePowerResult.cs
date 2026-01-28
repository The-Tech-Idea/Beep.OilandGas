
namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public class SuckerRodFlowRatePowerResult : ModelEntityBase
    {
        /// <summary>
        /// Production rate in bbl/day
        /// </summary>
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }

        /// <summary>
        /// Pump displacement in bbl/day
        /// </summary>
        private decimal PumpDisplacementValue;

        public decimal PumpDisplacement

        {

            get { return this.PumpDisplacementValue; }

            set { SetProperty(ref PumpDisplacementValue, value); }

        }

        /// <summary>
        /// Volumetric efficiency (0-1)
        /// </summary>
        private decimal VolumetricEfficiencyValue;

        public decimal VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        }

        /// <summary>
        /// Polished rod horsepower
        /// </summary>
        private decimal PolishedRodHorsepowerValue;

        public decimal PolishedRodHorsepower

        {

            get { return this.PolishedRodHorsepowerValue; }

            set { SetProperty(ref PolishedRodHorsepowerValue, value); }

        }

        /// <summary>
        /// Hydraulic horsepower
        /// </summary>
        private decimal HydraulicHorsepowerValue;

        public decimal HydraulicHorsepower

        {

            get { return this.HydraulicHorsepowerValue; }

            set { SetProperty(ref HydraulicHorsepowerValue, value); }

        }

        /// <summary>
        /// Friction horsepower
        /// </summary>
        private decimal FrictionHorsepowerValue;

        public decimal FrictionHorsepower

        {

            get { return this.FrictionHorsepowerValue; }

            set { SetProperty(ref FrictionHorsepowerValue, value); }

        }

        /// <summary>
        /// Total horsepower required
        /// </summary>
        private decimal TotalHorsepowerValue;

        public decimal TotalHorsepower

        {

            get { return this.TotalHorsepowerValue; }

            set { SetProperty(ref TotalHorsepowerValue, value); }

        }

        /// <summary>
        /// Motor horsepower
        /// </summary>
        private decimal MotorHorsepowerValue;

        public decimal MotorHorsepower

        {

            get { return this.MotorHorsepowerValue; }

            set { SetProperty(ref MotorHorsepowerValue, value); }

        }

        /// <summary>
        /// System efficiency (0-1)
        /// </summary>
        private decimal SystemEfficiencyValue;

        public decimal SystemEfficiency

        {

            get { return this.SystemEfficiencyValue; }

            set { SetProperty(ref SystemEfficiencyValue, value); }

        }

        /// <summary>
        /// Energy consumption in kWh/day
        /// </summary>
        private decimal EnergyConsumptionValue;

        public decimal EnergyConsumption

        {

            get { return this.EnergyConsumptionValue; }

            set { SetProperty(ref EnergyConsumptionValue, value); }

        }
    }
}
