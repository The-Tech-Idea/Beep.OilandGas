using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaDeclineModel : ModelEntityBase
    {
        /// <summary>
        /// Type of decline model (Exponential, Hyperbolic, Harmonic)
        /// </summary>
        private string ModelTypeValue;

        public string ModelType

        {

            get { return this.ModelTypeValue; }

            set { SetProperty(ref ModelTypeValue, value); }

        }

        /// <summary>
        /// Fitted parameters [qi, di, b]
        /// </summary>
        private double[] ParametersValue;

        public double[] Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }

        /// <summary>
        /// Coefficient of determination (R²)
        /// </summary>
        private double RSquaredValue;

        public double RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }

        /// <summary>
        /// Akaike Information Criterion
        /// </summary>
        private double AICValue;

        public double AIC

        {

            get { return this.AICValue; }

            set { SetProperty(ref AICValue, value); }

        }

        /// <summary>
        /// Bayesian Information Criterion
        /// </summary>
        private double BICValue;

        public double BIC

        {

            get { return this.BICValue; }

            set { SetProperty(ref BICValue, value); }

        }

        /// <summary>
        /// Root mean square error
        /// </summary>
        private double RMSEValue;

        public double RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        }
    }
}
