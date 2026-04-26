namespace Beep.OilandGas.Models.Data
{
    public class DummyDataDeleteResponse : ModelEntityBase
    {
        private bool successValue;

        public bool Success
        {
            get => successValue;
            set => SetProperty(ref successValue, value);
        }

        private string messageValue = string.Empty;

        public string Message
        {
            get => messageValue;
            set => SetProperty(ref messageValue, value);
        }

        private int recordsDeletedValue;

        public int RecordsDeleted
        {
            get => recordsDeletedValue;
            set => SetProperty(ref recordsDeletedValue, value);
        }
    }
}
