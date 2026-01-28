using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaMonteCarloMonthlyStats : ModelEntityBase
    {
        /// <summary>
        /// Forecast month number (1-N)
        /// </summary>
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }

        /// <summary>
        /// Mean production rate (bbl/day)
        /// </summary>
        private double MeanProductionValue;

        public double MeanProduction

        {

            get { return this.MeanProductionValue; }

            set { SetProperty(ref MeanProductionValue, value); }

        }

        /// <summary>
        /// Median production rate (bbl/day)
        /// </summary>
        private double MedianProductionValue;

        public double MedianProduction

        {

            get { return this.MedianProductionValue; }

            set { SetProperty(ref MedianProductionValue, value); }

        }

        /// <summary>
        /// 10th percentile production rate
        /// </summary>
        private double P10ProductionValue;

        public double P10Production

        {

            get { return this.P10ProductionValue; }

            set { SetProperty(ref P10ProductionValue, value); }

        }

        /// <summary>
        /// 50th percentile production rate
        /// </summary>
        private double P50ProductionValue;

        public double P50Production

        {

            get { return this.P50ProductionValue; }

            set { SetProperty(ref P50ProductionValue, value); }

        }

        /// <summary>
        /// 90th percentile production rate
        /// </summary>
        private double P90ProductionValue;

        public double P90Production

        {

            get { return this.P90ProductionValue; }

            set { SetProperty(ref P90ProductionValue, value); }

        }

        /// <summary>
        /// Minimum production in simulations
        /// </summary>
        private double MinProductionValue;

        public double MinProduction

        {

            get { return this.MinProductionValue; }

            set { SetProperty(ref MinProductionValue, value); }

        }

        /// <summary>
        /// Maximum production in simulations
        /// </summary>
        private double MaxProductionValue;

        public double MaxProduction

        {

            get { return this.MaxProductionValue; }

            set { SetProperty(ref MaxProductionValue, value); }

        }

        /// <summary>
        /// Standard deviation of production
        /// </summary>
        private double StandardDeviationValue;

        public double StandardDeviation

        {

            get { return this.StandardDeviationValue; }

            set { SetProperty(ref StandardDeviationValue, value); }

        }
    }
}
