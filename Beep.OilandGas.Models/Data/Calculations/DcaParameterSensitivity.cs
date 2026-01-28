using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaParameterSensitivity : ModelEntityBase
    {
        /// <summary>
        /// Production at baseline parameter value
        /// </summary>
        private double BaselineProductionValue;

        public double BaselineProduction

        {

            get { return this.BaselineProductionValue; }

            set { SetProperty(ref BaselineProductionValue, value); }

        }

        /// <summary>
        /// Production with low variation (-X%)
        /// </summary>
        private double LowVariationProductionValue;

        public double LowVariationProduction

        {

            get { return this.LowVariationProductionValue; }

            set { SetProperty(ref LowVariationProductionValue, value); }

        }

        /// <summary>
        /// Production with high variation (+X%)
        /// </summary>
        private double HighVariationProductionValue;

        public double HighVariationProduction

        {

            get { return this.HighVariationProductionValue; }

            set { SetProperty(ref HighVariationProductionValue, value); }

        }

        /// <summary>
        /// Total impact on final production as percentage
        /// </summary>
        private double ImpactOnFinalProductionValue;

        public double ImpactOnFinalProduction

        {

            get { return this.ImpactOnFinalProductionValue; }

            set { SetProperty(ref ImpactOnFinalProductionValue, value); }

        }
    }
}
