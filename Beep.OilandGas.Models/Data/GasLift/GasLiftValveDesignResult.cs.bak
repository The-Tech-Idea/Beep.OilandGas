using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
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
}
