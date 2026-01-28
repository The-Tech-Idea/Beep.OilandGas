using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class WellTestDataPoint : ModelEntityBase
    {
        private decimal? TimeValue;

        public decimal? Time

        {

            get { return this.TimeValue; }

            set { SetProperty(ref TimeValue, value); }

        } // hours
        private decimal? PressureValue;

        public decimal? Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        } // psi
    }
}
