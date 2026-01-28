using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class NodalPoint : ModelEntityBase
    {
        private string PointNameValue = string.Empty;

        public string PointName

        {

            get { return this.PointNameValue; }

            set { SetProperty(ref PointNameValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal RestrictionTypeValue;

        public decimal RestrictionType

        {

            get { return this.RestrictionTypeValue; }

            set { SetProperty(ref RestrictionTypeValue, value); }

        } // 0=Capacity, 1=Restriction
    }
}
