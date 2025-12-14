using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.DataLoaders.Models
{
    /// <summary>
    /// Result of a data loading operation.
    /// </summary>
    /// <typeparam name="T">The type of data loaded.</typeparam>
    public class DataLoadResult<T>
    {
        /// <summary>
        /// Gets or sets whether the load was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the loaded data.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets error messages if load failed.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets warning messages.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the number of records loaded.
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// Gets or sets the load duration.
        /// </summary>
        public TimeSpan LoadDuration { get; set; }

        /// <summary>
        /// Gets or sets additional metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the load statistics.
        /// </summary>
        public DataLoadStatistics Statistics { get; set; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static DataLoadResult<T> CreateSuccess(T data, int recordCount = 0)
        {
            return new DataLoadResult<T>
            {
                Success = true,
                Data = data,
                RecordCount = recordCount
            };
        }

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        public static DataLoadResult<T> CreateFailure(string error, params string[] additionalErrors)
        {
            var errors = new List<string> { error };
            if (additionalErrors != null)
                errors.AddRange(additionalErrors);

            return new DataLoadResult<T>
            {
                Success = false,
                Errors = errors
            };
        }
    }
}

