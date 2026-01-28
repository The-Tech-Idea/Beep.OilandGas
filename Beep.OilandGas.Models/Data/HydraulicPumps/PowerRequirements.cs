using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PowerRequirements : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal HydraulicPowerValue;

        public decimal HydraulicPower

        {

            get { return this.HydraulicPowerValue; }

            set { SetProperty(ref HydraulicPowerValue, value); }

        }
        private decimal MechanicalPowerValue;

        public decimal MechanicalPower

        {

            get { return this.MechanicalPowerValue; }

            set { SetProperty(ref MechanicalPowerValue, value); }

        }
        private decimal TotalPowerRequiredValue;

        public decimal TotalPowerRequired

        {

            get { return this.TotalPowerRequiredValue; }

            set { SetProperty(ref TotalPowerRequiredValue, value); }

        }
        private decimal PowerEfficiencyValue;

        public decimal PowerEfficiency

        {

            get { return this.PowerEfficiencyValue; }

            set { SetProperty(ref PowerEfficiencyValue, value); }

        }
        private string PowerUnitValue = "HP";

        public string PowerUnit

        {

            get { return this.PowerUnitValue; }

            set { SetProperty(ref PowerUnitValue, value); }

        }
    }
}
