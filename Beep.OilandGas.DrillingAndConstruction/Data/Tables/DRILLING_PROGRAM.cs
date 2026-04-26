using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// Drilling program for a well — defines objectives, target depths, and well design basis.
    /// Per PPDM 3.9 / IADC drilling-program standards.
    /// </summary>
    public partial class DRILLING_PROGRAM : ModelEntityBase
    {
        private System.String PROGRAM_IDValue = string.Empty;
        /// <summary>PK — unique drilling program identifier.</summary>
        public System.String PROGRAM_ID
        {
            get { return this.PROGRAM_IDValue; }
            set { SetProperty(ref PROGRAM_IDValue, value); }
        }

        private System.String WELL_IDValue = string.Empty;
        /// <summary>FK to WELL.UWI — the planned/target well.</summary>
        public System.String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private System.String FIELD_IDValue = string.Empty;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String PROGRAM_NAMEValue = string.Empty;
        public System.String PROGRAM_NAME
        {
            get { return this.PROGRAM_NAMEValue; }
            set { SetProperty(ref PROGRAM_NAMEValue, value); }
        }

        private System.String PROGRAM_STATUSValue = string.Empty;
        /// <summary>DRAFT / APPROVED / IN_PROGRESS / COMPLETED / SUSPENDED.</summary>
        public System.String PROGRAM_STATUS
        {
            get { return this.PROGRAM_STATUSValue; }
            set { SetProperty(ref PROGRAM_STATUSValue, value); }
        }

        private System.String WELL_TYPEValue = string.Empty;
        /// <summary>WILDCAT / APPRAISAL / DEVELOPMENT / SIDETRACK / INFILL.</summary>
        public System.String WELL_TYPE
        {
            get { return this.WELL_TYPEValue; }
            set { SetProperty(ref WELL_TYPEValue, value); }
        }

        private System.String WELL_PROFILEValue = string.Empty;
        /// <summary>Directional profile: VERTICAL / J_SHAPE / S_SHAPE / HORIZONTAL.</summary>
        public System.String WELL_PROFILE
        {
            get { return this.WELL_PROFILEValue; }
            set { SetProperty(ref WELL_PROFILEValue, value); }
        }

        private System.Decimal? PLANNED_MEASURED_DEPTHValue;
        /// <summary>Planned total measured depth (MD).</summary>
        public System.Decimal? PLANNED_MEASURED_DEPTH
        {
            get { return this.PLANNED_MEASURED_DEPTHValue; }
            set { SetProperty(ref PLANNED_MEASURED_DEPTHValue, value); }
        }

        private System.Decimal? PLANNED_TRUE_VERTICAL_DEPTHValue;
        /// <summary>Planned total vertical depth (TVD).</summary>
        public System.Decimal? PLANNED_TRUE_VERTICAL_DEPTH
        {
            get { return this.PLANNED_TRUE_VERTICAL_DEPTHValue; }
            set { SetProperty(ref PLANNED_TRUE_VERTICAL_DEPTHValue, value); }
        }

        private System.String DEPTH_OUOMValue = string.Empty;
        public System.String DEPTH_OUOM
        {
            get { return this.DEPTH_OUOMValue; }
            set { SetProperty(ref DEPTH_OUOMValue, value); }
        }

        private System.Decimal? PLANNED_DURATION_DAYSValue;
        /// <summary>Planned drilling duration in days.</summary>
        public System.Decimal? PLANNED_DURATION_DAYS
        {
            get { return this.PLANNED_DURATION_DAYSValue; }
            set { SetProperty(ref PLANNED_DURATION_DAYSValue, value); }
        }

        private System.Decimal? AFE_COSTValue;
        /// <summary>Authorized For Expenditure drilling cost (budget).</summary>
        public System.Decimal? AFE_COST
        {
            get { return this.AFE_COSTValue; }
            set { SetProperty(ref AFE_COSTValue, value); }
        }

        private System.String AFE_CURRENCYValue = string.Empty;
        public System.String AFE_CURRENCY
        {
            get { return this.AFE_CURRENCYValue; }
            set { SetProperty(ref AFE_CURRENCYValue, value); }
        }

        private System.String AFE_NUMBERValue = string.Empty;
        /// <summary>Authorization for Expenditure number.</summary>
        public System.String AFE_NUMBER
        {
            get { return this.AFE_NUMBERValue; }
            set { SetProperty(ref AFE_NUMBERValue, value); }
        }

        private System.String DRILLING_CONTRACTORValue = string.Empty;
        public System.String DRILLING_CONTRACTOR
        {
            get { return this.DRILLING_CONTRACTORValue; }
            set { SetProperty(ref DRILLING_CONTRACTORValue, value); }
        }

        private System.String RIG_NAMEValue = string.Empty;
        public System.String RIG_NAME
        {
            get { return this.RIG_NAMEValue; }
            set { SetProperty(ref RIG_NAMEValue, value); }
        }

        private System.String RIG_TYPEValue = string.Empty;
        /// <summary>LAND / JACK_UP / SEMI_SUBMERSIBLE / DRILLSHIP / PLATFORM.</summary>
        public System.String RIG_TYPE
        {
            get { return this.RIG_TYPEValue; }
            set { SetProperty(ref RIG_TYPEValue, value); }
        }

        private System.DateTime? PLANNED_SPUD_DATEValue;
        public System.DateTime? PLANNED_SPUD_DATE
        {
            get { return this.PLANNED_SPUD_DATEValue; }
            set { SetProperty(ref PLANNED_SPUD_DATEValue, value); }
        }

        private System.DateTime? PLANNED_COMPLETION_DATEValue;
        public System.DateTime? PLANNED_COMPLETION_DATE
        {
            get { return this.PLANNED_COMPLETION_DATEValue; }
            set { SetProperty(ref PLANNED_COMPLETION_DATEValue, value); }
        }

        private System.String DESCRIPTION_OBJECTIVEValue = string.Empty;
        /// <summary>Well drilling objectives and geological targets.</summary>
        public System.String DESCRIPTION_OBJECTIVE
        {
            get { return this.DESCRIPTION_OBJECTIVEValue; }
            set { SetProperty(ref DESCRIPTION_OBJECTIVEValue, value); }
        }

        public DRILLING_PROGRAM() { }
    }
}
