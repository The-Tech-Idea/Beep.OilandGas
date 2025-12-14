using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders
{
    /// <summary>
    /// Interface for loading well schematic data from various sources (PPDM38, SeaBed, database, etc.).
    /// </summary>
    public interface ISchematicLoader : IDataLoader<WellData>
    {
        /// <summary>
        /// Loads well schematic data for a specific well.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier (UWI, API, etc.).</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The well data.</returns>
        WellData LoadSchematic(string wellIdentifier, WellSchematicLoadConfiguration configuration = null);

        /// <summary>
        /// Loads well schematic data asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The well data.</returns>
        Task<WellData> LoadSchematicAsync(string wellIdentifier, WellSchematicLoadConfiguration configuration = null);

        /// <summary>
        /// Loads well schematic data with result object.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The load result.</returns>
        DataLoadResult<WellData> LoadSchematicWithResult(string wellIdentifier, WellSchematicLoadConfiguration configuration = null);

        /// <summary>
        /// Loads well schematic data with result object asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The load result.</returns>
        Task<DataLoadResult<WellData>> LoadSchematicWithResultAsync(string wellIdentifier, WellSchematicLoadConfiguration configuration = null);

        /// <summary>
        /// Loads well schematic data with extended information.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The well schematic data with metadata.</returns>
        WellSchematicData LoadSchematicData(string wellIdentifier, WellSchematicLoadConfiguration configuration = null);

        /// <summary>
        /// Loads well schematic data with extended information asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The well schematic data with metadata.</returns>
        Task<WellSchematicData> LoadSchematicDataAsync(string wellIdentifier, WellSchematicLoadConfiguration configuration = null);

        /// <summary>
        /// Loads multiple well schematics.
        /// </summary>
        /// <param name="wellIdentifiers">List of well identifiers.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>Dictionary of well identifier to well data.</returns>
        Dictionary<string, WellData> LoadSchematics(List<string> wellIdentifiers, WellSchematicLoadConfiguration configuration = null);

        /// <summary>
        /// Loads multiple well schematics asynchronously.
        /// </summary>
        /// <param name="wellIdentifiers">List of well identifiers.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>Dictionary of well identifier to well data.</returns>
        Task<Dictionary<string, WellData>> LoadSchematicsAsync(List<string> wellIdentifiers, WellSchematicLoadConfiguration configuration = null);

        /// <summary>
        /// Gets available well identifiers.
        /// </summary>
        /// <returns>List of available well identifiers.</returns>
        List<string> GetAvailableWells();

        /// <summary>
        /// Gets available well identifiers asynchronously.
        /// </summary>
        /// <returns>List of available well identifiers.</returns>
        Task<List<string>> GetAvailableWellsAsync();

        /// <summary>
        /// Loads deviation survey for a well.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="boreholeIdentifier">Optional borehole identifier.</param>
        /// <returns>The deviation survey.</returns>
        DeviationSurvey LoadDeviationSurvey(string wellIdentifier, string boreholeIdentifier = null);

        /// <summary>
        /// Loads deviation survey asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="boreholeIdentifier">Optional borehole identifier.</param>
        /// <returns>The deviation survey.</returns>
        Task<DeviationSurvey> LoadDeviationSurveyAsync(string wellIdentifier, string boreholeIdentifier = null);
    }
}
