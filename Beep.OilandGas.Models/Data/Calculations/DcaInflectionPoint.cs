using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaInflectionPoint : ModelEntityBase
    {
        /// <summary>
        /// Month number of inflection
        /// </summary>
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }

        /// <summary>
        /// Date of inflection
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Production rate at inflection
        /// </summary>
        private double ProductionValue;

        public double Production

        {

            get { return this.ProductionValue; }

            set { SetProperty(ref ProductionValue, value); }

        }
    }
}
