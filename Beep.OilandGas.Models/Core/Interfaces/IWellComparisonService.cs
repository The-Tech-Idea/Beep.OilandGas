using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for comparing wells
    /// </summary>
    public interface IWellComparisonService
    {
        /// <summary>
        /// Compares multiple wells by their identifiers (UWIs)
        /// </summary>
        /// <param name="wellIdentifiers">List of well identifiers (UWIs) to compare</param>
        /// <param name="fieldNames">Optional list of specific fields to compare. If null, compares all common fields</param>
        /// <param name="connectionName">Connection name (defaults to repository connection)</param>
        /// <returns>WellComparison with side-by-side comparison</returns>
        Task<WellComparison> CompareWellsAsync(
            List<string> wellIdentifiers,
            List<string> fieldNames = null,
            string connectionName = null);

        /// <summary>
        /// Compares wells from different data sources
        /// </summary>
        /// <param name="wellComparisons">List of well identifiers with their data sources</param>
        /// <param name="fieldNames">Optional list of specific fields to compare</param>
        /// <returns>WellComparison with side-by-side comparison across data sources</returns>
        Task<WellComparison> CompareWellsFromMultipleSourcesAsync(
            List<WellSourceMapping> wellComparisons,
            List<string> fieldNames = null);

        /// <summary>
        /// Gets available comparison fields for wells
        /// </summary>
        /// <returns>List of available comparison fields with metadata</returns>
        Task<List<ComparisonField>> GetAvailableComparisonFieldsAsync();
    }
}




