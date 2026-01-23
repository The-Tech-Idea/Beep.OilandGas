namespace Beep.OilandGas.PPDM39.Models
{
    public partial class SEIS_BIN_POINT
    {
        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get
            {
                return this.SOURCEValue;
            }

            set { SetProperty(ref SOURCEValue, value); }
        }
    }
}
