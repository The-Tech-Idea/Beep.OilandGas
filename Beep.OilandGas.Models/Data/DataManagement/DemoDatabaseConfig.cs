namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Configuration for demo database settings
    /// </summary>
    public class DemoDatabaseConfig : ModelEntityBase
    {
        /// <summary>
        /// Storage path for demo SQLite databases
        /// </summary>
        private string StoragePathValue = "./demos";

        public string StoragePath

        {

            get { return this.StoragePathValue; }

            set { SetProperty(ref StoragePathValue, value); }

        }

        /// <summary>
        /// Number of days to retain demo databases before cleanup
        /// </summary>
        private int RetentionDaysValue = 7;

        public int RetentionDays

        {

            get { return this.RetentionDaysValue; }

            set { SetProperty(ref RetentionDaysValue, value); }

        }

        /// <summary>
        /// Cron expression for cleanup schedule (default: daily at 2 AM)
        /// </summary>
        private string CleanupScheduleValue = "0 2 * * *";

        public string CleanupSchedule

        {

            get { return this.CleanupScheduleValue; }

            set { SetProperty(ref CleanupScheduleValue, value); }

        }

        /// <summary>
        /// Whether demo database creation is enabled
        /// </summary>
        private bool EnabledValue = true;

        public bool Enabled

        {

            get { return this.EnabledValue; }

            set { SetProperty(ref EnabledValue, value); }

        }

        /// <summary>
        /// Default seed data option for new demo databases
        /// </summary>
        private string DefaultSeedOptionValue = "reference-sample";

        public string DefaultSeedOption

        {

            get { return this.DefaultSeedOptionValue; }

            set { SetProperty(ref DefaultSeedOptionValue, value); }

        }
    }
}





