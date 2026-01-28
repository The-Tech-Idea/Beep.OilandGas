using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class BreakevenPoint : ModelEntityBase
    {
        /// <summary>
        /// Variable value at this point (multiplier or percentage)
        /// </summary>
        private double VariableValue;

        public double Variable

        {

            get { return this.VariableValue; }

            set { SetProperty(ref VariableValue, value); }

        }

        /// <summary>
        /// NPV calculated at this variable level
        /// </summary>
        private double NPVAtVariableValue;

        public double NPVAtVariable

        {

            get { return this.NPVAtVariableValue; }

            set { SetProperty(ref NPVAtVariableValue, value); }

        }

        /// <summary>
        /// Indicates if this is (approximately) the breakeven point
        /// </summary>
        private bool IsBreakevenPointValue;

        public bool IsBreakevenPoint

        {

            get { return this.IsBreakevenPointValue; }

            set { SetProperty(ref IsBreakevenPointValue, value); }

        }
    }
}
