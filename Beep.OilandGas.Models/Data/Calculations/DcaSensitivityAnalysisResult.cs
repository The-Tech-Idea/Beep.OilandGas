using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaSensitivityAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Base parameters used for analysis [qi, di, b]
        /// </summary>
        private double[] BaseParametersValue;

        public double[] BaseParameters

        {

            get { return this.BaseParametersValue; }

            set { SetProperty(ref BaseParametersValue, value); }

        }

        /// <summary>
        /// Percentage variation applied to parameters (e.g., 20 for ±20%)
        /// </summary>
        private double VariationPercentValue;

        public double VariationPercent

        {

            get { return this.VariationPercentValue; }

            set { SetProperty(ref VariationPercentValue, value); }

        }

        /// <summary>
        /// Sensitivity results for initial production rate (qi)
        /// </summary>
        private DcaParameterSensitivity QiSensitivityValue;

        public DcaParameterSensitivity QiSensitivity

        {

            get { return this.QiSensitivityValue; }

            set { SetProperty(ref QiSensitivityValue, value); }

        }

        /// <summary>
        /// Sensitivity results for initial decline rate (di)
        /// </summary>
        private DcaParameterSensitivity DiSensitivityValue;

        public DcaParameterSensitivity DiSensitivity

        {

            get { return this.DiSensitivityValue; }

            set { SetProperty(ref DiSensitivityValue, value); }

        }

        /// <summary>
        /// Sensitivity results for decline exponent (b)
        /// </summary>
        private DcaParameterSensitivity BSensitivityValue;

        public DcaParameterSensitivity BSensitivity

        {

            get { return this.BSensitivityValue; }

            set { SetProperty(ref BSensitivityValue, value); }

        }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }
}
