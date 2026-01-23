namespace Beep.OilandGas.PPDM39.Models
{
    public partial class WELL
    {
        private System.String WELL_IDValue;
        public System.String WELL_ID
        {
            get
            {
                return this.WELL_IDValue;
            }

            set { SetProperty(ref WELL_IDValue, value); }
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

        private System.String WELL_TYPEValue;
        public System.String WELL_TYPE
        {
            get
            {
                return this.WELL_TYPEValue;
            }

            set { SetProperty(ref WELL_TYPEValue, value); }
        }

        private System.String WELL_PURPOSEValue;
        public System.String WELL_PURPOSE
        {
            get
            {
                return this.WELL_PURPOSEValue;
            }

            set { SetProperty(ref WELL_PURPOSEValue, value); }
        }
    }
}
