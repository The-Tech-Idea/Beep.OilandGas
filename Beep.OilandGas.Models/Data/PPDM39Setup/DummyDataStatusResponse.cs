namespace Beep.OilandGas.Models.Data
{
    public class DummyDataStatusResponse : ModelEntityBase
    {
        private bool hasDummyDataValue;

        public bool HasDummyData
        {
            get => hasDummyDataValue;
            set => SetProperty(ref hasDummyDataValue, value);
        }

        private string reasonValue = string.Empty;

        public string Reason
        {
            get => reasonValue;
            set => SetProperty(ref reasonValue, value);
        }
    }
}
