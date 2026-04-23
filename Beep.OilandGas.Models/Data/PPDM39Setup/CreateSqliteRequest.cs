namespace Beep.OilandGas.Models.Data
{
    public class CreateSqliteRequest : ModelEntityBase
    {
        private string connectionNameValue = "PPDM39";

        public string ConnectionName
        {
            get => connectionNameValue;
            set => SetProperty(ref connectionNameValue, value);
        }

        private string? fileNameValue;

        public string? FileName
        {
            get => fileNameValue;
            set => SetProperty(ref fileNameValue, value);
        }

        private string? savePathValue;

        public string? SavePath
        {
            get => savePathValue;
            set => SetProperty(ref savePathValue, value);
        }
    }
}