using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public partial class EXPLORATION_PROGRAM : ModelEntityBase
    {
        private System.String PROGRAM_IDValue;
        public System.String PROGRAM_ID
        {
            get
            {
                return this.PROGRAM_IDValue;
            }
            set { SetProperty(ref PROGRAM_IDValue, value); }
        }
        private System.String PROGRAM_NAMEValue;
        public System.String PROGRAM_NAME
        {
            get
            {
                return this.PROGRAM_NAMEValue;
            }
            set { SetProperty(ref PROGRAM_NAMEValue, value); }
        }
        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get
            {
                return this.FIELD_IDValue;
            }
            set { SetProperty(ref FIELD_IDValue, value); }
        }
        private System.String OPERATORValue;
        public System.String OPERATOR
        {
            get
            {
                return this.OPERATORValue;
            }
            set { SetProperty(ref OPERATORValue, value); }
        }
        private System.String OBJECTIVESValue;
        public System.String OBJECTIVES
        {
            get
            {
                return this.OBJECTIVESValue;
            }
            set { SetProperty(ref OBJECTIVESValue, value); }
        }
        private System.DateTime? PROGRAM_START_DATEValue;
        public System.DateTime? PROGRAM_START_DATE
        {
            get
            {
                return this.PROGRAM_START_DATEValue;
            }
            set { SetProperty(ref PROGRAM_START_DATEValue, value); }
        }
        private System.DateTime? PROGRAM_END_DATEValue;
        public System.DateTime? PROGRAM_END_DATE
        {
            get
            {
                return this.PROGRAM_END_DATEValue;
            }
            set { SetProperty(ref PROGRAM_END_DATEValue, value); }
        }
        private System.Decimal? BUDGETValue;
        public System.Decimal? BUDGET
        {
            get
            {
                return this.BUDGETValue;
            }
            set { SetProperty(ref BUDGETValue, value); }
        }
        private System.String BUDGET_CURRENCYValue;
        public System.String BUDGET_CURRENCY
        {
            get
            {
                return this.BUDGET_CURRENCYValue;
            }
            set { SetProperty(ref BUDGET_CURRENCYValue, value); }
        }
        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get
            {
                return this.DESCRIPTIONValue;
            }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }
        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

        private System.String REMARKValue;

        private System.String SOURCEValue;

        public EXPLORATION_PROGRAM() { }
    }
}


