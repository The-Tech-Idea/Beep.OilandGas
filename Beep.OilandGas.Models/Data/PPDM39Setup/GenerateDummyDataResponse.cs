namespace Beep.OilandGas.Models.Data
{
    public class GenerateDummyDataResponse : ModelEntityBase
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

        private string seedOptionValue = string.Empty;

        public string SeedOption
        {
            get => seedOptionValue;
            set => SetProperty(ref seedOptionValue, value);
        }

        private int fieldsCreatedValue;

        public int FieldsCreated
        {
            get => fieldsCreatedValue;
            set => SetProperty(ref fieldsCreatedValue, value);
        }

        private int wellsCreatedValue;

        public int WellsCreated
        {
            get => wellsCreatedValue;
            set => SetProperty(ref wellsCreatedValue, value);
        }

        private int productionRecordsValue;

        public int ProductionRecords
        {
            get => productionRecordsValue;
            set => SetProperty(ref productionRecordsValue, value);
        }

        private int facilitiesCreatedValue;

        public int FacilitiesCreated
        {
            get => facilitiesCreatedValue;
            set => SetProperty(ref facilitiesCreatedValue, value);
        }

        private int wellTestsCreatedValue;

        public int WellTestsCreated
        {
            get => wellTestsCreatedValue;
            set => SetProperty(ref wellTestsCreatedValue, value);
        }

        private int activitiesCreatedValue;

        public int ActivitiesCreated
        {
            get => activitiesCreatedValue;
            set => SetProperty(ref activitiesCreatedValue, value);
        }

        private string? errorDetailsValue;

        public string? ErrorDetails
        {
            get => errorDetailsValue;
            set => SetProperty(ref errorDetailsValue, value);
        }
    }
}