namespace Beep.OilandGas.Models.Data
{
    public class GenerateDummyDataRequest : ModelEntityBase
    {
        private string seedOptionValue = "standard";

        public string SeedOption
        {
            get => seedOptionValue;
            set => SetProperty(ref seedOptionValue, value);
        }

        private string userIdValue = "SYSTEM";

        public string UserId
        {
            get => userIdValue;
            set => SetProperty(ref userIdValue, value);
        }
    }
}