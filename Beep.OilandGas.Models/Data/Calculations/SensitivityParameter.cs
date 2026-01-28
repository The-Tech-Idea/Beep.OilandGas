using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class SensitivityParameter : ModelEntityBase
    {
        /// <summary>
        /// Name of the parameter being tested
        /// </summary>
        private string ParameterNameValue;

        public string ParameterName

        {

            get { return this.ParameterNameValue; }

            set { SetProperty(ref ParameterNameValue, value); }

        }

        /// <summary>
        /// Base value of the parameter
        /// </summary>
        private double BaseValueValue;

        public double BaseValue

        {

            get { return this.BaseValueValue; }

            set { SetProperty(ref BaseValueValue, value); }

        }

        /// <summary>
        /// NPV with parameter reduced by 10%
        /// </summary>
        private double NegativeVariationNPVValue;

        public double NegativeVariationNPV

        {

            get { return this.NegativeVariationNPVValue; }

            set { SetProperty(ref NegativeVariationNPVValue, value); }

        }

        /// <summary>
        /// NPV with parameter increased by 10%
        /// </summary>
        private double PositiveVariationNPVValue;

        public double PositiveVariationNPV

        {

            get { return this.PositiveVariationNPVValue; }

            set { SetProperty(ref PositiveVariationNPVValue, value); }

        }

        /// <summary>
        /// Absolute impact on NPV from variation
        /// </summary>
        private double NPVImpactValue;

        public double NPVImpact

        {

            get { return this.NPVImpactValue; }

            set { SetProperty(ref NPVImpactValue, value); }

        }

        /// <summary>
        /// Sensitivity index (percentage change in NPV per 1% change in parameter)
        /// </summary>
        private double SensitivityIndexValue;

        public double SensitivityIndex

        {

            get { return this.SensitivityIndexValue; }

            set { SetProperty(ref SensitivityIndexValue, value); }

        }
    }
}
