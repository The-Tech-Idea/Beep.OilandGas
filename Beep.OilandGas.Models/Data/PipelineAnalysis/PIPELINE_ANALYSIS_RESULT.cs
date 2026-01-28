using System;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class PIPELINE_ANALYSIS_RESULT : ModelEntityBase {
        private string ANALYSIS_IDValue;
        public string ANALYSIS_ID
        {
            get { return this.ANALYSIS_IDValue; }
            set { SetProperty(ref ANALYSIS_IDValue, value); }
        }

        private string PIPELINE_IDValue;
        public string PIPELINE_ID
        {
            get { return this.PIPELINE_IDValue; }
            set { SetProperty(ref PIPELINE_IDValue, value); }
        }

        private DateTime? ANALYSIS_DATEValue;
        public DateTime? ANALYSIS_DATE
        {
            get { return this.ANALYSIS_DATEValue; }
            set { SetProperty(ref ANALYSIS_DATEValue, value); }
        }

        private decimal? FLOW_RATEValue;
        public decimal? FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private decimal? PRESSURE_DROPValue;
        public decimal? PRESSURE_DROP
        {
            get { return this.PRESSURE_DROPValue; }
            set { SetProperty(ref PRESSURE_DROPValue, value); }
        }

        private decimal? VELOCITYValue;
        public decimal? VELOCITY
        {
            get { return this.VELOCITYValue; }
            set { SetProperty(ref VELOCITYValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string? PipelineIdValue;

        public string? PipelineId

        {

            get { return this.PipelineIdValue; }

            set { SetProperty(ref PipelineIdValue, value); }

        }
        private string PipelineTypeValue = string.Empty;

        public string PipelineType

        {

            get { return this.PipelineTypeValue; }

            set { SetProperty(ref PipelineTypeValue, value); }

        }
        private string AnalysisTypeValue = string.Empty;

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }

        // Flow results
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // Mscf/day (gas) or bbl/day (liquid)
        private decimal? InletPressureValue;

        public decimal? InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        } // psia
        private decimal? OutletPressureValue;

        public decimal? OutletPressure

        {

            get { return this.OutletPressureValue; }

            set { SetProperty(ref OutletPressureValue, value); }

        } // psia
        private decimal? PressureDropValue;

        public decimal? PressureDrop

        {

            get { return this.PressureDropValue; }

            set { SetProperty(ref PressureDropValue, value); }

        } // psi
        private decimal? AveragePressureValue;

        public decimal? AveragePressure

        {

            get { return this.AveragePressureValue; }

            set { SetProperty(ref AveragePressureValue, value); }

        } // psia

        // Capacity results
        private decimal? MaximumCapacityValue;

        public decimal? MaximumCapacity

        {

            get { return this.MaximumCapacityValue; }

            set { SetProperty(ref MaximumCapacityValue, value); }

        } // Mscf/day (gas) or bbl/day (liquid)
        private decimal? UtilizationValue;

        public decimal? Utilization

        {

            get { return this.UtilizationValue; }

            set { SetProperty(ref UtilizationValue, value); }

        } // fraction 0-1

        // Flow regime analysis
        private decimal? ReynoldsNumberValue;

        public decimal? ReynoldsNumber

        {

            get { return this.ReynoldsNumberValue; }

            set { SetProperty(ref ReynoldsNumberValue, value); }

        }
        private decimal? FrictionFactorValue;

        public decimal? FrictionFactor

        {

            get { return this.FrictionFactorValue; }

            set { SetProperty(ref FrictionFactorValue, value); }

        }
        private string FlowRegimeValue = string.Empty;

        public string FlowRegime

        {

            get { return this.FlowRegimeValue; }

            set { SetProperty(ref FlowRegimeValue, value); }

        } // LAMINAR, TURBULENT, TRANSITION

        // Pipeline properties used
        private decimal? LengthValue;

        public decimal? Length

        {

            get { return this.LengthValue; }

            set { SetProperty(ref LengthValue, value); }

        } // miles or feet
        private decimal? DiameterValue;

        public decimal? Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        } // inches
        private decimal? RoughnessValue;

        public decimal? Roughness

        {

            get { return this.RoughnessValue; }

            set { SetProperty(ref RoughnessValue, value); }

        } // inches

        // Additional metadata
        public Beep.OilandGas.Models.Data.Calculations.PipelineAnalysisAdditionalResults? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        public string AnalysisId { get; set; }
        public DateTime? AnalysisDate { get; set; }
        public decimal? Velocity { get; set; }
        public string Recommendations { get; set; }
    }
}
