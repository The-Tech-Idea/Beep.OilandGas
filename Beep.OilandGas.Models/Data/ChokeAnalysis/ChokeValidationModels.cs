using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    /// <summary>
    /// Validation result for choke configuration.
    /// </summary>
    public class ChokeValidationResult : ModelEntityBase
    {
        /// <summary>
        /// Indicates if configuration is valid.
        /// </summary>
        private bool IsValidValue;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }

        /// <summary>
        /// List of validation errors.
        /// </summary>
        private string[] ErrorsValue = Array.Empty<string>();

        public string[] Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }

        /// <summary>
        /// List of warnings.
        /// </summary>
        private string[] WarningsValue = Array.Empty<string>();

        public string[] Warnings

        {

            get { return this.WarningsValue; }

            set { SetProperty(ref WarningsValue, value); }

        }
    }

    /// <summary>
    /// Choke performance curve data point.
    /// </summary>
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



