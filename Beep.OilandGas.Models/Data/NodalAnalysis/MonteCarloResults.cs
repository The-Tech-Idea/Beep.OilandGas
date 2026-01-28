using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class MonteCarloResults : ModelEntityBase {
        /// <summary>
        /// Mean production rate from simulation (bbl/day or equivalent)
        /// </summary>
        private double _meanValue;
        public double Mean
        {
            get { return _meanValue; }
            set { SetProperty(ref _meanValue, value); }
        }

        /// <summary>
        /// Standard deviation of production rate
        /// </summary>
        private double _standardDeviationValue;
        public double StandardDeviation
        {
            get { return _standardDeviationValue; }
            set { SetProperty(ref _standardDeviationValue, value); }
        }

        /// <summary>
        /// 10th percentile production rate (P10 - optimistic case)
        /// </summary>
        private double _p10Value;
        public double P10
        {
            get { return _p10Value; }
            set { SetProperty(ref _p10Value, value); }
        }

        /// <summary>
        /// 50th percentile production rate (P50 - median/expected case)
        /// </summary>
        private double _p50Value;
        public double P50
        {
            get { return _p50Value; }
            set { SetProperty(ref _p50Value, value); }
        }

        /// <summary>
        /// 90th percentile production rate (P90 - conservative case)
        /// </summary>
        private double _p90Value;
        public double P90
        {
            get { return _p90Value; }
            set { SetProperty(ref _p90Value, value); }
        }

        /// <summary>
        /// Minimum production rate from all iterations
        /// </summary>
        private double _minimumValue;
        public double Minimum
        {
            get { return _minimumValue; }
            set { SetProperty(ref _minimumValue, value); }
        }

        /// <summary>
        /// Maximum production rate from all iterations
        /// </summary>
        private double _maximumValue;
        public double Maximum
        {
            get { return _maximumValue; }
            set { SetProperty(ref _maximumValue, value); }
        }

        /// <summary>
        /// Number of Monte Carlo iterations performed
        /// </summary>
        private int? _iterationsValue;
        public int? Iterations
        {
            get { return _iterationsValue; }
            set { SetProperty(ref _iterationsValue, value); }
        }

      

        /// <summary>
        /// Default constructor
        /// </summary>
        public MonteCarloResults()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
