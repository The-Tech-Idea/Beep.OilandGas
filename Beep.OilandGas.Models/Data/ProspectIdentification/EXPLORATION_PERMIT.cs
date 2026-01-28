using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public partial class EXPLORATION_PERMIT : ModelEntityBase
    {
        private System.String PERMIT_IDValue;
        public System.String PERMIT_ID
        {
            get
            {
                return this.PERMIT_IDValue;
            }
            set { SetProperty(ref PERMIT_IDValue, value); }
        }
        private System.String PERMIT_NUMBERValue;
        public System.String PERMIT_NUMBER
        {
            get
            {
                return this.PERMIT_NUMBERValue;
            }
            set { SetProperty(ref PERMIT_NUMBERValue, value); }
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
        private System.String PERMIT_HOLDERValue;
        public System.String PERMIT_HOLDER
        {
            get
            {
                return this.PERMIT_HOLDERValue;
            }
            set { SetProperty(ref PERMIT_HOLDERValue, value); }
        }
        private System.DateTime? PERMIT_START_DATEValue;
        public System.DateTime? PERMIT_START_DATE
        {
            get
            {
                return this.PERMIT_START_DATEValue;
            }
            set { SetProperty(ref PERMIT_START_DATEValue, value); }
        }
        private System.DateTime? PERMIT_END_DATEValue;
        public System.DateTime? PERMIT_END_DATE
        {
            get
            {
                return this.PERMIT_END_DATEValue;
            }
            set { SetProperty(ref PERMIT_END_DATEValue, value); }
        }
        private System.Decimal? PERMIT_AREAValue;
        public System.Decimal? PERMIT_AREA
        {
            get
            {
                return this.PERMIT_AREAValue;
            }
            set { SetProperty(ref PERMIT_AREAValue, value); }
        }
        private System.String PERMIT_AREA_OUOMValue;
        public System.String PERMIT_AREA_OUOM
        {
            get
            {
                return this.PERMIT_AREA_OUOMValue;
            }
            set { SetProperty(ref PERMIT_AREA_OUOMValue, value); }
        }
        private System.String PERMIT_TERMSValue;
        public System.String PERMIT_TERMS
        {
            get
            {
                return this.PERMIT_TERMSValue;
            }
            set { SetProperty(ref PERMIT_TERMSValue, value); }
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

        public EXPLORATION_PERMIT() { }
    }
}
