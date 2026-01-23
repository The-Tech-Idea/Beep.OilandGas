namespace Beep.OilandGas.PPDM39.Models
{
    public partial class PDEN_FACILITY
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
