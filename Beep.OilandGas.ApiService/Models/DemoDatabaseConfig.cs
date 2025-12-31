namespace Beep.OilandGas.ApiService.Models
{
    /// <summary>
    /// Configuration for demo database settings
    /// </summary>
    public class DemoDatabaseConfig
    {
        /// <summary>
        /// Storage path for demo SQLite databases
        /// </summary>
        public string StoragePath { get; set; } = "./demos";

        /// <summary>
        /// Number of days to retain demo databases before cleanup
        /// </summary>
        public int RetentionDays { get; set; } = 7;

        /// <summary>
        /// Cron expression for cleanup schedule (default: daily at 2 AM)
        /// </summary>
        public string CleanupSchedule { get; set; } = "0 2 * * *";

        /// <summary>
        /// Whether demo database creation is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Default seed data option for new demo databases
        /// </summary>
        public string DefaultSeedOption { get; set; } = "reference-sample";
    }
}

