using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Performance diagnosis result for gas lift operations
    /// </summary>
    public class GasLiftPerformanceDiagnosisResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the diagnosis date
        /// </summary>
        private DateTime DiagnosisDateValue;

        public DateTime DiagnosisDate

        {

            get { return this.DiagnosisDateValue; }

            set { SetProperty(ref DiagnosisDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the user who performed the diagnosis
        /// </summary>
        private string DiagnosedByUserValue = string.Empty;

        public string DiagnosedByUser

        {

            get { return this.DiagnosedByUserValue; }

            set { SetProperty(ref DiagnosedByUserValue, value); }

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
        /// Gets or sets the production change percentage
        /// </summary>
        private decimal ProductionChangePercentValue;

        public decimal ProductionChangePercent

        {

            get { return this.ProductionChangePercentValue; }

            set { SetProperty(ref ProductionChangePercentValue, value); }

        }

        /// <summary>
        /// Gets or sets the gas injection rate change percentage
        /// </summary>
        private decimal GasInjectionChangePercentValue;

        public decimal GasInjectionChangePercent

        {

            get { return this.GasInjectionChangePercentValue; }

            set { SetProperty(ref GasInjectionChangePercentValue, value); }

        }

        /// <summary>
        /// Gets or sets the efficiency change percentage
        /// </summary>
        private decimal EfficiencyChangePercentValue;

        public decimal EfficiencyChangePercent

        {

            get { return this.EfficiencyChangePercentValue; }

            set { SetProperty(ref EfficiencyChangePercentValue, value); }

        }

        /// <summary>
        /// Gets or sets the list of detected issues
        /// </summary>
        private List<string> IssuesDetectedValue = new();

        public List<string> IssuesDetected

        {

            get { return this.IssuesDetectedValue; }

            set { SetProperty(ref IssuesDetectedValue, value); }

        }

        /// <summary>
        /// Gets or sets the performance status (Normal, Degraded, Poor, Critical)
        /// </summary>
        private string PerformanceStatusValue = "Normal";

        public string PerformanceStatus

        {

            get { return this.PerformanceStatusValue; }

            set { SetProperty(ref PerformanceStatusValue, value); }

        }

        /// <summary>
        /// Gets or sets the recommended actions
        /// </summary>
        private List<string> RecommendedActionsValue = new();

        public List<string> RecommendedActions

        {

            get { return this.RecommendedActionsValue; }

            set { SetProperty(ref RecommendedActionsValue, value); }

        }
    }
}



