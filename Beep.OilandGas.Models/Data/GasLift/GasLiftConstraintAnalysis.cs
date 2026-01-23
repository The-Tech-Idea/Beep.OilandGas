using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Constraint analysis result for gas lift design
    /// </summary>
    public class GasLiftConstraintAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the analysis date
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the user who performed the analysis
        /// </summary>
        private string AnalyzedByUserValue = string.Empty;

        public string AnalyzedByUser

        {

            get { return this.AnalyzedByUserValue; }

            set { SetProperty(ref AnalyzedByUserValue, value); }

        }

        /// <summary>
        /// Gets or sets the well UWI
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum tubing pressure (psia)
        /// </summary>
        private decimal MaxTubingPressureValue;

        public decimal MaxTubingPressure

        {

            get { return this.MaxTubingPressureValue; }

            set { SetProperty(ref MaxTubingPressureValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum casing pressure (psia)
        /// </summary>
        private decimal MaxCasingPressureValue;

        public decimal MaxCasingPressure

        {

            get { return this.MaxCasingPressureValue; }

            set { SetProperty(ref MaxCasingPressureValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum surface equipment pressure (psia)
        /// </summary>
        private decimal MaxSurfaceEquipmentPressureValue;

        public decimal MaxSurfaceEquipmentPressure

        {

            get { return this.MaxSurfaceEquipmentPressureValue; }

            set { SetProperty(ref MaxSurfaceEquipmentPressureValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum available gas supply (Mscf/day)
        /// </summary>
        private decimal MaxAvailableGasSupplyValue;

        public decimal MaxAvailableGasSupply

        {

            get { return this.MaxAvailableGasSupplyValue; }

            set { SetProperty(ref MaxAvailableGasSupplyValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum production capacity (BPD)
        /// </summary>
        private decimal MaxProductionCapacityValue;

        public decimal MaxProductionCapacity

        {

            get { return this.MaxProductionCapacityValue; }

            set { SetProperty(ref MaxProductionCapacityValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum tubing temperature (°F)
        /// </summary>
        private decimal MaxTubingTemperatureValue;

        public decimal MaxTubingTemperature

        {

            get { return this.MaxTubingTemperatureValue; }

            set { SetProperty(ref MaxTubingTemperatureValue, value); }

        }

        /// <summary>
        /// Gets or sets the list of active constraints
        /// </summary>
        private List<string> ActiveConstraintsValue = new();

        public List<string> ActiveConstraints

        {

            get { return this.ActiveConstraintsValue; }

            set { SetProperty(ref ActiveConstraintsValue, value); }

        }
    }
}



