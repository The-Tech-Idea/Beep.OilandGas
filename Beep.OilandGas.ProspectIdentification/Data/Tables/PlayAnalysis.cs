namespace Beep.OilandGas.Models.Data
{
    public class PlayAnalysis : ModelEntityBase
    {
        private string PlayIdValue = string.Empty;

        public string PlayId

        {

            get { return this.PlayIdValue; }

            set { SetProperty(ref PlayIdValue, value); }

        }
    }
}
