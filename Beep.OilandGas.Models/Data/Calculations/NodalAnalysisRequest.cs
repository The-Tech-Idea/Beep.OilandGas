using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class NodalAnalysisRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Nodal analysis parameters
        /// </summary>
        private NodalAnalysisParameters AnalysisParametersValue = null!;

        [Required(ErrorMessage = "AnalysisParameters are required")]
        public NodalAnalysisParameters AnalysisParameters

        {

            get { return this.AnalysisParametersValue; }

            set { SetProperty(ref AnalysisParametersValue, value); }

        }

        public decimal? ReservoirPressure { get; set; }
        public decimal? ProductivityIndex { get; set; }
        public decimal? WaterCut { get; set; }
        public decimal? GasOilRatio { get; set; }
        public decimal? OilGravity { get; set; }
        public decimal? WellDepth { get; set; }
        public decimal? TubingDiameter { get; set; }
        public decimal? WellheadPressure { get; set; }
        public decimal? GasGravity { get; set; }
        public decimal? Temperature { get; set; }
        public string? IPRModel { get; set; }
        public int? NumberOfPoints { get; set; }
        public decimal? FlowRateRangeMax { get; set; }
        public string? UserId { get; set; }
        public string AnalysisType { get; set; }
        public string FieldId { get; set; }
        public string? VLPModel { get; set; }
        public NodalAnalysisOptions? AdditionalParameters { get; set; }
    }
}
