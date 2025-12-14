using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Statistics about a data loading operation.
    /// </summary>
    public class DataLoadStatistics
    {
        /// <summary>
        /// Gets or sets the start time of the load operation.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the end time of the load operation.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets the duration of the load operation.
        /// </summary>
        public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.Now - StartTime;

        /// <summary>
        /// Gets or sets the number of records loaded.
        /// </summary>
        public int RecordsLoaded { get; set; }

        /// <summary>
        /// Gets or sets the number of records skipped.
        /// </summary>
        public int RecordsSkipped { get; set; }

        /// <summary>
        /// Gets or sets the number of errors encountered.
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Gets or sets the number of warnings.
        /// </summary>
        public int WarningCount { get; set; }

        /// <summary>
        /// Gets or sets the data size in bytes.
        /// </summary>
        public long DataSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets the load rate (records per second).
        /// </summary>
        public double LoadRate
        {
            get
            {
                var duration = Duration.TotalSeconds;
                return duration > 0 ? RecordsLoaded / duration : 0;
            }
        }

        /// <summary>
        /// Gets or sets additional statistics.
        /// </summary>
        public Dictionary<string, object> AdditionalStats { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Marks the load operation as complete.
        /// </summary>
        public void Complete()
        {
            EndTime = DateTime.Now;
        }

        /// <summary>
        /// Gets a summary string.
        /// </summary>
        public string GetSummary()
        {
            return $"Loaded {RecordsLoaded} records in {Duration.TotalSeconds:F2} seconds " +
                   $"({LoadRate:F2} records/sec). Errors: {ErrorCount}, Warnings: {WarningCount}.";
        }
    }
}

