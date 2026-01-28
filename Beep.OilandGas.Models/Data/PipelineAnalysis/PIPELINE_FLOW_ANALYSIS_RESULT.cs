using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public partial class PIPELINE_FLOW_ANALYSIS_RESULT : ModelEntityBase {
        private String PIPELINE_FLOW_ANALYSIS_RESULT_IDValue;
        public String PIPELINE_FLOW_ANALYSIS_RESULT_ID
        {
            get { return this.PIPELINE_FLOW_ANALYSIS_RESULT_IDValue; }
            set { SetProperty(ref PIPELINE_FLOW_ANALYSIS_RESULT_IDValue, value); }
        }

        private String PIPELINE_PROPERTIES_IDValue;
        public String PIPELINE_PROPERTIES_ID
        {
            get { return this.PIPELINE_PROPERTIES_IDValue; }
            set { SetProperty(ref PIPELINE_PROPERTIES_IDValue, value); }
        }

        private Decimal? FLOW_RATEValue;
        public Decimal? FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private Decimal? PRESSURE_DROPValue;
        public Decimal? PRESSURE_DROP
        {
            get { return this.PRESSURE_DROPValue; }
            set { SetProperty(ref PRESSURE_DROPValue, value); }
        }

        private Decimal? FLOW_VELOCITYValue;
        public Decimal? FLOW_VELOCITY
        {
            get { return this.FLOW_VELOCITYValue; }
            set { SetProperty(ref FLOW_VELOCITYValue, value); }
        }

        private Decimal? REYNOLDS_NUMBERValue;
        public Decimal? REYNOLDS_NUMBER
        {
            get { return this.REYNOLDS_NUMBERValue; }
            set { SetProperty(ref REYNOLDS_NUMBERValue, value); }
        }

        private Decimal? FRICTION_FACTORValue;
        public Decimal? FRICTION_FACTOR
        {
            get { return this.FRICTION_FACTORValue; }
            set { SetProperty(ref FRICTION_FACTORValue, value); }
        }

        private Decimal? PRESSURE_GRADIENTValue;
        public Decimal? PRESSURE_GRADIENT
        {
            get { return this.PRESSURE_GRADIENTValue; }
            set { SetProperty(ref PRESSURE_GRADIENTValue, value); }
        }

        private Decimal? OUTLET_PRESSUREValue;
        public Decimal? OUTLET_PRESSURE
        {
            get { return this.OUTLET_PRESSUREValue; }
            set { SetProperty(ref OUTLET_PRESSUREValue, value); }
        }

        private String FLOW_REGIMEValue;
        public String FLOW_REGIME
        {
            get { return this.FLOW_REGIMEValue; }
            set { SetProperty(ref FLOW_REGIMEValue, value); }
        }

        // Analysis Compatibility
        public string AnalysisId
        {
            get { return this.PIPELINE_FLOW_ANALYSIS_RESULT_ID; }
            set { this.PIPELINE_FLOW_ANALYSIS_RESULT_ID = value; }
        }
        public string PipelineId
        {
            get { return this.PIPELINE_PROPERTIES_ID; }
            set { this.PIPELINE_PROPERTIES_ID = value; }
        }
        public DateTime AnalysisDate
        {
             get { return DateTime.Now; } // Placeholder if not in PPDM
             set {  }
        }
        public double FlowRate
        {
            get { return (double)(this.FLOW_RATE ?? 0); }
            set { this.FLOW_RATE = (decimal)value; }
        }
        // Missing PPDM property for InletPressure, adding usage
        private decimal? INLET_PRESSUREValue;
        public decimal? INLET_PRESSURE
        {
            get { return INLET_PRESSUREValue; }
            set { SetProperty(ref INLET_PRESSUREValue, value); }
        }
        public double InletPressure {
            get { return (double)(this.INLET_PRESSURE ?? 0); }
            set { this.INLET_PRESSURE = (decimal)value; }
        }

        public double OutletPressure
        {
            get { return (double)(this.OUTLET_PRESSURE ?? 0); }
            set { this.OUTLET_PRESSURE = (decimal)value; }
        }
        public double PressureDrop
        {
            get { return (double)(this.PRESSURE_DROP ?? 0); }
            set { this.PRESSURE_DROP = (decimal)value; }
        }
        public double Velocity
        {
            get { return (double)(this.FLOW_VELOCITY ?? 0); }
            set { this.FLOW_VELOCITY = (decimal)value; }
        }
        public string FlowRegime
        {
            get { return this.FLOW_REGIME; }
            set { this.FLOW_REGIME = value; }
        }
        
        private string STATUSValue;
        public string Status
        {
             get { return this.STATUSValue; }
             set { SetProperty(ref STATUSValue, value); }
        }

        private string RECOMMENDATIONSValue;
        public string Recommendations
        {
             get { return this.RECOMMENDATIONSValue; }
             set { SetProperty(ref RECOMMENDATIONSValue, value); }
        }

    }
}
