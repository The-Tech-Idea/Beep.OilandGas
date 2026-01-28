using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaModelComparisonMetricsResult : ModelEntityBase
    {
        /// <summary>
        /// Number of models compared
        /// </summary>
        private int ModelsComparedValue;

        public int ModelsCompared

        {

            get { return this.ModelsComparedValue; }

            set { SetProperty(ref ModelsComparedValue, value); }

        }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Metrics for each model
        /// </summary>
        private List<DcaModelMetric> ModelMetricsValue = new();

        public List<DcaModelMetric> ModelMetrics

        {

            get { return this.ModelMetricsValue; }

            set { SetProperty(ref ModelMetricsValue, value); }

        }

        /// <summary>
        /// Index of model with best AIC
        /// </summary>
        private int BestAICIndexValue;

        public int BestAICIndex

        {

            get { return this.BestAICIndexValue; }

            set { SetProperty(ref BestAICIndexValue, value); }

        }

        /// <summary>
        /// Index of model with best R²
        /// </summary>
        private int BestRSquaredIndexValue;

        public int BestRSquaredIndex

        {

            get { return this.BestRSquaredIndexValue; }

            set { SetProperty(ref BestRSquaredIndexValue, value); }

        }

        /// <summary>
        /// Index of model with best BIC
        /// </summary>
        private int BestBICIndexValue;

        public int BestBICIndex

        {

            get { return this.BestBICIndexValue; }

            set { SetProperty(ref BestBICIndexValue, value); }

        }
    }
}
