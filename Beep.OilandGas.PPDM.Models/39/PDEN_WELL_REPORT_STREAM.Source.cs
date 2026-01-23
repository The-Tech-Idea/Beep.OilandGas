namespace Beep.OilandGas.PPDM39.Models
{
    public partial class PDEN_WELL_REPORT_STREAM
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
