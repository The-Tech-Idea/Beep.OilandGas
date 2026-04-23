namespace Beep.OilandGas.Models.Data
{
    public class CreateSqliteResult : ModelEntityBase
    {
        private bool successValue;

        public bool Success
        {
            get => successValue;
            set => SetProperty(ref successValue, value);
        }

        private string? connectionNameValue;

        public string? ConnectionName
        {
            get => connectionNameValue;
            set => SetProperty(ref connectionNameValue, value);
        }

        private string? filePathValue;

        public string? FilePath
        {
            get => filePathValue;
            set => SetProperty(ref filePathValue, value);
        }

        private string? dbTypeValue;

        public string? DbType
        {
            get => dbTypeValue;
            set => SetProperty(ref dbTypeValue, value);
        }

        private string? messageValue;

        public string? Message
        {
            get => messageValue;
            set => SetProperty(ref messageValue, value);
        }

        private string? errorDetailsValue;

        public string? ErrorDetails
        {
            get => errorDetailsValue;
            set => SetProperty(ref errorDetailsValue, value);
        }
    }
}