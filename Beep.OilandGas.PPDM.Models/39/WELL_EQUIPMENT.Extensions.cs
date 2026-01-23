namespace Beep.OilandGas.PPDM39.Models
{
    public partial class WELL_EQUIPMENT
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

        private System.String EQUIPMENT_TYPEValue;
        public System.String EQUIPMENT_TYPE
        {
            get
            {
                return this.EQUIPMENT_TYPEValue;
            }

            set { SetProperty(ref EQUIPMENT_TYPEValue, value); }
        }

        private System.String EQUIPMENT_NAMEValue;
        public System.String EQUIPMENT_NAME
        {
            get
            {
                return this.EQUIPMENT_NAMEValue;
            }

            set { SetProperty(ref EQUIPMENT_NAMEValue, value); }
        }

        private System.String MANUFACTURERValue;
        public System.String MANUFACTURER
        {
            get
            {
                return this.MANUFACTURERValue;
            }

            set { SetProperty(ref MANUFACTURERValue, value); }
        }

        private System.String MODELValue;
        public System.String MODEL
        {
            get
            {
                return this.MODELValue;
            }

            set { SetProperty(ref MODELValue, value); }
        }

        private System.DateTime? INSTALLATION_DATEValue;
        public System.DateTime? INSTALLATION_DATE
        {
            get
            {
                return this.INSTALLATION_DATEValue;
            }

            set { SetProperty(ref INSTALLATION_DATEValue, value); }
        }
    }
}
