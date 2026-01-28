using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class NodalAnalysisResult : ModelEntityBase
    {
        private string AnalysisIdValue;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        public string CalculationId

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
        public DateTime CalculationDate

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
        public decimal OperatingFlowRate

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
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        public NodalAnalysisAdditionalResults? AdditionalResults { get; set; }
        public string WellId { get; set; }
        public string WellboreId { get; set; }
        public string FieldId { get; set; }
        public string AnalysisType { get; set; }
        public string? UserId { get; set; }
        public List<string> Recommendations { get; set; }
        public decimal? OperatingTemperature { get; set; }
        public decimal? MaximumFlowRate { get; set; }
        public decimal MinimumFlowRate { get; set; }
        public decimal OptimalFlowRate { get; set; }
        public decimal PressureDrop { get; set; }
        public decimal SystemEfficiency { get; set; }
        public NodalOperatingPoint? OperatingPoint { get; set; }
    }
}
