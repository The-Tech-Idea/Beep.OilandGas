using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Result of gas lift valve design calculation
    /// DTO for calculations - Entity class: GAS_LIFT_VALVE_DESIGN_RESULT
    /// </summary>
    public class GasLiftValveDesignResult : ModelEntityBase
    {
        /// <summary>
        /// List of designed valves
        /// </summary>
        private List<GasLiftValve> ValvesValue = new List<GasLiftValve>();

        public List<GasLiftValve> Valves

        {

            get { return this.ValvesValue; }

            set { SetProperty(ref ValvesValue, value); }

        }

        /// <summary>
        /// Total gas injection rate for all valves
        /// </summary>
        private decimal TotalGasInjectionRateValue;

        public decimal TotalGasInjectionRate

        {

            get { return this.TotalGasInjectionRateValue; }

            set { SetProperty(ref TotalGasInjectionRateValue, value); }

        }

        /// <summary>
        /// Expected production rate
        /// </summary>
        private decimal ExpectedProductionRateValue;

        public decimal ExpectedProductionRate

        {

            get { return this.ExpectedProductionRateValue; }

            set { SetProperty(ref ExpectedProductionRateValue, value); }

        }

        /// <summary>
        /// System efficiency
        /// </summary>
        private decimal SystemEfficiencyValue;

        public decimal SystemEfficiency

        {

            get { return this.SystemEfficiencyValue; }

            set { SetProperty(ref SystemEfficiencyValue, value); }

        }
    }

    /// <summary>
    /// Represents a gas lift valve
    /// DTO for calculations - Entity class: GAS_LIFT_VALVE
    /// </summary>
    public class GasLiftValve : ModelEntityBase
    {
        /// <summary>
        /// Valve depth (feet or meters)
        /// </summary>
        private decimal DepthValue;

        public decimal Depth

        {

            get { return this.DepthValue; }

            set { SetProperty(ref DepthValue, value); }

        }

        /// <summary>
        /// Port size (inches or mm)
        /// </summary>
        private decimal PortSizeValue;

        public decimal PortSize

        {

            get { return this.PortSizeValue; }

            set { SetProperty(ref PortSizeValue, value); }

        }

        /// <summary>
        /// Opening pressure (psia or kPa)
        /// </summary>
        private decimal OpeningPressureValue;

        public decimal OpeningPressure

        {

            get { return this.OpeningPressureValue; }

            set { SetProperty(ref OpeningPressureValue, value); }

        }

        /// <summary>
        /// Closing pressure (psia or kPa)
        /// </summary>
        private decimal ClosingPressureValue;

        public decimal ClosingPressure

        {

            get { return this.ClosingPressureValue; }

            set { SetProperty(ref ClosingPressureValue, value); }

        }

        /// <summary>
        /// Valve type
        /// </summary>
        private GasLiftValveType ValveTypeValue;

        public GasLiftValveType ValveType

        {

            get { return this.ValveTypeValue; }

            set { SetProperty(ref ValveTypeValue, value); }

        }

        /// <summary>
        /// Temperature at valve depth (°F or °C)
        /// </summary>
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }

        /// <summary>
        /// Gas injection rate through this valve (Mscf/day or m³/day)
        /// </summary>
        private decimal GasInjectionRateValue;

        public decimal GasInjectionRate

        {

            get { return this.GasInjectionRateValue; }

            set { SetProperty(ref GasInjectionRateValue, value); }

        }
    }
}






