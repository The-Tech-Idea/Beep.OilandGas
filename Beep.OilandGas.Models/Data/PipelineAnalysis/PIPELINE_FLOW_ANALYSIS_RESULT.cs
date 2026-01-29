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

        private decimal  FLOW_RATEValue;
        public decimal  FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private decimal  PRESSURE_DROPValue;
        public decimal  PRESSURE_DROP
        {
            get { return this.PRESSURE_DROPValue; }
            set { SetProperty(ref PRESSURE_DROPValue, value); }
        }

        private decimal  FLOW_VELOCITYValue;
        public decimal  FLOW_VELOCITY
        {
            get { return this.FLOW_VELOCITYValue; }
            set { SetProperty(ref FLOW_VELOCITYValue, value); }
        }

        private decimal  REYNOLDS_NUMBERValue;
        public decimal  REYNOLDS_NUMBER
        {
            get { return this.REYNOLDS_NUMBERValue; }
            set { SetProperty(ref REYNOLDS_NUMBERValue, value); }
        }

        private decimal  FRICTION_FACTORValue;
        public decimal  FRICTION_FACTOR
        {
            get { return this.FRICTION_FACTORValue; }
            set { SetProperty(ref FRICTION_FACTORValue, value); }
        }

        private decimal  PRESSURE_GRADIENTValue;
        public decimal  PRESSURE_GRADIENT
        {
            get { return this.PRESSURE_GRADIENTValue; }
            set { SetProperty(ref PRESSURE_GRADIENTValue, value); }
        }

        private decimal  OUTLET_PRESSUREValue;
        public decimal  OUTLET_PRESSURE
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
            get { return (double)(this.FLOW_RATE ); }
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
            get { return (double)(this.OUTLET_PRESSURE ); }
            set { this.OUTLET_PRESSURE = (decimal)value; }
        }
        public double PressureDrop
        {
            get { return (double)(this.PRESSURE_DROP ); }
            set { this.PRESSURE_DROP = (decimal)value; }
        }
        public double Velocity
        {
            get { return (double)(this.FLOW_VELOCITY); }
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
