using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Request for performing nodal analysis
    /// </summary>
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
    }

    /// <summary>
    /// Request for optimizing system performance
    /// </summary>
    public class OptimizeSystemRequest : ModelEntityBase
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
        /// Optimization goals
        /// </summary>
        private OptimizationGoals OptimizationGoalsValue = null!;

        [Required(ErrorMessage = "OptimizationGoals are required")]
        public OptimizationGoals OptimizationGoals

        {

            get { return this.OptimizationGoalsValue; }

            set { SetProperty(ref OptimizationGoalsValue, value); }

        }
    }
    public class NodalCurvePoint : ModelEntityBase
    {
        private decimal RateValue;

        public decimal Rate

        {

            get { return this.RateValue; }

            set { SetProperty(ref RateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private string CurveTypeValue;

        public string CurveType

        {

            get { return this.CurveTypeValue; }

            set { SetProperty(ref CurveTypeValue, value); }

        } // IPR or VLP

        public decimal FlowRate { get; set; }
    }

    public class NodalOperatingPoint : ModelEntityBase
    {
        private decimal RateValue;

        public decimal Rate

        {

            get { return this.RateValue; }

            set { SetProperty(ref RateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private bool IsStableValue;

        public bool IsStable

        {

            get { return this.IsStableValue; }

            set { SetProperty(ref IsStableValue, value); }

        }
    }

    public class NodalAnalysisResult : ModelEntityBase
    {
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal OperatingRateValue;

        public decimal OperatingRate

        {

            get { return this.OperatingRateValue; }

            set { SetProperty(ref OperatingRateValue, value); }

        }
        private decimal OperatingPressureValue;

        public decimal OperatingPressure

        {

            get { return this.OperatingPressureValue; }

            set { SetProperty(ref OperatingPressureValue, value); }

        }
        private List<NodalCurvePoint> IPRCurveValue;

        public List<NodalCurvePoint> IPRCurve

        {

            get { return this.IPRCurveValue; }

            set { SetProperty(ref IPRCurveValue, value); }

        }
        private List<NodalCurvePoint> VLPCurveValue;

        public List<NodalCurvePoint> VLPCurve

        {

            get { return this.VLPCurveValue; }

            set { SetProperty(ref VLPCurveValue, value); }

        }
        private List<NodalOperatingPoint> IntersectionsValue;

        public List<NodalOperatingPoint> Intersections

        {

            get { return this.IntersectionsValue; }

            set { SetProperty(ref IntersectionsValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string ErrorMessageValue;

        public string ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public decimal OperatingFlowRate { get; set; }
        public string CalculationId { get; set; }
        public string WellId { get; set; }
        public string WellboreId { get; set; }
        public string FieldId { get; set; }
        public string AnalysisType { get; set; }
        public DateTime CalculationDate { get; set; }
        public string? UserId { get; set; }
        public List<string> Recommendations { get; set; }
        public decimal? OperatingTemperature { get; set; }
        public decimal? MaximumFlowRate { get; set; }
        public decimal MinimumFlowRate { get; set; }
        public decimal OptimalFlowRate { get; set; }
        public decimal PressureDrop { get; set; }
        public decimal SystemEfficiency { get; set; }
    }
}






