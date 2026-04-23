namespace Beep.OilandGas.Models.Data
{
    public class SetupStatusResult : ModelEntityBase
    {
        private bool hasConnectionValue;

        public bool HasConnection
        {
            get => hasConnectionValue;
            set => SetProperty(ref hasConnectionValue, value);
        }

        private string? connectionNameValue;

        public string? ConnectionName
        {
            get => connectionNameValue;
            set => SetProperty(ref connectionNameValue, value);
        }

        private string? dbTypeValue;

        public string? DbType
        {
            get => dbTypeValue;
            set => SetProperty(ref dbTypeValue, value);
        }

        private bool isSchemaReadyValue;

        public bool IsSchemaReady
        {
            get => isSchemaReadyValue;
            set => SetProperty(ref isSchemaReadyValue, value);
        }
    }
}