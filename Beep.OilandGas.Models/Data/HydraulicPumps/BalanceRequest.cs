using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class BalanceRequest : ModelEntityBase
    {
        private decimal DischargeFlowValue;

        public decimal DischargeFlow

        {

            get { return this.DischargeFlowValue; }

            set { SetProperty(ref DischargeFlowValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
    }
}
