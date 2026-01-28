using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaModelMetric : ModelEntityBase
    {
        /// <summary>
        /// Model index in comparison
        /// </summary>
        private int ModelIndexValue;

        public int ModelIndex

        {

            get { return this.ModelIndexValue; }

            set { SetProperty(ref ModelIndexValue, value); }

        }

        /// <summary>
        /// R² coefficient of determination
        /// </summary>
        private double RSquaredValue;

        public double RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }

        /// <summary>
        /// Adjusted R²
        /// </summary>
        private double AdjustedRSquaredValue;

        public double AdjustedRSquared

        {

            get { return this.AdjustedRSquaredValue; }

            set { SetProperty(ref AdjustedRSquaredValue, value); }

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

        /// <summary>
        /// Mean absolute error
        /// </summary>
        private double MAEValue;

        public double MAE

        {

            get { return this.MAEValue; }

            set { SetProperty(ref MAEValue, value); }

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
        /// Whether model converged
        /// </summary>
        private bool ConvergedValue;

        public bool Converged

        {

            get { return this.ConvergedValue; }

            set { SetProperty(ref ConvergedValue, value); }

        }

        /// <summary>
        /// Number of parameters in model
        /// </summary>
        private int ParameterCountValue;

        public int ParameterCount

        {

            get { return this.ParameterCountValue; }

            set { SetProperty(ref ParameterCountValue, value); }

        }

        /// <summary>
        /// Overall performance score (0-100)
        /// </summary>
        private double PerformanceScoreValue;

        public double PerformanceScore

        {

            get { return this.PerformanceScoreValue; }

            set { SetProperty(ref PerformanceScoreValue, value); }

        }
    }
}
