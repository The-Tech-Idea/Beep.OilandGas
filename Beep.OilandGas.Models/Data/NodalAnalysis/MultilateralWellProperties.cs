using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public class MultilateralWellProperties : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier).
        /// </summary>
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Main wellbore depth in feet.
        /// </summary>
        private double MainWellboreDepthValue;

        public double MainWellboreDepth

        {

            get { return this.MainWellboreDepthValue; }

            set { SetProperty(ref MainWellboreDepthValue, value); }

        }

        /// <summary>
        /// Collection of lateral branches in the well.
        /// </summary>
        private List<LateralBranch> LateralBranchesValue = new List<LateralBranch>();

        public List<LateralBranch> LateralBranches

        {

            get { return this.LateralBranchesValue; }

            set { SetProperty(ref LateralBranchesValue, value); }

        }

        /// <summary>
        /// Junction depth where laterals meet in feet.
        /// </summary>
        private double? JunctionDepthValue;

        public double? JunctionDepth

        {

            get { return this.JunctionDepthValue; }

            set { SetProperty(ref JunctionDepthValue, value); }

        }

        /// <summary>
        /// Total pressure drop from junction to wellhead in psia.
        /// </summary>
        private double? JunctionToWellheadPressureDropValue;

        public double? JunctionToWellheadPressureDrop

        {

            get { return this.JunctionToWellheadPressureDropValue; }

            set { SetProperty(ref JunctionToWellheadPressureDropValue, value); }

        }

        /// <summary>
        /// Wellhead pressure in psia.
        /// </summary>
        private double? WellheadPressureValue;

        public double? WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        }
    }
}
