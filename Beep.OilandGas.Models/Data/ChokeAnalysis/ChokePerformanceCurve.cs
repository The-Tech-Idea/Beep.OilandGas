using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    public class ChokePerformanceCurve : ModelEntityBase
    {
        /// <summary>
        /// Downstream pressure for this data point.
        /// </summary>
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        }

        /// <summary>
        /// Calculated flow rate at this condition.
        /// </summary>
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }

        /// <summary>
        /// Flow regime at this condition.
        /// </summary>
        private FlowRegime FlowRegimeValue;

        public FlowRegime FlowRegime

        {

            get { return this.FlowRegimeValue; }

            set { SetProperty(ref FlowRegimeValue, value); }

        }

        /// <summary>
        /// Pressure ratio (P2/P1) for this condition.
        /// </summary>
        private decimal PressureRatioValue;

        public decimal PressureRatio

        {

            get { return this.PressureRatioValue; }

            set { SetProperty(ref PressureRatioValue, value); }

        }

        /// <summary>
        /// Indicates if flow is critical at this condition.
        /// </summary>
        private bool IsCriticalFlowValue;

        public bool IsCriticalFlow

        {

            get { return this.IsCriticalFlowValue; }

            set { SetProperty(ref IsCriticalFlowValue, value); }

        }
    }
}
