using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Represents the result of a multilateral well deliverability calculation.
    /// </summary>
    public class MultilateralDeliverabilityResult : ModelEntityBase
    {
        /// <summary>
        /// Bottomhole pressure at the junction in psia.
        /// </summary>
        private double JunctionBottomholePressureValue;

        public double JunctionBottomholePressure

        {

            get { return this.JunctionBottomholePressureValue; }

            set { SetProperty(ref JunctionBottomholePressureValue, value); }

        }

        /// <summary>
        /// Total production rate from all branches in bbl/day (oil) or Mscf/day (gas).
        /// </summary>
        private double TotalProductionRateValue;

        public double TotalProductionRate

        {

            get { return this.TotalProductionRateValue; }

            set { SetProperty(ref TotalProductionRateValue, value); }

        }

        /// <summary>
        /// Production rates by branch name in bbl/day or Mscf/day.
        /// </summary>
        public Dictionary<string, double> BranchProductionRates { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Average pressure throughout the system in psia.
        /// </summary>
        private double? AveragePressureValue;

        public double? AveragePressure

        {

            get { return this.AveragePressureValue; }

            set { SetProperty(ref AveragePressureValue, value); }

        }

        /// <summary>
        /// Maximum pressure drop across any branch in psi.
        /// </summary>
        private double? MaximumPressureDropValue;

        public double? MaximumPressureDrop

        {

            get { return this.MaximumPressureDropValue; }

            set { SetProperty(ref MaximumPressureDropValue, value); }

        }

        /// <summary>
        /// Indicates if the system is stable or unstable.
        /// </summary>
        private bool IsStableValue = true;

        public bool IsStable

        {

            get { return this.IsStableValue; }

            set { SetProperty(ref IsStableValue, value); }

        }
    }
}



