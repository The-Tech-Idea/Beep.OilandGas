using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeOpeningAdjustment : ModelEntityBase
    {
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }
        private decimal CurrentChokeDiameterValue;

        public decimal CurrentChokeDiameter

        {

            get { return this.CurrentChokeDiameterValue; }

            set { SetProperty(ref CurrentChokeDiameterValue, value); }

        }
        private decimal AdjustToChokeDiameterValue;

        public decimal AdjustToChokeDiameter

        {

            get { return this.AdjustToChokeDiameterValue; }

            set { SetProperty(ref AdjustToChokeDiameterValue, value); }

        }
        private decimal PercentChangeValue;

        public decimal PercentChange

        {

            get { return this.PercentChangeValue; }

            set { SetProperty(ref PercentChangeValue, value); }

        }
        private string ReasonValue = string.Empty;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
        private decimal ExpectedProductionChangeValue;

        public decimal ExpectedProductionChange

        {

            get { return this.ExpectedProductionChangeValue; }

            set { SetProperty(ref ExpectedProductionChangeValue, value); }

        }
    }
}
