using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class SRPDiagnosticsResult : ModelEntityBase
    {
        /// <summary>
        /// Diagnosis date
        /// </summary>
        private System.DateTime DiagnosisDateValue;

        public System.DateTime DiagnosisDate

        {

            get { return this.DiagnosisDateValue; }

            set { SetProperty(ref DiagnosisDateValue, value); }

        }

        /// <summary>
        /// User who performed diagnosis
        /// </summary>
        private string DiagnosedByUserValue = string.Empty;

        public string DiagnosedByUser

        {

            get { return this.DiagnosedByUserValue; }

            set { SetProperty(ref DiagnosedByUserValue, value); }

        }

        /// <summary>
        /// Well UWI
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Current load in pounds
        /// </summary>
        private decimal CurrentLoadValue;

        public decimal CurrentLoad

        {

            get { return this.CurrentLoadValue; }

            set { SetProperty(ref CurrentLoadValue, value); }

        }

        /// <summary>
        /// Rated load in pounds
        /// </summary>
        private decimal RatedLoadValue;

        public decimal RatedLoad

        {

            get { return this.RatedLoadValue; }

            set { SetProperty(ref RatedLoadValue, value); }

        }

        /// <summary>
        /// Current motor amperage
        /// </summary>
        private decimal CurrentAmpsValue;

        public decimal CurrentAmps

        {

            get { return this.CurrentAmpsValue; }

            set { SetProperty(ref CurrentAmpsValue, value); }

        }

        /// <summary>
        /// Rated motor amperage
        /// </summary>
        private decimal RatedAmpsValue;

        public decimal RatedAmps

        {

            get { return this.RatedAmpsValue; }

            set { SetProperty(ref RatedAmpsValue, value); }

        }

        /// <summary>
        /// Load percentage (0-100+)
        /// </summary>
        private decimal LoadPercentageValue;

        public decimal LoadPercentage

        {

            get { return this.LoadPercentageValue; }

            set { SetProperty(ref LoadPercentageValue, value); }

        }

        /// <summary>
        /// Amperage percentage (0-100+)
        /// </summary>
        private decimal AmpPercentageValue;

        public decimal AmpPercentage

        {

            get { return this.AmpPercentageValue; }

            set { SetProperty(ref AmpPercentageValue, value); }

        }

        /// <summary>
        /// List of detected issues
        /// </summary>
        private List<string> IssuesDetectedValue = new();

        public List<string> IssuesDetected

        {

            get { return this.IssuesDetectedValue; }

            set { SetProperty(ref IssuesDetectedValue, value); }

        }

        /// <summary>
        /// Diagnosis status (Normal, Warning, Critical)
        /// </summary>
        private string DiagnosisStatusValue = "Normal";

        public string DiagnosisStatus

        {

            get { return this.DiagnosisStatusValue; }

            set { SetProperty(ref DiagnosisStatusValue, value); }

        }

        /// <summary>
        /// Recommended actions
        /// </summary>
        private List<string> RecommendedActionsValue = new();

        public List<string> RecommendedActions

        {

            get { return this.RecommendedActionsValue; }

            set { SetProperty(ref RecommendedActionsValue, value); }

        }
    }
}
