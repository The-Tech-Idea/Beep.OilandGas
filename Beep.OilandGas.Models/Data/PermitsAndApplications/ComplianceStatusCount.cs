using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class ComplianceStatusCount : ModelEntityBase
    {
        private string StatusValue = string.Empty;
        public string Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }

        private int CountValue;
        public int Count
        {
            get { return this.CountValue; }
            set { SetProperty(ref CountValue, value); }
        }
    }
}
